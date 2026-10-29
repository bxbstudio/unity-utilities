# Installation Guide

This guide covers the supported ways to add `dev.bxbstudio.utilities` to a Unity project and the package requirements that matter for the current codebase.

## Requirements

- Unity `2020.3.17f1` or newer
- `com.unity.mathematics` `1.2.6`

## Install with Unity Package Manager

### Git URL

1. Open your Unity project.
2. Go to `Window > Package Manager`.
3. Select `+ > Add package from git URL...`.
4. Enter:

```text
https://github.com/bxbstudio/unity-utilities.git
```

5. Click `Add`.

### `manifest.json`

You can also add the dependency directly to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "dev.bxbstudio.utilities": "https://github.com/bxbstudio/unity-utilities.git"
  }
}
```

If your project does not already include `com.unity.mathematics`, add it as well:

```json
{
  "dependencies": {
    "com.unity.mathematics": "1.2.6",
    "dev.bxbstudio.utilities": "https://github.com/bxbstudio/unity-utilities.git"
  }
}
```

## Embedded or Local Package Setup

If you prefer to keep a local editable copy inside your project:

1. Copy the package folder into `Packages/dev.bxbstudio.utilities`.
2. Make sure the folder contains `package.json`, `README.md`, `Documentation/`, and `Scripts/`.
3. Open Unity and allow the package database to refresh.

This approach is useful when you want to inspect or customize editor utilities, managed reference drawers, or the large `Utility.cs` surface directly in your project.

## Verify the Installation

Create or open any script and add:

```csharp
using Utilities;
using Utilities.Core;
using Utilities.Core.Managed;
```

For editor-only APIs:

```csharp
using Utilities.Editor;
```

Compilation should succeed, and the following menu items should appear under `Tools/Utilities`:

- `Units Converter`
- `Wheel Radius Calculator`
- the debug helpers exposed by `EditorUtilities`

## Notes About Package Surface

- Several commonly used types are nested inside the `Utility` class. Use names such as `Utility.Interval` and `Utility.SerializableColor`, not bare `Interval` or `SerializableColor`.
- `Utilities.Core.Managed` provides serializable scene/resource reference types together with their editor drawers.
- The `Utilities.Extensions` assembly includes helpers for `LayerMask`, `GameObject`, `Unity.Mathematics.AABB`, and an ECS-oriented `DynamicBuffer<T>.AsNativeList()` convenience method.

## Troubleshooting

### Missing `Unity.Mathematics`

If you see errors related to `float2`, `float3`, `AABB`, or `math`, ensure `com.unity.mathematics` is installed at `1.2.6` or newer.

### Missing editor APIs

`Utilities.Editor` is editor-only. Wrap editor usages in `#if UNITY_EDITOR` and avoid referencing `UnityEditor` assemblies from runtime code.

### Types like `Interval` do not resolve

Use the full nested type name:

```csharp
Utility.Interval range = new Utility.Interval(0f, 1f);
```

### Managed references show blank fields

`ResourcesReference<T>` expects a path relative to a `Resources` folder. `SceneAssetReference` expects a valid scene asset selected through the editor drawer.