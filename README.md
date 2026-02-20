# Server

## 프로젝트 구조

```
project/
├── client/
├── config/
│   └── dev/
│       └── database.json   # DB 접속 정보
├── server/
│   └── server/
│       └── Scripts/
│           ├── Database/
│           │   ├── UserDbContext.cs
│           │   ├── GameDbContext.cs
│           │   ├── GameDbShardManager.cs
│           │   └── Entity/
│           ├── Handler/
│           └── Middleware/
└── shared/                 # 공유 Request/Response 모델
```

