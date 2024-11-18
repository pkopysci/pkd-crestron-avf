namespace pkd_domain_service.Data.RoutingData
{
	using pkd_domain_service.Data.ConnectionData;
	using Newtonsoft.Json;
	using System.Text;

	[JsonObject(MemberSerialization.OptIn)]
	public class MatrixData : BaseData
	{
		[JsonProperty("Label")]
		public string Label { get; set; }

		[JsonProperty("ClassName")]
		public string ClassName { get; set; }

		[JsonProperty("Inputs")]
		public int Inputs { get; set; }

		[JsonProperty("Outputs")]
		public int Outputs { get; set; }

		[JsonProperty("Connection")]
		public Connection Connection { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("MatrixData ").Append(Id ?? "NULL").Append(":\n\r");
			bldr.Append("ClassName = ").Append(ClassName ?? "NULL").Append(", ");
			bldr.Append("Inputs = ").Append(Inputs).Append(", ");
			bldr.Append("Outputs = ").Append(Outputs).Append(", ");
			bldr.Append("Connection:\n\r").Append(Connection);
			return bldr.ToString();
		}
	}
}
