namespace pkd_domain_service.Data.CameraData
{
	using pkd_domain_service.Data.ConnectionData;

	public class Camera : BaseData
	{
		public string Model { get; set; } = string.Empty;

		public string Label { get; set; } = string.Empty;

		public Connection Connection { get; set; } = new();
	}
}
