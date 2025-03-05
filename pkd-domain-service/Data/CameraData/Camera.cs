using pkd_domain_service.Data.ConnectionData;

namespace pkd_domain_service.Data.CameraData
{
	public class Camera : BaseData
	{
		public string Label { get; set; } = string.Empty;
		public string ClassName { get; set; } = string.Empty;
		public Connection Connection { get; set; } = new();
		public List<PresetData> Presets { get; set; } = [];
		public List<string> Tags { get; set; } = [];
	}
}
