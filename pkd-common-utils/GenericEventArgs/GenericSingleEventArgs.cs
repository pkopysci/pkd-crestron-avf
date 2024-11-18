namespace pkd_common_utils.GenericEventArgs
{
	using System;

	/// <summary>
	/// Generic arguments package for sending information during application
	/// events.
	/// </summary>
	/// <typeparam name="T">The type of data being sent durring an event.</typeparam>
	public class GenericSingleEventArgs<T> : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GenericSingleEventArgs{T}"/> class.
		/// </summary>
		/// <param name="arg">The data object associated with the triggering event.</param>
		public GenericSingleEventArgs(T arg)
		{
			this.Arg = arg;
		}

		/// <summary>
		/// Gets a value representing the data sent during the event.
		/// </summary>
		public T Arg { get; private set; }
	}
}
