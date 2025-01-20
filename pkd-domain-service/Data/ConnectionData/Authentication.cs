namespace pkd_domain_service.Data.ConnectionData
{
	/// <summary>
	/// Login information used to connect to a device for TCP/IP control.
	/// </summary>
	/// 
	public class Authentication : BaseData
	{
		/// <summary>
		/// Gets or sets the username used to log into the target device for control.
		/// </summary>
		public string UserName { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the password used to log into the target device for control.
		/// </summary>
		public string Password { get; set; } = string.Empty;
	}
}
