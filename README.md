# Unity Systems Showcase

I'm Efe Arda, a Unity/C# developer working on PC games under the name Mossmallow Studio.

This repository collects a few systems I use in my own projects, rewritten as a small standalone package.
They are game-independent on purpose: no project code, art or content is included here.
Each system is a handful of files, has no dependencies outside Unity, and comes with EditMode tests.

## Systems

| System | Namespace | Folder |
| --- | --- | --- |
| Prefab pool | `Mossmallow.Systems.Pooling` | `Runtime/Pooling` |
| Versioned JSON save | `Mossmallow.Systems.Saving` | `Runtime/Saving` |
| Hit-stop | `Mossmallow.Systems.Timing` | `Runtime/Timing` |
| Trauma camera shake | `Mossmallow.Systems.Shake` | `Runtime/Shake` |

### Prefab pool

Spawning and destroying lots of short-lived objects (hit sparks, projectiles, debris) causes GC spikes.
`PrefabPool` keeps one free list per prefab, reuses inactive instances and quietly skips any that were destroyed
by a scene change. `ReleaseAfter` returns an instance after a fixed lifetime, so one-shot effects need no extra code.

```csharp
using Mossmallow.Systems.Pooling;

var spark = PrefabPool.Shared.Spawn(sparkPrefab, hitPoint, Quaternion.identity);
// later, or let a ReleaseAfter component on the prefab do it:
PrefabPool.Shared.Release(spark);
```

### Versioned JSON save

Save files outlive the code that wrote them. `VersionedJsonStore<T>` stores a schema version in the file and runs
registered migrations one step at a time when an older save is loaded. Writes go to a temporary file and replace
the old one atomically, keeping the previous save as `.bak`; if the main file is corrupt, the backup is loaded instead.

```csharp
using Mossmallow.Systems.Saving;

[Serializable] class Profile : SaveData { public int coins; public string playerName; }

var store = new VersionedJsonStore<Profile>(Path.Combine(Application.persistentDataPath, "profile.json"), 2)
    .AddMigration(1, p => p.playerName = "Player"); // v1 had no name field

Profile profile = store.Load();
profile.coins += 10;
store.Save(profile);
```

### Hit-stop

A few frames of freeze on impact make hits feel heavy. `HitStop.Freeze` pauses `Time.timeScale`, optionally
followed by a short slow-motion tail, and restores the previous scale when done. Overlapping requests extend to the
latest end time instead of stacking. The timing logic lives in `HitStopClock`, which takes time as a parameter and is fully unit tested.

```csharp
using Mossmallow.Systems.Timing;

HitStop.Freeze(0.06f);          // light hit
HitStop.Freeze(0.12f, 0.15f);   // heavy hit with a slow-motion tail
HitStop.Strength = 0f;          // accessibility setting: off
```

### Trauma camera shake

Shake driven by "trauma": every impact adds to a 0..1 value that decays over time, and the visible shake is
trauma squared times Perlin noise. Small hits stay subtle, large ones read clearly, and several hits blend
smoothly instead of jittering. `CameraShake` writes only a local offset, so it sits under any camera follow script.
It runs on unscaled time, so it keeps moving during hit-stop.

```csharp
using Mossmallow.Systems.Shake;

// CameraShake component on a child of the camera rig
CameraShake.Add(0.3f);   // hit
CameraShake.Add(0.8f);   // explosion
CameraShake.Scale = 0.5f; // settings slider
```

## Installation

Requires Unity 6 (6000.0 or newer).

Open **Window > Package Manager**, choose **+ > Install package from git URL** and enter:

```
https://github.com/efearda1/unity-systems-showcase.git
```

Or add it to `Packages/manifest.json`:

```json
"com.mossmallow.systems": "https://github.com/efearda1/unity-systems-showcase.git"
```

To run the tests from the Test Runner, also add the package to `testables` in the same file:

```json
"testables": [ "com.mossmallow.systems" ]
```

## Games

Gameplay videos: (link coming soon)

## License

All rights reserved. The code is published for review only; see [LICENSE](LICENSE).
