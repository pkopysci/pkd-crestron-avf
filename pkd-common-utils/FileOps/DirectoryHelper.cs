namespace pkd_common_utils.FileOps
{
	using Crestron.SimplSharp;
	using Crestron.SimplSharp.CrestronIO;
	using pkd_common_utils.Validation;
	using System;

	/// <summary>
	/// Provides methods for reading files into memory.
	/// </summary>
	public static class DirectoryHelper
	{
		/// <summary>
		/// Converts the directory delimiters depending on the underlying platform type.
		/// If the platform is a server then all '\' are replaced with '/', otherwise all '/' are replaced with '\'.
		/// </summary>
		/// <param name="currentPath">The relative or absolute file path to correct.</param>
		/// <returns>The same file path given as an argument but formatted correctly based on the plaform.</returns>
		public static string NormalizePath(string currentPath)
		{
			ParameterValidator.ThrowIfNullOrEmpty(currentPath, "NormalizePath", "currentPath");

			var crestronSeries = Type.GetType("Mono.Runtime") != null ? eCrestronSeries.Series4 : eCrestronSeries.Series3;
			var platform = CrestronEnvironment.DevicePlatform;
			if (crestronSeries == eCrestronSeries.Series4 || platform == eDevicePlatform.Server)
			{
				return currentPath.Replace("\\", "/");
			}
			else
			{
				return currentPath.Replace("/", "\\");
			}
		}

		/// <summary>
		/// Returns the directory path to the control systems User folder. string does not include the trailing '/'.
		/// </summary>
		/// <returns>the application directory with '/User' or 'user' appended.</returns>
		public static string GetUserFolder()
		{
			switch (CrestronEnvironment.DevicePlatform)
			{
				case eDevicePlatform.Server:
					return NormalizePath(string.Format("{0}/User", Directory.GetApplicationRootDirectory()));

				default:
					return NormalizePath(string.Format("{0}/user", Directory.GetApplicationRootDirectory()));
			}
		}

		/// <summary>
		/// Check to see if a given file exists on the control system.
		/// </summary>
		/// <param name="filepath">The full filepath to check, including file extension.</param>
		/// <returns>true of the file exists, false otherwise.</returns>
		public static bool FileExists(string filepath)
		{
			return File.Exists(filepath);
		}
	}
}
