﻿namespace pkd_common_utils.Validation
{
	using System.Globalization;

	/// <summary>
	/// Helper class to standardize and check config arguments.
	/// </summary>
	public static class DataFormatter
	{
		/// <summary>
		/// Removes all leading & trailing white spaces and removes any hyphens.
		/// </summary>
		/// <param name="arg">The config argument to convert</param>
		/// <returns>A new string with all whitespaces and hyphens removed.</returns>
		public static string NormalizeDeviceModel(string arg)
		{
			if (string.IsNullOrEmpty(arg))
			{
				return string.Empty;
			}

			return arg.Trim().ToUpper(CultureInfo.InvariantCulture).Replace("-", string.Empty);
		}
	}
}
