using pkd_domain_service.Data.ConnectionData;

namespace pkd_domain_service.Data.DspData
{
	public class Dsp : BaseData
	{
		public int CoreId { get; set; }

		public List<string> Dependencies { get; set; } = [];

		public Connection Connection { get; set; } = new();

		public List<Preset> Presets { get; set; } = [];
	}
}
