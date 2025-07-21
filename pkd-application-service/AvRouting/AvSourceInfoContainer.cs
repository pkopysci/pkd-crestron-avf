using pkd_application_service.Base;

namespace pkd_application_service.AvRouting;

/// <summary>
/// Container object for sending information a single audio/video source to subscribers.
/// </summary>
public class AvSourceInfoContainer : InfoContainer
{
	/// <summary>
	/// Default / Empty AV source.
	/// </summary>
	public new static readonly AvSourceInfoContainer Empty = new AvSourceInfoContainer("EMPTYINFO", "EMPTY INFO", "alert", new List<string>(), string.Empty);

	/// <summary>
	/// Creates a new instance of <see cref="AvSourceInfoContainer"/>
	/// </summary>
	/// <param name="id">The unique ID of the source. Used for internal referencing.</param>
	/// <param name="label">The user-friendly name of the source.</param>
	/// <param name="icon">The image tag used for referencing the UI icon.</param>
	/// <param name="tags">A collection of custom tags used by the subscribed service.</param>
	/// <param name="controlId">The unique ID of the transport device that is associated with the source if it exists.</param>
	public AvSourceInfoContainer(string id, string label, string icon, List<string> tags, string controlId)
		: base(id, label, icon, tags)
	{
		ControlId = controlId;
	}

	/// <summary>
	/// Gets the controllable device that is associated with this source.
	/// </summary>
	public string ControlId { get; private set; }
	
	public bool SupportSync { get; init; }
	
	public bool HasSync { get; init; }
}