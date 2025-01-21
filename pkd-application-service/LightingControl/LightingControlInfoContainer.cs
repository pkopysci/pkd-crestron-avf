namespace pkd_application_service.LightingControl
{
	using Base;
	using System.Collections.Generic;

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

		public List<LightingItemInfoContainer> Zones { get; private set; }

		public List<LightingItemInfoContainer> Scenes { get; private set; }

		public string StartupSceneId { get; private set; }

		public string ShutdownSceneId { get; private set; }

	}
}
