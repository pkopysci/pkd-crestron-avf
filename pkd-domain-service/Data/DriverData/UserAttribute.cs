namespace pkd_domain_service.Data.DriverData
{
	/// <summary>
	/// Configuration item for a tcp or serial connection.
	/// Valid values:
	/// String, Number, Hex, Boolean.
	/// </summary>
	public class UserAttribute : BaseData
	{
		public string DataType { get; set; } = string.Empty;

		public string Value { get; set; } = string.Empty;
	}
}
