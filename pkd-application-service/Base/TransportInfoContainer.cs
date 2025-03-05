namespace pkd_application_service.Base
{
	/// <summary>
	/// data object for sending information about a Transport device (Blu-ray, Cable TV, DVD, etc.) to subscribers.
	/// </summary>
	public class TransportInfoContainer : InfoContainer
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="TransportInfoContainer"/>.
		/// </summary>
		/// <param name="id">The unique ID of the transport device. Used for internal referencing.</param>
		/// <param name="label">The user-friendly name of the transport device.</param>
		/// <param name="icon">The image tag used for referencing the UI icon.</param>
		/// <param name="tags">A collection of custom tags used by the subscribed service.</param>
		/// <param name="supportsColors">true = enable colored buttons for this device, false = disable colored buttons</param>
		/// <param name="supportsDiscretePower">true = enable power on/off control, false = enable power toggle controls.</param>
		/// <param name="favorites">A collection of data objects that provide information on favorite channels.</param>
		public TransportInfoContainer(
			string id,
			string label,
			string icon,
			List<string> tags,
			bool supportsColors,
			bool supportsDiscretePower,
			List<TransportFavorite> favorites)
			: base(id, label, icon, tags)
		{
			SupportsColors = supportsColors;
			SupportsDiscretePower = supportsDiscretePower;
			Favorites = favorites;
		}

		/// <summary>
		/// Gets a value indicating that this transport device supports color buttons (red, green, blue, yellow).
		/// </summary>
		public bool SupportsColors { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this transport device supports discrete power on/off (false = supports toggle only).
		/// </summary>
		public bool SupportsDiscretePower { get; private set; }

		/// <summary>
		/// Gets a collection of data objects containing information on favorite channels for this transport device.
		/// </summary>
		public List<TransportFavorite> Favorites { get; private set; }
	}

	/// <summary>
	/// Data object for storing and sending channel information to subscribers.
	/// </summary>
	public class TransportFavorite
	{
		/// <summary>
		/// Creates a new instance of <see cref="TransportFavorite"/>.
		/// </summary>
		/// <param name="id">The unique ID of the favorite. Used for internal referencing.</param>
		/// <param name="label">The user-friendly name of the favorite</param>
		public TransportFavorite(string id, string label)
		{
			Id = id;
			Label = label;
		}

		/// <summary>
		/// Gets the unique ID of this transport device. Used for internal referencing.
		/// </summary>
		public string Id { get; private set; }

		/// <summary>
		/// Gets the user-friendly name of the favorite.
		/// </summary>
		public string Label { get; private set; }
	}
}
