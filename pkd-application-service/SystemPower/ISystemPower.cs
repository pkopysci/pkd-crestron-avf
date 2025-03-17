namespace pkd_application_service.SystemPower
{
	/// <summary>
	/// Common properties and methods for AV System power state management.
	/// </summary>
	public interface ISystemPowerApp
	{
		/// <summary>
		/// Triggered whenever the system changes power state (standby -> active or active -> standby).
		/// </summary>
		event EventHandler SystemStateChanged;

		/// <summary>
		/// Gets a value indicating whether system is in use or in standby.
		/// True = system is active, false = system is in standby.
		/// </summary>
		bool CurrentSystemState { get; }

		/// <summary>
		/// Gets a value indicating whether the auto-shutdown feature is enabled or disabled.
		/// true = enabled, false = disabled.
		/// </summary>
		bool AutoShutdownEnabled { get; }

		/// <summary>
		/// Request to transition to the active state and trigger any startup automation.
		/// </summary>
		void SetActive();

		/// <summary>
		/// Request to transition to the standby state and trigger any shutdown automation.
		/// </summary>
		void SetStandby();

		/// <summary>
		/// Enable the automatic shutdown feature.
		/// </summary>
		void AutoShutdownEnable();

		/// <summary>
		/// Disable tha automatic shutdown feature.
		/// </summary>
		void AutoShutdownDisable();

		/// <summary>
		/// Configure the system power app to shut down at the specific time if enabled.
		/// </summary>
		/// <param name="hour">The hour when the system should shut down (24-hour format).</param>
		/// <param name="minute">The specific minute when the system should shut down (0-59).</param>
		void SetAutoShutdownTime(int hour, int minute);
	}
}
