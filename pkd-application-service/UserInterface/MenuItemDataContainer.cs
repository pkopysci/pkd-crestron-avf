using pkd_application_service.Base;

namespace pkd_application_service.UserInterface
{
	/// <summary>
	/// Data object for sending UI menu items to subscribers.
	/// </summary>
	public class MenuItemDataContainer : InfoContainer
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="MenuItemDataContainer"/>.
		/// </summary>
		/// <param name="id">The unique ID of the channel. Used for internal referencing.</param>
		/// <param name="label">The user-friendly name of the channel.</param>
		/// <param name="icon">The image tag used for referencing the UI icon.</param>
		/// <param name="control">The activity associated with this menu item.</param>
		/// <param name="sourceSelect">DEPRECATED - The source ID to route on selection (can be the empty string "").</param>
		/// <param name="tags">collection of behavior tags associated with the item. Values are determined by the interface plugin implementation.</param>
		public MenuItemDataContainer(string id, string label, string icon, string control, string sourceSelect, List<string> tags)
			: base(id, label, icon, tags)
		{
			Control = control;
			SourceSelect = sourceSelect;
		}

		/// <summary>
		/// the device controls or activity to display when selected.
		/// </summary>
		public string Control { get; private set; }

		/// <summary>
		/// If associated with an input to route, this is the ID of the source to send to all destinations.
		/// </summary>
		public string SourceSelect { get; private set; }
	}
}
