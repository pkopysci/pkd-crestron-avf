namespace pkd_ui_service.Interfaces
{
	using pkd_application_service.DisplayControl;
	using pkd_common_utils.GenericEventArgs;
	using System;
	using System.Collections.ObjectModel;

	/// <summary>
	/// Required methods, events, and properties for implementing a user interface that supports display control.
	/// </summary>
	public interface IDisplayUserInterface
	{
		/// <summary>
		/// Triggered when the user requests to change the current power status of a display.
		/// arg = display ID.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, bool>>? DisplayPowerChangeRequest;

		/// <summary>
		/// Triggered when the user requests to change the current video freeze status of a display.
		/// arg = display ID.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? DisplayFreezeChangeRequest;

		/// <summary>
		/// Triggered when the user requests to change the current video blank/mute status of a display.
		/// arg = display ID.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? DisplayBlankChangeRequest;

		/// <summary>
		/// Triggered when the user requests to raise the screen associated with a display.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? DisplayScreenUpRequest;

		/// <summary>
		/// Triggered when the user requests to lower the screen associated with a display.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? DisplayScreenDownRequest;

		/// <summary>
		/// Triggered when the user requests to change a student workstation to the local input.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? StationLocalInputRequest;

		/// <summary>
		/// Triggered when the user requests to change a student workstation to the lectern input.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>>? StationLecternInputRequest;

		/// <summary>
		/// Update the UI with the current power status of the display.
		/// </summary>
		/// <param name="id">The unique ID of the display to update.</param>
		/// <param name="newState">true = display on, false = display off</param>
		void UpdateDisplayPower(string id, bool newState);

		/// <summary>
		/// Update the UI with the current video blank status.
		/// </summary>
		/// <param name="id">The unique ID of the display to update.</param>
		/// <param name="newState">true = blank on (no video), false = blank of (show video).</param>
		void UpdateDisplayBlank(string id, bool newState);

		/// <summary>
		/// Update the UI wth the current video freeze status.
		/// </summary>
		/// <param name="id">The unique ID of the display to update.</param>
		/// <param name="newState">True = freeze on (no motion), false = freeze off (show motion).</param>
		void UpdateDisplayFreeze(string id, bool newState);

		/// <summary>
		/// Updates the user interface to display the new collection of controllable video outputs.
		/// </summary>
		/// <param name="displayData">The collection of data objects for all displays in the configuration.</param>
		void SetDisplayData(ReadOnlyCollection<DisplayInfoContainer> displayData);

		/// <summary>
		/// Update the UI to indicated that the target student workstation is on the "Local" input.
		/// </summary>
		/// <param name="id">The ID if the station display to update.</param>
		void SetStationLocalInput(string id);

		/// <summary>
		/// Update the Ui to indicate that the target student workstation is on the "Lectern" input.
		/// </summary>
		/// <param name="id">The ID of the station display to update.</param>
		void SetStationLecternInput(string id);
	}
}
