namespace pkd_ui_service.Interfaces
{
	/// <summary>
	/// required methods for implementing a user interface that is html-based.
	/// </summary>
	public interface IHtmlUserInterface
	{
		/// <summary>
		/// Send the general system type flag to the interface hardware.
		/// </summary>
		/// <param name="systemType">"baseline", "restaurant", or "flex".</param>
		void SetSystemType(string systemType);
	}
}
