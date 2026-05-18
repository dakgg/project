# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All commands are run from `server/server/` unless noted.

```bash
# Run API server (Swagger: http://localhost:5031/swagger)
dotnet run

# Run admin panel (Blazor Server)
cd server/admin && dotnet run

# Sync game tables from Google Sheets → shared/table/*.csv
cd sync && dotnet run

# EF migrations
dotnet ef migrations add <Name> --context UserDbContext --output-dir Migrations/UserDb
dotnet ef migrations add <Name> --context GameDbContext --output-dir Migrations/GameDb

# Apply migrations (GameDb must be applied per-shard)
dotnet ef database update --context UserDbContext
dotnet ef database update --context GameDbContext              # GameDb1 (default)
dotnet ef database update --context GameDbContext -- GameDb2  # GameDb2

# Drop DB
dotnet ef database drop --context UserDbContext --force
dotnet ef database drop --context GameDbContext --force
dotnet ef database drop --context GameDbContext --force -- GameDb2
```

## Config Files

`config/dev/database.json` — DB credentials (not committed). Required keys: `GameDbShardingCount`, `UserDb`, `GameDb1` … `GameDb{N}`.

`config/dev/config.json` — Optional. Redis: `{ "Redis": { "ConnectionString": "..." } }`. Missing file or empty `{}` disables Redis.

## Architecture

### Handler Auto-Discovery (`Scripts/HandlerHelper.cs`)

Any class ending in `Handler` is auto-registered as a scoped DI service and its public methods are mapped as routes — no manual route registration needed.

- Route pattern: `POST /{RequestTypeName}` (e.g., `POST /LoginRequest`)
- To add an endpoint: define `*Request`/`*Response` types in `shared/src/`, add a public method to a `*Handler` class taking the request as its first parameter.
- Request types must extend `RequestBase`; response types extend `ResponseBase` (both in `dakg.shared`).

### Transaction Middleware (`Scripts/Middleware/TransactionMiddleware.cs`)

Wraps every request in a transaction across UserDb and, lazily, one GameDb shard.

- **UserDb**: transaction always starts automatically; middleware commits/rolls back.
- **GameDb**: handler must call `GameShardTransactionContext.SetShardAsync(shard)` to enlist the shard. Only one shard per request is allowed.
- **`SaveChanges` is forbidden in handlers** — the middleware handles it. Exception: call `SaveChangesAsync()` mid-handler only when you need the DB-generated `Id` immediately (EF flushes SQL within the open transaction; commit stays with middleware).

### GameDb Sharding (`Scripts/Database/`)

- `GameDbShardManager.GetShard(user)` selects a shard via `user.Id % shardCount`.
- All shards share the same `GameDbContext` schema — migrations must be applied to each shard individually.
- `DesignTimeDbContextFactory` targets `GameDb1` by default; pass `-- GameDb{N}` args to `dotnet ef` for other shards.

### Shared Models (`shared/src/`)

All request/response types live in the `dakg.shared` namespace. Both `server` and `client` reference `shared.csproj`. Enums (`MessageId`, `ResponseResult`) are in `shared/src/enum/`.

### Game Tables (`shared/src/table/`)

`TableManager` reads CSV files from `shared/table/`. The `sync/` tool regenerates these CSVs from Google Sheets. Columns prefixed with `#` and rows whose first cell starts with `#` are excluded from CSV output.
