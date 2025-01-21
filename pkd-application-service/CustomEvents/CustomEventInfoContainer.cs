namespace pkd_application_service.CustomEvents
{
	using Base;
	using System.Collections.Generic;

	public class CustomEventInfoContainer(
		string id,
		string label,
		string icon,
		List<string> tags,
		bool isOnline = false,
		bool isActive = false)
		: InfoContainer(id, label, icon, tags, isOnline)
	{
		public bool IsActive { get; set; } = isActive;
	}
}
