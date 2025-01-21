namespace pkd_ui_service.Interfaces
{
	using pkd_application_service.LightingControl;
	using pkd_common_utils.GenericEventArgs;
	using System;
	using System.Collections.ObjectModel;

	public interface ILightingUserInterface
	{
		/// <summary>
		/// Triggered when the user requests to change the selected lighting scene on the target controller.
		/// Arg1 = controller id, arg2 = scene id.
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, string>>? LightingSceneRecallRequest;

		/// <summary>
		/// Triggered when the user requests to change the load level of a target lighting zone.
		/// arg1 = controller id, arg2 = zone id, arg3 = level to set.
		/// </summary>
		event EventHandler<GenericTrippleEventArgs<string, string, int>>? LightingLoadChangeRequest;

		/// <summary>
		/// Update the UI with a new collection of lighting controllers and their associated zones/scenes.
		/// </summary>
		/// <param name="lightingData">The collection of all controllable lighting devices.</param>
		void SetLightingData(ReadOnlyCollection<LightingControlInfoContainer> lightingData);

		/// <summary>
		/// Update the UI with the currently selected scene for a target lighting controller.
		/// </summary>
		/// <param name="controlId">the unique ID of the controller being updated.</param>
		/// <param name="sceneId">The unique ID of the scene that is active.</param>
		void UpdateActiveLightingScene(string controlId, string sceneId);

		/// <summary>
		/// Update the UI with the current load level of the zone on the target controller.
		/// </summary>
		/// <param name="controlId">The unique ID of the lighting controller.</param>
		/// <param name="zoneId">The unique ID of the zone being updated.</param>
		/// <param name="level">The new load level of the target zone.</param>
		void UpdateLightingZoneLoad(string controlId, string zoneId, int level);
	}
}
