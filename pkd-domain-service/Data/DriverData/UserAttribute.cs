namespace pkd_domain_service.Data.DriverData
{
	using Newtonsoft.Json;

	/// <summary>
	/// Configuration item for a tcp or serial connection.
	/// Valid values:
	/// String, Number, Hex, Boolean.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class UserAttribute : BaseData
	{
		[JsonProperty("DataType")]
		public string DataType { get; set; }

		[JsonProperty("Value")]
		public string Value { get; set; }

		public override string ToString()
		{
			return string.Format(
				"UserAttribute: DataType: {0}, Value: {1}",
				DataType ?? "NULL",
				Value ?? "NULL");
		}
	}
}
