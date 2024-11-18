
namespace pkd_domain_service.Data.LightingData
{
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using System.ComponentModel;

	[JsonObject(MemberSerialization.OptIn)]
	public class LightingAttribute : BaseData
	{
		[JsonProperty("Label")]
		public string Label { get; set; }

		[JsonProperty("Index")]
		public int Index { get; set; }

		[JsonProperty("Tags")]
		public List<string> Tags { get; set; }

		public override string ToString()
		{
			return $"[{this.Id ?? "NO ID"}] - {Label ?? "NO LABEL"}\n\r";
		}
	}
}
