# Project Info

## Overview
- NSMB-MarioVsLuigi-accurate: a recreation of New Super Mario Bros. DS's "Mario vs Luigi" battle mode, built in Unity (URP).
- Online multiplayer via Photon PUN; a fork on top adds offline local play (one machine, keyboard + controller(s)) and split-screen.
- Uses Unity's new Input System (`Assets/Controls.inputactions` + auto-generated `Assets/Controls.cs`).
- `unity-cmd.ps1` (repo root) is a "Unity Bridge" - lets an agent send JSON commands to a running Unity Editor to inspect/create/edit GameObjects, components, scenes, and prefabs, recompile, and run one-off C# via a scratch pad, without manually clicking through the Editor.

## Most-edited / most important scripts
- `Assets/Scripts/GameManager.cs` - central per-match state: player spawning, win conditions, local-vs-online switching, camera rig activation, background switching.
- `Assets/Scripts/Entity/Player/PlayerController.cs` - the biggest one: movement/physics, per-mode input binding, powerups, beta-mode gated behaviors (kick, jump SFX, animations).
- `Assets/Scripts/Entity/Player/PlayerAnimationController.cs` - animation state machine; has its own separate `betaAnims` flag, independent from PlayerController's.
- `Assets/Scripts/Camera/CameraController.cs` - per-player camera follow logic; `SetTargetCamera()` rebinds a player to a specific rig; signals background wrap events.
- `Assets/Scripts/Camera/HorizontalCamera.cs` - per-camera orthographic size / FOV computation.
- `Assets/Scripts/Camera/SecondaryCameraPositioner.cs` - positions the duplicate camera used for the seamless level-wrap illusion.
- `Assets/Scripts/BackgroundLoop.cs` - background tiling/scrolling/parallax; supports an explicit per-rig `backgroundsRoot` (falls back to tag lookup if unassigned).
- `Assets/Scripts/UI/UIUpdater.cs` - drives the in-game HUD (stars, coins, lives, timer, reserve item); derives some references from child hierarchy at runtime.
- `Assets/Scripts/UI/Menu/MainMenuManager.cs` - main menu logic; `StartLocalGrass`/`StartLocalBricks`/`StartLocalBeta` kick off local play.
- `Assets/Scripts/GlobalController.cs` - persistent (DontDestroyOnLoad) cross-scene singleton; holds settings, NDS-mode canvas, and `startingLocalGame` (signals local mode to the next scene load).
- `Assets/Resources/Prefabs/Static/GameManager.prefab` - shared prefab instanced in every level scene; contains the camera rig(s) and the "New HUD" canvas.
- `Assets/Resources/Prefabs/PlayerMario.prefab` / `PlayerLuigi.prefab` - separate prefabs per character (not one shared "Player" prefab with skin-swapping).

## Important things to remember
- `isLocalGame` is set via `GlobalController.Instance.startingLocalGame`, assigned by MainMenuManager's `StartLocalX` functions before the scene loads - not tied to scene build index, so any level can run local play.
- `enableBeta` is forced `false` whenever `isLocalGame` - beta (E3) moveset/animations/SFX are a separate feature and should never combine with local split-screen.
- `gameObject.name.Equals("PlayerLuigi(Clone)")` is the standard way to tell Mario/Luigi apart, but that name also occurs in online play - always check `isLocalGame` first.
- `betaAnims` exists as two separate fields (on `PlayerController` and `PlayerAnimationController`), both independently reading the same `NewPowerups` room property - keep both in sync when touching this.
- `GameManager.prefab` edits auto-propagate to every scene, *unless* a scene already has its own override on that specific property - after a prefab edit, check/re-apply overrides on any scenes that need it.
- Reparenting a GameObject that lives inside a prefab's own hierarchy (e.g. anything under "New HUD") must be done by editing the prefab asset itself (`PrefabUtility.LoadPrefabContents`/`SaveAsPrefabAsset`) - a scene-instance `SetParent` silently fails to persist.
- Camera culling masks are precisely tuned (root `Camera` = TransparentFX-only base pass, `BaseCamera`/`ScrollCamera` exclude it) - never blindly reset masks to `-1`/"Everything".
- Local split-screen rigs: rig 1 = Mario's camera (the original "Camera"), rig 2 = "Camera (Player2)" (duplicated, inactive by default, activated only in local mode).
- Split-screen controller auto-assignment: keyboard always drives Luigi; first connected gamepad drives Mario; a second gamepad (if present) also drives Luigi alongside keyboard.
- The Bridge's `set` command doesn't reliably support every property (e.g. `GameObject.tag`, some `Camera` fields silently no-op) - when in doubt, use a scratch script (`Assets/Editor/BridgeScratch.cs`) for direct, reliable C# access, and reset it to empty afterward.
- Always run `refresh` after editing scripts/scenes via the Bridge, and check `errors` before considering a change done.
- `LocalGrasslands`/`LocalBricks`/`LocalBeta` scenes are slated for eventual removal - local play now also works on the regular `Default*` levels.
- The HUD's `CanvasScaler` uses "Match Width Or Height = 0" (pure width match) - canvas virtual height varies with aspect ratio, so only elements anchored at `(0.5, 0.5)` with zero offset stay perfectly centered on every aspect ratio; corner-anchored elements only stay width-stable.
- `HorizontalCamera.AdjustCamera()` sets `orthographicSize = orthoSize + OFFSET` directly (no aspect-ratio compensation) - vertical FOV is constant across aspect ratios, horizontal varies naturally with the screen shape.
