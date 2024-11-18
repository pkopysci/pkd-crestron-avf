namespace pkd_domain_service.Data.RoutingData
{
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using System.Text;

	[JsonObject(MemberSerialization.OptIn)]
	public class Destination : BaseData
	{
		[JsonProperty("Label")]
		public string Label { get; set; }

		[JsonProperty("Icon")]
		public string Icon { get; set; }

		[JsonProperty("Matrix")]
		public string Matrix { get; set; }

		[JsonProperty("Output")]
		public int Output { get; set; }

		[JsonProperty("RoutingGroup")]
		public int RoutingGroup { get; set; }

		[JsonProperty("Tags")]
		public List<string> Tags { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("Destination ").Append(Id ?? "NULL").Append(":\n\r");
			bldr.Append("Icon = ").Append(Icon ?? "NULL").Append(", ");
			bldr.Append("Matrix = ").Append(Matrix ?? "NULL").Append(", ");
			bldr.Append("Output = ").Append(Output).Append(", ");
			bldr.Append("RoutingGroup = ").Append(RoutingGroup);
			if (Tags == null)
			{
				bldr.Append("\n\rTags: NULL");
			}
			else
			{
				bldr.Append("\n\rTags: [ ");
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
