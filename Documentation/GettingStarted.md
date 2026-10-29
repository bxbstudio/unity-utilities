# Getting Started

This guide covers the current public surface of `dev.bxbstudio.utilities` and shows the imports and patterns that are actually present in the package.

## Install the Package

### Git URL

1. Open `Window > Package Manager`.
2. Select `+ > Add package from git URL...`.
3. Enter:

```text
https://github.com/bxbstudio/unity-utilities.git
```

### Requirements

- Unity `2020.3.17f1` or newer
- `com.unity.mathematics` `1.2.6`

## Import the Right Namespace

Use the namespace that matches the API surface you need:

```csharp
using Utilities;
using Utilities.Core;
using Utilities.Core.Managed;
```

Editor-only APIs live under:

```csharp
using Utilities.Editor;
```

Important: several commonly used value types are nested inside the `Utility` class. Use names such as `Utility.Interval`, `Utility.Interval2`, `Utility.SerializableVector2`, and `Utility.SerializableColor`.

## Runtime Extensions

The `Utilities.Extensions` static class provides extension methods for common Unity and .NET types.

```csharp
using UnityEngine;
using Utilities;

public class RuntimeExtensionsExample : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;

    private void Start()
    {
        Transform child = transform.FindStartsWith("Wheel", caseSensitive: false);
        AnimationCurve clamped = curve.Clamp01();

        string joined = new[] { "front", "left" }.Join("-");
        Debug.Log(child ? joined : "Child not found");
        Debug.Log(clamped.length);
    }
}
```

Available extension groups include:

- `Transform` name matching helpers
- `AnimationCurve` clamp and clone helpers
- `string` null/whitespace, split, join, and replace helpers
- `Enum.GetAttribute<T>()`
- `Color.SetAlpha()`

## Utility Nested Types and Helpers

The main `Utility` class contains enums, nested structs, serializable wrappers, and a large collection of static helpers.

```csharp
using UnityEngine;
using Utilities;

public class UtilityTypesExample : MonoBehaviour
{
    private void Start()
    {
        Utility.Interval travel = new Utility.Interval(0f, 0.25f);
        Utility.Interval2 viewport = new Utility.Interval2(0f, 1f, 0f, 1f);

        float midpoint = travel.Lerp(0.5f);
        string speed = Utility.NumberToValueWithUnit(30f, Utility.Units.Speed, Utility.UnitType.Imperial, 1);

        Debug.Log($"{midpoint:F3} | {viewport.CenterVector2}");
        Debug.Log(speed);
    }
}
```

The current package includes these notable nested types inside `Utility`:

- `Interval`, `SimpleInterval`, `IntervalInt`
- `Interval2`, `SimpleInterval2`
- `Interval3`, `SimpleInterval3`
- `SerializableVector2`, `SerializableRect`, `SerializableColor`
- `SerializableAudioClip`, `SerializableParticleSystem`, `SerializableMaterial`, `SerializableLight`
- `ColorSheet`, `TransformAccess`, `float1`
- enums such as `Units`, `UnitType`, `RenderPipeline`, `TextureEncodingType`, `WorldSide`, and `WorldSurface`

Representative `Utility` helper groups include:

- Unit conversion and formatting
- Direction, distance, and averaging helpers
- Bounds and collider utilities
- Screenshot and texture export helpers
- Render-pipeline detection
- Layer-mask based GameObject searches

## Bezier Paths

`Bezier.Path` provides a mutable spline path type for sampling points, generating meshes, and editing control points.

```csharp
using UnityEngine;
using Utilities;

public class BezierExample : MonoBehaviour
{
    private void Start()
    {
        Bezier.Path path = new Bezier.Path(LayerMask.GetMask("Default"));
        path.AddSegment(new Vector3(5f, 0f, 0f));
        path.AddSegment(new Vector3(10f, 0f, 3f));

        Vector3[] points = path.GetSpacedPoints(1f, resolution: 8);
        Debug.Log(points.Length);
    }
}
```

## Data Serialization

`DataSerializationUtility<T>` saves and loads reference-type data as UTF-8 JSON, using `JsonUtility`, from a regular file path or from `Resources`.

- Only public or `[SerializeField]` fields of `[Serializable]` types are stored. Root arrays, dictionaries and polymorphic references aren't supported.
- Constructors don't run when data is loaded.
- Files saved in the BinaryFormatter format used before version 1.1.14 are migrated to JSON automatically in the Unity Editor, or manually from `Tools/Utilities/Migrate Legacy Data Files`. The original files are backed up to `Library/BxBStudio/LegacyData`. Player builds can only read JSON files.

```csharp
using System;
using UnityEngine;
using Utilities;

[Serializable]
public class SettingsData
{
    public float masterVolume;
}

public class SerializationExample : MonoBehaviour
{
    private void Start()
    {
        DataSerializationUtility<SettingsData> serializer =
            new DataSerializationUtility<SettingsData>("SaveData/Settings.dat", false, bypassExceptions: true);

        serializer.SaveOrCreate(new SettingsData { masterVolume = 0.8f });
        SettingsData loaded = serializer.Load();

        Debug.Log(loaded != null ? loaded.masterVolume : 0f);
    }
}
```

## Core Helpers

The `Utilities.Core` namespace groups project-level helpers and reusable patterns.

### `BehaviourSingleton<T>`

Use this for scene-based singletons that optionally persist across scene loads.

```csharp
using Utilities.Core;

public sealed class GameSettings : BehaviourSingleton<GameSettings>
{
    public override bool Persistent => true;
}
```

### `ScriptableSingleton<T>`

Use this for `Resources/Settings/<TypeName>` backed configuration assets.

```csharp
using UnityEngine;
using Utilities.Core;

[CreateAssetMenu(menuName = "Settings/Audio Settings")]
public class AudioSettings : ScriptableSingleton<AudioSettings>
{
    public float volume = 1f;
}
```

### `ResourcesReference<T>` and `SceneAssetReference`

These types provide serializable references for resource-backed assets and scenes.

```csharp
using UnityEngine;
using Utilities.Core.Managed;

public class ReferenceExample : MonoBehaviour
{
    [SerializeField] private ResourcesReference<GameObject> uiPrefab;
    [SerializeField] private SceneAssetReference gameplayScene;
}
```

### Other core helpers

- `GameLogger` for formatted runtime logs
- `CoreUtility.TryGetTypeFromString()` for resolving types by string
- `SerializedDictionary<TKey, TValue>` for serializing dictionary-like data

## Editor Utilities

Editor APIs live under `Utilities.Editor` and are wrapped in editor-only assemblies.

```csharp
#if UNITY_EDITOR
using UnityEditor;
using Utilities.Editor;

public static class EditorUtilitiesExample
{
    [MenuItem("Tools/Utilities/Examples/Enable Demo Define")]
    private static void EnableDemoDefine()
    {
        if (!EditorUtilities.ScriptingDefineSymbolExists("DEMO_DEFINE"))
            EditorUtilities.AddScriptingDefineSymbol("DEMO_DEFINE");
    }
}
#endif
```

The current editor tooling includes:

- `EditorUtilities` styles, icons, scripting define helpers, and debug menu actions
- `LayersManager` add/remove/rename/query helpers for Unity layers
- `ScriptableObjectUtility` asset creation helpers
- `UnitsConverterEditor`
- `WheelRadiusCalculatorEditor`

## Next Steps

- Use the [API Reference](APIReference.md) for a curated map of the current public surface.
- Use the [Installation Guide](Installation.md) if you need embedded-package or manual setup details.
