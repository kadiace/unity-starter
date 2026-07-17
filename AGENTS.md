# AGENTS.md

## Managers pattern

### Core idea

- `Managers` is the central access point for globally available systems.
- Access systems through `Managers.*`.
- Treat `Managers.*` as always-available infrastructure; do not add defensive null checks around manager access.
- Keep individual managers as plain C# classes by default.
- Avoid making every system a MonoBehaviour. Prefer one bootstrap MonoBehaviour: `Managers`.

Current minimal structure:

```text
Assets/Scripts/
  Bootstrap/
    AppBootstrap.cs
  Managers/
    Managers.cs
    Core/
      DataManager.cs
      InputManager.cs
      PoolManager.cs
      Poolable.cs
      ResourceManager.cs
      SceneManagerEx.cs
      SoundManager.cs
      UIManager.cs
    Contents/
      GameStateManager.cs
      SpawnManager.cs
  Controllers/
    PlayerController.cs
  Scenes/
    GameSceneContext.cs
  UIs/
    Scenes/
      UI_ExampleScene.cs
    Menus/
      UI_ExampleMenu.cs
    WorldSpace/
      UI_ExampleWorldSpace.cs
    UI_Base.cs
    UI_Scene.cs
    UI_Menu.cs
    UI_EventHandler.cs
  Utils/
    Define.cs
```

### Bootstrap flow

`AppBootstrap` creates the global app root before the first scene loads:

```text
AppBootstrap
-> Managers.EnsureExists()
-> @App GameObject
-> Managers component
-> DontDestroyOnLoad(@App)
```

Do not place `@App` manually in every scene. `@App` is persistent and survives scene transitions.

Scene-specific setup belongs in a scene context such as `GameSceneContext`, not in `Managers`.

## Current manager usage

### `Managers.Resource`

Use this for loading and instantiating assets from the Unity `Resources` folder.

Example:

```csharp
GameObject player = Managers.Resource.Instantiate("Prefabs/Player");
TextAsset table = Managers.Resource.Load<TextAsset>("Datas/StageData");
Managers.Resource.Destroy(player);
```

Rules:

- Paths are relative to an `Assets/**/Resources/` folder and omit file extensions.
- `Instantiate` automatically uses PoolManager when the prefab has a `Poolable` component.
- `Destroy` automatically returns Poolable objects to PoolManager.
- Do not create fallback objects when a resource path is wrong.
- Missing resources should fail visibly so the authoring error is fixed.

### `Managers.Pool`

Use this as the internal reusable-object backend for prefabs marked with `Poolable`.

Normal gameplay code should usually call ResourceManager:

Example:

```csharp
GameObject bullet = Managers.Resource.Instantiate("Prefabs/Bullet");
Managers.Resource.Destroy(bullet);
```

If `Prefabs/Bullet` has a `Poolable` component, ResourceManager routes it through PoolManager automatically.

Direct PoolManager use is allowed when manually controlling pool warm-up or advanced spawn behavior:

Example:

```csharp
GameObject bullet = Managers.Pool.Pop("Prefabs/Bullet", preloadCount: 20);
Managers.Pool.Push(bullet);
```

Rules:

- Add `Poolable` to prefab assets that should be pooled.
- Do not add `Poolable` at runtime as a fallback.
- Pool keys are prefab paths under `Resources`, without file extensions.
- Use ResourceManager for ordinary create/destroy calls; use PoolManager directly only when pool-specific control is needed.
- If an object has complex reset behavior, reset it in its own component before or after pop/push, not inside PoolManager.

### `Managers.Data`

Use this for JSON save/load data that must survive game restart or application exit.

Client code should only know `Load` and `Save`:

Example:

```csharp
[System.Serializable]
public class SaveData
{
    public string playerName = "Player";
    public int bestScore;
}

SaveData data = Managers.Data.Load<SaveData>();
data.bestScore = 100;
Managers.Data.Save(data);
```

Rules:

- `DataManager` owns file paths, file existence checks, and first-save creation.
- Gameplay code should not call `File.Exists`, `Application.persistentDataPath`, or `JsonUtility` directly for ordinary save data.
- Save data classes must be `[System.Serializable]` and have a public parameterless constructor path, usually by using public fields with defaults.
- Keep public `DataManager` API small. Add methods beyond `Load`/`Save` only when explicitly needed.

### `Managers.Input`

Use this for gameplay input. Controllers should consume semantic helpers instead of reading raw keyboard/mouse state directly.

```csharp
Vector2 move = Managers.Input.ReadMove();

if (Managers.Input.WasAttackPressedThisFrame())
    Attack();
```

Rules:

- Gameplay helpers must return neutral values when `Mode != Define.InputMode.Player`.
- UI/menu code changes input state through `Managers.Input.SetMode(...)`.
- Add new gameplay input as a semantic helper, e.g. `WasDashPressedThisFrame`, not `WasVPressedThisFrame` unless the key itself is the domain concept.

### `Managers.Scene`

Use this for scene transitions and scene reloads.

```csharp
Managers.Scene.Load(Define.Scene.Game);
Managers.Scene.Load(Define.Scene.Menu);
Managers.Scene.ReloadCurrentScene();

if (Managers.Scene.IsCurrentScene(Define.Scene.Game))
{
}
```

Rules:

- Use `Managers.Scene.Load(Define.Scene.SceneName)` for scene transitions.
- Do not expose string-based scene loading to gameplay code.
- `Define.Scene` values must match `.unity` scene asset names and Build Settings entries.
- Add a new `Define.Scene` enum value whenever a new scene is added.
- Avoid direct `SceneManager.LoadScene(...)` calls outside `SceneManagerEx`.

### `Managers.Sound`

Use this for BGM and one-shot sound effects loaded from `Resources/Sounds`.

Example:

```csharp
Managers.Sound.PlayBgm(Define.Sound.MainTheme);
Managers.Sound.PlaySfx(Define.Sound.Jump);
Managers.Sound.SetMasterVolume(0.7f);
Managers.Sound.SetBgmVolume(0.5f);
Managers.Sound.SetSfxVolume(0.8f);
Managers.Sound.StopBgm();
```

Expected resource paths:

```text
Assets/Resources/Sounds/MainTheme.wav
Assets/Resources/Sounds/Jump.wav
```

Rules:

- Client code should use semantic methods: `PlayBgm`, `PlaySfx`, `StopBgm`, `StopAll`, `SetMasterVolume`, `SetBgmVolume`, `SetSfxVolume`.
- Play calls take `Define.Sound`, not string paths.
- Add a new `Define.Sound` enum value whenever a new sound clip is added.
- `Define.Sound.SomeClip` maps to `Assets/Resources/Sounds/SomeClip.*`.
- Menu sliders should call `SetMasterVolume`, `SetBgmVolume`, and `SetSfxVolume` directly.
- `SoundManager` owns the persistent `@Sound` root and internal BGM/Sfx `AudioSource` channels.
- Do not place sound playback logic directly on random gameplay objects unless it is truly positional/3D audio.
- Missing clips throw instead of falling back to silence or placeholder audio.
- Add positional/world audio as a deliberate extension when needed; keep the base API simple.

### `Managers.GameState`

Reserved content manager for game-specific global state.

Current file is a stub only. Fill it after the exam/game concept is known.

Use it for:

- score
- timer
- player health
- win/lose state
- checkpoint/progress state
- etc.

Do not turn it into a generic framework before the game rules are known.

### `Managers.Spawn`

Reserved content manager for game-specific spawn rules.

Current file is a stub only. Fill it only when spawning policy becomes game-specific.

Use it for:

- timed enemy waves
- random spawn point selection
- max active enemy/item counts
- difficulty-based spawn rates

Do not use it for simple one-off prefab creation; use `Managers.Resource` or `Managers.Pool` directly.

### `Managers.UI`

Use this for two distinct UI surfaces:

- Screen UI: scene HUDs and menus rendered on `@UI_Root` as `ScreenSpaceOverlay`.
- WorldSpace UI: labels, health bars, prompts, and markers placed under world objects as `WorldSpace` canvases.

Example:

```csharp
UI_Game ui = Managers.UI.ShowSceneUI<UI_Game>();
UI_PauseMenu menu = Managers.UI.ShowMenuUI<UI_PauseMenu>();
UI_Nameplate nameplate = Managers.UI.CreateWorldSpaceUI<UI_Nameplate>(targetTransform);
Managers.UI.HideSceneUI();
Managers.UI.Clear();
```

Expected prefab paths:

```text
Assets/Resources/Prefabs/UIs/Scenes/UI_Game.prefab
Assets/Resources/Prefabs/UIs/Menus/UI_PauseMenu.prefab
Assets/Resources/Prefabs/UIs/WorldSpace/UI_Nameplate.prefab
```

Included examples:

```csharp
Managers.UI.ShowSceneUI<UI_ExampleScene>();
Managers.UI.ShowMenuUI<UI_ExampleMenu>();
Managers.UI.CreateWorldSpaceUI<UI_ExampleWorldSpace>(targetTransform);
```

```text
Assets/Resources/Prefabs/UIs/Scenes/UI_ExampleScene.prefab
Assets/Resources/Prefabs/UIs/Menus/UI_ExampleMenu.prefab
Assets/Resources/Prefabs/UIs/WorldSpace/UI_ExampleWorldSpace.prefab
```

Rules:

- `@UI_Root` and `@EventSystem` are global infrastructure and may be created by `UIManager`.
- This project uses the New Input System; generated `@EventSystem` must use `InputSystemUIInputModule`, not `StandaloneInputModule`.
- Screen UI prefabs are parented under `@UI_Root` and their Canvas is set to `ScreenSpaceOverlay`.
- WorldSpace UI prefabs are parented under the provided world transform and their Canvas is set to `WorldSpace`.
- UI prefabs must already contain required `Canvas`, `GraphicRaycaster` when needed, UI script, and required child components.
- Do not auto-add missing UI scripts/components in `UIManager`.
- Use `UI_Base` enum binding for buttons/text/images when it speeds up UI wiring.
- Blocking menu UI should switch input mode to `Define.InputMode.UI`; HUD-only UI should not.

## Manager lifecycle rules

When adding a new manager:

1. Create it under `Assets/Scripts/Managers/Core/` if it is reusable infrastructure.
2. Create it under `Assets/Scripts/Managers/Contents/` if it is specific to this game prototype.
3. Register it in `Managers.cs` with:
   - private instance field
   - public static accessor
   - `Init()` call if needed
   - `OnUpdate()` call only if needed
   - `Clear()` call only if state must reset

Preferred manager shape:

```csharp
public class GameStateManager
{
    public void Init()
    {
        Clear();
    }

    public void OnUpdate()
    {
    }

    public void Clear()
    {
    }
}
```

Only add lifecycle methods that are actually used.

## Input rules

Input is centralized through `Managers.Input`.

### Responsibilities

`InputManager` owns:

- current input mode
- key/action reads
- semantic helper methods such as `ReadMove`, `WasJumpPressedThisFrame`, `WasAttackPressedThisFrame`
- returning neutral values when input mode is not `Player`

Controllers own:

- movement behavior
- attack behavior
- interaction behavior
- animation/physics effects

Do not spread raw input reads across gameplay scripts. If a gameplay script needs input, add a semantic helper to `InputManager` first, then consume it from the controller.

Good:

```csharp
if (Managers.Input.WasAttackPressedThisFrame())
    Attack();
```

Avoid:

```csharp
if (Keyboard.current.xKey.wasPressedThisFrame)
    Attack();
```

Scene-local debug keys or one-off editor-only shortcuts may read raw input directly, but gameplay actions should go through `Managers.Input`.

## Scene and UI direction

This project may start as a single-scene prototype. Do not add scene/UI framework code until the game loop needs it.

When scene flow becomes necessary:

- Add `Define.Scene` values.
- Add a small `SceneManagerEx` under `Managers/Core`.
- Load scenes through `Managers.Scene`, not scattered direct `SceneManager.LoadScene` calls.
- Clear manager state intentionally before scene changes or restarts.

When UI becomes necessary:

- Prefer one scene UI for HUD, game over, and restart controls.
- Coordinate blocking menus with `Managers.Input.SetMode(Define.InputMode.UI)`.
- HUD-only UI should not switch input mode.

## Prototype priorities

For exam-style work, implement in this order:

1. Player control
2. Camera visibility
3. Objective
4. Collision/interaction
5. Score/time/health state
6. Win/fail condition
7. Restart
8. UI feedback
9. Polish only after the loop works

Avoid early over-engineering:

- no generic event bus unless clearly needed
- no large framework extraction
- no pooling unless many spawned objects cause a real issue
- no save/load unless required
- no multi-scene flow unless required

## No unnecessary fallback code

Do not add fallback behavior that hides missing setup or misconfiguration.

- Intentional infrastructure bootstrapping is allowed for stable global roots such as `@App`, `@UI_Root`, and `@EventSystem`.
- Do not create placeholder objects when a required prefab is missing.
- Do not silently add required UI/gameplay components at runtime just to make an incomplete prefab work.
- Do not add alternate code paths for hypothetical legacy formats or unused configurations.
- Prefer fail-fast behavior for incorrectly authored scenes, prefabs, or Resources paths.
- Required static relationships should be wired in the scene or prefab, not patched by gameplay code.

Good:

Example:

```csharp
GameObject prefab = Resources.Load<GameObject>("Prefabs/UIs/Scenes/UI_Game");
GameObject ui = Object.Instantiate(prefab);
```

Avoid:

Example:

```csharp
GameObject prefab = Resources.Load<GameObject>("Prefabs/UIs/Scenes/UI_Game");
GameObject ui = prefab != null ? Object.Instantiate(prefab) : new GameObject("UI_Game");
```

## Verification rules

After changing scripts:

- Run diagnostics on changed files.
- Test in Unity Play Mode when possible.
- Drive the feature through the real surface: attach the component, press the keys, collide with the object, click the button, or restart the scene.
- Keep the project buildable at all times.
