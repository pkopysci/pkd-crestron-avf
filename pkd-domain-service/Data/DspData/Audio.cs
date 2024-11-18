namespace pkd_domain_service.Data.DspData
{
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using System.Text;

	[JsonObject(MemberSerialization.OptIn)]
	public class Audio
	{
		[JsonProperty("Dsps")]
		public List<Dsp> Dsps { get; set; }

		[JsonProperty("Channels")]
		public List<Channel> Channels { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("Audio:\n\rDSPs:\n\r");
			if (Dsps == null)
			{
				bldr.Append("NULL\n\r");
			}
			else
			{
				foreach (var dsp in Dsps)
				{
					bldr.Append(dsp.ToString());
					bldr.Append("\n\r");
				}
			}

			bldr.Append("Channels:\n\r");
			if (Channels == null)
			{
				bldr.Append("NULL\n\r");
			}
			else
			{
				foreach (var channel in Channels)
				{
					bldr.Append(channel.ToString());
					bldr.Append("\n\r");
				}
			}

			return bldr.ToString();

		}
	}

}
