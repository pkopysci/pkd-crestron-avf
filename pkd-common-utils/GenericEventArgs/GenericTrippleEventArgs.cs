namespace pkd_common_utils.GenericEventArgs
{
	using System;

	public class GenericTrippleEventArgs<T1, T2, T3> : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GenericTrippleEventArgs{T1, T2, T3}"/> class.
		/// </summary>
		/// <param name="arg1">the first data object supplied when the event was thrown.</param>
		/// <param name="arg2">the second data object supplied when the event was thrown.</param>
		/// <param name="arg3">the thrid data object supplied when the event was thrown.</param>
		public GenericTrippleEventArgs(T1 arg1, T2 arg2, T3 arg3)
		{
			this.Arg1 = arg1;
			this.Arg2 = arg2;
			this.Arg3 = arg3;
		}

		/// <summary>
		/// Gets the first data object supplied when the event was thrown.
		/// </summary>
		public T1 Arg1 { get; private set; }

		/// <summary>
		/// Gets the second data object supplied when the event was thrown.
		/// </summary>
		public T2 Arg2 { get; private set; }

		/// <summary>
		/// Gets the Third data object supplied when the event was thrown.
		/// </summary>
		public T3 Arg3 { get; private set; }
	}
}
