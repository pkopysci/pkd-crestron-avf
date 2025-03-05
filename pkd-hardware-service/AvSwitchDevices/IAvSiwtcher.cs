using pkd_hardware_service.BaseDevice;
using pkd_hardware_service.Routable;

namespace pkd_hardware_service.AvSwitchDevices
{
	/// <summary>
	/// Properties and methods common to all devices that are capable of audio and video routing.
	/// </summary>
	public interface IAvSwitcher : IBaseDevice, IVideoRoutable
	{
		/// <summary>
		/// Initialize the device with the given data (does not connect to device)
		/// </summary>
		/// <param name="hostName">The IP address or hostname used to connect.</param>
		/// <param name="port">the port number used to connect.</param>
		/// <param name="id">the unique ID of the device.</param>
		/// <param name="label">The user-friendly name of the device.</param>
		/// <param name="numInputs">Number of inputs supported.</param>
		/// <param name="numOutputs">Number of outputs supported.</param>
		void Initialize(
			string hostName,
			int port,
			string id,
			string label,
			int numInputs,
			int numOutputs);
	}
}
