namespace pkd_domain_service.Data.CameraData
{
	using pkd_domain_service.Data.ConnectionData;
	using Newtonsoft.Json;
	using System.Text;

	[JsonObject(MemberSerialization.OptIn)]
	public class Camera : BaseData
	{
		[JsonProperty("Model")]
		public string Model { get; set; }

		[JsonProperty("Label")]
		public string Label { get; set; }

		[JsonProperty("Connection")]
		public Connection Connection { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append(string.Format(
				"Camera {0} - {1}\n",
				Model ?? "NULL",
				Label ?? "NULL"
			));

			bldr.Append("Connection Info:\n");
			bldr.Append(Connection.ToString());

			return bldr.ToString();
		}
	}
}
