# Server

## DB 구성

`config/dev/database.json`에서 설정

- `UserDb` : 유저 DB (1개)
- `GameDb1` ~ `GameDb{N}` : 게임 DB 샤딩
- `GameDbShardingCount` : 게임 DB 샤드 수

## Migration

> DB 서버 실행 후 진행

### Migration 생성

```bash
# User DB
dotnet ef migrations add InitialCreate --context UserDbContext --output-dir Migrations/UserDb

# Game DB (샤드 공통 스키마)
dotnet ef migrations add InitialCreate --context GameDbContext --output-dir Migrations/GameDb
```

### Migration 적용

```bash
# User DB
dotnet ef database update --context UserDbContext

# Game DB - 각 샤드에 동일한 Migration 적용
dotnet ef database update --context GameDbContext --connection "Server=localhost;Port=3306;Database=game_db_1;User=root;Password=your_password;"
dotnet ef database update --context GameDbContext --connection "Server=localhost;Port=3306;Database=game_db_2;User=root;Password=your_password;"
```

## 샤드 라우팅

핸들러에서 `GameDbShardManager`를 주입받아 사용

```csharp
public class GameHandler(GameDbShardManager shards)
{
    public async Task<...> Handle(... request)
    {
        var db = shards.GetShard(request.UserId); // userId % shardCount 로 자동 선택
        var game = await db.Games.FindAsync(...);
    }
}
```
