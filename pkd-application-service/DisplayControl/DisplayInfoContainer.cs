namespace pkd_application_service.DisplayControl
{
	using pkd_application_service.Base;
	using System.Collections.Generic;

	/// <summary>
	/// Data object used for sending display information to subscribers.
	/// </summary>
	public class DisplayInfoContainer : InfoContainer
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="DisplayInfoContainer"/>.
		/// </summary>
		/// <param name="id">The unique ID of the display. Used for internal referencing.</param>
		/// <param name="label">The user-friendly name of the display.</param>
		/// <param name="icon">The image tag used for referencing the UI icon.</param>
		/// <param name="tags">A collection of custom tags used by the subscribed service.</param>
		/// <param name="hasScreen">true = there is a screen associated with this display, false = no screen associated</param>
		public DisplayInfoContainer(string id, string label, string icon, List<string> tags, bool hasScreen, bool isOnline = false)
			: base(id, label, icon, tags, isOnline)
		{
			this.HasScreen = hasScreen;
		}

		/// <summary>
		/// Gets a value indicating whether or not this display has an associated screen.
		/// </summary>
		public bool HasScreen { get; private set; }

		/// <summary>
		/// Gets or sets a collection of all selectable inputs on the display.
		/// </summary>
		public List<DisplayInput> Inputs { get; set; } = new List<DisplayInput>();
	}
}
