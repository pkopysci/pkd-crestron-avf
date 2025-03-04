namespace pkd_domain_service.Data.ConnectionData
{
	/// <summary>
	/// Configuration data object for TCP/IP, RS-232, or IR control of a device.
	/// </summary>
	public class Connection : BaseData
	{
		/// <summary>
		/// Gets or sets the communication method (tcp, ir, or serial).
		/// </summary>
		public string Transport { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the DLL that should be loaded when using Crestron Certified Drivers.
		/// </summary>
		public string Driver { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the IP address or hostname used to control the device.
		/// if Transport is serial or IR, then this should contain either 'control' for the root control system,
		/// or the ID of the endpoint the device is connected to.
		/// </summary>
		public string Host { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the TCP/IP, rs-232, or ir port used to control the device.
		/// </summary>
		public int Port { get; set; } = 0;

		/// <summary>
		/// Gets or sets the credentials used to log into the device.
		/// </summary>
		public Authentication Authentication { get; set; } = new();

		/// <summary>
		/// Gets or sets the serial communication protocol if the device is serial controlled.
		/// </summary>
		public ComSpec ComSpec { get; set; } = new();
	}
}
