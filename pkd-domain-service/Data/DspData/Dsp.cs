namespace pkd_domain_service.Data.DspData
{
	using System.Collections.Generic;
	using pkd_domain_service.Data.ConnectionData;

	public class Dsp : BaseData
	{
		public int CoreId { get; set; }

		public List<string> Dependencies { get; set; } = [];

		public Connection Connection { get; set; } = new();

		public List<Preset> Presets { get; set; } = [];
	}
}
