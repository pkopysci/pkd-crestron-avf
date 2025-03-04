namespace pkd_domain_service.Data.LightingData
{
	using pkd_domain_service.Data.ConnectionData;
	using System.Collections.Generic;

	public class LightingInfo : BaseData
	{
		public string Label { get; set; } = string.Empty;

		public string ClassName { get; set; } = string.Empty;

		public string StartupSceneId { get; set; } = string.Empty;

		public string ShutdownSceneId { get; set; } = string.Empty;

		public List<string> Tags { get; set; } = [];

		public Connection Connection { get; set; } = new();

		public List<LightingAttribute> Zones { get; set; } = [];

		public List<LightingAttribute> Scenes { get; set; } = [];
	}
}
