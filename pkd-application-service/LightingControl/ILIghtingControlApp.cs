using System.Collections.ObjectModel;
using pkd_common_utils.GenericEventArgs;

namespace pkd_application_service.LightingControl
{
	/// <summary>
	/// Required events, methods, and properties for implementing an application service that supports lighting controls.
	/// </summary>
	public interface ILightingControlApp
	{
		/// <summary>
		/// Triggered whenever a lighting controller reports that a zone load level has changed.
		/// args package is: arg1 = controller ID, arg 2 = zone ID.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>> LightingLoadLevelChanged;

		/// <summary>
		/// Triggered whenever a lighting control reports that a new scene has been recalled.
		/// Args package is the ID of the controller that reported a change.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>> LightingSceneChanged;

		/// <summary>
		/// Triggered when any lighting controller device reports offline/online status.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, bool>> LightingControlConnectionChanged;

		/// <summary>
		/// Request to recall a saved lighting scene on the target controller.
		/// </summary>
		/// <param name="deviceId">The unique ID of the lighting controller to change.</param>
		/// <param name="sceneId">The unique ID of the scene to recall.</param>
		void RecallLightingScene(string deviceId, string sceneId);

		/// <summary>
		/// Request to change the load level of a zone on the target controller.
		/// </summary>
		/// <param name="deviceId">The unique ID of the lighting controller to change.</param>
		/// <param name="zoneId">The unique ID of the zone to change.</param>
		/// <param name="level">0-100 value representing the load level to set on the lighting zone.</param>
		void SetLightingLoad(string deviceId, string zoneId, int level);

		/// <summary>
		/// Get the currently active scene on the lighting controller if scenes are supported.
		/// </summary>
		/// <param name="deviceId">The unique ID of the lighting controller to query.</param>
		/// <returns>the unique ID of the active scene associated with the lighting controller.</returns>
		string GetActiveScene(string deviceId);

		/// <summary>
		/// Get the current load level of the target lighting zone.
		/// </summary>
		/// <param name="deviceId">The unique ID of the lighting controller to query.</param>
		/// <param name="zoneId">The unique ID of the zone to query.</param>
		/// <returns>0-100 value representing the current lighting load level.</returns>
		int GetZoneLoad(string deviceId, string zoneId);

		/// <summary>
		/// Get the configuration information for all lighting controllers in the system.
		/// </summary>
		/// <returns>A collection of lighting controller data for all devices in the system.</returns>
		ReadOnlyCollection<LightingControlInfoContainer> GetAllLightingDeviceInfo();
	}
}
