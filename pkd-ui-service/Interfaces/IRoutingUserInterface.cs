namespace pkd_ui_service.Interfaces
{
	using pkd_application_service.AvRouting;
	using pkd_application_service.Base;
	using pkd_common_utils.GenericEventArgs;
	using System;
	using System.Collections.ObjectModel;

	public interface IRoutingUserInterface
	{
		/// <summary>
		/// Triggered when the user requests to route an AV input to an AV output.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>> AvRouteChangeRequest;

		/// <summary>
		/// Update the user interface with the routing status of the target output.
		/// </summary>
		/// <param name="inputInfo">The input infomration for updating the UI routing status.</param>
		/// <param name="outputId">The unique ID of the output being udated.</param>
		void UpdateAvRoute(AvSourceInfoContainer inputInfo, string outputId);

		/// <summary>
		/// Configure the interface with the routing source and destination information.
		/// </summary>
		/// <param name="sources">Collection of all source data in the system configuration.</param>
		/// <param name="destinations">Collection of all destinations in the system configuration.</param>
		/// <param name="AvRouters">Collection of all AVR devices in the system configuration.</param>
		void SetRoutingData(ReadOnlyCollection<AvSourceInfoContainer> sources, ReadOnlyCollection<InfoContainer> destinations, ReadOnlyCollection<InfoContainer> AvRouters);
	}
}
