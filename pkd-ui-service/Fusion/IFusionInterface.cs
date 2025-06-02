using pkd_common_utils.GenericEventArgs;
using pkd_ui_service.Fusion.DeviceUse;
using pkd_ui_service.Fusion.ErrorManagement;

namespace pkd_ui_service.Fusion
{
	/// <summary>
	/// Required methods, properties, and events for supporting a fusion connection in the AVF.
	/// </summary>
	public interface IFusionInterface : IFusionDeviceUse, IFusionErrorManager, IDisposable
	{
		/// <summary>
		/// Triggered when the Fusion server connection comes online or goes offline.
		/// </summary>
		event EventHandler<EventArgs>? OnlineStatusChanged;

		/// <summary>
		/// Triggered whenever a system power event is received from the Fusion server.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<bool>>? SystemStateChangeRequested;

		/// <summary>
		/// Triggered whenever a display power event is received from the Fusion server.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<bool>>? DisplayPowerChangeRequested;

		/// <summary>
		/// Triggered whenever a program audio mute event is received from the Fusion server.
		/// </summary>
		event EventHandler<EventArgs>? AudioMuteChangeRequested;

		/// <summary>
		/// Triggered whenever a "blank all displays" event is received from the Fusion server.
		/// </summary>
		event EventHandler<EventArgs>? DisplayBlankChangeRequested;

		/// <summary>
		/// Triggered whenever a "freeze all displays" event is received from the Fusion server.
		/// </summary>
		event EventHandler<EventArgs>? DisplayFreezeChangeRequested;

		/// <summary>
		/// Triggered whenever a request to change the program audio is received from the server.
		/// Args package will contain the 0-100 value that the level should be set to.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<uint>>? ProgramAudioChangeRequested;

		/// <summary>
		/// Triggered whenever a request to change the mute state of a microphone is received from the Fusion server.
		/// Args package will contain the ID of the microphone to toggle.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? MicMuteChangeRequested;

		/// <summary>
		/// Triggered whenever a request to change the selected AV input is received from the Fusion server.
		/// Args package with contain the ID of the input source to select.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? SourceSelectRequested;

		/// <summary>
		/// Gets a value indicating whether the system is connected to the Fusion server.
		/// </summary>
		bool IsOnline { get; }

		/// <summary>
		/// Send a notification to the Fusion server that the system use state has changed (active or standby).
		/// </summary>
		/// <param name="state">True = system is active, false = system is in standby.</param>
		void UpdateSystemState(bool state);

		/// <summary>
		/// Send a notification to the Fusion server that the program audio mute status has changed.
		/// </summary>
		/// <param name="state">True = program audio is muted (no audio), false = program audio is not muted (audio is passing).</param>
		void UpdateProgramAudioMute(bool state);

		/// <summary>
		/// Send a notification to the Fusion server that the program audio level has changed.
		/// </summary>
		/// <param name="level">The 0-100 value representing the new audio level.</param>
		void UpdateProgramAudioLevel(uint level);

		/// <summary>
		/// Send a notification to the Fusion server that there is a display powered on.
		/// </summary>
		/// <param name="state">True = a display is on, false = a display is off.</param>
		void UpdateDisplayPower(bool state);

		/// <summary>
		/// Send a notification to the Fusion server that the global video blank status has changed.
		/// </summary>
		/// <param name="state">True = video is blanked (no video), false = video is active.</param>
		void UpdateDisplayBlank(bool state);

		/// <summary>
		/// Send a notification to the Fusion server that the global video freeze state has changed.
		/// </summary>
		/// <param name="state">True = video is frozen (no motion), false  = video is not frozen (normal output).</param>
		void UpdateDisplayFreeze(bool state);

		/// <summary>
		/// Add a microphone to the internal tracker used when requesting mic mute changes.
		/// System will look for either "podium" and assign the associated 'id' parameter to the podium mute toggle in the Fusion interface.
		/// Any other mic will be assigned to the generic "mic mute" trigger. If multiple mics are added to either trigger then the last mic added
		/// will be sent on the event.
		/// </summary>
		/// <param name="id">The unique ID of the mic to add.</param>
		/// <param name="label">The user-friendly name of the microphone</param>
		/// <param name="tags">Collection of functionality Tags associated with the mic. These will be searched for the keyword "podium".</param>
		void AddMicrophone(string id, string label, string[] tags);

		/// <summary>
		/// Send a notification to the Fusion server that the mute state has changed for a microphone in the system.
		/// </summary>
		/// <param name="id">The unique ID of the microphone that changed, as set in the system configuration.</param>
		/// <param name="state">True = mic is muted, false = mic is passing audio.</param>
		void UpdateMicMute(string id, bool state);

		/// <summary>
		/// Add an AV source to the internal collection of sources that are selectable from the Fusion server dashboard.
		/// Tags assigned will be searched for keywords vga, hdmi, and pc, and then assigned to the triggers send from the server.
		/// </summary>
		/// <param name="id">The unique ID of the source to add. This will be used when sending change events.</param>
		/// <param name="label">The user-friendly name or label of the AV source.</param>
		/// <param name="tags">A Collection of functionality Tags associated with the input, usually defined in the system configuration.</param>
		void AddAvSource(string id, string label, string[] tags);

		/// <summary>
		/// Send a notification to the Fusion server that the currently selected AV source has changed.
		/// </summary>
		/// <param name="id">The unique ID of the source that is currently routed.</param>
		void UpdateSelectedSource(string id);

		/// <summary>
		/// Registers internal Fusion server objects and attempts to establish a connection.
		/// </summary>
		void Initialize();
	}
}
