namespace pkd_domain_service.Data.DspData
{
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public class ZoneEnableToggle
	{
		[JsonProperty("ZoneId")]
		public string ZoneId { get; set; }

		[JsonProperty("Label")]
		public string Label { get; set; }

		[JsonProperty("Tag")]
		public string Tag { get; set; }

		public override string ToString()
		{
			return string.Format(
				"ZoneEnableToggle {0}: Label = {1}, Tag = {2}",
				ZoneId ?? "NO ID",
				Label ?? "NULL",
				Tag ?? "NULL");
		}
	}
}