namespace pkd_common_utils.GenericEventArgs
{
	/// <summary>
    /// Generic arguments package for sending information during application
    /// events.
    /// </summary>
    /// <typeparam name="T">The type of data being sent durring an event.</typeparam>
    /// <remarks>
    /// Initializes a new instance of the <see cref="GenericSingleEventArgs{T}"/> class.
    /// </remarks>
    /// <param name="arg">The data object associated with the triggering event.</param>
    public class GenericSingleEventArgs<T>(T arg) : EventArgs
	{

        /// <summary>
        /// Gets a value representing the data sent during the event.
        /// </summary>
        public T Arg { get; private set; } = arg;
    }
}
