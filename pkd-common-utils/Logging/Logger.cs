using System.Text;
using Crestron.SimplSharp;
using pkd_common_utils.Validation;

namespace pkd_common_utils.Logging
{
	/// <summary>
	/// Logging system for writing messages to the control system error long and console output.
	/// </summary>
	public static class Logger
	{
		private static bool _debugEnabled;
		private static string _programId = "NO ID";

		/// <summary>
		/// Set the ID tag that is included in all log statements.
		/// </summary>
		/// <param name="id">A program unique ID used to identify what program slot is writing to the log.</param>
		public static void SetProgramId(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "Logger.SetProgramId", nameof(id));
			_programId = id;
		}

		/// <summary>
		/// Enables additional log statements for debugging purposes. Should only be used when debugging an issue and disabled when done.
		/// </summary>
		public static void SetDebugOn()
		{
			Info("Debug mode enabled.");
			_debugEnabled = true;
		}

		/// <summary>
		/// Disables the additional logging statements if debug mode is active.
		/// </summary>
		public static void SetDebugOff()
		{
			Info("Debug mode disabled.");
			_debugEnabled = false;
		}

		/// <summary>
		/// Writes a basic information statement to the logging system, Usually to non-permanent logs (CLI).
		/// Does nothing if 'message' is null or empty.
		/// </summary>
		/// <param name="message">The statement to write to the logging system.</param>
		public static void Info(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			string line = string.Format(
				"|{0}| {1}",
				_programId,
				message);

			Console.WriteLine(line);
		}

		/// <summary>
		/// Writes a basic information statement to the logging system, Usually to non-permanent logs (CLI).
		/// Does nothing if 'message' is null or empty.
		/// </summary>
		/// <param name="message">string format for the statement to write to the logging system.</param>
		/// <param name="args">string formatting arguments</param>
		public static void Info(string message, params object[] args)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			StringBuilder builder = new StringBuilder();
			builder.Append('|')
				.Append(_programId)
				.Append("| ")
				.Append(string.Format(message, args));

			Console.WriteLine(builder.ToString());
			builder.Remove(0, builder.Length);
		}

		/// <summary>
		/// Writes an error to the logging system, both permanent and non-permanent logs.
		/// </summary>
		/// <param name="message">The message to record in the logging system.</param>
		public static void Error(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			string line = string.Format(
				"|{0}| {1}",
				_programId,
				message);

			ErrorLog.Error(line);
			Console.WriteLine(line);
		}

		/// <summary>
		/// Writes a formatted string to the error logging system.
		/// </summary>
		/// <param name="message">The formatted string that will be logged</param>
		/// <param name="args">all string format parameters.</param>
		public static void Error(string message, params object[] args)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			var builder = new StringBuilder();
			builder.Append('|')
				.Append(_programId)
				.Append("| ")
				.Append(string.Format(message, args));

			var line = builder.ToString();
			ErrorLog.Error(line);
			Console.WriteLine(line);
		}

		/// <summary>
		/// Write an exception stack trace to the logging system.
		/// </summary>
		/// <param name="e">the .NET exception that will be logged to the system.</param>
		/// <param name="message">The custom message to include with the exception log.</param>
		public static void Error(Exception e, string message)
		{
			var line = $"|{_programId}| {message} - {e.Message}:{e.StackTrace?.Replace("\n","\n\r")}";

			ErrorLog.Error(line);
			Console.WriteLine($"|{_programId}| {message} - {e.Message} - See error log for stack trace.");
		}

		/// <summary>
		/// Write an exception stack trace to the logging system.
		/// </summary>
		/// <param name="e">The .NET exception that will be logged to the system.</param>
		/// <param name="message">The formatted string to include with the error log.</param>
		/// <param name="args">parameters for the formatted message.</param>
		public static void Error(Exception e, string message, params object[] args)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			var builder = new StringBuilder();
			builder.Append('|')
				.Append(_programId)
				.Append("| ")
				.Append(string.Format(message, args))
				.Append(" - ")
				.Append(e.Message)
				.Append(" || ")
				.Append(e.StackTrace?.Replace("\n","\n\r"));

			var line = builder.ToString();
			ErrorLog.Error(line);
			Console.WriteLine($"|{_programId}| {message} - {e.Message} - See error log for stack trace.");
		}

		/// <summary>
		/// Write a warning message to the logging system.
		/// </summary>
		/// <param name="message">The message to write to the logs.</param>
		public static void Warn(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			var line = $"|{_programId}| {message}";

			ErrorLog.Warn(line);
			Console.WriteLine(line);
		}

		/// <summary>
		/// Write a warning message to the logging system as a formatted string.
		/// </summary>
		/// <param name="message">The string format to use when writing to the log.</param>
		/// <param name="args">parameters to include in the formatted string.</param>
		public static void Warn(string message, params object[] args)
		{
			if (string.IsNullOrEmpty(message))
			{
				return;
			}

			var builder = new StringBuilder();
			builder.Append('|')
				.Append(_programId)
				.Append("| ")
				.Append(string.Format(message, args));

			var line = builder.ToString();
			ErrorLog.Warn(line);
			Console.WriteLine(line);
		}

		/// <summary>
		/// Write a debug message to the logging system. This will only display if debug mode is enabled.
		/// </summary>
		/// <param name="message">The string format to use when writing to the log.</param>
		/// <param name="args">parameters for the formatted string.</param>
		public static void Debug(string message, params object[] args)
		{
#if DEBUG
			var builder = new StringBuilder();
			builder.Append("| ").Append(_programId).Append(" DEBUG LOG | ")
				.Append(string.Format(message, args));
			
			Console.WriteLine(builder.ToString());
#else


            if (!_debugEnabled || string.IsNullOrEmpty(message))
            {
                return;
            }

            var builder = new StringBuilder();
            builder.Append("| ").Append(_programId).Append(" DEBUG LOG | ")
                .Append(string.Format(message, args));

            Console.WriteLine(builder.ToString());
#endif
		}

		/// <summary>
		/// Write a debug message to the logging system. This will only display if debug mode is enabled.
		/// </summary>
		/// <param name="message">The message to write to the debug log.</param>
		public static void Debug(string message)
		{
#if DEBUG
			Console.WriteLine($"| {_programId} DEBUG LOG | {message}");
#else
            if (!_debugEnabled || string.IsNullOrEmpty(message))
            {
                return;
            }

            Console.WriteLine($"| {_programId} DEBUG LOG | {message}");
#endif
		}

	}
}
