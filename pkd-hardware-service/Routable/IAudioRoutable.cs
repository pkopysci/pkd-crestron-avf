using pkd_common_utils.GenericEventArgs;

namespace pkd_hardware_service.Routable
{
	/// <summary>
	/// Common methods and attributes for all devices that can route audio.
	/// </summary>
	public interface IAudioRoutable
	{
		/// <summary>
		/// Triggered when there is a change in the audio source for the output ID in the
		/// arguments package.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>> AudioRouteChanged;

		/// <summary>
		/// Query the device for the audio input that is currently routed to the target output.
		/// An error will be written to the logging system if a failure occurs.
		/// </summary>
		/// <param name="outputId">The output ID to query.</param>
		/// <returns>the audio input ID that is currently routed, or 0 if the query fails.</returns>
		string GetCurrentAudioSource(string outputId);

		/// <summary>
		/// Route the target audio input to the target video output.
		/// </summary>
		/// <param name="sourceId">The input ID that will be routed.</param>
		/// <param name="outputId">The output ID to route to.</param>
		void RouteAudio(string sourceId, string outputId);

		/// <summary>
		/// Clear the output of all audio signals.
		/// </summary>
		/// <param name="outputId">The output to clear audio content on.</param>
		void ClearAudioRoute(string outputId);
	}
}
