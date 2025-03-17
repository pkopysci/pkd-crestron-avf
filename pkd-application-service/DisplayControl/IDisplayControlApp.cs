using System.Collections.ObjectModel;
using pkd_common_utils.GenericEventArgs;

namespace pkd_application_service.DisplayControl
{
	/// <summary>
	/// Common properties and methods for display control applications.
	/// </summary>
	public interface IDisplayControlApp
	{
		/// <summary>
		/// Triggered whenever a display in the collection report a change in power status.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, bool>> DisplayPowerChange;

		/// <summary>
		/// Triggered whenever a display in the collection report a change in video blank status.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, bool>> DisplayBlankChange;

		/// <summary>
		/// Triggered whenever a display in the collection report a change in video freeze status.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, bool>> DisplayFreezeChange;

		/// <summary>
		/// Triggered whenever a display in the collection report a change in connection status.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, bool>> DisplayConnectChange;

		/// <summary>
		/// Triggered whenever a display indicates the on-device input selection has changed.
		/// On triggers for displays that implement IVideoRoutable.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>> DisplayInputChanged;

		/// <summary>
		/// Request to change the power state of a target display.
		/// </summary>
		/// <param name="id">The unique ID of the display to change.</param>
		/// <param name="newState">true = turn on, false = turn off.</param>
		void SetDisplayPower(string id, bool newState);

		/// <summary>
		/// Query the system for the current power state of a target display.
		/// </summary>
		/// <param name="id">The unique ID of the display to query.</param>
		/// <returns>True = display power is on, false = display power is off.</returns>
		bool DisplayPowerQuery(string id);

		/// <summary>
		/// Gets a value indication whether the target routable display is on the lectern input source.
		/// </summary>
		/// <param name="id">The unique ID of the display to query.</param>
		/// <returns>True if the display is IVideoRoutable and has the lectern source selected, false otherwise.</returns>
		bool DisplayInputLecternQuery(string id);

		/// <summary>
		/// Gets a value indication whether the target routable display is on the Station input source.
		/// </summary>
		/// <param name="id">The unique ID of the display to query.</param>
		/// <returns>True if the display is IVideoRoutable and has the station source selected, false otherwise.</returns>
		bool DisplayInputStationQuery(string id);

		/// <summary>
		/// Request to change the video blank state of the target display.
		/// </summary>
		/// <param name="id">The unique ID of the display to change.</param>
		/// <param name="newState">true = set blank on, false = set blank off.</param>
		void SetDisplayBlank(string id, bool newState);

		/// <summary>
		/// Query the system for the current video blank status of the target display.
		/// </summary>
		/// <param name="id">The unique ID of the display to query.</param>
		/// <returns>The current state of the display blank or false if the display was not found.</returns>
		bool DisplayBlankQuery(string id);

		/// <summary>
		/// Request to change the video freeze state of the target display
		/// </summary>
		/// <param name="id">The unique ID of the display to change</param>
		/// <param name="state">true = set freeze on, false = set freeze off</param>
		void SetDisplayFreeze(string id, bool state);

		/// <summary>
		/// Query the system for the current video freeze state of the target display.
		/// </summary>
		/// <param name="id">The unique ID of the display to query</param>
		/// <returns>true = video is frozen, false = video motion is active.</returns>
		bool DisplayFreezeQuery(string id);

		/// <summary>
		/// Request to raise the relay-controlled screen associated with the target display.
		/// </summary>
		/// <param name="displayId">The unique ID of the display associated with the screen being raised.</param>
		void RaiseScreen(string displayId);

		/// <summary>
		/// Request to lower the relay-controlled screen associated with the target display.
		/// </summary>
		/// <param name="displayId">The unique ID of the display associated with the screen being lowered.</param>
		void LowerScreen(string displayId);

		/// <summary>
		/// Request to set the target station display to the Lectern source defined in the configuration. This will do nothing
		/// if the target display does not implement Infrastructure.Routable.IVideoRoutable.
		/// </summary>
		/// <param name="displayId">The target station display to attempt to change.</param>
		void SetInputLectern(string displayId);

		/// <summary>
		/// Request to set the target station display to the local station source defined in the configuration. This will do nothing
		/// if the target display does not implement Infrastructure.Routable.IVideoRoutable.
		/// </summary>
		/// <param name="displayId">The target station display to attempt to change.</param>
		void SetInputStation(string displayId);

		/// <summary>
		/// Get the configuration information for all displays in the system.
		/// </summary>
		/// <returns>A collection of display data for all displays in the system.</returns>
		ReadOnlyCollection<DisplayInfoContainer> GetAllDisplayInfo();
	}
}
