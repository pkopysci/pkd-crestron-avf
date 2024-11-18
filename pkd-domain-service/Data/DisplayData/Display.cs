namespace pkd_domain_service.Data.DisplayData
{
	using System.Collections.Generic;
	using pkd_domain_service.Data.ConnectionData;
	using pkd_domain_service.Data.DriverData;
	using System.Text;
	using Newtonsoft.Json;

	[JsonObject(MemberSerialization.OptIn)]
	public class Display : BaseData
	{
		[JsonProperty("Label")]
		public string Label { get; set; }

		[JsonProperty("Icon")]
		public string Icon { get; set; }

		[JsonProperty("Tags")]
		public List<string> Tags { get; set; }

		[JsonProperty("HasScreen")]
		public bool HasScreen { get; set; }

		[JsonProperty("RelayController")]
		public string RelayController { get; set; }

		[JsonProperty("ScreenUpRelay")]
		public int ScreenUpRelay { get; set; }

		[JsonProperty("ScreenDownRelay")]
		public int ScreenDownRelay { get; set; }

		[JsonProperty("LecternInput")]
		public int LecternInput { get; set; }

		[JsonProperty("StationInput")]
		public int StationInput { get; set; }

		[JsonProperty("Connection")]
		public Connection Connection { get; set; }

		[JsonProperty("UserAttributes")]
		public List<UserAttribute> UserAttributes { get; set; }

		[JsonProperty("CustomCommands")]
		public CustomCommands CustomCommands { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("Display: ");
			bldr.Append(Id);
			bldr.Append("\n");
			bldr.Append("Icon = ");
			bldr.Append(Icon ?? "NULL");
			bldr.Append("\nTags: [ ");

			if (Tags != null)
			{
				foreach (var tag in Tags)
				{
					bldr.Append(tag);
					bldr.Append(", ");
				}
			}
			bldr.Append("]\n");
			bldr.Append("Has Screen: ");
			bldr.Append(HasScreen);
			bldr.Append("\nRelayController: ");
			bldr.Append(RelayController ?? "NULL");
			bldr.Append("\nScreenUpRelay = ").Append(ScreenUpRelay).Append(", ScreenDownRelay = ").Append(ScreenDownRelay);
			bldr.Append("\nLecternInput = ").Append(LecternInput).Append(", StationInput: ").Append(StationInput);
			bldr.Append("\n");
			bldr.Append(Connection.ToString());
			bldr.Append("\nUser Attributes:\n");
			if (UserAttributes == null)
			{
				bldr.Append("NULL\n");
			}
			else
			{
				foreach (var attr in UserAttributes)
				{
					bldr.Append(attr.ToString());
					bldr.Append("\n");
				}
			}

			return bldr.ToString();
		}
	}

}
