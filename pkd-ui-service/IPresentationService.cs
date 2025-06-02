namespace pkd_ui_service
{
	/// <summary>
	/// minimum features of any presentation service implementation.
	/// </summary>
	public interface IPresentationService
	{
		/// <summary>
		/// Connect and register all interface connections.
		/// </summary>
		void Initialize();
	}
}
