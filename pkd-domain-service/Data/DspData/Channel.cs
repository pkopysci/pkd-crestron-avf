
namespace pkd_domain_service.Data.DspData
{
	public class Channel : BaseData
	{
		public string LevelControlTag { get; set; } = string.Empty;

		public string MuteControlTag { get; set; } = string.Empty;

		public string RouterControlTag { get; set; } = string.Empty;

		public string DspId { get; set; } = string.Empty;

		public int RouterIndex { get; set; }

		public int BankIndex { get; set; }

		public string Label { get; set; } = string.Empty;

		public string Icon { get; set; } = string.Empty;

		public int LevelMax { get; set; }

		public int LevelMin { get; set; }

		public List<ZoneEnableToggle> ZoneEnableToggles { get; set; } = [];

		public List<string> Tags { get; set; } = [];
	}
}
