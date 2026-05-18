# Game Server

ASP.NET Core 9 기반 게임 백엔드 서버.

---

## 프로젝트 구조

```
project/
├── config/dev/
│   ├── database.json       # DB 접속 정보 (커밋 안 됨)
│   └── config.json         # 선택적 설정 (Redis 등)
├── server/
│   ├── server/             # ASP.NET Core 9 API 서버
│   │   └── Scripts/
│   │       ├── Database/   # DbContext, 샤드 매니저, EF 마이그레이션
│   │       ├── Handler/    # 요청 핸들러 (리플렉션으로 자동 등록)
│   │       ├── Middleware/ # TransactionMiddleware
│   │       └── Redis/      # RedisClient
│   └── admin/              # Blazor Server 관리자 패널
├── shared/                 # 서버·클라이언트 공유 계약 레이어
│   └── src/
│       ├── Shared.Auth.cs / Shared.Game.cs / ...
│       ├── enum/           # MessageId, ResponseResult
│       └── table/          # TableManager (CSV 게임 테이블)
├── client/                 # 콘솔 클라이언트 (테스트 하네스)
└── sync/                   # Google Sheets → CSV 동기화 도구
```

---

## 실행 방법

```bash
# API 서버 (Swagger: http://localhost:5031/swagger)
cd server/server
dotnet run

# 관리자 패널
cd server/admin
dotnet run

# Google Sheets → shared/table/*.csv 동기화
cd sync
dotnet run
```

---

## 아키텍처

### Handler 자동 등록

`HandlerHelper`가 리플렉션으로 `*Handler` 클래스를 스캔해 DI 등록과 라우트 매핑을 자동으로 처리한다.

- 라우트: `POST /{RequestTypeName}` (예: `POST /GachaRequest`)
- 새 엔드포인트 추가: `shared/src/`에 `*Request`/`*Response` 정의 → `*Handler` 클래스에 해당 Request를 첫 번째 파라미터로 받는 public 메서드 작성. 라우트 등록 코드 불필요.

### 트랜잭션 미들웨어

`TransactionMiddleware`가 모든 요청을 트랜잭션으로 감싼다.

- **UserDb**: 항상 자동으로 트랜잭션 시작 → 미들웨어가 커밋/롤백.
- **GameDb**: 핸들러가 `GameShardTransactionContext.SetShardAsync(shard)`를 명시적으로 호출해야 해당 샤드가 트랜잭션에 편입됨. 요청당 한 샤드만 허용.
- **핸들러에서 `SaveChanges` 금지** — 미들웨어가 처리. 단, DB 생성 Id가 필요한 경우 `SaveChangesAsync()`를 호출해 SQL을 flush할 수 있음 (커밋은 미들웨어가 담당).

### GameDb 샤딩

- 샤드 선택: `GameDbShardManager.GetShard(user)` → `user.Id % shardCount`
- 모든 샤드는 동일한 `GameDbContext` 스키마를 공유 → 마이그레이션을 각 샤드에 개별 적용해야 함.

### Shared 라이브러리

`shared/`는 서버와 클라이언트가 함께 참조하는 **통신 계약 레이어**다. `dakg.shared` 네임스페이스에 Request/Response/DTO/Enum만 담는다. DB Entity, 비즈니스 로직, 서버 전용 코드는 넣지 않는다. 자세한 규칙은 [shared/README.md](shared/README.md) 참고.

---

## 설정 파일

### `config/dev/database.json` (필수, 커밋 안 됨)

```json
{
  "GameDbShardingCount": 2,
  "UserDb":  { "Server": "", "Port": 3306, "Database": "", "User": "", "Password": "" },
  "GameDb1": { "Server": "", "Port": 3306, "Database": "", "User": "", "Password": "" },
  "GameDb2": { "Server": "", "Port": 3306, "Database": "", "User": "", "Password": "" }
}
```
