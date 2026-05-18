# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# 서버 실행 (Swagger: http://localhost:5031/swagger)
dotnet run

# 마이그레이션 생성
dotnet ef migrations add <Name> --context UserDbContext --output-dir Migrations/UserDb
dotnet ef migrations add <Name> --context GameDbContext --output-dir Migrations/GameDb

# 마이그레이션 적용 (GameDb는 샤드별로 실행)
dotnet ef database update --context UserDbContext
dotnet ef database update --context GameDbContext              # GameDb1 (기본값)
dotnet ef database update --context GameDbContext -- GameDb2  # GameDb2

# 마이그레이션 되돌리기
dotnet ef migrations remove --context GameDbContext

# DB 드랍
dotnet ef database drop --context UserDbContext --force
dotnet ef database drop --context GameDbContext --force
dotnet ef database drop --context GameDbContext --force -- GameDb2
```

## 설정 파일

`../../config/dev/database.json` — DB 접속 정보 (커밋 안 됨). `GameDbShardingCount`, `UserDb`, `GameDb1` ~ `GameDb{N}` 키 필요.

`../../config/dev/config.json` — Redis 등 선택적 설정. 파일이 없거나 빈 JSON(`{}`)이면 Redis 없이 실행됨.

## 아키텍처

### Handler 자동 등록

`HandlerHelper`가 리플렉션으로 `*Handler` 클래스를 스캔해 DI 등록 및 라우트 매핑을 자동으로 처리한다.

- 라우트: `POST /{RequestTypeName}` (예: `POST /LoginRequest`)
- 새 엔드포인트 추가 방법: `shared/`에 `*Request`/`*Response` 타입 정의 → `*Handler` 클래스에 해당 Request를 첫 번째 파라미터로 받는 public 메서드 작성. 라우트 등록 코드 불필요.

### 트랜잭션 미들웨어

`TransactionMiddleware`가 모든 요청을 트랜잭션으로 감싼다.

- **UserDb**: 모든 요청에서 자동으로 트랜잭션 시작 → 미들웨어가 커밋/롤백.
- **GameDb**: 핸들러가 `GameShardTransactionContext.SetShardAsync(shard)`를 명시적으로 호출해야 해당 샤드가 트랜잭션에 편입됨 (lazy enlist). 요청당 한 샤드만 허용.
- **핸들러에서 `SaveChanges` 금지** — 미들웨어가 처리. 단, DB 생성 Id가 필요한 경우 예외적으로 `SaveChangesAsync()`를 호출해 SQL을 flush할 수 있음 (커밋은 미들웨어가 담당).

### GameDb 샤딩

- 샤드 선택: `GameDbShardManager.GetShard(user)` → `user.Id % shardCount`
- `DesignTimeDbContextFactory`는 기본값으로 `GameDb1`을 타겟. `dotnet ef` 명령에서 다른 샤드를 지정하려면 `-- GameDb2` 형태로 args 전달.
- 모든 샤드는 동일한 스키마(`GameDbContext`)를 공유하므로 마이그레이션을 각 샤드에 개별 적용해야 함.
