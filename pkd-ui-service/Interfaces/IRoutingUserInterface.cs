using System.Collections.ObjectModel;
using pkd_application_service.AvRouting;
using pkd_application_service.Base;
using pkd_common_utils.GenericEventArgs;

namespace pkd_ui_service.Interfaces
{
	/// <summary>
	/// Required events, methods and properties for implementing a user interface that supports AV routing.
	/// </summary>
	public interface IRoutingUserInterface
	{
		/// <summary>
		/// Triggered when the user requests to route an AV input to an AV output.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>>? AvRouteChangeRequest;

		/// <summary>
		/// Update the user interface with the routing status of the target output.
		/// </summary>
		/// <param name="inputInfo">The input information for updating the UI routing status.</param>
		/// <param name="outputId">The unique ID of the output being updated.</param>
		void UpdateAvRoute(AvSourceInfoContainer inputInfo, string outputId);

		/// <summary>
		/// Configure the interface with the routing source and destination information.
		/// </summary>
		/// <param name="sources">Collection of all source data in the system configuration.</param>
		/// <param name="destinations">Collection of all destinations in the system configuration.</param>
		/// <param name="avRouters">Collection of all AVR devices in the system configuration.</param>
		void SetRoutingData(ReadOnlyCollection<AvSourceInfoContainer> sources, ReadOnlyCollection<InfoContainer> destinations, ReadOnlyCollection<InfoContainer> avRouters);
		
		/// <param name="avrId">the id of the AV router that is being updated.</param>
		/// <param name="isOnline">true = device is online, false = device is offline.</param>
		void UpdateAvRouterConnectionStatus(string avrId, bool isOnline);
	}
}
