namespace pkd_ui_service.Interfaces
{
	/// <summary>
	/// Required events, methods, and properties for implementing a user interface that supports device error reporting.
	/// </summary>
	public interface IErrorInterface
	{
		/// <summary>
		/// Add a notice to the UI display indicating that the target device is offline.
		/// </summary>
		/// <param name="id">The unique ID of the device to add.</param>
		/// <param name="label">The user-friendly label that will be displayed on the UI.</param>
		void AddDeviceError(string id, string label);

		/// <summary>
		/// Remove the target device from the offline list.
		/// </summary>
		/// <param name="id">The unique ID of the device to remove.</param>
		void ClearDeviceError(string id);
	}
}
