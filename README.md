# Utilities

[![Unity Version](https://img.shields.io/badge/Unity-2020.3%2B-blue.svg)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)
[![Package](https://img.shields.io/badge/UPM-dev.bxbstudio.utilities-blue)](https://github.com/BxB-Studio/Unity-CSharp-Utilities)

`dev.bxbstudio.utilities` is a Unity utility package that groups runtime helpers, nested math/value types, serializable wrappers, editor tooling, and a small set of core patterns such as singletons and managed asset references.

## Features

- Runtime extension methods for `Transform`, `AnimationCurve`, `string`, `Enum`, and `Color`
- Large `Utility` surface for units, intervals, math helpers, bounds helpers, render-pipeline checks, screenshots, and asset helpers
- `Bezier.Path` utilities for building and sampling spline paths
- File and `Resources`-based JSON serialization through `DataSerializationUtility<T>`, with automatic Editor migration of legacy BinaryFormatter files
- Core helpers such as `BehaviourSingleton<T>`, `ScriptableSingleton<T>`, `GameLogger`, and `SerializedDictionary<TKey, TValue>`
- Managed references for scene assets and `Resources` assets with editor drawers
- Editor utilities for scripting define symbols, debug menu items, layers management, and small workflow windows
- Serializable wrapper types such as `Utility.SerializableVector2`, `Utility.SerializableRect`, `Utility.SerializableColor`, `Utility.SerializableAudioClip`, `Utility.SerializableMaterial`, `Utility.SerializableLight`, and `Utility.SerializableParticleSystem`

## Requirements

- Unity `2020.3.17f1` or newer
- `com.unity.mathematics` `1.2.6`

## Installation

### Using Unity Package Manager

1. Open `Window > Package Manager`.
2. Select `+ > Add package from git URL...`.
3. Enter:

```text
https://github.com/bxbstudio/unity-utilities.git
```

For more setup options, see the [Installation Guide](Documentation/Installation.md).

## Namespaces

Most consumers will use one or more of these namespaces:

- `Utilities`
- `Utilities.Core`
- `Utilities.Core.Managed`
- `Utilities.Editor`

Important: many value types are nested inside the `Utility` class, so their full names are things like `Utility.Interval`, `Utility.Interval2`, `Utility.SerializableColor`, and `Utility.ColorSheet`.

## Usage Examples

### Runtime extensions

```csharp
using UnityEngine;
using Utilities;

public class UtilityExamples : MonoBehaviour
{
    [SerializeField] private AnimationCurve throttleCurve;

    private void Start()
    {
        Transform wheel = transform.FindContains("wheel", caseSensitive: false);
        AnimationCurve safeCurve = throttleCurve.Clamp01();

        if ("  ".IsNullOrWhiteSpace())
        {
            Debug.Log(wheel ? "Wheel found" : "Wheel not found");
        }
    }
}
```

### Utility nested types and unit helpers

```csharp
using UnityEngine;
using Utilities;

public class IntervalExample : MonoBehaviour
{
    private void Start()
    {
        Utility.Interval suspensionTravel = new Utility.Interval(0f, 0.2f);
        float halfTravel = suspensionTravel.Lerp(0.5f);

        string metric = Utility.NumberToValueWithUnit(27f, Utility.Units.Speed, Utility.UnitType.Metric, 1);
        Debug.Log($"{halfTravel:F3} m, {metric}");
    }
}
```

### Data serialization

Data is stored as UTF-8 JSON using `JsonUtility`, so only public or `[SerializeField]` fields of `[Serializable]` types are saved.

```csharp
using System;
using UnityEngine;
using Utilities;

[Serializable]
public class PlayerSaveData
{
    public string playerName;
    public int score;
}

public class SaveExample : MonoBehaviour
{
    private readonly DataSerializationUtility<PlayerSaveData> serializer =
        new DataSerializationUtility<PlayerSaveData>("SaveData/Player.dat", false, bypassExceptions: true);

    private void Start()
    {
        serializer.SaveOrCreate(new PlayerSaveData
        {
            playerName = "Player 1",
            score = 9000
        });

        PlayerSaveData loaded = serializer.Load();
        Debug.Log(loaded != null ? loaded.playerName : "No save file");
    }
}
```

### Core references and singleton patterns

```csharp
using UnityEngine;
using Utilities.Core;
using Utilities.Core.Managed;

public sealed class AudioManager : BehaviourSingleton<AudioManager>
{
    public override bool Persistent => true;
}

public class PrefabSpawner : MonoBehaviour
{
    [SerializeField] private ResourcesReference<GameObject> prefabReference;

    private void Start()
    {
        GameObject prefab = prefabReference.Load();

        if (prefab != null)
            Instantiate(prefab);
    }
}
```

### Editor utilities

```csharp
#if UNITY_EDITOR
using UnityEditor;
using Utilities.Editor;

public static class UtilityEditorExample
{
    [MenuItem("Tools/Utilities/Examples/Add Gameplay Layer")]
    private static void AddGameplayLayer()
    {
        if (!LayersManager.LayerExists("Gameplay"))
            LayersManager.AddLayer("Gameplay");

        EditorUtilities.AddScriptingDefineSymbol("GAMEPLAY_LAYER_READY");
    }
}
#endif
```

## Included Editor Tools

The package adds several menu-driven utilities under `Tools/Utilities`, including:

- Units Converter
- Wheel Radius Calculator
- GameObject bounds and mesh debugging
- Place selected object on the zero surface
- Texture2DArray export helpers

## Documentation

- [Getting Started](Documentation/GettingStarted.md)
- [API Reference](Documentation/APIReference.md)
- [Installation Guide](Documentation/Installation.md)

## License

This project is licensed under the MIT License. See [LICENSE.md](LICENSE.md).

---

Developed by [BxB Studio](https://bxbstudio.dev)