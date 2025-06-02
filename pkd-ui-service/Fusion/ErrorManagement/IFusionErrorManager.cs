namespace pkd_ui_service.Fusion.ErrorManagement
{
	/// <summary>
	/// Common methods and properties used for reporting errors to a Crestron Fusion server.
	/// </summary>
	public interface IFusionErrorManager
	{
		/// <summary>
		/// Add an offline error to the current error queue for the target device.
		/// </summary>
		/// <param name="devId">The unique ID of the device to report an error on. This will be used to locate the device when clearing an error.</param>
		/// <param name="devName"></param>
		void AddOfflineDevice(string devId, string devName);

		/// <summary>
		/// Remove an error from the current queue. If it is the currently displayed error then the next error in the queue
		/// will be sent to the server. If there are no more errors in the queue then an "OK" message will be sent to the server.
		/// </summary>
		/// <param name="devId">The unique ID of the device that was assigned when calling AddOfflineDevice().</param>
		void ClearOfflineDevice(string devId);
	}
}
