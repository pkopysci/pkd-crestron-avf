using pkd_application_service.Base;

namespace pkd_application_service.LightingControl
{
	/// <summary>
	/// Data object representing a single lighting control object managed by the application service.
	/// </summary>
	public class LightingControlInfoContainer : InfoContainer
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="LightingControlInfoContainer"/>.
		/// </summary>
		/// <param name="id">The unique ID of the lighting control. Used for internal referencing.</param>
		/// <param name="label">The user-friendly name of the controller.</param>
		/// <param name="icon">The image tag used for referencing the UI icon.</param>
		/// <param name="startupSceneId">the id of the startup scene to recall when prompted.</param>
		/// <param name="shutdownSceneId">The id of the shutdown scene to recall when prompted.</param>
		/// <param name="tags">A collection of custom tags used by the subscribed service.</param>
		/// <param name="zones">A collection of data objects for all zones controlled by this device.</param>
		/// <param name="scenes">A collection of data objects for all scenes that can be recalled by this device.</param>
		public LightingControlInfoContainer(
			string id,
			string label,
			string icon,
			string startupSceneId,
			string shutdownSceneId,
			List<string> tags,
			List<LightingItemInfoContainer> zones,
			List<LightingItemInfoContainer> scenes)
			: base(id, label, icon, tags)
		{
			Zones = zones;
			Scenes = scenes;
			StartupSceneId = startupSceneId;
			ShutdownSceneId = shutdownSceneId;
		}

		/// <summary>
		/// A collection of all zones associated with this lighting controller.
		/// </summary>
		public List<LightingItemInfoContainer> Zones { get; private set; }

		/// <summary>
		/// A collection of all scenes associated with this lighting controller.
		/// </summary>
		public List<LightingItemInfoContainer> Scenes { get; private set; }

		/// <summary>
		/// The id of the lighting scene to recall when the system enters the active state.
		/// </summary>
		public string StartupSceneId { get; private set; }

		/// <summary>
		/// The id of the lighting scene to recall when the system enters the standby state.
		/// </summary>
		public string ShutdownSceneId { get; private set; }

	}
}
