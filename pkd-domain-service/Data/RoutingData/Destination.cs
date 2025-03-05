namespace pkd_domain_service.Data.RoutingData
{
	public class Destination : BaseData
	{
		public string Label { get; set; } = string.Empty;

		public string Icon { get; set; } = string.Empty;

		public string Matrix { get; set; } = string.Empty;

		public int Output { get; set; }

		public int RoutingGroup { get; set; }

		public List<string> Tags { get; set; } = [];
	}
}
