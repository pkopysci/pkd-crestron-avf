using pkd_common_utils.GenericEventArgs;

namespace pkd_hardware_service.DisplayDevices
{
	/// <summary>
	/// Common properties and methods for a video output device.
	/// </summary>
	public interface IVideoControllable
	{
		/// <summary>
		/// Event triggered whenever a video blank status is reported by the display.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>> VideoBlankChanged;

		/// <summary>
		/// Event triggered whenever a video freeze status is reported by the display (if supported).
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>> VideoFreezeChanged;

		/// <summary>
		/// Gets a value indicating whether the video freeze is on or off.
		/// true = video frozen, false = in motion. Will be false if freeze is not supported.
		/// </summary>
		bool FreezeState { get; }

		/// <summary>
		/// Gets a value indicating whether the video blank is on or off.
		/// true = video is blanked, false = video is active.
		/// </summary>
		bool BlankState { get; }

		/// <summary>
		/// Enable video blank on the display hardware (no picture shown).
		/// </summary>
		void VideoBlankOn();

		/// <summary>
		/// Disable video blank on the display hardware (picture is visible).
		/// </summary>
		void VideoBlankOff();

		/// <summary>
		/// If supported, will send the "freeze video on" command to the display hardware.
		/// Does nothing if video freeze is not supported.
		/// </summary>
		void FreezeOn();

		/// <summary>
		/// If supported, will send the "freeze video off" command to the display hardware.
		/// Does nothing if video freeze is not supported.
		/// </summary>
		void FreezeOff();
	}
}
