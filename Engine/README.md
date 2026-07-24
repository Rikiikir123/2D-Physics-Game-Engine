# RPhysicsEngine

Lightweight 2D physics library for platformer games (diploma thesis).  
Built as **netstandard2.1** so it can run in the WinForms sandbox (`EngineRunner`) and in **Unity** (via `Assets/Plugins`).

Assembly name: `RPhysicsEngine.dll`

## Coordinate system

This engine uses **screen-space Y-down**:

- `+Y` is downward
- Gravity is typically `(0, +500)`
- Jump sets `Velocity.Y` to a **negative** value
- `RVector2.Up` is `(0, -1)`

Unity uses **Y-up**. When integrating with Unity, convert at the boundary (see `UnityBridge/RUnityConvert.cs`) — do not change core physics.

## Core loop

Drive the simulation with a **fixed timestep** (e.g. `1/50` or `1/60`):

```csharp
RPhysicsWorld world = new RPhysicsWorld();
// add bodies, static colliders, triggers...

void FixedUpdate() // or your own accumulator loop
{
    world.Step(Time.fixedDeltaTime);
}
```

`RPhysicsWorld.Step`:

1. Carry riders on moving platforms  
2. Move platforms  
3. Integrate dynamic bodies  
4. Resolve solid collisions (multi-iteration)  
5. Process trigger enter/stay/exit events  
6. Sleep resting bodies  

## Main types

| Type | Role |
|------|------|
| `RVector2` | 2D math |
| `RRigidBody` | Dynamic/static body (position, velocity, mass, sleep, `PlatformVelocity`) |
| `RRectangleShape` / `RCircleShape` | Collider shapes |
| `RAABB` / `RStaticCollider` | Static geometry (solid, one-way, moving, trigger) |
| `RPhysicsWorld` | Simulation owner |
| `RPlayerController` | Platformer move/jump (no input API — feed booleans each step) |
| `RContactEvent` / `OnStaticContact` | Trigger enter/stay/exit |

## Static colliders

```csharp
// solid floor
world.StaticColliders.Add(new RStaticCollider(new RAABB(left, right, top, bottom)));

// one-way
var oneWay = new RStaticCollider(bounds, isOneWay: true);

// moving
mover.Velocity = new RVector2(80f, 0f);
mover.PathMin = 0f;   // used by demo path reverse; optional
mover.PathMax = 400f;

// trigger (sensor — no push)
var coin = new RStaticCollider(bounds);
coin.IsTrigger = true;
coin.Tag = "coin";
```

Subscribe:

```csharp
world.OnStaticContact += e =>
{
    if (e.Phase == RContactPhase.Enter && e.Collider.Tag == "coin")
        e.Collider.Enabled = false;
};
```

## Player controller

```csharp
var controller = new RPlayerController(playerBody);
controller.ApplyInput(moveLeft, moveRight, jumpPressed, jumpHeld, deltaTime);
```

Call once per physics step. `jumpPressed` should be true only on the frame the jump key was pressed.

## Unity

1. Build Release: `dotnet build Engine/Engine.csproj -c Release`  
2. Copy `Engine/bin/Release/netstandard2.1/RPhysicsEngine.dll` → Unity `Assets/Plugins/`  
3. Copy scripts from repo `UnityBridge/` → Unity `Assets/Scripts/RPhysics/`  
4. Follow [UnityBridge/IMPORT.md](../UnityBridge/IMPORT.md)

## WinForms sandbox

`EngineRunner` remains the development / evaluation harness (debug draw, stress scene, pause/step). It is **not** part of the Unity library.
