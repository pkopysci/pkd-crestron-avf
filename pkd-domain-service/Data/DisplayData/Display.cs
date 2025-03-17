using pkd_domain_service.Data.ConnectionData;
using pkd_domain_service.Data.DriverData;

namespace pkd_domain_service.Data.DisplayData
{
	public class Display : BaseData
	{
		public string Label { get; set; } = string.Empty;

		public string Icon { get; set; } = string.Empty;

		public List<string> Tags { get; set; } = [];

		public bool HasScreen { get; set; }

		public string RelayController { get; set; } = string.Empty;

		public int ScreenUpRelay { get; set; }

		public int ScreenDownRelay { get; set; }

		public int LecternInput { get; set; }

		public int StationInput { get; set; }

		public Connection Connection { get; set; } = new();

		public List<UserAttribute> UserAttributes { get; set; } = [];

		public CustomCommands CustomCommands { get; set; } = new();
	}

}
