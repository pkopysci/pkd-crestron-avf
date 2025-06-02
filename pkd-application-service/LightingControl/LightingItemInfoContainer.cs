using pkd_application_service.Base;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace pkd_application_service.LightingControl
{
	public class LightingItemInfoContainer(
		string id,
		string label,
		string icon,
		List<string> tags,
		int index)
		: InfoContainer(id, label, icon, tags)
	{
		public int Index { get; private set; } = index;
	}
}
