using Newtonsoft.Json;

namespace pkd_domain_service.Data
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ServerInfo
	{

		[JsonProperty("Host")]
		public string Host { get; set; }

		[JsonProperty("User")]
		public string User { get; set; }

		[JsonProperty("Key")]
		public string Key { get; set; }

		public override string ToString()
		{
			return string.Format(
				"Server Info:\nHost = {0}, User = {1}, Key = {2}",
				Host ?? "NULL",
				User ?? "NULL",
				Key ?? "NULL");
		}
	}
}
