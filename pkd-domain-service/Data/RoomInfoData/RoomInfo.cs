namespace pkd_domain_service.Data.RoomInfoData
{
	/// <summary>
	/// Configuration item for setting the basic room information in the system.
	/// </summary>
	public class RoomInfo : BaseData
	{
		/// <summary>
		/// Gets or sets the name / number of the room from the config.
		/// </summary>
		public string RoomName { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the phone number or other contact information in the config.
		/// </summary>
		public string HelpContact { get; set; } = string.Empty;

		/// <summary>
		/// What system behavior will be expected by this system (baseline, active, lecture).
		/// </summary>
		public string SystemType { get; set; } = string.Empty;

		/// <summary>
		/// The application service plug-in used to drive the room. If this is empty then the default
		/// choice related to "SystemType" will be loaded.
		/// </summary>
		public Logic Logic { get; set; } = new();
	}
}
