using Crestron.SimplSharpPro;
using pkd_common_utils.GenericEventArgs;

namespace pkd_hardware_service.AvIpMatrix
{
	/// <summary>
	/// Required events, methods, and properties for implementing an AV over IP device control.
	/// </summary>
	public interface IAvIpMatrix
	{
		/// <summary>
		/// Triggered when an encoder or decoder status changes, typically used for online/offline events.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>> AvIpEndpointStatusChanged;

		/// <summary>
		/// Get the target AV over IP endpoint data object.
		/// </summary>
		/// <param name="deviceId">The unique ID of the endpoint to request.</param>
		/// <returns>The target AvIp endpoint, or null if the endpoint cannot be found.</returns>
		IAvIpEndpoint? GetAvIpEndpoint(string deviceId);

		/// <summary>
		/// Add an AV over IP endpoint to the routing object.
		/// </summary>
		/// <param name="id">The unique ID of the endpoint. This is used internally for routing events.</param>
		/// <param name="tags">The collection of device tags that were set in the configuration file.</param>
		/// <param name="ioIndex">The input or output index on the hardware that is associated with this routing endpoint.</param>
		/// <param name="endpointType">Define whether this will be added as an encoder or decoder endpoint.</param>
		/// <param name="control">The root control system object that runs this program.</param>
		void AddEndpoint(
			string id,
			List<string> tags,
			int ioIndex,
			AvIpEndpointTypes endpointType,
			CrestronControlSystem control);
	}
}
