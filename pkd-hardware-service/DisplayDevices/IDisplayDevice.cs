using pkd_common_utils.GenericEventArgs;
using pkd_hardware_service.BaseDevice;
using pkd_hardware_service.PowerControl;

namespace pkd_hardware_service.DisplayDevices
{
	/// <summary>
	/// Common attributes and methods of all video display devices.
	/// </summary>
	public interface IDisplayDevice : IBaseDevice, IVideoControllable, IPowerControllable
	{
        // /// <summary>
        // /// Event triggered whenever the power status change is reported by the display.
        // /// </summary>
        // event EventHandler<GenericSingleEventArgs<string>>? PowerChanged;

		/// <summary>
		/// Event triggered when a "lamp hours" update is reported by the display driver.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? HoursUsedChanged;

		// /// <summary>
		// /// Gets a value indicating whether the device is on or off.
		// /// true = power on, false = power off.
		// /// </summary>
		// bool PowerState { get; }

		/// <summary>
		/// Gets a value indicating whether the specific display supports video freeze.
		/// true = is supported, false = not supported.
		/// </summary>
		bool SupportsFreeze { get; }

		/// <summary>
		/// Gets the number of hours used as of the last lamp hours response from the driver.
		/// </summary>
		uint HoursUsed { get; }

		/// <summary>
		/// Gets or sets a value that indicates whether the object should
		/// try to reconnect if disconnected from the hardware for any reason.
		/// </summary>
		bool EnableReconnect { get; set; }

		// /// <summary>
		// /// Send the "Power on" command to the display hardware.
		// /// </summary>
		// void PowerOn();
		//
		// /// <summary>
		// /// Send the "Power off" command to the display hardware.
		// /// </summary>
		// void PowerOff();

		/// <summary>
		/// Allow the display device to poll for current status based on a device-specific interval.
		/// </summary>
		void EnablePolling();

		/// <summary>
		/// Disable the polling functions if they are enabled.
		/// </summary>
		void DisablePolling();

		/// <summary>
		/// Configure the underlying connection of the display.
		/// </summary>
		/// <param name="ipAddress">The IP address or hostname to connect to.</param>
		/// <param name="port">The port number used to connect to the device.</param>
		/// <param name="label">The user-friendly name of the display.</param>
		/// <param name="id">The unique ID of the display used when referencing it for control.</param>
		void Initialize(string ipAddress, int port, string label, string id);
	}
}
