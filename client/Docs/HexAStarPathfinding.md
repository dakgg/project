# 2D 헥사곤 A* 길찾기 (Hex A* Pathfinding)

BattleScene에서 동작하는 **2D pointy-top 헥사곤 그리드 + A* 길찾기** 기능 문서.

- 위치: `Assets/Scripts/Battle/Hex/`
- 네임스페이스: `Battle.Hex`
- 어셈블리: `Assembly-CSharp` (별도 asmdef 없음)
- 좌표/레이아웃 수학은 [Red Blob Games – Hexagonal Grids](https://www.redblobgames.com/grids/hexagons/) 규약을 따른다.

---

## 1. 개요

| 항목 | 값 |
|------|-----|
| 헥스 방향 | **Pointy-top** (꼭짓점이 위/아래, 가로 행이 어긋남) |
| 좌표 저장 | **axial (q, r)** |
| 거리/라운딩 | **cube (q, r, s = -q-r)** |
| 렌더링 | **런타임 절차 생성 메시** (fill + 외곽선), 스프라이트/타일 에셋 불필요 |
| 데모 | 클릭 길찾기 + 유닛 이동 + 장애물 토글 |

빌트인 렌더 파이프라인 + 3D 프로젝트지만, 카메라를 orthographic으로 두고 XY 평면에 헥스를 그려 2D처럼 동작시킨다.

---

## 2. 파일 구성

| 파일 | 종류 | 책임 |
|------|------|------|
| `Hex.cs` | 순수 C# struct | axial/cube 좌표, 이웃/거리, world↔hex 변환, 꼭짓점 계산 |
| `HexGrid.cs` | 순수 C# 클래스 | 헥사곤 형태 맵, 장애물(blocked) 집합, 걷기가능 이웃 |
| `HexPathfinder.cs` | 정적 클래스 | A* 알고리즘 + 이진 최소힙(직접 구현) |
| `HexGridRenderer.cs` | MonoBehaviour | 절차 메시 생성/렌더, 셀 색상 in-place 갱신 |
| `BattlePathfindingDemo.cs` | MonoBehaviour | 씬 부트스트랩, 입력, 유닛 이동 오케스트레이션 |

의존 방향: `BattlePathfindingDemo → HexGridRenderer → (HexGrid, Hex)`, `HexPathfinder → (HexGrid, Hex)`. 코어 로직(`Hex`/`HexGrid`/`HexPathfinder`)은 Unity 씬에 의존하지 않아 단위 테스트가 쉽다.

---

## 3. 좌표계 / 레이아웃 수학 (Pointy-top)

`size` = 헥스 중심에서 꼭짓점까지 거리.

**hex → world (XY 평면)**
```
x = size * (√3 * q + √3/2 * r)
y = size * (3/2 * r)
```

**world → hex** (이후 cube_round로 정수 셀로 반올림)
```
q = (√3/3 * x − 1/3 * y) / size
r = (2/3 * y) / size
```

**꼭짓점 각도**: `60° * i − 30°` (i = 0..5)

**헥스 거리(A* 휴리스틱)**
```
distance = (|dq| + |dq + dr| + |dr|) / 2
```

**6방향 이웃 (axial)**: `(+1,0) (+1,−1) (0,−1) (−1,0) (−1,+1) (0,+1)`

---

## 4. A* 알고리즘 (`HexPathfinder`)

```csharp
List<Hex> path = HexPathfinder.FindPath(grid, start, goal);
```

- **이동 비용 균일(1)** → 반환 경로는 헥스 수가 최소인 최단 경로.
- **휴리스틱 = 헥스 거리** → admissible & consistent 하므로 최적성 보장.
- 경로가 없거나 start/goal이 걷기 불가면 **빈 리스트** 반환.
- 우선순위 큐는 **직접 구현한 이진 최소힙** 사용. (Unity 6000.1은 .NET Standard 2.1 타깃이라 `System.Collections.Generic.PriorityQueue` 미제공.)

검증된 동작 (execute_code 기준):
- 경로 연속성: 인접한 셀만 연결 (각 스텝 거리 = 1)
- 장애물 회피: 경로에 blocked 셀 미포함
- 도달 불가: 빈 경로 반환

---

## 5. 렌더링 (`HexGridRenderer`)

- `MeshFilter`/`MeshRenderer`를 자동 요구(`RequireComponent`).
- 머티리얼: `Shader.Find("Sprites/Default")` — 빌트인이며 정점 색상(vertex color)을 곱한다.
- **fill 메시**: 셀당 7정점(중심 + 6꼭짓점)으로 삼각형 팬 6개. 셀 → 정점 인덱스 매핑을 보관해 `mesh.colors`만 교체(메시 재생성 없이 색 갱신).
- **외곽선 메시**: 자식 `Border` 오브젝트에 `MeshTopology.Lines` 메시(어두운 테두리), z를 살짝 앞에 둠.

주요 API:
```csharp
void Build(HexGrid grid, float size, Color def, Color blocked, Color border);
void SetCellColor(Hex h, Color color);   // 단일 셀 즉시 갱신
void ResetColors(HexGrid grid);          // 전체 기본/장애물 색으로 복귀
```

---

## 6. 데모 (`BattlePathfindingDemo`)

### Inspector 파라미터
| 필드 | 기본값 | 설명 |
|------|--------|------|
| `mapRadius` | 5 | 헥사곤 맵 반지름(링 수). radius N → 셀 수 = `3N(N+1)+1` |
| `hexSize` | 0.5 | 헥스 크기(중심→꼭짓점) |
| `obstacleRatio` | 0.15 | 무작위 장애물 비율 |
| `randomSeed` | 12345 | 장애물 생성 시드(재현성) |
| `moveSpeed` | 4 | 유닛 이동 속도(초당 헥스) |
| 색상들 | - | 기본/장애물/외곽선/경로/시작/도착/유닛 색 |

### 조작
- **좌클릭**: 도착 셀 지정 → A* 경로 계산 → 하이라이트 → 유닛이 경로를 따라 이동 (이동 중 클릭 무시)
- **우클릭**: 셀 장애물 토글 (유닛이 선 셀은 막지 않음)

### 입력 시스템
프로젝트가 신규 **Input System(`com.unity.inputsystem`)** 를 사용하므로 구 `UnityEngine.Input` 대신 `UnityEngine.InputSystem.Mouse.current` API를 쓴다.
- 클릭: `Mouse.current.leftButton.wasPressedThisFrame`
- 좌표: `Mouse.current.position.ReadValue()`

---

## 7. 씬 구성

BattleScene에 빈 GameObject **`BattleDirector`** 를 두고 `BattlePathfindingDemo`를 부착하면 끝. 나머지(그리드 메시, 외곽선, 유닛 토큰, 카메라 orthographic 세팅)는 모두 런타임에 생성된다.

```
BattleScene
├── Main Camera        (런타임에 orthographic으로 전환, 그리드 중심 정렬)
├── Directional Light
└── BattleDirector     [BattlePathfindingDemo, HexGridRenderer, MeshFilter, MeshRenderer]
    ├── Border         (외곽선 라인 메시, 런타임 생성)
    └── Unit           (Quad 토큰, 런타임 생성)
```

---

## 8. 확장 포인트

- **지형별 이동 비용(가중치)**: `HexPathfinder`의 균일 비용 1을 셀별 비용으로 교체하고, 휴리스틱과 함께 일관성 유지.
- **셀 점유/유닛 충돌**: `HexGrid.IsWalkable`에 동적 점유 상태 반영.
- **경로 비용/턴 표시**: 경로 셀에 g-score 라벨 렌더.
- **맵 형태 변경**: `HexGrid` 생성자를 사각형/임의 형태로 확장.

---

## 9. 빠른 시작

1. Unity 에디터에서 `Assets/Game/Scene/BattleScene.unity` 열기
2. ▶ **Play**
3. 좌클릭으로 목적지 지정 → 빨간 유닛이 경로 따라 이동, 우클릭으로 장애물 토글
