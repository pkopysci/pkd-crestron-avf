namespace pkd_domain_service.Data.ConnectionData
{
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public class ComSpec
	{
		[JsonProperty("Protocol")]
		public string Protocol { get; set; }

		[JsonProperty("BaudRate")]
		public int BaudRate { get; set; }

		[JsonProperty("DataBits")]
		public int DataBits { get; set; }

		[JsonProperty("StopBits")]
		public int StopBits { get; set; }

		[JsonProperty("HwHandshake")]
		public string HwHandshake { get; set; }

		[JsonProperty("SwHandshake")]
		public string SwHandshake { get; set; }

		[JsonProperty("Parity")]
		public string Parity { get; set; }

		public override string ToString()
		{
			return string.Format(
				"ComSpec: Protocol = {0}, BaudRate = {1}, DataBits = {2}, StopBits = {3}, HWHandshake = {4}, SwHandshake = {5}, Parity = {6}",
				Protocol?? "NULL",
				BaudRate,
				DataBits,
				StopBits,
				HwHandshake ?? "NULL",
				SwHandshake ?? "NULL",
				Parity ?? "NULL");
		}
	}
}
