namespace pkd_domain_service.Data.DisplayData
{
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public class CustomCommands
	{
		[JsonProperty("FreezeOnTx")]
		public string FreezeOnTx { get; set; }

		[JsonProperty("FreezeOnRx")]
		public string FreezeOnRx { get; set; }

		[JsonProperty("FreezeOffTx")]
		public string FreezeOffTx { get; set; }

		[JsonProperty("FreezeOffRx")]
		public string FreezeOffRx { get; set; }

		public override string ToString()
		{
			return string.Format(
				"CustomCommands: FreezeOnTx: {0}, FreezeOnRx: {1}, FreezeOffTx: {2}, FreezeOffTx: {3}",
				FreezeOnTx ?? "NULL",
				FreezeOnRx ?? "NULL",
				FreezeOffTx ?? "NULL",
				FreezeOffTx ?? "NULL");
		}
	}
}
