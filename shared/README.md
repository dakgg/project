# shared

서버와 클라이언트 양쪽이 참조하는 **통신 계약(contract) 레이어**다. 네트워크를 경계로 주고받는 타입만 정의한다.

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

### 1. MessageId 블록 할당

기능별로 1000 단위 블록을 배정한다. Request는 홀수, Response는 짝수.

```
Auth      1001 / 1002
(예비)    2001 / 2002
Gacha     3001 / 3002
Battle    4001 / 4002
Inventory 5001 / 5002
```

새 기능 추가 시 다음 빈 블록을 사용하고 `Shared.MessageId.cs`에 등록한다.

### 2. 클래스 패턴

```csharp
// Request: MessageId.XXX_REQUEST로 base 호출
public class FooRequest : RequestBase
{
    public FooRequest() : base((int)MessageId.FOO_REQUEST) { }

    public long Uid { get; set; }
    // ...
}

// Response: ResponseResult.SUCCESS로 base 호출 (기본값)
public class FooResponse : ResponseBase
{
    public FooResponse() : base((int)ResponseResult.SUCCESS) { }

    public string Data { get; set; } = string.Empty;
    // ...
}
```

- 모든 클래스는 `dakg.shared` 네임스페이스
- `ResponseBase.Result`의 기본값이 `SUCCESS`이므로, 오류 시 핸들러에서 `Result = ResponseResult.Error`로 덮어쓴다

### 3. DTO

Request/Response 안에 담기는 복합 객체(예: `User`, `GachaItem`)는 같은 파일 안에 같이 정의한다. 프로퍼티만 갖고 로직은 없어야 한다.

---

## HandlerHelper와의 연결

`*Request` 타입의 이름이 그대로 서버 라우트가 된다.

```
GachaRequest → POST /GachaRequest
GetInventoryRequest → POST /GetInventoryRequest
```

따라서 Request 클래스명은 의도를 명확히 드러내야 하며, 한 번 배포 후에는 변경하기 어렵다.
