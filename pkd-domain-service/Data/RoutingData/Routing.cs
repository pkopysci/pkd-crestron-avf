namespace pkd_domain_service.Data.RoutingData
{
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using System.Text;

	[JsonObject(MemberSerialization.OptIn)]
	public class Routing : BaseData
	{
		[JsonProperty("RouteOnSourceSelect")]
		public bool RouteOnSourceSelect { get; set; }

		[JsonProperty("StartupSource")]
		public string StartupSource { get; set; }

		[JsonProperty("Sources")]
		public List<Source> Sources { get; set; }

		[JsonProperty("Destinations")]
		public List<Destination> Destinations { get; set; }

		[JsonProperty("MatrixData")]
		public List<MatrixData> MatrixData { get; set; }

		[JsonProperty("MatrixEdges")]
		public List<MatrixEdge> MatrixEdges { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("Routing ").Append(Id ?? "NULL").Append(":\n\r");
			bldr.Append("RouteOnSourceSelect = ").Append(RouteOnSourceSelect).Append(", ");
			bldr.Append("Sources:\n\r");
			if (Sources == null)
			{
				bldr.Append("\n\rSources: NULL");
			}
			else
			{
				bldr.Append("\n\rSources:\n\r");
				foreach (var source in Sources)
				{
					bldr.Append(source).Append("\n\r");
				}
			}

			if (Destinations == null)
			{
				bldr.Append("\n\rDestinations: NULL");
			}
			else
			{
				bldr.Append("\n\rDestinations:\n\r");
				foreach (var dest in Destinations)
				{
					bldr.Append(dest).Append("\n\r");
				}
			}

			return bldr.ToString();
		}

	}

}
