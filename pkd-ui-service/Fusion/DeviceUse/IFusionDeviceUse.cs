namespace pkd_ui_service.Fusion.DeviceUse
{
	/// <summary>
	/// Common methods and properties for tracking device usage via Crestron Fusion.
	/// </summary>
	public interface IFusionDeviceUse
	{
		/// <summary>
		/// Add a device to the internal collection used for tracking use.
		/// </summary>
		/// <param name="id">The unique ID of the device to track. This will be used when starting or stopping an in-use event.</param>
		/// <param name="label">The user-friendly label of the device. This will be logged and displayed when reporting use statistics.</param>
		void AddDeviceToUseTracking(string id, string label);

		/// <summary>
		/// start recording use time for the target device.
		/// </summary>
		/// <param name="id">The unique ID of a device that was added to the internal collection with AddDeviceToUseTracking().</param>
		void StartDeviceUse(string id);

		/// <summary>
		/// Stop recording the use time for the target device and send a usage log to the Fusion server.
		/// </summary>
		/// <param name="id">The unique ID of a device that was added to the internal collection with AddDeviceToUseTracking().</param>
		void StopDeviceUse(string id);

		/// <summary>
		/// Add a display to the internal collection used for tracking use statistics
		/// </summary>
		/// <param name="id">The unique id of the display to track. This will be used to locate a display when starting or stopping use tracking.</param>
		/// <param name="label">The user-friendly name of the display to track. This will be used in the usage report.</param>
		void AddDisplayToUseTracking(string id, string label);

		/// <summary>
		/// Start recording the use time for the target display.
		/// </summary>
		/// <param name="id">The unique ID of the display to start tracking.</param>
		void StartDisplayUse(string id);

		/// <summary>
		/// Stop recording the use time for the target display and send the data to the Fusion server.
		/// </summary>
		/// <param name="id">The unique ID of the device to stop and send report for.</param>
		void StopDisplayUse(string id);
	}
}
