namespace pkd_application_service.CustomEvents
{
	using pkd_application_service.Base;
	using System.Collections.Generic;

	public class CustomEventInfoContainer : InfoContainer
	{
		public bool IsActive { get; set; }

		public CustomEventInfoContainer(string id, string label, string icon, List<string> tags, bool isOnline = false, bool isActive = false)
			: base(id, label, icon, tags, isOnline)
		{
			IsActive = isActive;
		}
	}
}
