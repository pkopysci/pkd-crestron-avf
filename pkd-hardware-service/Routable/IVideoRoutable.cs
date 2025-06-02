using pkd_common_utils.GenericEventArgs;

namespace pkd_hardware_service.Routable
{
	/// <summary>
	/// Common methods and attributes for all devices that can route video.
	/// </summary>
	public interface IVideoRoutable
	{
		/// <summary>
		/// Triggered when there is a change in the video source for the output number in the
		/// arguments package.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, uint>> VideoRouteChanged;

		/// <summary>
		/// Query the device for the video input that is currently routed to the target output.
		/// An error will be written to the logging system if a failure occurs.
		/// </summary>
		/// <param name="output">The output number to query.</param>
		/// <returns>the video input number that is currently routed, or 0 if the query fails.</returns>
		uint GetCurrentVideoSource(uint output);

		/// <summary>
		/// Route the target video input to the target video output.
		/// </summary>
		/// <param name="source">The input number that will be routed.</param>
		/// <param name="output">The output number to route to.</param>
		void RouteVideo(uint source, uint output);

		/// <summary>
		/// Clear the output of all video signals.
		/// </summary>
		/// <param name="output">The output to clear video content on.</param>
		void ClearVideoRoute(uint output);
	}
}
