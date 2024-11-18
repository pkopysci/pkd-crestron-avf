using Newtonsoft.Json;

namespace pkd_domain_service.Data.RoutingData
{
	[JsonObject(MemberSerialization.OptIn)]
	public class MatrixEdge : BaseData
	{
		[JsonProperty("StartNodeId")]
		public string StartNodeId { get; set; }

		[JsonProperty("EndNodeId")]
		public string EndNodeId { get; set; }

		public override string ToString()
		{
			return string.Format(
				"MatrixEdge {0}: StartNodeId = {1}, EndNodeId = {2}",
				Id ?? "NO ID",
				StartNodeId ?? "NULL",
				EndNodeId?? "NULL");
		}
	}
}
