# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Unity **6000.1.17f1** game client (built-in render pipeline, new Input System).
Part of the parent `project` git repo — the game server backend is documented in
[../CLAUDE.md](../CLAUDE.md).

There is no CLI build or test setup. Code compiles by opening this directory in
the Unity Editor. The `*.csproj` / `client.slnx` files are Unity-generated —
never hand-edit them (some are stale, e.g. URP csprojs for packages no longer
installed). `.meta` files must be committed alongside any new asset/script.

## Repository layout quirks

- **`Packages/ui` and `Packages/addressable-packer` are separate git repos**
  embedded in this project (not submodules of `project`). Commits there are
  independent. `Packages/ui` has its own CLAUDE.md — read it before touching
  the UI framework.
- `ProjectSettings/EditorBuildSettings.asset` points to a nonexistent
  `TitleScene` — ignore it. Scenes are loaded via **Addressables**
  (`LoadSceneAsync("LobbyScene@Scene")`), not build-settings indices.
- All scripts under `Assets/Scripts/` compile into `Assembly-CSharp` (no
  asmdef). Only the `Battle.Hex` module uses a namespace; the rest are global.

## Architecture

### Startup and UI flow (ViewSystem)

UI is driven by the embedded **`com.dakgg.ui`** package (namespace
`ViewSystem` — see `Packages/ui/CLAUDE.md` for the framework internals).
App-side wiring:

1. `Assets/Scripts/Startup.cs` — `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]`
   creates the `_ViewManager` singleton before any scene loads.
2. `InitialScene` waits for `Addressables.InitializeAsync()`, then opens the
   root page: `ViewRequest.Open("InitialSceneRootPage@View", null, true)`.
3. Each scene has a `*RootPage : PageView` class (`Assets/Scripts/UI/`) with a
   matching prefab in `Assets/Game/UI/Prefab/View/`. The `[ViewLoad("Name@View")]`
   attribute binds class → Addressables key.
4. Scene transitions go through Addressables scene keys (`*@Scene`).

Addressables naming convention: `<AssetName>@View` for view prefabs,
`<SceneName>@Scene` for scenes. Groups are auto-managed per-directory by the
`addressable-packer` editor package (`DirectoryBaseGroupSchema`) — don't
hand-assign assets to groups.

`Assets/Game/Resources/ViewRootCanvas.prefab` is loaded by name via
`Resources.Load` inside the UI package; don't rename or move it.

### Battle: hex A* pathfinding (`Assets/Scripts/Battle/Hex/`)

Self-contained module, namespace `Battle.Hex`, documented in
[Docs/HexAStarPathfinding.md](Docs/HexAStarPathfinding.md). Pointy-top hexes,
axial coords, Red Blob Games conventions. `BattleScene` contains a
`BattleDirector` object whose `BattlePathfindingDemo` builds everything else
(grid mesh, unit, camera setup) at runtime — the demo overrides Main Camera
settings (orthographic, size, position) in `Start()`.

Core logic (`Hex`, `HexGrid`, `HexPathfinder`) is pure C# with no Unity scene
dependency; only the renderer/demo are MonoBehaviours.

## Gotchas

- **Input**: the project is new-Input-System-only. `UnityEngine.Input` throws
  `InvalidOperationException` at runtime — use
  `UnityEngine.InputSystem.Mouse.current` etc.
- **Unity fake-null**: never use `??` / `??=` on `UnityEngine.Object`
  (e.g. `GetComponent<T>() ?? AddComponent<T>()` fails on destroyed-but-not-null
  objects). Use explicit `if (x == null)` checks.
- Unity 6000.1 targets .NET Standard 2.1 — no
  `System.Collections.Generic.PriorityQueue` (hence the hand-rolled heap in
  `HexPathfinder`).

## Editor automation (Unity MCP)

The **MCP For Unity** package (`com.coplaydev.unity-mcp`, HTTP port 8080) is
installed. When the MCP server is connected, use it to verify work instead of
guessing: `read_console` for compile errors after edits, `manage_scene` /
`manage_gameobject` for scene setup, `manage_editor` for play mode,
`execute_code` for in-editor logic checks (code is wrapped in a method — it
must `return` a value).
