using System.Collections.ObjectModel;
using pkd_application_service.Base;
using pkd_common_utils.GenericEventArgs;

namespace pkd_application_service.AvRouting;

/// <summary>
/// Common properties and methods for Audio and video routing applications.
/// </summary>
public interface IAvRoutingApp
{
	/// <summary>
	/// Triggered when a destination in the routing map changes what input is displayed.
	/// Args package contains the ID of the destination that was changed.
	/// </summary>
	event EventHandler<GenericSingleEventArgs<string>> RouteChanged;

	/// <summary>
	/// Triggered when an AV routing device in the system has come online or gone offline.
	/// Args package contains the ID of the device that changed.
	/// </summary>
	event EventHandler<GenericSingleEventArgs<string>> RouterConnectChange;

	/// <summary>
	/// Request the current online/offline status of the target AV routing device.
	/// </summary>
	/// <param name="id">The unique ID of the device to query.</param>
	/// <returns>True if the device is online, false otherwise.</returns>
	bool QueryRouterConnectionStatus(string id);

	/// <summary>
	/// Query the service for all routable audio/video inputs.
	/// </summary>
	/// <returns>A data collection of all AV inputs in the system configuration.</returns>
	ReadOnlyCollection<AvSourceInfoContainer> GetAllAvSources();

	/// <summary>
	/// Query the service for all routable audio/video outputs.
	/// </summary>
	/// <returns>A data collection of all AV outputs in the system configuration.</returns>
	ReadOnlyCollection<InfoContainer> GetAllAvDestinations();

	/// <summary>
	/// Query the service for all AVR devices.
	/// </summary>
	/// <returns>A data collection representing all AV routing devices in the configuration.</returns>
	ReadOnlyCollection<InfoContainer> GetAllAvRouters();

	/// <summary>
	/// Request to route the target input to the target output.
	/// </summary>
	/// <param name="inputId">The unique ID of the input to route.</param>
	/// <param name="outputId">The unique ID of the output to be routed to.</param>
	void MakeRoute(string inputId, string outputId);

	/// <summary>
	/// Route the target input to all destinations in the system.
	/// </summary>
	/// <param name="inputId">The unique ID of the input to route.</param>
	void RouteToAll(string inputId);

	/// <summary>
	/// Print out the current state of the routing graph.
	/// </summary>
	void ReportGraph();

	/// <summary>
	/// Query the routing system for what input is currently routed to that endpoint.
	/// If no route is made then the return object will contain an ID of "NONE".
	/// </summary>
	/// <param name="outputId">The target output to query.</param>
	/// <returns>An information container with data about the currently routed source, or an id of "NONE" if no route is active.</returns>
	AvSourceInfoContainer QueryCurrentRoute(string outputId);
}