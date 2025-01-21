namespace pkd_ui_service.Interfaces
{
	using pkd_application_service.AudioControl;
	using pkd_common_utils.GenericEventArgs;
	using System;
	using System.Collections.ObjectModel;

	public interface IAudioUserInterface
	{
		/// <summary>
		/// Triggered when the user requests to increase the target output volume.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? AudioOutputLevelUpRequest;

		/// <summary>
		/// Triggered when the user requests to lower the target output volume.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? AudioOutputLevelDownRequest;

		/// <summary>
		/// Triggered when the user requests to toggle the output mute state.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? AudioOutputMuteChangeRequest;

		/// <summary>
		/// Triggered when the user requests to increase the target input volume.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? AudioInputLevelUpRequest;

		/// <summary>
		/// Triggered when the user requests to lower the target input volume.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? AudioInputLevelDownRequest;

		/// <summary>
		/// Triggered when the user requests to toggle the input mute state.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? AudioInputMuteChangeRequest;

		/// <summary>
		/// Triggered when the user requests to change an audio route. 
		/// args.Arg1 = source id, args.Agr2 = destination id.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>>? AudioOutputRouteRequest;

		/// <summary>
		/// Triggered when the user requests to toggle the status of a zone audio enable/disable control.
		/// Args package: arg1 = channel ID, arg2 = zone ID.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>>? AudioZoneEnableToggleRequest;

		/// <summary>
		/// Update the UI with all supported input and output audio channels.
		/// </summary>
		/// <param name="inputs">Collection of input/microphone data that the user can control.</param>
		/// <param name="outputs">Collection of output data that the user can control.</param>
		void SetAudioData(ReadOnlyCollection<AudioChannelInfoContainer> inputs, ReadOnlyCollection<AudioChannelInfoContainer> outputs);

		/// <summary>
		/// Update the UI with the audio channels current audio level.
		/// </summary>
		/// <param name="id">The unique ID of the input channel to update.</param>
		/// <param name="newLevel">The new level to display on the UI.</param>
		void UpdateAudioInputLevel(string id, int newLevel);

		/// <summary>
		/// Update the UI with the current mute state of the audio input.
		/// </summary>
		/// <param name="id">The unique ID of the channel to update.</param>
		/// <param name="muteState">true = mute is active (no audio), false = mute is inactive (audio passing).</param>
		void UpdateAudioInputMute(string id, bool muteState);

		/// <summary>
		/// Update the UI with the audio channels current audio level.
		/// </summary>
		/// <param name="id">The unique ID of the output channel to update.</param>
		/// <param name="newLevel">The new level to display on the UI.</param>
		void UpdateAudioOutputLevel(string id, int newLevel);

		/// <summary>
		/// Update the UI with the current mute state of the audio output.
		/// </summary>
		/// <param name="id">The unique ID of the channel to update.</param>
		/// <param name="muteState">true = mute is active (no audio), false = mute is inactive (audio passing).</param>
		void UpdateAudioOutputMute(string id, bool muteState);

		/// <summary>
		/// Update the connected user interface with the new routing state.
		/// </summary>
		/// <param name="srcId">The unique ID of the audio input that was routed.</param>
		/// <param name="destId">The unique ID of the audio output that is being updated.</param>
		void UpdateAudioOutputRoute(string srcId, string destId);

		/// <summary>
		/// Update the user interface with the current state of the target channel/zone enable.
		/// </summary>
		/// <param name="channelId">The unique ID of the audio channel that will be updated.</param>
		/// <param name="zoneId">The unique ID of the zone that was changed.</param>
		/// <param name="newState">True = audio for that channel is active in the zone, false = audio disabled for that channel/zone.</param>
		void UpdateAudioZoneState(string channelId, string zoneId, bool newState);
	}
}
