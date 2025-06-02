namespace pkd_domain_service.Data.LightingData
{
	public class LightingAttribute : BaseData
	{
		public string Label { get; set; } = string.Empty;

		public int Index { get; set; }

		public List<string> Tags { get; set; } = [];
	}
}
