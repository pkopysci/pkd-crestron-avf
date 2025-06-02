using System.Collections.ObjectModel;
using pkd_application_service.Base;
using pkd_common_utils.GenericEventArgs;

namespace pkd_application_service.AudioControl
{
	/// <summary>
	/// Common properties and methods for audio device control management.
	/// </summary>
	public interface IAudioControlApp
	{
		/// <summary>
		/// Triggered when the internal audio monitor detects a change on an output channel level status.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? AudioOutputLevelChanged;

		/// <summary>
		/// Triggered when the internal audio monitor detects a change on an output channel mute status.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? AudioOutputMuteChanged;

		/// <summary>
		/// Triggered when the internal audio monitor detects a change on an input channel level status.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? AudioInputLevelChanged;

		/// <summary>
		/// Triggered when the internal audio monitor detects a change on an input channel mute status.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>> ?AudioInputMuteChanged;

		/// <summary>
		/// Triggered whenever the connection to the DSP changes (if a DSP is present in the system).
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? AudioDspConnectionStatusChanged;

		/// <summary>
		/// Triggered whenever an audio-routable DSP reports a route change on an output channel.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? AudioOutputRouteChanged;

		/// <summary>
		/// Triggered whenever the system detects an audio zone enable/disable event.
		/// Args package: arg1 = the channel ID that was changed, arg2 = the zone ID that was changed.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>>? AudioZoneEnableChanged;

		/// <summary>
		/// Get all input channels defined in the system configuration (both DSP and AV switch channels
		/// that support audio control).
		/// </summary>
		/// <returns>All AV input channels in the system design.</returns>
		ReadOnlyCollection<AudioChannelInfoContainer> GetAudioInputChannels();

		/// <summary>
		/// Get all output channels defined in the system configuration (both DSP and AV switch channels
		/// that support audio control).
		/// </summary>
		/// <returns>All AV output channels in the system design.</returns>
		ReadOnlyCollection<AudioChannelInfoContainer> GetAudioOutputChannels();

		/// <summary>
		/// Gets information about all audio DSP devices that exist in the configuration.
		/// An empty collection is returned if there are none.
		/// </summary>
		/// <returns>A collection of information on all DSP devices in the system.</returns>
		ReadOnlyCollection<InfoContainer> GetAllAudioDspDevices();

		/// <summary>
		/// Get the current connection status of the target DSP. False is returned if there is no
		/// DSP with a matching ID.
		/// </summary>
		/// <param name="id">The unique ID of the DSP to query.</param>
		/// <returns>True if the device exists and is online, false otherwise.</returns>
		bool QueryAudioDspConnectionStatus(string id);

		/// <summary>
		/// Get the current level of the target input.
		/// </summary>
		/// <param name="id">The unique ID of the input to query.</param>
		/// <returns>a value in the range 0-100 representing the current volume level of the input.</returns>
		int QueryAudioInputLevel(string id);

		/// <summary>
		/// Get the current level of the target output.
		/// </summary>
		/// <param name="id">The unique ID of the output to query.</param>
		/// <returns>A value in the range 0-100 representing the current volume level of the output.</returns>
		int QueryAudioOutputLevel(string id);

		/// <summary>
		/// Get the current mute status of the target output.
		/// </summary>
		/// <param name="id">The unique ID of the output to query.</param>
		/// <returns>True if the output exists and is muted, false otherwise.</returns>
		bool QueryAudioOutputMute(string id);

		/// <summary>
		/// Get the current route status of the target output
		/// </summary>
		/// <param name="id">The unique ID of the output channel to query.</param>
		/// <returns>The unique ID of the input channel that is routed to that output.</returns>
		string QueryAudioOutputRoute(string id);

		/// <summary>
		/// Get the current mute status of the target input.
		/// </summary>
		/// <param name="id">The unique ID of the input to query.</param>
		/// <returns>True if the input exists and is muted, false otherwise.</returns>
		bool QueryAudioInputMute(string id);

		/// <summary>
		/// Get the current state of the target zone enable control for the given channel.
		/// </summary>
		/// <param name="channelId">The unique ID of the audio channel to query.</param>
		/// <param name="zoneId">The unique ID of the zone control for the channel that will be queried.</param>
		/// <returns>the current state of the zone enable or false if no channel/zone pair was found.</returns>
		bool QueryAudioZoneState(string channelId, string zoneId);

		/// <summary>
		/// Send a request to change the audio level of an input channel.
		/// </summary>
		/// <param name="id">The unique ID of the input channel to adjust.</param>
		/// <param name="level">a 0-100 value representing the new level</param>
		void SetAudioInputLevel(string id, int level);

		/// <summary>
		/// Send a request to change the mute state of an input channel.
		/// </summary>
		/// <param name="id">The unique ID of the input channel to update.</param>
		/// <param name="mute">True = enable mute (no audio), false = disable mute (pass audio)</param>
		void SetAudioInputMute(string id, bool mute);

		/// <summary>
		/// Send a request to change the audio level of an output channel.
		/// </summary>
		/// <param name="id">The unique ID of the output channel to adjust.</param>
		/// <param name="level">a 0-100 value representing the new level</param>
		void SetAudioOutputLevel(string id, int level);

		/// <summary>
		/// Send a request to change the mute state of an output channel.
		/// </summary>
		/// <param name="id">The unique ID of the output channel to update.</param>
		/// <param name="mute">True = enable mute (no audio), false = disable mute (pass audio)</param>
		void SetAudioOutputMute(string id, bool mute);

		/// <summary>
		/// Set the audio route on the target output channel.
		/// </summary>
		/// <param name="srcId">The unique ID of the input channel to route.</param>
		/// <param name="destId">The unique ID of the output channel to update.</param>
		void SetAudioOutputRoute(string srcId, string destId);

		/// <summary>
		/// Send a command to the hardware service to toggle the current enable state of the target
		/// channel and zone combination.
		/// 
		/// Does nothing if a channel ID/ zone ID match cannot be found.
		/// </summary>
		/// <param name="channelId">The unique ID of the audio channel to modify.</param>
		/// <param name="zoneId">The unique ID of the zone toggle to change.</param>
		void ToggleAudioZoneState(string channelId, string zoneId);
		
		/// <summary>
		/// Send a command to the hardware service to discretely set an output zone mix state.
		/// </summary>
		/// <param name="channelId">The unique ID of the audio channel to modify.</param>
		/// <param name="zoneId">The unique ID of the zone toggle to change.</param>
		/// <param name="state">true = enable the channel mix, false = disable/mute the channel mix.</param>
		void SetAudioZoneState(string channelId, string zoneId, bool state);
	}
}
