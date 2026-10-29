#region Namespaces

using System;
using Utilities;

#endregion

namespace Utilities.Core
{
	public static class CoreUtility
	{
		/// <summary>
		/// Gets a type from a string by searching through all loaded assemblies.
		/// </summary>
		/// <param name="typeName">The full name of the type to find.</param>
		/// <param name="type">The type if found, null otherwise.</param>
		/// <returns>True if the type was found, false otherwise.</returns>
		public static bool TryGetTypeFromString(string typeName, out Type type)
		{
			if (typeName.IsNullOrEmpty())
			{
				type = null;

				return false;
			}

			// First try the standard Type.GetType() for types in mscorlib
			type = Type.GetType(typeName);

			if (type != null)
				return true;

			// Search through all loaded assemblies
#if UNITY_6000_4_OR_NEWER
			foreach (var assembly in UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies())
#else
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
#endif
			{
				type = assembly.GetType(typeName);

				if (type != null)
					return true;
			}

			return false;
		}
	}
}
