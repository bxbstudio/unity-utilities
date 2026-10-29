#region Namespaces

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
#if !UNITY_2021_2_OR_NEWER
using System.Runtime.Serialization;
#endif
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
#endif

#endregion

[assembly: InternalsVisibleTo("Utilities.Editor")]

namespace Utilities
{
	/// <summary>
	/// Utility class for serializing and deserializing data to and from files.
	/// Provides methods for saving, loading, and deleting serialized data with support for both regular file paths and Unity Resources.
	/// Data is stored as UTF-8 JSON using <see cref="JsonUtility"/>, so only public or [SerializeField] fields of [Serializable] types are persisted
	/// (no root arrays, dictionaries, polymorphism or null class references).
	/// Legacy BinaryFormatter files are migrated to JSON inside the Unity Editor only; player builds can only read JSON files.
	/// </summary>
	/// <typeparam name="T">The type of data to serialize and deserialize. Must be a reference type.</typeparam>
	public class DataSerializationUtility<T> where T : class
	{
		#region Variables

		/// <summary>
		/// The path to save and load the data.
		/// For regular file operations, this is a file system path.
		/// For Resources, this is the path within the Resources folder without the extension.
		/// </summary>
		private readonly string path;

		/// <summary>
		/// Whether to load the data from Unity's Resources system instead of the file system.
		/// When true, data is loaded from Resources using the path as a resource path.
		/// When false, data is loaded directly from the file system.
		/// </summary>
		private readonly bool useResources;

		/// <summary>
		/// Whether to bypass exceptions by returning default values instead of throwing errors.
		/// When true, methods will return null or false on failure instead of throwing exceptions.
		/// When false, exceptions will be propagated to the caller.
		/// </summary>
		private readonly bool bypassExceptions;

		#endregion

		#region Methods

		#region Utilities

		/// <summary>
		/// Saves data as JSON to a file at the specified path, replacing any existing content.
		/// Creates the directory structure if it doesn't exist.
		/// Appends ".bytes" extension when saving for Resources.
		/// </summary>
		/// <param name="data">The data object to serialize and save.</param>
		/// <returns>True if the data was saved successfully, false if an error occurred and exceptions are bypassed.</returns>
		/// <exception cref="Exception">Thrown when an error occurs during saving and bypassExceptions is false.</exception>
		public bool SaveOrCreate(T data)
		{
			CheckValidity();

			try
			{
				if (!Directory.Exists(Path.GetDirectoryName(path)))
					Directory.CreateDirectory(Path.GetDirectoryName(path));

				DataSerializationFormat.WriteJson($"{path}{(useResources ? ".bytes" : "")}", JsonUtility.ToJson(data, true));

				return true;
			}
			catch (Exception e)
			{
				if (!bypassExceptions)
					throw e;
				else
					return false;
			}
		}

		/// <summary>
		/// Loads and deserializes data from a file at the specified path.
		/// For Resources, loads from a TextAsset. For regular files, loads directly from the file system.
		/// In the Unity Editor, legacy BinaryFormatter files are migrated to JSON before being returned.
		/// </summary>
		/// <returns>The deserialized data object, or null if the file doesn't exist or an error occurred and exceptions are bypassed.</returns>
		/// <exception cref="ArgumentException">Thrown when the file doesn't exist and bypassExceptions is false.</exception>
		/// <exception cref="InvalidDataException">Thrown when the file isn't valid JSON data (or is legacy data in a player build) and bypassExceptions is false.</exception>
		/// <exception cref="Exception">Thrown when an error occurs during loading and bypassExceptions is false.</exception>
		public T Load()
		{
			CheckValidity();

			TextAsset textAsset = useResources ? Resources.Load<TextAsset>(path) : null;

			if (useResources && !textAsset || !useResources && !File.Exists(path))
			{
				if (bypassExceptions)
					return null;
				else
					throw new ArgumentException($"The file ({path}) doesn't exist");
			}

			try
			{
				byte[] bytes = useResources ? textAsset.bytes : File.ReadAllBytes(path);

				if (DataSerializationFormat.IsLegacy(bytes))
					return LoadLegacy(bytes, textAsset);

				if (!DataSerializationFormat.IsJson(bytes))
					throw new InvalidDataException($"The file ({path}) doesn't contain valid data");

				return DataSerializationFormat.FromJson<T>(DataSerializationFormat.DecodeJson(bytes));
			}
			catch (Exception e)
			{
				if (bypassExceptions)
					return null;
				else
					throw e;
			}
		}

		/// <summary>
		/// Deletes the file at the specified path and its associated .meta file if it exists.
		/// Cannot delete files from Resources as they are compiled into the application.
		/// </summary>
		/// <returns>True if the file was deleted successfully, false if the file doesn't exist, is a resource, or an error occurred and exceptions are bypassed.</returns>
		/// <exception cref="FileNotFoundException">Thrown when the file doesn't exist and bypassExceptions is false.</exception>
		/// <exception cref="Exception">Thrown when an error occurs during deletion and bypassExceptions is false.</exception>
		public bool Delete()
		{
			CheckValidity();

			if (useResources)
			{
				Debug.LogError("You can't delete a resource file, use normal path finding instead!");

				return false;
			}

			if (!File.Exists(path))
			{
				if (!bypassExceptions)
					throw new FileNotFoundException($"We couldn't delete ({path}), as it doesn't exist!");

				return false;
			}

			try
			{
				File.Delete(path);

				string metaFilePath = $"{path}.meta";

				if (File.Exists(metaFilePath))
					File.Delete(metaFilePath);
			}
			catch (Exception e)
			{
				if (!bypassExceptions)
					throw e;

				return false;
			}

			return true;
		}

		/// <summary>
		/// Handles a legacy BinaryFormatter file.
		/// In the Unity Editor, the file is migrated to JSON and the migrated data is returned; if the migration isn't possible, the legacy data is returned as is.
		/// In player builds, legacy files can't be read.
		/// </summary>
		/// <param name="bytes">The legacy file content.</param>
		/// <param name="textAsset">The loaded resource, or null for regular files.</param>
		/// <returns>The deserialized data object.</returns>
		/// <exception cref="InvalidDataException">Thrown in player builds.</exception>
		private T LoadLegacy(byte[] bytes, TextAsset textAsset)
		{
#if UNITY_EDITOR
			string filePath = textAsset ? Path.GetFullPath(AssetDatabase.GetAssetPath(textAsset)) : path;

			if (DataSerializationFormat.TryMigrateLegacyFile(filePath, out string json, out string error))
			{
				if (textAsset)
				{
					string assetPath = AssetDatabase.GetAssetPath(textAsset);

					EditorApplication.delayCall += () => AssetDatabase.ImportAsset(assetPath);
				}

				Debug.Log($"<b>Utilities:</b> Migrated legacy BinaryFormatter data to JSON ({filePath})");

				return DataSerializationFormat.FromJson<T>(json);
			}

			Debug.LogWarning($"<b>Utilities:</b> Unable to migrate legacy BinaryFormatter data ({filePath}): {error}\r\nPlayer builds won't be able to read this file.");

			return DataSerializationFormat.DeserializeLegacy(bytes) as T;
#else
			throw new InvalidDataException($"The file ({path}) contains legacy BinaryFormatter data, open the project in the Unity Editor to migrate it");
#endif
		}

		/// <summary>
		/// Validates the path and ensures the directory structure exists.
		/// For non-resource paths, creates the directory if it doesn't exist and verifies it's a valid directory.
		/// For resource paths, no validation is performed as Resources are read-only.
		/// </summary>
		/// <exception cref="DirectoryNotFoundException">Thrown when the path is not a valid directory.</exception>
		private void CheckValidity()
		{
			if (useResources)
				return;

			if (!Directory.Exists(Path.GetDirectoryName(path)))
				Directory.CreateDirectory(Path.GetDirectoryName(path));

			FileAttributes fileAttributes = File.GetAttributes(Path.GetDirectoryName(path));

			if (!fileAttributes.HasFlag(FileAttributes.Directory))
				throw new DirectoryNotFoundException($"The `path` argument of value \"{Path.GetDirectoryName(path)}\" must be a valid directory");
		}

		#endregion

		#region Constructors & Operators

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the DataSerializationUtility class with the specified parameters.
		/// Validates the path upon initialization to ensure it's ready for operations.
		/// </summary>
		/// <param name="path">The path to save and load the data. For Resources, this is the path within the Resources folder without extension.</param>
		/// <param name="loadFromResources">Whether to load the data from Unity's Resources system instead of the file system.</param>
		/// <param name="bypassExceptions">Whether to bypass exceptions by returning default values instead of throwing errors. Defaults to false.</param>
		public DataSerializationUtility(string path, bool loadFromResources, bool bypassExceptions = false)
		{
			this.path = path;
			useResources = loadFromResources;
			this.bypassExceptions = bypassExceptions;

			CheckValidity();
		}

		#endregion

		#region Operators

		/// <summary>
		/// Implicitly converts a DataSerializationUtility<T> instance to a boolean value.
		/// Allows for null-checking syntax like: if(serializationUtility) { ... }
		/// </summary>
		/// <param name="serializationUtility">The DataSerializationUtility<T> instance to convert.</param>
		/// <returns>True if the DataSerializationUtility<T> instance is not null, false otherwise.</returns>
		public static implicit operator bool(DataSerializationUtility<T> serializationUtility) => serializationUtility != null;

		#endregion

		#endregion

		#endregion
	}

	/// <summary>
	/// Internal helpers for the data file format used by <see cref="DataSerializationUtility{T}"/>.
	/// Handles format detection, JSON reading and writing and, in the Unity Editor only, legacy BinaryFormatter migration.
	/// </summary>
	internal static class DataSerializationFormat
	{
		#region Variables

		/// <summary>
		/// The header every BinaryFormatter (NRBF) stream starts with: a SerializationHeaderRecord with a root ID of 1 and a header ID of -1.
		/// </summary>
		private static readonly byte[] legacyHeader = { 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF };

		/// <summary>
		/// UTF-8 encoding without a byte order mark, used to write JSON files.
		/// </summary>
		private static readonly UTF8Encoding utf8 = new UTF8Encoding(false);

		/// <summary>
		/// The number of bytes needed to detect a legacy file.
		/// </summary>
		internal static int LegacyHeaderLength => legacyHeader.Length;

		#endregion

		#region Methods

		/// <summary>
		/// Checks whether the given bytes start with the BinaryFormatter (NRBF) header.
		/// </summary>
		/// <param name="bytes">The file content, or at least its first bytes.</param>
		/// <returns>True if the content is legacy BinaryFormatter data.</returns>
		internal static bool IsLegacy(byte[] bytes)
		{
			if (bytes == null || bytes.Length < legacyHeader.Length)
				return false;

			for (int i = 0; i < legacyHeader.Length; i++)
				if (bytes[i] != legacyHeader[i])
					return false;

			return true;
		}

		/// <summary>
		/// Checks whether the given bytes contain a JSON object, ignoring a UTF-8 byte order mark and leading whitespace.
		/// </summary>
		/// <param name="bytes">The file content.</param>
		/// <returns>True if the content starts with a JSON object.</returns>
		internal static bool IsJson(byte[] bytes)
		{
			if (bytes == null)
				return false;

			int start = HasByteOrderMark(bytes) ? 3 : 0;

			for (int i = start; i < bytes.Length; i++)
			{
				byte b = bytes[i];

				if (b == ' ' || b == '\t' || b == '\r' || b == '\n')
					continue;

				return b == '{';
			}

			return false;
		}

		/// <summary>
		/// Decodes UTF-8 JSON bytes into a string, skipping a byte order mark if present.
		/// </summary>
		/// <param name="bytes">The file content.</param>
		/// <returns>The JSON string.</returns>
		internal static string DecodeJson(byte[] bytes)
		{
			int start = HasByteOrderMark(bytes) ? 3 : 0;

			return utf8.GetString(bytes, start, bytes.Length - start);
		}

		/// <summary>
		/// Writes a JSON string to a file as UTF-8 without a byte order mark, replacing any existing content.
		/// </summary>
		/// <param name="filePath">The file to write.</param>
		/// <param name="json">The JSON string.</param>
		internal static void WriteJson(string filePath, string json)
		{
			File.WriteAllBytes(filePath, utf8.GetBytes(json));
		}

		/// <summary>
		/// Deserializes JSON into a new object without running its constructors, matching the legacy BinaryFormatter behaviour.
		/// This keeps data types whose constructors have side effects (such as reading global state) safe to load.
		/// </summary>
		/// <typeparam name="T">The type to deserialize.</typeparam>
		/// <param name="json">The JSON string.</param>
		/// <returns>The deserialized object.</returns>
		internal static T FromJson<T>(string json) where T : class
		{
			return FromJson(json, typeof(T)) as T;
		}

		/// <summary>
		/// Deserializes JSON into a new object of the given type without running its constructors.
		/// ScriptableObject types are created with <see cref="ScriptableObject.CreateInstance(Type)"/>.
		/// </summary>
		/// <param name="json">The JSON string.</param>
		/// <param name="type">The type to deserialize.</param>
		/// <returns>The deserialized object.</returns>
		/// <exception cref="NotSupportedException">Thrown for UnityEngine.Object types other than ScriptableObject.</exception>
		internal static object FromJson(string json, Type type)
		{
			object data;

			if (typeof(ScriptableObject).IsAssignableFrom(type))
				data = ScriptableObject.CreateInstance(type);
			else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
				throw new NotSupportedException($"The type `{type.FullName}` can't be created from a data file");
			else
#if UNITY_2021_2_OR_NEWER
				data = RuntimeHelpers.GetUninitializedObject(type);
#else
				data = FormatterServices.GetUninitializedObject(type);
#endif

			JsonUtility.FromJsonOverwrite(json, data);

			return data;
		}

		/// <summary>
		/// Checks whether the given bytes start with a UTF-8 byte order mark.
		/// </summary>
		/// <param name="bytes">The file content.</param>
		/// <returns>True if a byte order mark is present.</returns>
		private static bool HasByteOrderMark(byte[] bytes)
		{
			return bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
		}

#if UNITY_EDITOR
		/// <summary>
		/// Deserializes legacy BinaryFormatter data. Editor only, used for migrating legacy files to JSON.
		/// </summary>
		/// <param name="bytes">The legacy file content.</param>
		/// <returns>The deserialized object.</returns>
		internal static object DeserializeLegacy(byte[] bytes)
		{
			using MemoryStream stream = new MemoryStream(bytes);

			// BinaryFormatter is deprecated (UAC0023) and removed from .NET 9, it's only kept in the Editor to read and migrate legacy files.
#pragma warning disable UAC0023
			return new BinaryFormatter().Deserialize(stream);
#pragma warning restore UAC0023
		}

		/// <summary>
		/// Migrates a legacy BinaryFormatter file to JSON in place. Editor only.
		/// The data is converted and checked with a JSON round-trip before the file is touched,
		/// then the original file is backed up to "Library/BxBStudio/LegacyData" and overwritten with JSON.
		/// Files whose data can't be represented by JsonUtility without losses are left untouched.
		/// </summary>
		/// <param name="filePath">The legacy file to migrate.</param>
		/// <param name="json">The migrated JSON content, if successful.</param>
		/// <param name="error">The reason the migration failed, if unsuccessful.</param>
		/// <returns>True if the file was migrated.</returns>
		internal static bool TryMigrateLegacyFile(string filePath, out string json, out string error)
		{
			json = null;
			error = null;

			try
			{
				byte[] bytes = File.ReadAllBytes(filePath);

				if (!IsLegacy(bytes))
				{
					error = "The file doesn't contain legacy BinaryFormatter data";

					return false;
				}

				object data = DeserializeLegacy(bytes);

				if (data == null)
				{
					error = "The file contains null data";

					return false;
				}

				Type type = data.GetType();

				error = GetJsonIncompatibility(type);

				if (error != null)
					return false;

				string newJson = JsonUtility.ToJson(data, true);
				string roundTripJson = JsonUtility.ToJson(FromJson(newJson, type), true);

				if (newJson != roundTripJson)
				{
					error = "The JSON round-trip check failed";

					return false;
				}

				BackupLegacyFile(filePath, bytes);
				WriteJson(filePath, newJson);

				json = newJson;

				return true;
			}
			catch (Exception e)
			{
				error = $"{e.GetType().Name}: {e.Message}";

				return false;
			}
		}

		/// <summary>
		/// Copies a legacy file to "Library/BxBStudio/LegacyData", mirroring its path relative to the project folder.
		/// An existing backup is never overwritten; a timestamp is appended instead.
		/// </summary>
		/// <param name="filePath">The legacy file path.</param>
		/// <param name="bytes">The legacy file content.</param>
		private static void BackupLegacyFile(string filePath, byte[] bytes)
		{
			string projectPath = Path.GetFullPath(Path.GetDirectoryName(Application.dataPath));
			string fullPath = Path.GetFullPath(filePath);
			string relativePath = fullPath.StartsWith(projectPath, StringComparison.OrdinalIgnoreCase) ? fullPath.Substring(projectPath.Length).TrimStart('\\', '/') : Path.GetFileName(fullPath);
			string backupPath = Path.Combine(projectPath, "Library", "BxBStudio", "LegacyData", relativePath);

			if (File.Exists(backupPath))
				backupPath = $"{backupPath}.{DateTime.Now:yyyyMMddHHmmss}";

			Directory.CreateDirectory(Path.GetDirectoryName(backupPath));
			File.WriteAllBytes(backupPath, bytes);
		}

		/// <summary>
		/// Checks whether every field BinaryFormatter stored for the given root type can also be stored by JsonUtility.
		/// </summary>
		/// <param name="type">The root data type.</param>
		/// <returns>The reason the type isn't compatible, or null if it is.</returns>
		private static string GetJsonIncompatibility(Type type)
		{
			if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type.IsArray || IsList(type))
				return $"The root type `{type.FullName}` must be a class or a struct";

			return GetJsonIncompatibility(type, type.Name, new HashSet<Type>());
		}

		/// <summary>
		/// Recursively checks whether a field type can be stored by JsonUtility without losing data.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <param name="fieldPath">The field path used for error messages.</param>
		/// <param name="visited">The types that have already been checked.</param>
		/// <returns>The reason the type isn't compatible, or null if it is.</returns>
		private static string GetJsonIncompatibility(Type type, string fieldPath, HashSet<Type> visited)
		{
			if (type.IsPrimitive || type.IsEnum || type == typeof(string))
				return type == typeof(IntPtr) || type == typeof(UIntPtr) ? $"`{fieldPath}` is a pointer" : null;

			if (type.IsArray || IsList(type))
			{
				Type elementType = type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];

				if (type.IsArray && type.GetArrayRank() != 1 || elementType.IsArray || IsList(elementType))
					return $"`{fieldPath}` is a multi-dimensional or nested collection";

				return GetJsonIncompatibility(elementType, $"{fieldPath}[]", visited);
			}

			if (typeof(UnityEngine.Object).IsAssignableFrom(type))
				return null;

			if (type.IsAbstract || type.IsInterface || type == typeof(object))
				return $"`{fieldPath}` is polymorphic ({type.Name})";

			if (type.IsGenericType)
				return $"`{fieldPath}` is a generic type ({type.Name})";

			if (!type.IsSerializable)
				return $"`{fieldPath}` type isn't [Serializable] ({type.FullName})";

			if (!visited.Add(type))
				return null;

			for (Type current = type; current != null && current != typeof(object) && current != typeof(ValueType); current = current.BaseType)
				foreach (FieldInfo field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
				{
					if (field.IsNotSerialized)
						continue;

					string path = $"{fieldPath}.{field.Name}";

					if (!field.IsPublic && !field.IsDefined(typeof(SerializeField), true))
						return $"`{path}` is a non-public field without [SerializeField]";

					if (field.IsInitOnly)
						return $"`{path}` is a readonly field";

					string fieldIncompatibility = GetJsonIncompatibility(field.FieldType, path, visited);

					if (fieldIncompatibility != null)
						return fieldIncompatibility;
				}

			return null;
		}

		/// <summary>
		/// Checks whether a type is a generic <see cref="List{T}"/>.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns>True if the type is a List.</returns>
		private static bool IsList(Type type)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
		}
#endif

		#endregion
	}
}
