using Newtonsoft.Json;
using System.Text;

namespace pkd_domain_service.Data.EndpointData
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Endpoint : BaseData
	{
		[JsonProperty("Port")]
		public int Port { get; set; }

		[JsonProperty("Host")]
		public string Host { get; set; }

		[JsonProperty("Class")]
		public string Class { get; set; }

		[JsonProperty("Relays")]
		public int[] Relays { get; set; }

		[JsonProperty("Comports")]
		public int[] Comports { get; set; }

		[JsonProperty("IrPorts")]
		public int[] IrPorts { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("Endpoint ").Append(Id ?? "NO ID").Append(":\n\r");
			bldr.Append("Host = ").Append(Host ?? "NULL");
			bldr.Append("\n\rClass = ").Append(Class?? "NULL");
			if (Relays == null)
			{
				bldr.Append("\n\rRelays: NULL");
			}
			else
			{
				bldr.Append("\n\rRelays: [ ");
				foreach (var relay in Relays)
				{
					bldr.Append(relay).Append(", ");
				}

				bldr.Append("]");
			}

			if (Comports == null)
			{
				bldr.Append("\n\rComports: NULL");
			}
			else
			{
				bldr.Append("Comports: [");
				foreach (var port in Comports)
				{
					bldr.Append(port).Append(", ");
				}

				bldr.Append("]");
			}

			if (IrPorts == null)
			{
				bldr.Append("\n\rIrPorts: NULL");
			}
			else
			{
				bldr.Append("\n\rIrPorts: [ ");
				foreach (var port in IrPorts)
				{
					bldr.Append(port).Append(", ");
				}

				bldr.Append("]");
			}

			return bldr.ToString();
		}
	}

}
