namespace pkd_application_service.Base
{
	/// <summary>
	/// Base data container class containing common attributes for all device objects.
	/// </summary>
	public class InfoContainer
	{
		/// <summary>
		/// Default / Empty data object.
		/// </summary>
		public static readonly InfoContainer Empty = new("EMPTYINFO", "EMPTY INFO", "alert", []);

		/// <summary>
		/// Instantiates a new instance of <see cref="InfoContainer"/>
		/// </summary>
		/// <param name="id">The unique ID of the device. Used for internal referencing.</param>
		/// <param name="label">The user-friendly name of the device.</param>
		/// <param name="icon">The image tag used for referencing the UI icon.</param>
		/// <param name="tags">A collection of custom tags used by the subscribed service.</param>
		/// <param name="isOnline">true = the device is currently connected for communication, false = device offline (defaults to false)</param>
		public InfoContainer(string id, string label, string icon, List<string> tags, bool isOnline = false)
		{
			Icon = icon;
			Id = id;
			Label = label;
			Tags = tags;
			IsOnline = isOnline;
		}

		/// <summary>
		/// Gets the unique ID of this data item.
		/// </summary>
		public string Id { get; protected set; }

		/// <summary>
		/// Gets the user-friendly label of this data item.
		/// </summary>
		public string Label { get; protected set; }
		
		/// <summary>
		/// If relevant, this is the company that creates the represented device. Defaults to the empty string.
		/// </summary>
		public string Manufacturer { get; init; } = string.Empty;
		
		/// <summary>
		/// If relevant, this is the device name as defined by the manufacturer. Defaults to the empty string.
		/// </summary>
		public string Model { get; init; } = string.Empty;

		/// <summary>
		/// Gets the Icon key for this data item.
		/// </summary>
		public string Icon { get; protected set; }

		/// <summary>
		/// Gets a value indicating whether the device is currently online or offline.
		/// </summary>
		public bool IsOnline { get; set; }

		/// <summary>
		/// Gets the various tags that are associated with this data item.
		/// </summary>
		public List<string> Tags { get; protected set; }
	}
}
