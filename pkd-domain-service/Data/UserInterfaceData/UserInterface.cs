namespace pkd_domain_service.Data.UserInterfaceData
{
	/// <summary>
	/// Configuration data for a single user interface.
	/// </summary>
	public class UserInterface : BaseData
	{
		/// <summary>
		/// Gets or sets the IP-ID used to connect to the user interface. This is an integer representation
		/// of a hex value.
		/// </summary>
		public int IpId { get; set; }

		/// <summary>
		/// The smart graphics data library needed if the UI is a VTPro-e based project.
		/// </summary>
		public string Sgd { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the default activity to present when the system enters the active state.
		/// </summary>
		public string DefaultActivity { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the collection of main menu items to display on the UI.
		/// </summary>
		public List<MenuItem> Menu { get; set; } = [];

		/// <summary>
		/// Collection of tags that can define special behavior for the UI.
		/// </summary>
		public List<string> Tags { get; set; } = [];

		public string ClassName { get; set; } = string.Empty;

		public string Library { get; set; } = string.Empty;
	}
}
