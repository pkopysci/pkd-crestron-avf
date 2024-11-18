namespace pkd_domain_service.Data.TransportDeviceData
{
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public class TransportFavorite : BaseData
	{
		[JsonProperty("Label")]
		public string Label { get; set; }
		
		[JsonProperty("Number")]
		public string Number { get; set; }

		public override string ToString()
		{
			return string.Format(
				"TransportFavorite {0}: Label = {1}, Number = {2}",
				Id ?? "NO ID",
				Label ?? "NULL",
				Number ?? "NULL");
		}
	}
}
