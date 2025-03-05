using System.Globalization;

namespace pkd_common_utils.Validation
{
	/// <summary>
	/// Helper class to standardize and check config arguments.
	/// </summary>
	public static class DataFormatter
	{
		/// <summary>
		/// Removes all leading and trailing white spaces and removes any hyphens.
		/// </summary>
		/// <param name="arg">The config argument to convert</param>
		/// <returns>A new string with all whitespaces and hyphens removed.</returns>
		public static string NormalizeDeviceModel(string arg)
		{
			return string.IsNullOrEmpty(arg) ? string.Empty : arg.Trim().ToUpper(CultureInfo.InvariantCulture).Replace("-", string.Empty);
		}
	}
}
