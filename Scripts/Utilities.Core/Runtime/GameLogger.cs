#region Namespaces

using System;
using UnityEngine;
using Utilities;
using Object = UnityEngine.Object;

#endregion

namespace Utilities.Core
{
	/// <summary>
	/// Static utility class for logging and debugging purposes.
	/// </summary>
	public static class GameLogger
	{
		#region Properties

		/// <summary>
		/// Gets or sets whether verbose mode is enabled.
		/// </summary>
		private static readonly bool VerboseModeEnabled = Debug.isDebugBuild;

		#endregion

		#region Methods

		/// <summary>
		/// Logs an informational message to the console.
		/// </summary>
		/// <param name="message">The message to log.</param>
		/// <param name="componentName">The name of the component to log.</param>
		/// <param name="context">The context object to log.</param>
		public static void Log(string message, string componentName = null, Object context = null)
		{
			if (!VerboseModeEnabled)
				return;

			if (componentName.IsNullOrWhiteSpace())
				componentName = "Game";

			Debug.Log($"<b>{componentName}:</b> {message}", context);
		}

		/// <summary>
		/// Logs a warning message to the console.
		/// </summary>
		/// <param name="message">The warning message to log.</param>
		/// <param name="componentName">The name of the component to log.</param>
		/// <param name="context">The context object to log.</param>
		public static void LogWarning(string message, string componentName = null, Object context = null)
		{
			if (!VerboseModeEnabled)
				return;

			if (componentName.IsNullOrWhiteSpace())
				componentName = "Game";

			Debug.LogWarning($"<b>{componentName}:</b> {message}", context);
		}

		/// <summary>
		/// Logs an error message to the console.
		/// </summary>
		/// <param name="message">The error message to log.</param>
		/// <param name="componentName">The name of the component to log.</param>
		/// <param name="context">The context object to log.</param>
		public static void LogError(string message, string componentName = null, Object context = null)
		{
			if (componentName.IsNullOrWhiteSpace())
				componentName = "Game";

			Debug.LogError($"<b>{componentName}:</b> {message}", context);
		}

		/// <summary>
		/// Logs an exception to the console.
		/// </summary>
		/// <param name="exception">The exception to log.</param>
		/// <param name="context">The context object to log.</param>
		public static void LogException(Exception exception, Object context = null)
		{
			Debug.LogException(exception, context);
		}

		#endregion
	}
}
