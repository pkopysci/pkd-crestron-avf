namespace pkd_ui_service.Interfaces
{
	public interface IUiComponent
	{
		/// <summary>
		/// subscribes to UI hooks and initializes any data helpers.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Set any UI configuration items that are necessary when the system is powered on.
		/// </summary>
		void SetActiveDefaults();

		/// <summary>
		/// Set any UI configuration items that are necessary when system is set to standby.
		/// </summary>
		void SetStandbyDefaults();
	}
}
