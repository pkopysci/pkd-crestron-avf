using pkd_common_utils.GenericEventArgs;

namespace pkd_hardware_service.BaseDevice
{
	/// <summary>
	/// Base class for representing hardware controls.
	/// </summary>
	public abstract class BaseDevice : IBaseDevice
	{
		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? ConnectionChanged;

		/// <inheritdoc/>
		public string Id { get; protected set; } = string.Empty;

		/// <inheritdoc/>
		public string Label { get; protected set; } = string.Empty;

		/// <inheritdoc/>
		public virtual bool IsOnline { get; protected set; }

		/// <inheritdoc/>
		public virtual bool IsInitialized { get; protected set; }
		
		/// <inheritdoc/>
		public string Manufacturer { get; set; } = string.Empty;
		
		/// <inheritdoc/>
		public string Model { get; set; } = string.Empty;

		/// <inheritdoc/>
		public virtual void Connect() { }

		/// <inheritdoc/>
		public virtual void Disconnect() { }

		/// <summary>
		/// Method for notifying subscribers that the device online status has changed.
		/// </summary>
		protected virtual void NotifyOnlineStatus()
		{
			var temp = ConnectionChanged;
			temp?.Invoke(this, new GenericSingleEventArgs<string>(Id));
		}
	}
}
