namespace pkd_domain_service.Data.DspData
{
	using System.Collections.Generic;

	public class Audio
	{
		public List<Dsp> Dsps { get; set; } = [];

		public List<Channel> Channels { get; set; } = [];
	}
}
