namespace pkd_ui_service.Interfaces
{
	using pkd_application_service.UserInterface;
	using pkd_common_utils.GenericEventArgs;
	using System;

	/// <summary>
	/// required events, methods, and properties for implementing any user interface.
	/// </summary>
	public interface IUserInterface
	{
		/// <summary>
		/// Triggered when the user requests to start or end the current session.
		/// True = set system to in-use/on, false = set system to standby/off
		/// </summary>
		event EventHandler<GenericSingleEventArgs<bool>> SystemStateChangeRequest;

		/// <summary>
		/// Triggered whenever the connection to the underlying device has changed.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>> OnlineStatusChanged;

		/// <summary>
		/// Triggered when the user requests to change the global video freeze state.
		/// </summary>
		event EventHandler GlobalFreezeToggleRequest;

		/// <summary>
		/// Triggered when the user requests to change the global video blank state.
		/// </summary>
		event EventHandler GlobalBlankToggleRequest;

		/// <summary>
		/// Gets a value indicating whether the panel has been initialized and connected with the interface.
		/// true = is initialized, false = not yet initialized.
		/// </summary>
		bool IsInitialized { get; }

		/// <summary>
		/// Gets a value indicating whether the connected interface hardware is online with the control system.
		/// True = device is online, false = devices is offline.
		/// </summary>
		bool IsOnline { get; }

		/// <summary>
		/// Gets a value indicating whether the user interface is an XPanel/support interface.
		/// </summary>
		bool IsXpanel { get; }

		/// <summary>
		/// The unique identifier used when searching for or referencing this device.
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Update the UI to show either the in-use/active pages or the standby pages.
		/// </summary>
		/// <param name="state">true = the system is currently being used.
		/// false = the system is currently in standby mode.</param>
		void SetSystemState(bool state);

		/// <summary>
		/// Display a notice on the UI indicating that the system is transitioning
		/// from standby to active or active to standby.
		/// </summary>
		/// <param name="state">True = show changing to active, false = show changing to standby.</param>
		void ShowSystemStateChanging(bool state);

		/// <summary>
		/// Hide any notifications that indicate the system is changing state and notify the user that
		/// the change is complete.
		/// </summary>
		void HideSystemStateChanging();

		/// <summary>
		/// Update the user interface with the current status of the global video freeze.
		/// </summary>
		/// <param name="state">true = freeze active, false = normal video streaming.</param>
		void SetGlobalFreezeState(bool state);

		/// <summary>
		/// Update the user interface with the current status of the global video blank.
		/// </summary>
		/// <param name="state">true = blank is active, false = normal video operation.</param>
		void SetGlobalBlankState(bool state);

		/// <summary>
		/// Prepare the interface for initialization by defining the general configuration.
		/// </summary>
		/// <param name="uiData">The configuration data object that represents the UI being created.</param>
		void SetUiData(UserInterfaceDataContainer uiData);

		/// <summary>
		/// Call once all configuration information has been set. This will prepare any internal objects for connection.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Call once all necessary data as been populated and Initialize() has been successfully called. This will open a connection
		/// with the interface hardware.
		/// </summary>
		void Connect();
	}
}
