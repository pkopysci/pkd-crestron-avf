using pkd_common_utils.GenericEventArgs;
using pkd_hardware_service.BaseDevice;

namespace pkd_hardware_service.AudioDevices
{
	/// <summary>
	/// Common properties and methods for basic audio level control.
	/// </summary>
	public interface IAudioControl : IBaseDevice
	{
		/// <summary>
		/// Triggered when a volume/level change is detected on any input audio channel.
		/// args package: arg 1 = DSP ID, arg 2 = channel ID.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>> AudioInputLevelChanged;

		/// <summary>
		/// Triggered when a mute state change is detected on any input audio channel.
		/// args package: arg 1 = DSP ID, arg 2 = channel ID.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>> AudioInputMuteChanged;

		/// <summary>
		/// Triggered when a volume/level change is detected on any output audio channel.
		/// args package: arg 1 = DSP ID, arg 2 = channel ID.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>> AudioOutputLevelChanged;

		/// <summary>
		/// Triggered when a mute state change is detected on any output audio channel.
		/// args package: arg 1 = DSP ID, arg 2 = channel ID.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>> AudioOutputMuteChanged;

		/// <summary>
		/// Gets the Ids of all the presets that were added to this device when created.
		/// </summary>
		IEnumerable<string> GetAudioPresetIds();

		/// <summary>
		/// Gets the IDs of all the input channels added to this device when created.
		/// </summary>
		IEnumerable<string> GetAudioInputIds();

		/// <summary>
		/// Gets the IDs of all the output channels added to this device when created.
		/// </summary>
		IEnumerable<string> GetAudioOutputIds();

		/// <summary>
		/// Set the target input channel to the given audio level.
		/// Range is 0-100 and scaled internally to match the device limits.
		/// </summary>
		/// <param name="id">The unique ID of the input channel to adjust</param>
		/// <param name="level">the 0-100 level to set the channel volume to.</param>
		void SetAudioInputLevel(string id, int level);

		/// <summary>
		/// Query the device for the current input audio level.
		/// </summary>
		/// <param name="id">The unique ID of the input channel to query.</param>
		/// <returns>a 0-100 value representing the current audio level. Returns 0 if 'id' cannot be found.</returns>
		int GetAudioInputLevel(string id);

		/// <summary>
		/// Send a mute command to the target input channel.
		/// </summary>
		/// <param name="id">The unique ID of the channel to change.</param>
		/// <param name="mute">true = mute on, false = mute off.</param>
		void SetAudioInputMute(string id, bool mute);

		/// <summary>
		/// Gets the current mute status of the target input channel.
		/// </summary>
		/// <param name="id">The unique ID of the input channel to query.</param>
		/// <returns>true if mute is active, false is mute is off.</returns>
		bool GetAudioInputMute(string id);

		/// <summary>
		/// Set the target output channel to the given audio level.
		/// Range is 0-100 and scaled internally to match the device limits.
		/// </summary>
		/// <param name="id">The unique ID of the output channel to adjust</param>
		/// <param name="level">the 0-100 level to set the channel volume to.</param>
		void SetAudioOutputLevel(string id, int level);

		/// <summary>
		/// Query the device for the current output audio level.
		/// </summary>
		/// <param name="id">The unique ID of the output channel to query.</param>
		/// <returns>a 0-100 value representing the current audio level. Returns 0 if 'id' cannot be found.</returns>
		int GetAudioOutputLevel(string id);

		/// <summary>
		/// Send a mute command to the target output channel.
		/// </summary>
		/// <param name="id">The unique ID of the channel to change.</param>
		/// <param name="mute">true = mute on, false = mute off.</param>
		void SetAudioOutputMute(string id, bool mute);

		/// <summary>
		/// Gets the current mute status of the target output channel.
		/// </summary>
		/// <param name="id">The unique ID of the output channel to query.</param>
		/// <returns>true if mute is active, false is mute is off.</returns>
		bool GetAudioOutputMute(string id);

		/// <summary>
		/// Attempts to recall the target preset on the device.
		/// </summary>
		/// <param name="id">The unique ID of the preset to recall, as defined in the system configuration.</param>
		void RecallAudioPreset(string id);

		/// <summary>
		/// Add an input or microphone channel to the DSP. The DSP implementation will then update its control
		/// methods for that channel.
		/// </summary>
		/// <param name="id">The unique ID of the channel as defined in the configuration.</param>
		/// <param name="levelTag">The instance tag or named control associated with changing the gain.</param>
		/// <param name="muteTag">The instance tag or named control associated with the mute state.</param>
		/// <param name="bankIndex">The input number or position in a channel bank used for control.</param>
		/// <param name="levelMax">The maximum value expected by the hardware for the audio channel. This is in the range defined by the device and not necessarily 0-100.</param>
		/// <param name="levelMin">The minimum value expected by the hardware for the audio channel. This is in the range defined by the device and not necessarily 0-100.</param>
		/// <param name="routerIndex">The input index for this channel if routing is supported by the control. Can be 0 if unused.</param>
		/// <param name="tags">A collection of keywords used for plugin-specific or custom ui behavior.</param>
		void AddInputChannel(string id, string levelTag, string muteTag, int bankIndex, int levelMax, int levelMin, int routerIndex, List<string> tags);

		/// <summary>
		/// Add an output to the DSP. The DSP implementation will then update its control
		/// methods for that channel.
		/// </summary>
		/// <param name="id">The unique ID of the channel as defined in the configuration.</param>
		/// <param name="levelTag">The instance tag or named control associated with changing the gain.</param>
		/// <param name="muteTag">The instance tag or named control associated with the mute state.</param>
		/// <param name="routerTag">The instance tag or named control associated with a router block.</param>
		/// <param name="routerIndex">The output index of the router associated with this channel. Can be 0 if not routable.</param>
		/// <param name="bankIndex">The output number or position in a channel bank used for control.</param>
		/// <param name="levelMax">The maximum value expected by the hardware for the audio channel. This is in the range defined by the device and not necessarily 0-100.</param>
		/// <param name="levelMin">The minimum value expected by the hardware for the audio channel. This is in the range defined by the device and not necessarily 0-100.</param>
		/// <param name="tags">A collection of keywords used for plugin-specific or custom ui behavior.</param>
		void AddOutputChannel(
			string id,
			string levelTag,
			string muteTag,
			string routerTag,
			int routerIndex,
			int bankIndex,
			int levelMax,
			int levelMin,
			List<string> tags);

		/// <summary>
		/// Add a preset recall to the DSP. The DSP implementation will update its control methods for that
		/// preset or snapshot.
		/// </summary>
		/// <param name="id">The unique ID of the preset. This can either be a snapshot bank name or tag ID.</param>
		/// <param name="index">The index or preset number to recall.</param>
		void AddPreset(string id, int index);
	}
}
