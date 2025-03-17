
using Crestron.SimplSharpPro;

namespace pkd_ui_service.Interfaces
{
	/// <summary>
	/// Required events, methods, and properties for any Crestron-based interface, such as a TSW-xx70.
	/// </summary>
	public interface ICrestronUserInterface
	{
		///<summary>
		/// assign the root control system and IP-ID used when connecting to the interface hardware.
		/// </summary>
		/// <param name="parent">the root control system object.</param>
		/// <param name="ipId">The Crestron IP-ID assigned to the interface connection.</param>
		void SetCrestronControl(CrestronControlSystem parent, int ipId);
	}
}
