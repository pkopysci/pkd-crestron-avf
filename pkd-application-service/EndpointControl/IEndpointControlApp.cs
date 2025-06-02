using pkd_common_utils.GenericEventArgs;

namespace pkd_application_service.EndpointControl
{
	/// <summary>
	/// Common properties and methods for endpoint control applications.
	/// </summary>
	public interface IEndpointControlApp
	{
		/// <summary>
		/// Event triggered whenever a relay on an endpoint changes state.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, int>> EndpointRelayChanged;

		/// <summary>
		/// Event triggered whenever a control connection status changes for an endpoint.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, bool>> EndpointConnectionChanged;

		/// <summary>
		/// Close a relay on the target endpoint for the given amount of time.
		/// </summary>
		/// <param name="id">The unique ID of the endpoint to control</param>
		/// <param name="index">the 0-based relay index on the endpoint.</param>
		/// <param name="timeMs">The amount of time to latch the relay closed.</param>
		void PulseEndpointRelay(string id, int index, int timeMs);

		/// <summary>
		/// Set the relay closed on the target device until manually set to open or pulsed.
		/// </summary>
		/// <param name="id">The unique ID of the endpoint to control.</param>
		/// <param name="index">The 0-based index of the relay on the endpoint.</param>
		void LatchRelayClosed(string id, int index);

		/// <summary>
		/// Set the relay open on the target device until manually set to open or pulsed.
		/// </summary>
		/// <param name="id">The unique ID of the endpoint to control.</param>
		/// <param name="index">The 0-based index of the relay on the endpoint.</param>
		void LatchRelayOpen(string id, int index);
	}
}
