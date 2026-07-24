# UnityBridge

Thin MonoBehaviour adapters for **RPhysicsEngine**.  
These scripts are **not** part of the WinForms solution, copy them into a Unity project.

See **[IMPORT.md](IMPORT.md)** for step-by-step setup.

| Script | Purpose |
|--------|---------|
| `RUnityConvert.cs` | Y-down engine ↔ Y-up Unity |
| `RPhysicsWorldBehaviour.cs` | Owns `RPhysicsWorld`, `FixedUpdate` step |
| `RBodyView.cs` | Sync `RRigidBody` ↔ Transform |
| `RPlayerInputBehaviour.cs` | Keyboard → `RPlayerController` |
| `RStaticColliderView.cs` | Inspector platforms / triggers / movers |
