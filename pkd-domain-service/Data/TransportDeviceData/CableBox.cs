namespace pkd_domain_service.Data.TransportDeviceData
{
	using pkd_domain_service.Data.ConnectionData;
	using pkd_domain_service.Data.DriverData;
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using System.Text;

	[JsonObject(MemberSerialization.OptIn)]
	public class CableBox : BaseData
	{
		[JsonProperty("Label")]
		public string Label { get; set; }

		[JsonProperty("Connection")]
		public Connection Connection { get; set; }

		[JsonProperty("UserAttributes")]
		public List<UserAttribute> UserAttributes { get; set; }

		[JsonProperty("Favorites")]
		public List<TransportFavorite> Favorites { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("CableBox ").Append(Id ?? "NULL").Append(":\n\r");
			bldr.Append("Connection = ").Append(Connection).Append("\n\r");
			if (UserAttributes == null)
			{
				bldr.Append("\n\rUserAttributes: NULL");
			}
			else
			{
				bldr.Append("\n\rUserAttributes:\n\r");
				foreach (var attr in UserAttributes)
				{
					bldr.Append(attr).Append("\n\r");
				}
			}

			if (Favorites == null)
			{
				bldr.Append("\n\rFavorites: NULL");
			}
			else
			{
				bldr.Append("\n\rFavorites:\n\r");
				foreach (var fav in Favorites)
				{
					bldr.Append(fav).Append("\n\r");
				}
			}

			return bldr.ToString();
		}

	}

}
