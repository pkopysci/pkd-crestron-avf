using System.Collections.ObjectModel;
using pkd_application_service.Base;

namespace pkd_application_service.TransportControl
{
	/// <summary>
	/// Common attributes and methods for controlling one or more transport devices.
	/// </summary>
	public interface ITransportControlApp
	{
		/// <summary>
		/// Get a collection of data objects representing all available cable boxes in the system.
		/// </summary>
		/// <returns>All cable box data objects in the system, or an empty collection if none exist.</returns>
		ReadOnlyCollection<TransportInfoContainer> GetAllCableBoxes();

		/// <summary>
		/// Send the power on command to the target transport device
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportPowerOn(string deviceId);

		/// <summary>
		/// Send the power off command to the target transport device
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportPowerOff(string deviceId);

		/// <summary>
		/// Send the power toggle command to the target transport device
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportPowerToggle(string deviceId);

		/// <summary>
		/// Send a channel number to dial to the target transport device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		/// <param name="channel">The channel that will be dialed on the device. Cannot be null or empty.</param>
		void TransportDial(string deviceId, string channel);

		/// <summary>
		/// Send a request to dial the target favorite channel to the device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		/// <param name="favoriteId">the unique ID of the favorite to recall.</param>
		void TransportDialFavorite(string deviceId, string favoriteId);

		/// <summary>
		/// Send the dash (-) command to the target device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportDash(string deviceId);

		/// <summary>
		/// Send a command to the device to increase the channel number by 1.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportChannelUp(string deviceId);

		/// <summary>
		/// Send a command to the device to decrease the channel number by 1.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportChannelDown(string deviceId);

		/// <summary>
		/// Send a command to the device to increase the page or channel listing by 1
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportPageUp(string deviceId);

		/// <summary>
		/// Send a command to the device to decrease the page or channel listing by 1
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportPageDown(string deviceId);

		/// <summary>
		/// Send a command to the transport device to display the guide menu.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportGuide(string deviceId);

		/// <summary>
		/// SEnd a command to the device to display the main menu.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportMenu(string deviceId);

		/// <summary>
		/// Send a command to the device to display the information pop-up.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportInfo(string deviceId);

		/// <summary>
		/// Send a command to the device to exist the current menu.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportExit(string deviceId);

		/// <summary>
		/// Send a command to the device to go back one menu or step.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportBack(string deviceId);

		/// <summary>
		/// Send a "play" command to the device if supported
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportPlay(string deviceId);

		/// <summary>
		/// Send the "pause" command to the device if supported
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportPause(string deviceId);

		/// <summary>
		/// send the stop command to the device if supported.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportStop(string deviceId);

		/// <summary>
		/// send the record command to the device if it is supported.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportRecord(string deviceId);

		/// <summary>
		/// Send the scan forward / fast-forward command to the device if it is supported.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportScanForward(string deviceId);

		/// <summary>
		/// Send the scan backwards / rewind command to the device if it is supported.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportScanReverse(string deviceId);

		/// <summary>
		/// Send the Next / Skip Forward command to the device if it is supported.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportSkipForward(string deviceId);

		/// <summary>
		/// Send the back / skip reverse command to the device if it is supported.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportSkipReverse(string deviceId);

		/// <summary>
		/// Send the D-pad up/ nav up command to the device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportNavUp(string deviceId);

		/// <summary>
		/// Send the d-pad down / nav down command to the device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportNavDown(string deviceId);

		/// <summary>
		/// Send the d-pad left/ nav left command to the device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportNavLeft(string deviceId);

		/// <summary>
		/// Send the D-pad right / Nav right command to the device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportNavRight(string deviceId);

		/// <summary>
		/// Send the red (C) button command to the device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportRed(string deviceId);

		/// <summary>
		/// Send the Green (D) button command to the device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportGreen(string deviceId);

		/// <summary>
		/// Send the yellow (A) command to the device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportYellow(string deviceId);

		/// <summary>
		/// Send the Blue (B) command to the device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportBlue(string deviceId);

		/// <summary>
		/// Send the 'select' command to the device.
		/// </summary>
		/// <param name="deviceId">The unique ID of the device to control. Cannot be null or empty.</param>
		void TransportSelect(string deviceId);
	}
}
