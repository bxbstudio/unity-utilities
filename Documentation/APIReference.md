# API Reference

This is a curated reference for the current public API shipped in `dev.bxbstudio.utilities`. It is organized by namespace and feature area rather than attempting to mirror every member in the large `Utility.cs` source file.

## `Utilities`

### `Extensions`

Extension methods for common Unity and .NET types.

| API | Notes |
| --- | --- |
| `Transform Find(string name, bool caseSensitive = true)` | Match a direct child by exact name. |
| `Transform FindStartsWith(string name, bool caseSensitive = true)` | Match a direct child whose name starts with a value. |
| `Transform FindEndsWith(string name, bool caseSensitive = true)` | Match a direct child whose name ends with a value. |
| `Transform FindContains(string name, bool caseSensitive = true)` | Match a direct child whose name contains a value. |
| `AnimationCurve Clamp01()` | Clamp curve time and value to the `[0, 1]` range. |
| `AnimationCurve Clamp(Keyframe min, Keyframe max)` | Clamp a curve using min/max keyframes. |
| `AnimationCurve Clamp(float timeMin, float timeMax, float valueMin, float valueMax)` | Clamp a curve by explicit ranges. |
| `AnimationCurve Clone()` | Create a deep copy of a curve. |
| `bool IsNullOrEmpty(this string str)` | String null/empty check. |
| `bool IsNullOrWhiteSpace(this string str)` | String whitespace-aware null/empty check. |
| `string Join(this IEnumerable<string> strings, string separator)` | Join a sequence of strings. |
| `string Replace(this string str, IEnumerable<string> find, string replacement)` | Replace multiple string tokens with one value. |
| `string[] Split(this string str, string separator)` | Split with a string separator. |
| `T GetAttribute<T>(this Enum enumValue)` | Read an attribute from an enum value. |
| `Color SetAlpha(this Color color, float alpha)` | Return a color with a replaced alpha channel. |

### `Utility`

`Utility` is the package's largest runtime surface. It mixes enums, nested value types, serializable wrappers, and many static helpers.

#### Core enums

- `Utility.Precision`
- `Utility.UnitType`
- `Utility.Units`
- `Utility.RenderPipeline`
- `Utility.TextureEncodingType`
- `Utility.WorldSide`
- `Utility.WorldSurface`
- `Utility.Axis2`
- `Utility.Axis3`
- `Utility.Axis4`

#### Nested value types

- `Utility.Interval`
- `Utility.SimpleInterval`
- `Utility.IntervalInt`
- `Utility.Interval2`
- `Utility.SimpleInterval2`
- `Utility.Interval3`
- `Utility.SimpleInterval3`
- `Utility.ColorSheet`
- `Utility.TransformAccess`
- `Utility.float1`
- `Utility.JsonArray<T>`

#### Serializable wrappers

- `Utility.SerializableVector2`
- `Utility.SerializableRect`
- `Utility.SerializableColor`
- `Utility.SerializableAudioClip`
- `Utility.SerializableParticleSystem`
- `Utility.SerializableMaterial`
- `Utility.SerializableLight`

#### Frequently used helper groups

| API Group | Representative members |
| --- | --- |
| Unit conversion | `UnitMultiplier()`, `FullUnit()`, `ValueWithUnitToNumber()`, `NumberToValueWithUnit()` |
| Vector math | `Direction()`, `Distance()`, `Average()`, `Approximately()` |
| Runtime utilities | `GetTimestamp()`, `FindIntersection()`, `AddTorqueAtPosition()` |
| Audio/helpers | `NewAudioSource()` |
| Textures and rendering | `GetTextureArrayItem()`, `GetTextureArrayItems()`, `SaveTexture2D()`, `TakeScreenshot()`, `TextureToSprite()`, `GetCurrentRenderPipeline()` |
| Scene queries | `FindGameObjectsWithLayerMask()` |
| Bounds/collider helpers | `GetObjectPhysicsBounds()`, `GetObjectBounds()`, `GetUIBounds()`, `CheckPointInCollider()` |
| Safe destruction | `Destroy()` overloads |

#### Interval family

The interval types cover 1D, 2D, and 3D ranges and support clamping, containment checks, interpolation, and arithmetic operators.

| Type | Notes |
| --- | --- |
| `Utility.Interval` | Float range with `Lerp()`, `InverseLerp()`, `InRange()`, and arithmetic operators. |
| `Utility.SimpleInterval` | Minimal two-value float interval. |
| `Utility.IntervalInt` | Integer interval. |
| `Utility.Interval2` / `Utility.SimpleInterval2` | Two-axis interval variants. |
| `Utility.Interval3` / `Utility.SimpleInterval3` | Three-axis interval variants. |

### `Bezier`

Spline utilities for curve evaluation and editable paths.

| API | Notes |
| --- | --- |
| `Bezier.Path` | Editable path type with segment management, point sampling, normal generation, and mesh creation. |
| `Vector3 EvaluateLinear(Vector3 a, Vector3 b, float t)` | Evaluate a linear bezier segment. |
| `Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t)` | Evaluate a quadratic bezier segment. |
| `Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)` | Evaluate a cubic bezier segment. |

Useful `Bezier.Path` members include:

- `AddSegment()`
- `SplitSegment()`
- `RemoveSegment()`
- `EnableSegment()` / `DisableSegment()`
- `GetSpacedPoints()`
- `GetSegmentPoints()`
- `ClosestSegment()`
- `ClosestAnchorPoint()`
- `GetAnchorPoint()` / `SetAnchorPoint()`
- `CreateMesh()`

### `DataSerializationUtility<T>`

JSON file and `Resources` serialization helper for reference types, using `JsonUtility`. Legacy BinaryFormatter files are migrated to JSON in the Unity Editor.

| API | Notes |
| --- | --- |
| `DataSerializationUtility(string path, bool loadFromResources, bool bypassExceptions = false)` | Configure storage path and failure behavior. |
| `bool SaveOrCreate(T data)` | Save data to the configured path. |
| `T Load()` | Load and deserialize data. |
| `bool Delete()` | Delete a file-backed save. |

## `Utilities.Core`

### `BehaviourSingleton<T>`

Scene-based singleton base class.

| Member | Notes |
| --- | --- |
| `T Default` | Finds the active singleton instance and returns it. |
| `bool Persistent` | Override to keep the instance across scene loads. |
| `bool ToBeDestroyed` | Indicates whether a duplicate instance was marked for destruction. |

### `ScriptableSingleton<T>`

`ScriptableObject` singleton loaded from `Resources/Settings/<TypeName>`.

| Member | Notes |
| --- | --- |
| `T Default` | Loads and caches the singleton asset. |

### `CoreUtility`

| API | Notes |
| --- | --- |
| `bool TryGetTypeFromString(string typeName, out Type type)` | Resolve a type by searching loaded assemblies. |

### `GameLogger`

| API | Notes |
| --- | --- |
| `Log()` | Verbose runtime log. |
| `LogWarning()` | Verbose warning log. |
| `LogError()` | Error log, always emitted. |
| `LogException()` | Exception passthrough. |

### `SerializedDictionary<TKey, TValue>`

Serializable dictionary wrapper.

| API | Notes |
| --- | --- |
| `Dictionary<TKey, TValue> ToDictionary()` | Convert to a normal dictionary. |
| `void FromDictionary(Dictionary<TKey, TValue> dictionary)` | Populate from a normal dictionary. |
| `SerializedDictionary<TKey, TValue> Create(Dictionary<TKey, TValue> dictionary)` | Create a wrapper from an existing dictionary. |

## `Utilities.Core.Managed`

### `ResourcesReference<T>`

Serializable `Resources` path reference.

| Member | Notes |
| --- | --- |
| `string path` | Resource path relative to the `Resources` folder. |
| `bool IsEmpty` | Whether the reference is blank. |
| `T Load()` | Load the referenced object from `Resources`. |

### `SceneAssetReference`

Serializable scene reference with editor drawer support.

| Member | Notes |
| --- | --- |
| `string path` | Scene asset path. |
| `string guid` | Serialized GUID backing the scene reference. |
| `string Name` | Scene file name without extension. |
| `bool IsEmpty` | Whether the reference is blank. |

## `Utilities.Editor`

### `EditorUtilities`

Editor helper surface for styles, icons, defines, and debug menu actions.

#### Styles

- `Styles.Button`
- `Styles.ButtonActive`
- `Styles.MiniButton`
- `Styles.MiniButtonActive`
- `Styles.MiniButtonLeft`
- `Styles.MiniButtonMiddle`
- `Styles.MiniButtonRight`

#### Icons

The `Icons` nested class exposes editor textures such as add, check, warning, save, reload, pencil, sort, and trash icons, with automatic Personal/Pro skin variants.

#### Build define helpers

| API | Notes |
| --- | --- |
| `AddScriptingDefineSymbol(string symbol)` | Add a define symbol. |
| `RemoveScriptingDefineSymbol(string symbol)` | Remove a define by name. |
| `RemoveScriptingDefineSymbol(int symbolIndex)` | Remove a define by index. |
| `ScriptingDefineSymbolExists(string symbol)` | Check for a define. |
| `GetScriptingDefineSymbols()` | Read defines for the current target. |
| `GetCurrentNamedBuildTarget()` | Read the current named build target. |
| `GetCurrentBuildTargetGroup()` | Read the current build target group. |
| `GetCurrentBuildTarget()` | Read the current build target. |

#### Menu-driven debug helpers

- `DebugBounds()` / `DebugBoundsCheck()`
- `DebugMesh()` / `DebugMeshCheck()`
- `PlaceObjectOnSurface()` / `PlaceObjectOnSurfaceCheck()`
- `Texture2DArrayExport()` / `Texture2DArrayExportCheck()`

### `LayersManager`

Layer management helpers for the Unity `TagManager`.

| API | Notes |
| --- | --- |
| `AddLayer(string name)` | Add a layer to the first free user slot. |
| `RemoveLayer(int layerIndex)` | Remove a layer by index. |
| `RemoveLayer(string name)` | Remove a layer by name. |
| `RenameLayer(int layerIndex, string name)` | Rename a layer by index. |
| `RenameLayer(string currentName, string newName)` | Rename a layer by name. |
| `IsLayerEmpty(int layerIndex)` | Check whether a slot is unused. |
| `LayerExists(string name)` | Check whether a layer exists. |
| `GetLayers()` | Read the project layer table. |

### `ScriptableObjectUtility`

| API | Notes |
| --- | --- |
| `CreateAsset<T>(string path)` | Create a `ScriptableObject` asset. |
| `CreateAsset<T>(string path, T data)` | Create and initialize an asset. |

### Editor windows

- `UnitsConverterEditor`
- `WheelRadiusCalculatorEditor`

These windows are opened from the `Tools/Utilities` menu.

## `Utilities.Extensions`

The `Utilities.Extensions` runtime assembly currently exposes helpers in the `Utilities` namespace.

| Type | Notes |
| --- | --- |
| `GameObjectExtensions` | `GetObjectBounds(GameObject, LayerMask, bool, bool)` |
| `LayerMaskExtensions` | `HasLayer(LayerMask, int)` |
| `UnityMathematicsExtensions` | `AABB` helpers such as `Encapsulate()` and `SetMinMax()` |
| `CollectionsExtensions` | `DynamicBuffer<T>.AsNativeList()` convenience helper for ECS projects |