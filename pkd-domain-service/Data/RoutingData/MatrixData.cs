namespace pkd_domain_service.Data.RoutingData
{
	using pkd_domain_service.Data.ConnectionData;

	public class MatrixData : BaseData
	{
		public string Label { get; set; } = string.Empty;

		public string ClassName { get; set; } = string.Empty;

		public int Inputs { get; set; }

		public int Outputs { get; set; }

		public Connection Connection { get; set; } = new();
	}
}
