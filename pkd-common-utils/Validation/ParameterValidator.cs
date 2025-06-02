namespace pkd_common_utils.Validation
{
	/// <summary>
	/// Helper class for checking method arguments.
	/// </summary>
	public static class ParameterValidator
	{
		/// <summary>
		/// Throws an exception if the param is null.
		/// </summary>
		/// <param name="param">The object being evaluated for null.</param>
		/// <param name="methodName"></param>
		/// <param name="paramName">The name of the parameter being checked.</param>
		/// <exception cref="ArgumentNullException">If 'param' is null.</exception>
		public static void ThrowIfNull(object? param, string methodName, string paramName)
		{
			if (param == null)
			{
				throw new ArgumentNullException($"{methodName}() - {paramName} cannot be null.");
			}
		}

		/// <summary>
		/// Throws an exception if the string parameter is null or empty.
		/// </summary>
		/// <param name="param">The object to evaluate.</param>
		/// <param name="methodName">Name of the method running the check.</param>
		/// <param name="paramName">Name of the parameter being cheecked.</param>
		/// <exception cref="ArgumentException">if 'param' is null or empty.</exception>
		public static void ThrowIfNullOrEmpty(string param, string methodName, string paramName)
		{
			if (string.IsNullOrEmpty(param))
			{
				throw new ArgumentException($"{methodName}() - {paramName} cannot be null or empty.");
			}
		}
	}
}
