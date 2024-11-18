namespace pkd_domain_service.Data.LightingData
{
	using Newtonsoft.Json;
	using pkd_domain_service.Data.ConnectionData;
	using System.Collections.Generic;
	using System.Text;

	[JsonObject(MemberSerialization.OptIn)]
	public class LightingInfo : BaseData
	{
		[JsonProperty("Label")]
		public string Label { get; set; }

		[JsonProperty("ClassName")]
		public string ClassName { get; set; }

		[JsonProperty("StartupSceneId")]
		public string StartupSceneId { get; set; }

		[JsonProperty("ShutdownSceneId")]
		public string ShutdownSceneId { get; set; }

		[JsonProperty("Tags")]
		public List<string> Tags { get; set; }

		[JsonProperty("Connection")]
		public Connection Connection { get; set; }

		[JsonProperty("Zones")]
		public List<LightingAttribute> Zones { get; set; }

		[JsonProperty("Scenes")]
		public List<LightingAttribute> Scenes { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("LightingInfo ").Append(Id ?? "NO ID");
			bldr.Append("\n\rClassName: ").Append(ClassName ?? "NULL").Append("\n\r");
			bldr.Append(Connection.ToString()).Append("\n\r");

			if (Tags == null)
			{
				bldr.Append("TAGS: NULL\n\r");
			}
			else
			{
				bldr.Append("Tags: [");
				foreach (var tag in Tags)
				{
					bldr.Append(tag).Append(", ");
				}

				bldr.Append("]\n\r");
			}

			if (Zones == null)
			{
				bldr.Append("Zones: NULL\n\r");
			}
			else
			{
				bldr.Append("Zones:\n\r");
				foreach (var zone in Zones)
				{
					bldr.Append(zone.ToString()).Append("\n\r");
				}
			}

			if (Scenes == null)
			{
				bldr.Append("Scenes: NULL\n\r");
			}
			else
			{
				bldr.Append("Scenes:\n\r");
				foreach (var scene in Scenes)
				{
					bldr.Append(scene).Append("\n\r");
				}
			}

			return bldr.ToString();
		}
	}
}
