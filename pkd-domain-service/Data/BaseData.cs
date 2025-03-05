namespace pkd_domain_service.Data
{
	/// <summary>
	/// Base data object for all configuration items.
	/// </summary>
	public class BaseData
	{
		/// <summary>
		/// Gets or sets a Unique identifier for the data object.
		/// Used to reference the information during runtime.
		/// </summary>
		public string Id { get; set; } = string.Empty;
		
		/// <summary>
		/// Gets or sets the hardware manufacturer, if relevant. Defaults to the empty string.
		/// </summary>
		public string Manufacturer { get; set; } = string.Empty;
		
		/// <summary>
		/// Gets or sets the device name as defined by the manufacturer. Defaults to the empty string.
		/// </summary>
		public string Model { get; set; } = string.Empty;
	}
}
