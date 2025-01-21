namespace pkd_hardware_service.LightingDevices
{
	using pkd_common_utils.GenericEventArgs;
	using BaseDevice;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	public interface ILightingDevice : IBaseDevice
	{
		event EventHandler<GenericSingleEventArgs<string>> ZoneLoadChanged;
		event EventHandler<GenericSingleEventArgs<string>> ActiveSceneChanged;

		void Initialize(
			string hostName,
			int port,
			string id,
			string label,
			string userName,
			string password,
			List<string> tags);

		ReadOnlyCollection<string> ZoneIds { get; }

		ReadOnlyCollection<string> SceneIds { get; }

		string ActiveSceneId { get; }

		void AddZone(string id, string label, int index);

		void AddScene(string id, string label, int index);

		void RecallScene(string id);

		void SetZoneLoad(string id, int loadLevel);

		int GetZoneLoad(string id);
	}

}
