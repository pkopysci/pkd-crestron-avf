using pkd_common_utils.GenericEventArgs;

namespace pkd_hardware_service.AudioDevices
{
	/// <summary>
	/// events, properties, and methods for controlling which audio output channels a given input channel should be sending signal
	/// to.
	/// </summary>
	public interface IAudioZoneEnabler
	{
		/// <summary>
		/// Triggered when the device control detects a change on the channel audio zone enable.
		/// Args package: arg1 = channel ID, arg2 = zone toggle ID.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>> AudioZoneEnableChanged;

		/// <summary>
		/// Add a audio zone toggle control to the internal collection of the control object.
		/// NOTE: if a control object with matching channelId and zoneID is detected then the new one will be ignored.
		/// </summary>
		/// <param name="channelId">The unique ID of the input channel this control will be associated with.</param>
		/// <param name="zoneId">The unique ID of the toggle control object. This is used internally for referencing.</param>
		/// <param name="controlTag">The DSP design named control or Instance ID used for device control.</param>
		void AddAudioZoneEnable(string channelId, string zoneId, string controlTag);

		/// <summary>
		/// Remove an audio zone toggle control from the internal collection.
		/// if no object is found with a matching channelId and zoneId then no action is taken.
		/// </summary>
		/// <param name="channelId">The unique ID of the input channel associated with the control being removed.</param>
		/// <param name="zoneId">The unique ID of the zone enable control being removed.</param>
		void RemoveAudioZoneEnable(string channelId, string zoneId);

		/// <summary>
		/// Send a command to the hardware to toggle the current state of the zone enable control.
		/// If no matching channelId and zoneId is found then no action is taken.
		/// </summary>
		/// <param name="channelId">The unique ID associated with the zone enable control.</param>
		/// <param name="zoneId">The unique ID of the zone to send the change request to.</param>
		void ToggleAudioZoneEnable(string channelId, string zoneId);
		
		/// <summary>
		/// Discretely set whether an audio channel is mixed to a given output zone.
		/// </summary>
		/// <param name="channelId">The unique ID associated with the zone enable control.</param>
		/// <param name="zoneId">The unique ID of the zone to send the change request to.</param>
		/// <param name="enable">true = enable the channel mix, false = mute/disable the channel mix.</param>
		void SetAudioZoneEnable(string channelId, string zoneId, bool enable);

		/// <summary>
		/// Queries the device for the current status of the target zone enable control.
		/// </summary>
		/// <param name="channelId">The unique ID associated with the zone enable control.</param>
		/// <param name="zoneId">The unique ID of the zone control object being queried</param>
		/// <returns>The current state of the zone enable control. Returns false if a channelId/zoneId is not found.</returns>
		bool QueryAudioZoneEnable(string channelId, string zoneId);
	}

}
