namespace pkd_domain_service.Data.RoutingData
{
	public class Source : BaseData
	{
		public static readonly Source Empty = new()
		{
			Id = "SRCEMPTY",
			Label = "EMPTY SOURCE",
			Icon = "alert",
			Input = 0,
			Control = string.Empty,
			Matrix = string.Empty,
			Tags = new List<string>()
		};

		public string Label { get; set; } = string.Empty;

		public string Icon { get; set; } = string.Empty;

		public string Control { get; set; } = string.Empty;

		public string Matrix { get; set; } = string.Empty;

		public int Input { get; set; }

		public List<string> Tags { get; set; } = [];
	}
}
