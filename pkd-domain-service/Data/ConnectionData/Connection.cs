namespace pkd_domain_service.Data.ConnectionData
{
	using Newtonsoft.Json;

	/// <summary>
	/// Configuration data object for TCP/IP, RS-232, or IR control of a device.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class Connection : BaseData
	{
		/// <summary>
		/// Gets or sets the communication method (tcp, ir, or serial).
		/// </summary>
		[JsonProperty("Transport")]
		public string Transport { get; set; }

		/// <summary>
		/// Gets or sets the DLL that should be loaded when using Crestron Certified Drivers.
		/// </summary>
		[JsonProperty("Driver")]
		public string Driver { get; set; }

		/// <summary>
		/// Gets or sets the IP address or hostname used to control the device.
		/// if Transport is serial or IR, then this should contain either 'control' for the root control system,
		/// or the ID of the endpoint the device is connected to.
		/// </summary>
		[JsonProperty("Host")]
		public string Host { get; set; }

		/// <summary>
		/// Gets or sets the TCP/IP, rs-232, or ir port used to control the device.
		/// </summary>
		[JsonProperty("Port")]
		public int Port { get; set; }

		/// <summary>
		/// Gets or sets the credentials used to log into the device.
		/// </summary>
		[JsonProperty("Authentication")]
		public Authentication Authentication { get; set; }

		/// <summary>
		/// Gets or sets the serial communication protocol if the device is serial controlled.
		/// </summary>
		[JsonProperty("ComSpec")]
		public ComSpec ComSpec { get; set; }

		public override string ToString()
		{
			return string.Format(
				"Connection: Transport = {0}, Driver = {1}, Host = {2}, Port = {3}\n{4}\n{5}",
				Transport ?? "NULL",
				Driver ?? "NULL",
				Host ?? "NULL",
				Port,
				Authentication == null ? "NULL AUTHENTICATION" : Authentication.ToString(),
				ComSpec == null ? "NULL COMSPECT" : ComSpec.ToString());
		}
	}
}
