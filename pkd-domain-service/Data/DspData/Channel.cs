
namespace pkd_domain_service.Data.DspData
{
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using System.Text;

	[JsonObject(MemberSerialization.OptIn)]
	public class Channel : BaseData
	{
		[JsonProperty("LevelControlTag")]
		public string LevelControlTag { get; set; }

		[JsonProperty("MuteControlTag")]
		public string MuteControlTag { get; set; }

		[JsonProperty("RouterControlTag")]
		public string RouterControlTag { get; set; }

		[JsonProperty("DspId")]
		public string DspId { get; set; }

		[JsonProperty("RouterIndex")]
		public int RouterIndex { get; set; }

		[JsonProperty("BankIndex")]
		public int BankIndex { get; set; }

		[JsonProperty("Label")]
		public string Label { get; set; }

		[JsonProperty("Icon")]
		public string Icon { get; set; }

		[JsonProperty("LevelMax")]
		public int LevelMax { get; set; }

		[JsonProperty("LevelMin")]
		public int LevelMin { get; set; }

		[JsonProperty("ZoneEnableToggles")]
		public List<ZoneEnableToggle> ZoneEnableToggles { get; set; }

		[JsonProperty("Tags")]
		public List<string> Tags { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("Channel ").Append(Id ?? "NO ID");
			bldr.Append("\n\rLevelControlTag = ").Append(LevelControlTag ?? "NULL");
			bldr.Append("\n\rMuteControlTag = ").Append(MuteControlTag ?? "NULL");
			bldr.Append("\n\rDspId: ").Append(DspId ?? "NULL");
			bldr.Append("\n\rRouterIndex = ").Append(RouterIndex);
			bldr.Append("\n\rBankIndex = ").Append(BankIndex);
			bldr.Append("\n\rLabel: ").Append(Label ?? "NULL");
			bldr.Append("\n\rIcon: ").Append(Icon ?? "NULL");
			bldr.Append("\n\rLevelMin = ").Append(LevelMin).Append(", LevelMax = ").Append(LevelMax);
			bldr.Append("ZoneEnableToggles:");
			if (ZoneEnableToggles == null)
			{
				bldr.Append("NULL\n\r");
			}
			else
			{
				bldr.Append("\n\r");
				foreach (var toggle in ZoneEnableToggles)
				{
					bldr.Append(toggle.ToString()).Append("\n\r");
				}
			}

			bldr.Append("Tags:");
			if (Tags == null)
			{
				bldr.Append(" NULL\n\r");
			}
			else
			{
				bldr.Append(" [");
				foreach (var tag in Tags)
				{
					bldr.Append(tag).Append(", ");
				}

				bldr.Append("]");
			}

			return bldr.ToString();
		}
	}
}
