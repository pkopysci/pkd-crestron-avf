namespace pkd_domain_service.Data.RoomInfoData
{
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public class Logic
	{
		[JsonProperty("AppServiceLibrary")]
		public string AppServiceLibrary { get; set; }

		[JsonProperty("AppServiceClass")]
		public string AppServiceClass { get; set; }

		public override string ToString()
		{
			return string.Format(
				"Logic: AppServiceLibrary = {0}, AppServiceClass = {1}",
				AppServiceLibrary ?? "NULL",
				AppServiceClass ?? "NULL");
		}
	}
}
