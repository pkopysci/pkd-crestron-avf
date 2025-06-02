using pkd_hardware_service.BaseDevice;

namespace pkd_hardware_service.EndpointDevices
{
	/// <summary>
	/// Common properties and methods for all endpoint devices (such as DM-TX and RMC-100s).
	/// </summary>
	public interface IEndpointDevice : IBaseDevice
	{
		/// <summary>
		/// Gets a value indicating whether the object has been registered.
		/// </summary>
		bool IsRegistered { get; }

		/// <summary>
		/// Gets a value indicating whether the endpoint supports relay controls.
		/// </summary>
		bool SupportsRelays { get; }

		/// <summary>
		/// Gets a value indicating whether the endpoint supports IR controls.
		/// </summary>
		bool SupportsIr { get; }

		/// <summary>
		/// Gets a value indicating whether the endpoint supports RS-232 controls.
		/// </summary>
		bool SupportsRs232 { get; }

		/// <summary>
		/// Register any connections or control interfaces on the underlying hardware control.
		/// </summary>
		void Register();
	}
}
