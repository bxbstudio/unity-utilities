#region Namespaces

using System;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

#endregion

namespace Utilities.Core.Managed.Editor
{
    [CustomPropertyDrawer(typeof(ResourcesReference<>))]
    internal class ResourcesReferenceDrawer : PropertyDrawer
    {
		private static readonly char[] pathSeparators = new char[] { '\\', '/' };

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
			EditorGUI.BeginProperty(position, label, property);

			var pathProperty = property.FindPropertyRelative(nameof(ResourcesReference<Object>.path));
			var referenceType = property.GetUnderlyingType().GenericTypeArguments[0];
			var resource = Resources.Load(pathProperty.stringValue, referenceType);
			var newResource = EditorGUI.ObjectField(position, label, resource, referenceType, false);

			if (!newResource)
			{
				// Handle missing resource
				pathProperty.stringValue = string.Empty;
			}
			else if (resource != newResource)
			{
				string fullPath = AssetDatabase.GetAssetPath(newResource);
				string[] pathParts = fullPath.Split(pathSeparators, StringSplitOptions.RemoveEmptyEntries);
				int index = Array.IndexOf(pathParts, "Resources");

				if (index < 0)
					GameLogger.LogWarning($"The object {newResource.name} should be in the Resources folder!", "Resources Reference Property", property.serializedObject.targetObject);
				else
				{
					string newPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathParts.Skip(index + 1));

					pathProperty.stringValue = Path.Combine(Path.GetDirectoryName(newPath), Path.GetFileNameWithoutExtension(newPath));
				}
			}

			EditorGUI.EndProperty();
        }
    }
}
