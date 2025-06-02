using pkd_application_service.Base;

namespace pkd_application_service.CustomEvents
{
	/// <summary>
	/// Data container for a single custom event.
	/// </summary>
	/// <param name="id">id of the event used for internal referencing</param>
	/// <param name="label">human-friendly name of the event.</param>
	/// <param name="icon">icon tag of the event to be used by a user interface</param>
	/// <param name="tags">collection of behavior flags that may be used by the user interface or application service.</param>
	/// <param name="isOnline">true = the device associated with the event is connected, false otherwise.</param>
	/// <param name="isActive">true = this event is currently active/in use, false = this event is not selected.</param>
	public class CustomEventInfoContainer(
		string id,
		string label,
		string icon,
		List<string> tags,
		bool isOnline = false,
		bool isActive = false)
		: InfoContainer(id, label, icon, tags, isOnline)
	{
		/// <summary>
		/// Gets whether this event is currently active/selected or not.
		/// </summary>
		public bool IsActive { get; set; } = isActive;
	}
}
