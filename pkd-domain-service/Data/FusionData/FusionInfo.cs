using Newtonsoft.Json;

namespace pkd_domain_service.Data.FusionData
{
	[JsonObject(MemberSerialization.OptIn)]
	public class FusionInfo : BaseData
	{
		[JsonProperty("RoomName")]
		public string RoomName { get; set; }

		[JsonProperty("GUID")]
		public string GUID { get; set; }

		[JsonProperty("IpId")]
		public int IpId { get; set; }

		public override string ToString()
		{
			return string.Format(
				"FusionInfo {0}: RoomName = {1}, GUID = {2}, IpId = {3}",
				Id  ?? "NO ID",
				RoomName  ?? "NULL",
				GUID  ?? "NULL",
				IpId);
		}
	}
}
