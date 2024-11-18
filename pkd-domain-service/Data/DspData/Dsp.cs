namespace pkd_domain_service.Data.DspData
{
	using System.Collections.Generic;
	using System.Text;
	using pkd_domain_service.Data.ConnectionData;
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public class Dsp : BaseData
	{
		[JsonProperty("CoreId")]
		public int CoreId { get; set; }

		[JsonProperty("Dependencies")]
		public List<string> Dependencies { get; set; }

		[JsonProperty("Connection")]
		public Connection Connection { get; set; }

		[JsonProperty("Presets")]
		public List<Preset> Presets { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("DSP ").Append(Id ?? "NULL ID").Append(":\n\r");
			if (Dependencies == null)
			{
				bldr.Append("Dependencies: NULL\n\r");
			}
			else
			{
				bldr.Append("Dependencies:\n\r");
				foreach (var dep in Dependencies)
				{
					bldr.Append(dep).Append("\n\r");
				}
				bldr.Append("\n\r");
			}

			if (Presets == null)
			{
				bldr.Append("Presets: NULL\n\r");
			}
			else
			{
				bldr.Append("Presets:\n\r");
				foreach (var preset in Presets)
				{
					bldr.Append(preset).Append("\n\r");
				}
			}

			return bldr.ToString();
		}
	}
}
