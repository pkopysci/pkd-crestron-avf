namespace pkd_domain_service.Data.ConnectionData
{
	using Newtonsoft.Json;

	/// <summary>
	/// Login information used to connect to a device for TCP/IP control.
	/// </summary>
	/// 
	[JsonObject(MemberSerialization.OptIn)]
	public class Authentication : BaseData
	{
		/// <summary>
		/// Gets or sets the user name used to log into the target device for control.
		/// </summary>
		[JsonProperty("UserName")]		
		public string UserName { get; set; }

		/// <summary>
		/// Gets or sets the password used to log into the target device for control.
		/// </summary>
		[JsonProperty("Password")]
		public string Password { get; set; }

		public override string ToString()
		{
			return string.Format(
				"Authentication: UserName = {0}, Password = {1}",
				UserName ?? "NULL",
				Password ?? "NULL");
		}
	}
}
