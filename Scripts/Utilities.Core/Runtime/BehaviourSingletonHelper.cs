#region Namespaces

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

using Object = UnityEngine.Object;

#endregion

[assembly: AlwaysLinkAssembly]

namespace Utilities.Core
{
	/// <summary>
	/// Helper static class for BehaviourSingleton related utilities.
	/// </summary>
	[Preserve]
	internal static class BehaviourSingletonHelper
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void Initialize()
		{
			var behaviourSingletonType = typeof(BehaviourSingleton<>);

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				foreach (var type in assembly.GetTypes())
				{
					if (type.IsAbstract || !type.IsClass || type.IsGenericType)
						continue;

					// Check if type inherits from BehaviourSingleton<T>
					var baseType = type.BaseType;

					while (baseType != null)
					{
						if (!baseType.IsGenericType || baseType.GetGenericTypeDefinition() != behaviourSingletonType)
							goto CONTINUE_SEARCH;

						// Check for a static property 'Default'
						var defaultProp = baseType.GetProperty("Default", BindingFlags.Public | BindingFlags.Static);

						// Access the Default property to trigger instance creation
						var instance = defaultProp.GetValue(null) as MonoBehaviour;

						if (instance)
							goto CONTINUE_SEARCH;

						// If not found in scene, create a new GameObject and add the component
						GameObject gameObject = new GameObject($"[Singleton] {type.Name}");

						instance = gameObject.AddComponent(type) as MonoBehaviour;

						// Check for 'Persistent' property and call DontDestroyOnLoad if true
						var persistentProp = type.GetProperty("Persistent", BindingFlags.Public | BindingFlags.Instance);

						if (instance)
						{
							var persistent = (bool)persistentProp.GetValue(instance);

							if (persistent)
								Object.DontDestroyOnLoad(instance.gameObject);
						}

						break;

					CONTINUE_SEARCH:
						baseType = baseType.BaseType;
					}
				}
		}
	}
}
