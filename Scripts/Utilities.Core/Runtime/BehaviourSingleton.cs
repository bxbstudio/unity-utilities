#region Namespaces

using UnityEngine;

#endregion

namespace Utilities.Core
{
	/// <summary>
	/// Base class for creating behaviour singletons that can be found in the scene.
	/// This class provides a convenient way to access global settings and configurations.
	/// </summary>
	/// <typeparam name="T">The type of the behaviour singleton, must inherit from BehaviourSingleton</typeparam>
	public abstract class BehaviourSingleton<T> : MonoBehaviour where T : BehaviourSingleton<T>
	{
		#region Variables

		/// <summary>
		/// Gets the singleton instance of the behaviour.
		/// If the instance doesn't exist, it will be found in the scene.
		/// </summary>
		public static T Default
		{
			get
			{
				if (!instance || !instance.isActiveAndEnabled)
#if UNITY_2022_2_OR_NEWER
					instance = FindAnyObjectByType<T>();
#else
					instance = FindObjectOfType<T>();
#endif

				return instance;
			}
		}

		/// <summary>
		/// The cached instance of the behaviour singleton.
		/// </summary>
		private static T instance;

		/// <summary>
		/// Whether the singleton should persist between scene changes.
		/// </summary>
		public abstract bool Persistent { get; }

		/// <summary>
		/// Whether the singleton should be destroyed at Awake.
		/// </summary>
		public bool ToBeDestroyed { get; private set; }

		#endregion

		#region Unity Lifecycle

		protected virtual void Awake()
		{
			if (Default != this)
			{
				if (instance && !isActiveAndEnabled)
					GameLogger.LogWarning($"Singleton {typeof(T).Name} is not active and enabled at Awake. The game object has been destroyed.");

				ToBeDestroyed = true;

				Destroy(gameObject);

				return;
			}

			if (Persistent)
				DontDestroyOnLoad(gameObject);
		}

		#endregion
	}
}
