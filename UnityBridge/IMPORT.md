# Importing RPhysicsEngine into Unity

## Prerequisites

- Unity **2021.3 LTS**, **2022 LTS**, or **Unity 6**
- A **2D** project (URP 2D or Built-in 2D)
- This repo built in Release so the DLL exists

## 1. Build the library

From the `Engine` solution folder:

```powershell
dotnet build Engine\Engine.csproj -c Release
```

Output:

`Engine\bin\Release\netstandard2.1\RPhysicsEngine.dll`

## 2. Copy into Unity

1. Create folders if needed:
   - `Assets/Plugins/`
   - `Assets/Scripts/RPhysics/`
2. Copy **`RPhysicsEngine.dll`** → `Assets/Plugins/`
3. Copy all scripts from this folder (`UnityBridge/`) → `Assets/Scripts/RPhysics/`

Select the DLL in Unity Inspector:

- **Any Platform** checked  
- **Validate References** can stay on  

If Unity reports a scripting backend / API compatibility error, use the **source fallback** below.

## 3. Minimal scene setup

**Size workflow:** leave **Sync Size From Sprite** enabled (default) on `RBodyView` / `RStaticColliderView`.  
Scale objects in the Scene view until they look right — physics uses that world size automatically.  
You do **not** need to type Width/Height for each object unless you turn sync off.

### Physics world

1. Create empty GameObject: `RPhysicsWorld`
2. Add component **`RPhysicsWorldBehaviour`**
3. Optionally enable `manageScreenBounds` and set width/height (engine units)

### Ground

1. Create a Sprite (Square) named `Ground`
2. Scale it in the Scene until it looks like a floor (e.g. Scale X large, Y small)
3. Position it where you want the floor
4. Add **`RStaticColliderView`**
   - **Sync Size From Sprite** = on
   - One Way / Trigger / Moving = off

### Player

1. Create a Sprite named `Player`
2. Scale it to the character size you want
3. Position above the ground
4. Add **`RBodyView`** — Sync Size From Sprite on, Shape Rectangle, Mass 10, Restitution 0
5. Add **`RPlayerInputBehaviour`** (same object)

### Check precision

Select Player or Ground in the Scene view — a **wire gizmo** (green/cyan) should outline the physics box on top of the sprite. If the wire matches the sprite edges, size sync is correct.

## 4. Project settings

- **Edit → Project Settings → Time → Fixed Timestep**: `0.02` (50 Hz) or `0.01666` (60 Hz)  
- **Input**: legacy Input Manager (A/D/arrows + Space) — no Input System package required  

## 5. Play-mode checks

| Check | Expected |
|--------|----------|
| Press Play | No missing assembly errors |
| A / D | Player moves horizontally |
| Space | Jump (short tap = shorter jump) |
| Land on ground | Stops falling; can jump again |
| Visual Y | Player falls “down” the screen (Unity −Y if you mapped engine +Y down correctly) |

If the player falls **up**, the Y conversion is inverted — check `RUnityConvert` and sprite positions.

## Source fallback (if DLL fails)

1. Create `Assets/RPhysicsEngine/`
2. Copy the entire `Engine/Engine/` source tree (all `.cs` under Math, Physics, …) into it  
3. **Do not** copy `Engine.csproj`  
4. Remove the Plugins DLL to avoid duplicate types  
5. Keep using the same `UnityBridge` scripts  

## Scale tip

The WinForms demo uses **pixel-ish units** (player ~30×50, gravity 500). In Unity you can:

- Keep those numbers and zoom the camera out, or  
- Scale all sizes/gravity down (e.g. ÷50) consistently in the inspector  

Pick one scale and stick to it for the whole scene.

## After import works

Return to the WinForms **F2 stress scene** + **F3 broad-phase toggle** for thesis Evaluation graphs (pair counts / FPS). That does not depend on Unity.
