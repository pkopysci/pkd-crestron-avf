using pkd_common_utils.GenericEventArgs;

namespace pkd_hardware_service.BaseDevice
{
	/// <summary>
	/// Interface for attributes common to all devices.
	/// </summary>
	public interface IBaseDevice
	{
		/// <summary>
		/// Notification for when the device connection has changed.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>> ConnectionChanged;

		/// <summary>
		/// Gets the unique ID of the device.
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Gets the user friendly label of the device.
		/// </summary>
		string Label { get; }

		/// <summary>
		/// Gets a value indicating whether the device is online or not.
		/// </summary>
		bool IsOnline { get; }

		/// <summary>
		/// Gets a value indicating whether or not the device has been initialized and ready to connect.
		/// </summary>
		bool IsInitialized { get; }
		
		/// <summary>
		/// The name of the company that created the device.
		/// </summary>
		string Manufacturer { get; set; }
		
		/// <summary>
		/// The specific device/hardware name used by the manufacturer.
		/// </summary>
		string Model { get; set; }

		/// <summary>
		/// Connect the communications protocol to the hardware.
		/// </summary>
		void Connect();

		/// <summary>
		/// Closes an active connection/communications protocol with the hardware.
		/// </summary>
		void Disconnect();
	}

}
