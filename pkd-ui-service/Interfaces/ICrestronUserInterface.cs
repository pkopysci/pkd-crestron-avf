
namespace pkd_ui_service.Interfaces
{
	using Crestron.SimplSharpPro;

	public interface ICrestronUserInterface
	{
		void SetCrestronControl(CrestronControlSystem parent, int ipId);
	}
}
