using pkd_application_service.Base;

namespace pkd_application_service.AudioControl
{
	/// <summary>
	/// Data object for sending information to subscribers about a single audio channel in the system.
	/// </summary>
	public class AudioChannelInfoContainer : InfoContainer
	{
		/// <summary>
		/// Creates a new instance of <see cref="AudioChannelInfoContainer"/>.
		/// </summary>
		/// <param name="id">The unique ID of the channel. Used for internal referencing.</param>
		/// <param name="label">The user-friendly name of the channel.</param>
		/// <param name="icon">The image tag used for referencing the UI icon.</param>
		/// <param name="tags">A collection of custom tags used by the subscribed service.</param>
		/// <param name="zoneEnables">A collection of data objects that define what audio zones this channel can be routed to.</param>
		public AudioChannelInfoContainer(string id, string label, string icon, List<string> tags, List<InfoContainer> zoneEnables)
			: base(id, label, icon, tags)
		{
			this.ZoneEnableControls = zoneEnables;
		}

		/// <summary>
		/// Gets a collection of audio zone data objects that define where this channel can be routed to.
		/// </summary>
		public List<InfoContainer> ZoneEnableControls { get; private set; }
	}
}
