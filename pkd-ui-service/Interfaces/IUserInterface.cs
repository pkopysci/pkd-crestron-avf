using pkd_application_service.UserInterface;
using pkd_common_utils.GenericEventArgs;

namespace pkd_ui_service.Interfaces
{
	/// <summary>
	/// required events, methods, and properties for implementing any user interface.
	/// </summary>
	public interface IUserInterface
	{
		/// <summary>
		/// Triggered whenever the connection to the underlying device has changed.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>> OnlineStatusChanged;

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
