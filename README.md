# RPhysicsEngine

A small **2D physics engine** written from scratch in C#, aimed at platformer games. It is the software part of a FAMNIT diploma thesis: rigid bodies, collision detection/resolution, spatial hashing, and a platformer player controller — plus a WinForms sandbox and a thin Unity integration.

The physics library targets **.NET Standard 2.1** (`RPhysicsEngine.dll`) so the same code runs in the WinForms demo and inside Unity. Unity’s built-in Physics2D is **not** used.

## Features

- Axis-aligned boxes and circles (no rotation)
- Collision **detection** (manifold: normal + penetration) separate from **resolution** (separation + impulses)
- Spatial-hash broad-phase vs brute-force pairwise checks (toggle in the sandbox)
- Multi-iteration contact solving, sleeping, triggers (enter / stay / exit)
- Platformer extras: coyote time, jump buffering, variable jump height, one-way and moving platforms
- WinForms debug view, pause/single-step, stress scene
- Unity bridge: Y-down engine ↔ Y-up Unity, sprite-size → collider size

**Not implemented:** rotation, slopes, polygon colliders, continuous collision detection (fast objects can tunnel).

## Repository layout

```
Engine/           Physics library (netstandard2.1) → RPhysicsEngine.dll
EngineRunner/     WinForms sandbox (input, GDI+ draw, evaluation HUD)
UnityBridge/      MonoBehaviour adapters to copy into a Unity project
Engine.sln        Visual Studio / `dotnet` solution
```

API notes for the library: [Engine/README.md](Engine/README.md)  
Unity import detail: [UnityBridge/IMPORT.md](UnityBridge/IMPORT.md)

## Coordinate system

The engine is **Y-down** (like WinForms / screen space): gravity is typically `(0, +500)`, jump uses a **negative** `Velocity.Y`, `RVector2.Up` is `(0, -1)`.

Unity is **Y-up**. Do not rewrite gravity in the library — the bridge flips Y in `RUnityConvert`.

Physics is stepped with a **fixed** `dt` (sandbox: `1/120` s). That is independent of how often the window is drawn.

---

## WinForms sandbox

### Requirements

- .NET 8 SDK (Windows)
- Visual Studio 2022 or `dotnet` CLI

### Run

```powershell
dotnet build Engine.sln -c Release
dotnet run --project EngineRunner -c Release
```

Or open `Engine.sln` and run the **EngineRunner** project.

### Controls

| Key | Action |
|-----|--------|
| A / D or arrows | Move |
| Space / W / Up | Jump (hold for full jump, tap for a shorter hop) |
| F1 | Toggle debug overlay |
| F2 | Platformer scene ↔ stress test (many bodies) |
| F3 | Spatial hash on/off (compare pair counts / FPS) |
| P | Pause physics |
| . or N | While paused: advance one physics step |

The HUD shows FPS (physics **and** GDI+ drawing), physics step time, body count, sleeping count, candidate collision pairs, and whether the broad-phase is on.

At low body counts FPS is often limited by WinForms drawing (~40 FPS on the test machine), not by physics. Pair count is the fair measure of collision work. F3 is what the thesis evaluation used.

Path reverse for moving platforms is handled in the sandbox (and in the Unity collider view), not inside `RPhysicsWorld` itself.

---

## Unity

### 1. Build the library

From this repo root:

```powershell
dotnet build Engine\Engine.csproj -c Release
```

Output: `Engine\bin\Release\netstandard2.1\RPhysicsEngine.dll`

### 2. Copy into a Unity project

Use Unity **2021.3 LTS**, **2022 LTS**, or **Unity 6**, **2D** template.

1. Copy `RPhysicsEngine.dll` → `Assets/Plugins/`
2. Copy every `.cs` file from `UnityBridge/` → e.g. `Assets/Scripts/RPhysics/`  
   (Do not copy `IMPORT.md` / `README.md` unless you want them in the project.)
3. Select the DLL in the Inspector: **Any Platform** enabled

If the DLL will not load, copy the `Engine/Engine/` **source** `.cs` files into `Assets` instead and remove the plugin (see [IMPORT.md](UnityBridge/IMPORT.md)).

### 3. Scene setup

Leave **Sync Size From Sprite** on. Scale sprites in the Scene view; physics size is read from `SpriteRenderer.bounds` when Play starts (not every frame after that).

1. Empty GameObject + **`RPhysicsWorldBehaviour`** (one per scene).
2. Ground sprite + **`RStaticColliderView`**.
3. Player sprite + **`RBodyView`** (rectangle, mass 10, restitution 0) and **`RPlayerInputBehaviour`**.
4. Select an object: the wire gizmo should match the sprite if size sync is correct.

**Edit → Project Settings → Time → Fixed Timestep:** `0.02` (50 Hz) or `0.01666` (60 Hz).

Input uses the **legacy Input Manager** (A/D/arrows + Space). No Input System package required.

Play: A/D move, Space jump. If the player falls **up**, the Y conversion is wrong — check `RUnityConvert` and object positions.

Full checklist: [UnityBridge/IMPORT.md](UnityBridge/IMPORT.md).

The WinForms stress scene (F2 / F3) is still the place for performance numbers; Unity is for playing a level with the same library.

---

## Using the library in your own code

```csharp
var world = new RPhysicsWorld();
world.StaticColliders.Add(new RStaticCollider(new RAABB(0f, 800f, 400f, 450f)));

var player = new RRigidBody(
    new RVector2(50f, 340f),
    new RRectangleShape(30f, 50f),
    mass: 10f,
    isStatic: false,
    useGravity: true);
player.CanSleep = false;
player.Restitution = 0f;
world.Bodies.Add(player);

var controller = new RPlayerController(player);

// each physics tick:
controller.ApplyInput(moveLeft, moveRight, jumpPressed, jumpHeld, dt);
world.Step(dt);
```

`jumpPressed` should be true only on the press frame, not while the key is held.

Triggers: set `IsTrigger` and `Tag` on an `RStaticCollider`, subscribe to `world.OnStaticContact` (`Enter` / `Stay` / `Exit`).

---

## License / academic use

Written as diploma work at the University of Primorska, FAMNIT. You are free to use the code as a reference or starting point; please keep attribution if you reuse it.
