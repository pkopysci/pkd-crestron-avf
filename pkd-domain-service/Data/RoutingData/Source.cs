namespace pkd_domain_service.Data.RoutingData
{
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using System.Text;

	[JsonObject(MemberSerialization.OptIn)]
	public class Source : BaseData
	{
		public static readonly Source Empty = new Source()
		{
			Id = "SRCEMPTY",
			Label = "EMPTY SOURCE",
			Icon = "alert",
			Input = 0,
			Control = string.Empty,
			Matrix = string.Empty,
			Tags = new List<string>()
		};

		[JsonProperty("Label")]
		public string Label { get; set; }

		[JsonProperty("Icon")]
		public string Icon { get; set; }

		[JsonProperty("Control")]
		public string Control { get; set; }

		[JsonProperty("Matrix")]
		public string Matrix { get; set; }

		[JsonProperty("Input")]
		public int Input { get; set; }

		[JsonProperty("Tags")]
		public List<string> Tags { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("Source ").Append(Id ?? "NULL").Append(":\n\r");
			bldr.Append("Icon = ").Append(Icon ?? "NULL").Append(", ");
			bldr.Append("Matrix = ").Append(Matrix ?? "NULL").Append(", ");
			bldr.Append("Input = ").Append(Input).Append(", ");
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
