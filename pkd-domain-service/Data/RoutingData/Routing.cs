namespace pkd_domain_service.Data.RoutingData
{
	public class Routing : BaseData
	{
		public bool RouteOnSourceSelect { get; set; }

		public string StartupSource { get; set; } = string.Empty;

		public List<Source> Sources { get; set; } = [];

		public List<Destination> Destinations { get; set; } = [];

		public List<MatrixData> MatrixData { get; set; } = [];

		public List<MatrixEdge> MatrixEdges { get; set; } = [];
	}
}
