namespace pkd_domain_service.Data.EndpointData
{
	public class Endpoint : BaseData
	{
		public int Port { get; set; }

		public string Host { get; set; } = string.Empty;

		public string Class { get; set; } = string.Empty;

		public int[] Relays { get; set; } = [];

		public int[] Comports { get; set; } = [];

		public int[] IrPorts { get; set; } = [];

	}
}
