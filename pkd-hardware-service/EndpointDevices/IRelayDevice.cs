using pkd_common_utils.GenericEventArgs;
using pkd_hardware_service.BaseDevice;

namespace pkd_hardware_service.EndpointDevices
{
	/// <summary>
	/// Common properties and methods for basic relay device control.
	/// </summary>
	public interface IRelayDevice : IBaseDevice
	{
		/// <summary>
		/// Event triggered whenever the state of the relay changes (open -> close or close ->open).
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, int>> RelayChanged;

		/// <summary>
		/// Gets a value indicating whether the relay is closed or open.
		/// </summary>
		/// <param name="index">The 0-based index representing which relay on the device to query.</param>
		/// <returns>True if the rleay is closed, false if the relay is open</returns>
		bool? GetCurrentRelayState(int index);

		/// <summary>
		/// Close the relay for the specified amount of time.
		/// </summary>
		/// <param name="index">The 0-based index representing which relay on the device to control.</param>
		/// <param name="timeMs">The amount of time in Milliseconds that the relay should remain closed.</param>
		void PulseRelay(int index, int timeMs);

		/// <summary>
		/// Sets the relay closed until LatchOpen() or PulseRelay() is called.
		/// </summary>
		/// <param name="index">The 0-based index representing which relay on the device to control.</param>
		void LatchRelayClosed(int index);

		/// <summary>
		/// Sets the relay open until LatchClosed() or PulseRelay() is called.
		/// </summary>
		/// <param name="index">The 0-based index representing which relay on the device to control.</param>
		void LatchRelayOpen(int index);
	}

}
