# shared

서버와 클라이언트 양쪽이 참조하는 **통신 계약(contract) 레이어**다.  
네트워크 경계를 가로질러 주고받는 타입만 정의한다.

> 판단 기준: **클라이언트가 이 타입을 직접 사용하는가?**  
> 아니라면 server 프로젝트에 둔다.

---

## 넣는 것 / 넣지 않는 것

**넣는 것**
- `*Request` / `*Response` 클래스
- Request·Response 안에 담기는 DTO (`User`, `GachaItem`, `InventoryItem` …)
- `MessageId`, `ResponseResult` enum
- `TableManager` (게임 테이블 CSV 읽기 — 서버·클라이언트 모두 사용)

**넣지 않는 것**
- DB Entity (`UserEntity`, `GameEntity` …)
- 비즈니스 로직
- DB Context / Repository / Middleware
- 서버 전용 설정이나 인프라 코드

---

## 파일 구조 및 네이밍

기능 단위로 파일 하나에 Request + Response + 관련 DTO를 묶는다.

```
src/
├── Shared.Base.cs          # RequestBase, ResponseBase
├── Shared.Auth.cs          # LoginRequest, LoginResponse, User
├── Shared.Game.cs          # GachaRequest, GachaResponse, GachaItem
├── Shared.Battle.cs        # BattleRequest, BattleResponse
├── Shared.Inventory.cs     # GetInventoryRequest, GetInventoryResponse, InventoryItem
└── enum/
    ├── Shared.MessageId.cs
    └── Shared.ResponseResult.cs
```

파일명 규칙: `Shared.{Feature}.cs`

---

## Request / Response 작성 규칙

### MessageId 블록 할당

기능별로 1000 단위 블록을 배정한다. Request는 홀수, Response는 짝수.

```
Auth      1001 / 1002
(예비)    2001 / 2002
Gacha     3001 / 3002
Battle    4001 / 4002
Inventory 5001 / 5002
```

새 기능은 다음 빈 블록을 사용하고 `Shared.MessageId.cs`에 함께 등록한다.

### 클래스 패턴

```csharp
namespace dakg.shared
{
    // Request: 대응하는 MessageId로 base 호출
    public class FooRequest : RequestBase
    {
        public FooRequest() : base((int)MessageId.FOO_REQUEST) { }

        public long Uid { get; set; }
    }

    // Response: ResponseResult.SUCCESS로 base 호출 (기본값)
    public class FooResponse : ResponseBase
    {
        public FooResponse() : base((int)ResponseResult.SUCCESS) { }

        public string Data { get; set; } = string.Empty;
    }

    // DTO: 같은 파일 안에, 프로퍼티만 정의
    public class FooItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
```

- 모든 타입은 `dakg.shared` 네임스페이스
- `ResponseBase.Result` 기본값이 `SUCCESS`이므로, 오류 시 핸들러에서 `Result = ResponseResult.Error`로 덮어쓴다
- DTO는 프로퍼티만 가지며 로직을 포함하지 않는다

### Request 클래스명 주의

`*Request` 타입명이 그대로 서버 라우트가 된다.

```
GachaRequest        → POST /GachaRequest
GetInventoryRequest → POST /GetInventoryRequest
```

클래스명은 의도를 명확히 드러내야 하며, 배포 후에는 변경이 어렵다.
