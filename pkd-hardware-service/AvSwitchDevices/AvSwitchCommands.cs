#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace pkd_hardware_service.AvSwitchDevices
{
	/// <summary>
	/// Command types used by various AV switchers for device control.
	/// </summary>
	public enum AvSwitchCommands
	{
		RouteVideo,
		RouteAudio,
		VideoRouteQuery,
		AudioRouteQuery,
		VideoBlankOn,
		VideoBlankOff,
		VideoBlankToggle,
		VideoBlankQuery,
		VideoFreezeOn,
		VideoFreezeOff,
		VideoFreezeToggle,
		VideoFreezeQuery,
		AudioMuteOn,
		AudioMuteOff,
		AudioMuteToggle,
		AudioMuteQuery,
		VolumeSet,
		VolumeQuery,
		Handshake,
	}
}
