using Crestron.SimplSharp;
using pkd_common_utils.Validation;
using Directory = Crestron.SimplSharp.CrestronIO.Directory;
using File = Crestron.SimplSharp.CrestronIO.File;

namespace pkd_common_utils.FileOps
{
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
		/// <returns>The same file path given as an argument but formatted correctly based on the platform.</returns>
		public static string NormalizePath(string currentPath)
		{
			ParameterValidator.ThrowIfNullOrEmpty(currentPath, "NormalizePath", nameof(currentPath));
			return currentPath.Replace("\\", "/");
		}

		/// <summary>
		/// Returns the directory path to the control systems User folder. string does not include the trailing '/'.
		/// </summary>
		/// <returns>the application directory with '/User' or 'user' appended.</returns>
		public static string GetUserFolder()
		{
			return CrestronEnvironment.DevicePlatform switch
			{
				eDevicePlatform.Server => NormalizePath($"{Directory.GetApplicationRootDirectory()}/User"),
				_ => NormalizePath($"{Directory.GetApplicationRootDirectory()}/user")
			};
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
