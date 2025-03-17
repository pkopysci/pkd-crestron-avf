namespace pkd_hardware_service.AudioDevices
{
	/// <summary>
	/// Common properties and methods for controlling typical DSP devices.
	/// </summary>
	public interface IDsp : IDisposable, IAudioControl
	{
		/// <summary>
		/// Sets internal object configuration based on the supplied data.
		/// </summary>
		/// <param name="hostId">The unique ID of the DSP being controlled</param>
		/// <param name="coreId">the device number used by the hardware when sending or receiving data. Can be set to 0 if unused.</param>
		/// <param name="hostname">The hostname or IP address used to connect to the hardware.</param>
		/// <param name="port">The TCP port number used to connect to the hardware.</param>
		/// <param name="username">The authentication username used when connecting.</param>
		/// <param name="password">The authentication password used when connecting.</param>
		void Initialize(string hostId, int coreId, string hostname, int port, string username, string password);
	}
}
