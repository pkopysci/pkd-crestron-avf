namespace pkd_common_utils.GenericEventArgs
{
	using System;

	/// <summary>
	/// Generic arguments package for sending information during application which
	/// requires 2 discrete bits of data.
	/// </summary>
	/// <typeparam name="T1">The type of data being sent during an event for Arg1.</typeparam>
	/// <typeparam name="T2">The type of data being sent during an event for Arg2.</typeparam>
	public class GenericDualEventArgs<T1, T2> : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GenericDualEventArgs{T1, T2}"/> class.
		/// </summary>
		/// <param name="arg1">the first data object supplied when the event was thrown.</param>
		/// <param name="arg2">the second data object supplied when the event was thrown.</param>
		public GenericDualEventArgs(T1 arg1, T2 arg2)
		{
			this.Arg1 = arg1;
			this.Arg2 = arg2;
		}

		/// <summary>
		/// Gets the first data object supplied when the event was thrown.
		/// </summary>
		public T1 Arg1 { get; private set; }

		/// <summary>
		/// Gets the second data object supplied when the event was thrown.
		/// </summary>
		public T2 Arg2 { get; private set; }
	}
}
