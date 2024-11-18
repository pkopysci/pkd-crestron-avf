namespace pkd_domain_service.Data.RoomInfoData
{
	using Newtonsoft.Json;

	/// <summary>
	/// Configuration item for setting the basic room information in the system.
	/// </summary>
	public class RoomInfo : BaseData
	{
		/// <summary>
		/// Gets or sets the name / number of the room from the config.
		/// </summary>
		[JsonProperty("RoomName")]
		public string RoomName { get; set; }

		/// <summary>
		/// Gets or sets the phone number or other contact information in the config.
		/// </summary>
		[JsonProperty("HelpContact")]
		public string HelpContact { get; set; }

		/// <summary>
		/// What system behavior will be expected by this system (baseline, active, lecture).
		/// </summary>
		[JsonProperty("SystemType")]
		public string SystemType { get; set; }

		/// <summary>
		/// The application service plug-in used to drive the room. If this is empty then the default
		/// choice related to "SystemType" will be loaded.
		/// </summary>
		[JsonProperty("Logic")]
		public Logic Logic { get; set; }

		public override string ToString()
		{
			return string.Format(
				"RoomInfo {0}:\nRoomName = {1}, HelpContact = {2}, SystemType = {3}, Logic:\n{4}",
				Id ?? "NO ID",
				RoomName ?? "NO NAME",
				HelpContact ?? string.Empty,
				SystemType ?? string.Empty,
				Logic.ToString());
		}
	}
}
