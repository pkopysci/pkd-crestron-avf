using System.Collections.ObjectModel;
using pkd_application_service.Base;
using pkd_common_utils.GenericEventArgs;
using pkd_ui_service.Utility;

namespace pkd_ui_service.Interfaces
{
	/// <summary>
	/// Required events, methods and properties for implementing a user interface that supports transport device controls.
	/// </summary>
	public interface ITransportControlUserInterface
	{
		/// <summary>
		/// Trigger when the user requests a generic transport command (play, pause, up, down, etc.)
		/// args package: Arg1 = the id of the device to control, Arg2 = the enum of the transport triggered.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, TransportTypes>>? TransportControlRequest;
		
		/// <summary>
		/// Trigger when the user requests to dial a specific channel.
		/// Args package: Arg1 = id of the device to control, Arg2 = the channel to dial.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>>? TransportDialRequest;
		
		/// <summary>
		/// Trigger when the user requests to dial a stored favorite.
		/// Arg1 = the id of the device to control, Arg2 = the id of the preset to recall.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>>? TransportDialFavoriteRequest;

		/// <summary>
		/// Provide data on all devices in the configuration.
		/// </summary>
		/// <param name="data">A collection containg data objects for all controllable transport devices.</param>
		void SetCableBoxData(ReadOnlyCollection<TransportInfoContainer> data);
	}
}
