namespace pkd_domain_service.Data
{
	/// <summary>
	/// Base data object for all configuration items.
	/// </summary>
	public class BaseData
	{
		/// <summary>
		/// Gets or sets a Unique identifier for the data object.
		/// Used to reference the information durring runtime.
		/// </summary>
		public string Id { get; set; } = string.Empty;
	}
}
