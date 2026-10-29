#region Namespaces

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

#endregion

namespace Utilities.Editor
{
	/// <summary>
	/// Migrates legacy BinaryFormatter data files (written by older versions of <see cref="DataSerializationUtility{T}"/>) to JSON.
	/// Runs once per Editor session, before every build and on demand from the Tools menu.
	/// Original files are backed up to "Library/BxBStudio/LegacyData".
	/// </summary>
	[InitializeOnLoad]
	internal class LegacyDataMigrator : IPreprocessBuildWithReport
	{
		#region Modules

		/// <summary>
		/// The outcome of a migration pass.
		/// </summary>
		private struct MigrationResult
		{
			/// <summary>
			/// The project-relative paths of the migrated files.
			/// </summary>
			public List<string> migrated;

			/// <summary>
			/// The project-relative paths of the files that couldn't be migrated, with the reason.
			/// </summary>
			public List<KeyValuePair<string, string>> skipped;
		}

		#endregion

		#region Variables

		/// <summary>
		/// The SessionState key used to run the automatic migration once per Editor session.
		/// </summary>
		private const string SessionKey = "Utilities.LegacyDataMigrator.Ran";

		/// <summary>
		/// The file extensions that may contain data written by DataSerializationUtility.
		/// </summary>
		private static readonly string[] extensions = { ".bytes", ".data", ".dat", ".bin" };

		/// <summary>
		/// Files larger than this size in bytes are ignored.
		/// </summary>
		private const long MaxFileSize = 64L * 1024L * 1024L;

		/// <summary>
		/// The file that remembers skipped files, so each one is only reported once until it changes.
		/// </summary>
		private static string SkippedLogPath => Path.Combine(ProjectPath, "Library", "BxBStudio", "LegacyData", "Skipped.txt");

		/// <summary>
		/// The absolute project folder path.
		/// </summary>
		private static string ProjectPath => Path.GetFullPath(Path.GetDirectoryName(Application.dataPath));

		/// <summary>
		/// The callback order of the build preprocessor.
		/// </summary>
		public int callbackOrder => 0;

		#endregion

		#region Methods

		#region Menu Items

		/// <summary>
		/// Migrates every legacy data file in the project and reports all results, including previously reported skipped files.
		/// </summary>
		[MenuItem("Tools/Utilities/Migrate Legacy Data Files")]
		public static void MigrateFromMenu()
		{
			MigrationResult result = MigrateAll();

			if (result.migrated.Count == 0 && result.skipped.Count == 0)
				Debug.Log("<b>Utilities:</b> No legacy BinaryFormatter data files were found.");
			else
				LogResult(result, false);
		}

		#endregion

		#region Build Callbacks

		/// <summary>
		/// Migrates legacy data files before a build and warns about legacy files inside Resources folders, which players can't read.
		/// </summary>
		/// <param name="report">The build report.</param>
		public void OnPreprocessBuild(BuildReport report)
		{
			MigrationResult result = MigrateAll();

			LogResult(result, true);

			foreach (var skipped in result.skipped)
				if (skipped.Key.Replace('\\', '/').Split('/').Contains("Resources"))
					Debug.LogWarning($"<b>Utilities:</b> The legacy BinaryFormatter data file ({skipped.Key}) is included in the build but couldn't be migrated to JSON, players won't be able to read it.\r\nReason: {skipped.Value}");
		}

		#endregion

		#region Utilities

		/// <summary>
		/// Migrates every legacy data file found in the project's Assets folder and embedded packages.
		/// </summary>
		/// <returns>The migration result.</returns>
		private static MigrationResult MigrateAll()
		{
			MigrationResult result = new MigrationResult
			{
				migrated = new List<string>(),
				skipped = new List<KeyValuePair<string, string>>()
			};

			foreach (string filePath in FindLegacyFiles())
			{
				string relativePath = GetRelativePath(filePath);

				if (DataSerializationFormat.TryMigrateLegacyFile(filePath, out _, out string error))
					result.migrated.Add(relativePath);
				else
					result.skipped.Add(new KeyValuePair<string, string>(relativePath, error));
			}

			if (result.migrated.Count > 0)
				AssetDatabase.Refresh();

			return result;
		}

		/// <summary>
		/// Logs a migration result. Skipped files that were already reported and haven't changed since are not logged again when requested.
		/// </summary>
		/// <param name="result">The migration result.</param>
		/// <param name="onlyNewSkipped">Whether to only log skipped files that weren't reported before.</param>
		private static void LogResult(MigrationResult result, bool onlyNewSkipped)
		{
			HashSet<string> reported = LoadSkippedLog();
			List<string> skippedEntries = new List<string>();
			List<string> skippedLines = new List<string>();

			foreach (var skipped in result.skipped)
			{
				string entry = GetSkippedEntry(skipped.Key);

				skippedEntries.Add(entry);

				if (!onlyNewSkipped || !reported.Contains(entry))
					skippedLines.Add($"- {skipped.Key}: {skipped.Value}");
			}

			SaveSkippedLog(skippedEntries);

			if (result.migrated.Count > 0)
				Debug.Log($"<b>Utilities:</b> Migrated {result.migrated.Count} legacy BinaryFormatter data file(s) to JSON, the originals were backed up to \"Library/BxBStudio/LegacyData\".\r\n{string.Join("\r\n", result.migrated.Select(path => $"- {path}"))}");

			if (skippedLines.Count > 0)
				Debug.LogWarning($"<b>Utilities:</b> {skippedLines.Count} legacy BinaryFormatter data file(s) couldn't be migrated to JSON and were left untouched.\r\n{string.Join("\r\n", skippedLines)}");
		}

		/// <summary>
		/// Finds every file in the Assets folder and embedded packages that starts with the BinaryFormatter header.
		/// </summary>
		/// <returns>The absolute paths of the legacy files.</returns>
		private static IEnumerable<string> FindLegacyFiles()
		{
			List<string> files = new List<string>();
			byte[] header = new byte[DataSerializationFormat.LegacyHeaderLength];

			foreach (string root in new[] { Application.dataPath, Path.Combine(ProjectPath, "Packages") })
				foreach (string filePath in EnumerateFiles(root))
				{
					if (!extensions.Contains(Path.GetExtension(filePath).ToLowerInvariant()))
						continue;

					try
					{
						FileInfo info = new FileInfo(filePath);

						if (info.Length < header.Length || info.Length > MaxFileSize)
							continue;

						using (FileStream stream = info.OpenRead())
							if (stream.Read(header, 0, header.Length) != header.Length)
								continue;

						if (DataSerializationFormat.IsLegacy(header))
							files.Add(filePath);
					}
					catch (Exception)
					{
						continue;
					}
				}

			return files;
		}

		/// <summary>
		/// Recursively enumerates the files of a folder, skipping hidden folders (such as ".git").
		/// </summary>
		/// <param name="folder">The folder to enumerate.</param>
		/// <returns>The absolute file paths.</returns>
		private static IEnumerable<string> EnumerateFiles(string folder)
		{
			if (!Directory.Exists(folder))
				yield break;

			Stack<string> folders = new Stack<string>();

			folders.Push(folder);

			while (folders.Count > 0)
			{
				string current = folders.Pop();
				string[] currentFiles;
				string[] currentFolders;

				try
				{
					currentFiles = Directory.GetFiles(current);
					currentFolders = Directory.GetDirectories(current);
				}
				catch (Exception)
				{
					continue;
				}

				foreach (string filePath in currentFiles)
					yield return filePath;

				foreach (string subFolder in currentFolders)
					if (!Path.GetFileName(subFolder).StartsWith("."))
						folders.Push(subFolder);
			}
		}

		/// <summary>
		/// Gets a file path relative to the project folder.
		/// </summary>
		/// <param name="filePath">The absolute file path.</param>
		/// <returns>The relative file path with forward slashes.</returns>
		private static string GetRelativePath(string filePath)
		{
			string fullPath = Path.GetFullPath(filePath);

			if (fullPath.StartsWith(ProjectPath, StringComparison.OrdinalIgnoreCase))
				fullPath = fullPath.Substring(ProjectPath.Length).TrimStart('\\', '/');

			return fullPath.Replace('\\', '/');
		}

		/// <summary>
		/// Builds the entry identifying a skipped file and its current state.
		/// </summary>
		/// <param name="relativePath">The project-relative file path.</param>
		/// <returns>The skipped file entry.</returns>
		private static string GetSkippedEntry(string relativePath)
		{
			FileInfo info = new FileInfo(Path.Combine(ProjectPath, relativePath));

			return $"{relativePath}|{info.Length}|{info.LastWriteTimeUtc.Ticks}";
		}

		/// <summary>
		/// Loads the entries of the skipped files that were already reported.
		/// </summary>
		/// <returns>The skipped file entries.</returns>
		private static HashSet<string> LoadSkippedLog()
		{
			try
			{
				return File.Exists(SkippedLogPath) ? new HashSet<string>(File.ReadAllLines(SkippedLogPath)) : new HashSet<string>();
			}
			catch (Exception)
			{
				return new HashSet<string>();
			}
		}

		/// <summary>
		/// Saves the entries of the currently skipped files.
		/// </summary>
		/// <param name="entries">The skipped file entries.</param>
		private static void SaveSkippedLog(List<string> entries)
		{
			try
			{
				if (entries.Count == 0)
				{
					if (File.Exists(SkippedLogPath))
						File.Delete(SkippedLogPath);

					return;
				}

				Directory.CreateDirectory(Path.GetDirectoryName(SkippedLogPath));
				File.WriteAllLines(SkippedLogPath, entries);
			}
			catch (Exception)
			{
				// The log only prevents repeated warnings, failing to write it is harmless.
			}
		}

		/// <summary>
		/// Runs the automatic migration once the Editor finished loading.
		/// </summary>
		private static void MigrateOnLoad()
		{
			if (SessionState.GetBool(SessionKey, false) || EditorApplication.isPlayingOrWillChangePlaymode)
				return;

			SessionState.SetBool(SessionKey, true);
			LogResult(MigrateAll(), true);
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Schedules the automatic migration after the Editor loads.
		/// </summary>
		static LegacyDataMigrator()
		{
			EditorApplication.delayCall += MigrateOnLoad;
		}

		#endregion

		#endregion
	}
}
