namespace pkd_domain_service.Data.ConnectionData
{
	public class ComSpec
	{
		public string Protocol { get; set; } = string.Empty;

		public int BaudRate { get; set; }

		public int DataBits { get; set; }

		public int StopBits { get; set; }

		public string HwHandshake { get; set; } = string.Empty;

		public string SwHandshake { get; set; } = string.Empty;

		public string Parity { get; set; } = string.Empty;
	}
}
