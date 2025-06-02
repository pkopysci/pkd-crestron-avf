using pkd_domain_service.Data.CameraData;
using pkd_domain_service.Data.DisplayData;
using pkd_domain_service.Data.DspData;
using pkd_domain_service.Data.EndpointData;
using pkd_domain_service.Data.FusionData;
using pkd_domain_service.Data.LightingData;
using pkd_domain_service.Data.RoomInfoData;
using pkd_domain_service.Data.RoutingData;
using pkd_domain_service.Data.TransportDeviceData;
using pkd_domain_service.Data.UserInterfaceData;
using pkd_domain_service.Data.VideoWallData;

namespace pkd_domain_service.Data
{
	/// <summary>
	/// Object representation of the JSON configuration file.
	/// </summary>
	public class DataContainer
	{
		public ServerInfo ServerInfo { get; set; } = new();

		public RoomInfo RoomInfo { get; set; } = new();

		public List<UserInterface> UserInterfaces { get; set; } = [];

		public List<Display> Displays { get; set; } = [];

		public Routing Routing { get; set; } = new();

		public Audio Audio { get; set; } = new();

		public List<Camera> Cameras { get; set; } = [];

		public List<Bluray> Blurays { get; set; } = [];

		public List<CableBox> CableBoxes { get; set; } = [];

		public List<LightingInfo> LightingControllers { get; set; } = [];

		public List<Endpoint> Endpoints { get; set; } = [];
		
		public List<VideoWall> VideoWalls { get; set; } = [];

		public FusionInfo FusionInfo { get; set; } = new();
	}
}
