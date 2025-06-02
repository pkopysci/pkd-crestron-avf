using System.Collections.ObjectModel;
using pkd_common_utils.GenericEventArgs;
using pkd_hardware_service.BaseDevice;

namespace pkd_hardware_service.LightingDevices;

/// <summary>
/// Required events, methods, and properties required for creating a device control plugin.
/// </summary>
public interface ILightingDevice : IBaseDevice
{
	/// <summary>
	/// triggered whenever the device reports a change in a zone lighting load.
	/// Event arg is the id of the load that changed.
	/// </summary>
	event EventHandler<GenericSingleEventArgs<string>> ZoneLoadChanged;
	
	/// <summary>
	/// Triggered whenever the device reports a change in the active lighting scene.
	/// Event arg is the id of the scene that was set to active.
	/// </summary>
	event EventHandler<GenericSingleEventArgs<string>> ActiveSceneChanged;

	/// <summary>
	/// Connect to the hardware and register any internal event handlers.
	/// </summary>
	void Initialize(
		string hostName,
		int port,
		string id,
		string label,
		string userName,
		string password,
		List<string> tags);

	/// <summary>
	/// Get the ids of all controllable zones for the device.
	/// </summary>
	ReadOnlyCollection<string> ZoneIds { get; }

	/// <summary>
	/// Get the ids of all selectable scenes for the device.
	/// </summary>
	ReadOnlyCollection<string> SceneIds { get; }

	/// <summary>
	/// Get the currently selected lighting scene.
	/// </summary>
	string ActiveSceneId { get; }

	/// <summary>
	/// Add a controllable zone reference to the device.
	/// </summary>
	/// <param name="id">The unique id of the zone, used for internal referencing.</param>
	/// <param name="label">The human-friendly name of the zone.</param>
	/// <param name="index">the 0-based index of the zone.</param>
	void AddZone(string id, string label, int index);

	/// <summary>
	/// Add a selectable scene reference to the device.
	/// </summary>
	/// <param name="id">the unique id of the scene, used for internal referencing.</param>
	/// <param name="label">the human-friendly name of the scene.</param>
	/// <param name="index">The 0-based index of the scene.</param>
	void AddScene(string id, string label, int index);

	/// <summary>
	/// Send a request to the device to recall the target scene.
	/// </summary>
	/// <param name="id">The unique id of the scene to recall.</param>
	void RecallScene(string id);

	/// <summary>
	/// set the load level of the target zone.
	/// </summary>
	/// <param name="id">The unique id of the zone to change.</param>
	/// <param name="loadLevel">a 0-100 value representing the new load.</param>
	void SetZoneLoad(string id, int loadLevel);

	/// <param name="id">the unique id of the zone to query.</param>
	/// <returns>a 0-100 value representing the current load level.</returns>
	int GetZoneLoad(string id);
}
