namespace pkd_domain_service.Data.DspData
{
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public class Preset : BaseData
	{
		[JsonProperty("Bank")]
		public string Bank { get; set; }

		[JsonProperty("Index")]
		public int Index { get; set; }


		public override string ToString()
		{
			return string.Format(
				"Preset {0}: Bank = {1}, Index = {2}",
				Id ?? "NO ID",
				Bank ?? "NULL",
				Index);
		}
	}
}
