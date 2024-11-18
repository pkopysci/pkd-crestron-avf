namespace pkd_application_service.LightingControl
{
	using pkd_application_service.Base;
	using System.Collections.Generic;

	public class LightingItemInfoContainer : InfoContainer
	{
		public LightingItemInfoContainer(
			string id,
			string label,
			string icon,
			List<string> tags,
			int index)
			: base(id, label, icon, tags)
		{
			this.Index = index;
		}

		public int Index { get; private set; }
	}
}
