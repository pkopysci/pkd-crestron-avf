namespace pkd_application_service.Base
{
	/// <summary>
	/// Data object containing general information about the controlled location.
	/// </summary>
	public class RoomInfoContainer : InfoContainer
	{
		/// <summary>
		/// Instantiates an instance of <see cref="RoomInfoContainer"/>.
		/// </summary>
		/// <param name="id">The unique ID of the room set in the configuration.</param>
		/// <param name="label">The user-friendly room name.</param>
		/// <param name="helpContact">The contact information to use when contacting tech support.</param>
		/// <param name="systemType">The system behavior identifier that was set in the configuration file.</param>
		public RoomInfoContainer(string id, string label, string helpContact, string systemType)
			: base(id, label, Empty.Icon, Empty.Tags)
		{
			HelpContact = helpContact;
			SystemType = systemType;
		}

		/// <summary>
		/// Gets the help contact information that was set in the room configuration.
		/// </summary>
		public string HelpContact { get; private set; }

		/// <summary>
		/// Gets the system type for this room that was set in the configuration file.
		/// </summary>
		public string SystemType { get; private set; }

		/// <summary>
		/// the custom presentation service plugin library that will be loaded at system boot.
		/// </summary>
		public string PresentationServiceLibrary { get; init; } = string.Empty;
		
		/// <summary>
		/// class name of the custom presentation service plugin that will be instantiated from <see cref="PresentationServiceLibrary"/>.
		/// </summary>
		public string PresentationServiceClass { get; init; } = string.Empty;
	}
}
