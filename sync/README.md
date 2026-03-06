# sync

Google Sheets에서 게임 데이터 테이블을 가져와 CSV 파일로 변환하는 도구입니다.

## 역할

기획자가 Google Sheets에서 편집한 게임 데이터(캐릭터, 아이템 등)를 서버/클라이언트가 읽을 수 있는 CSV 파일로 변환합니다.

```
Google Sheets
    └─ character 스프레드시트
    │      └─ base 시트 → shared/table/character/character_base.csv
    └─ item 스프레드시트
           └─ base 시트   → shared/table/item/item_base.csv
```

## 프로젝트 구조

```
sync/
├── Program.cs          # 메인 로직
├── sheet.json          # 스프레드시트 이름 → ID 매핑
├── credentials.json    # Google OAuth2 클라이언트 인증 정보 (gitignore)
└── token.json/         # OAuth2 토큰 캐시 (자동 생성)
```

## 설정

### 1. Google Cloud 설정

1. [Google Cloud Console](https://console.cloud.google.com/)에서 프로젝트 생성
2. **Google Sheets API** 활성화
3. OAuth 2.0 클라이언트 ID 생성 (데스크톱 앱)
4. `credentials.json` 다운로드 후 sync 프로젝트 루트에 배치

### 2. sheet.json 설정

가져올 스프레드시트를 `이름 → 스프레드시트 ID` 형식으로 등록합니다.

```json
{
    "character": "1pjbriAZTNFIvaOt1JR3FVEuCW_inig5jf1nt1-zAByE",
    "item":      "1v2iTOtRwFY9SNFJuxgGyz7LX6bKhxsOC3Nx_yOm283c"
}
```

스프레드시트 ID는 Google Sheets URL에서 확인할 수 있습니다.
`https://docs.google.com/spreadsheets/d/{spreadsheetId}/edit`

## 실행

```bash
cd sync
dotnet run
```

첫 실행 시 브라우저가 열리며 Google 계정 인증을 요청합니다. 인증 후 토큰이 `token.json/`에 캐시되어 이후 실행에서는 자동 인증됩니다.

> **주의**: 실행 시 `shared/table` 디렉토리를 **전체 삭제** 후 재생성합니다. 수동으로 추가한 파일이 있다면 삭제됩니다.

## 출력

`sheet.json`에 등록된 각 스프레드시트의 모든 시트 탭이 CSV로 저장됩니다.

```
shared/table/{이름}/{이름_시트탭명}.csv
```

예시:
```
shared/table/character/character_base.csv
shared/table/item/item_base.csv
```

## 시트 작성 규칙

시트 데이터를 필터링하는 규칙이 있습니다.

**컬럼 필터**: 첫 번째 행(헤더)에서 `#`으로 시작하거나 빈 컬럼은 CSV에 포함되지 않습니다.

| id | name | #비고 |
|----|------|-------|
| 1  | tiger | 내부용 |

→ `id`, `name` 컬럼만 출력됩니다.

**행 필터**: 첫 번째 셀이 `#`으로 시작하거나 비어 있는 행은 건너뜁니다.

| id | name |
|----|------|
| 1  | tiger |
| #  | (비활성) |
| 2  | lion |

→ `#` 행은 출력되지 않습니다.

이 규칙을 활용해 시트 내에 주석 컬럼/비활성 데이터 행을 자유롭게 추가할 수 있습니다.

## 의존성

- [Google.Apis.Sheets.v4](https://www.nuget.org/packages/Google.Apis.Sheets.v4/) 1.73.0
- .NET 9
