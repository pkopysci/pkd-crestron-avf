namespace pkd_ui_service.Interfaces
{
	/// <summary>
	/// Required events, methods and properties for implementing a user interface that supports UI security features.
	/// </summary>
	public interface ISecurityUserInterface
	{
		/// <summary>
		/// Send a notice to the UI to block user interaction unless a valid passcode is entered.
		/// </summary>
		void EnableSecurityPasscodeLock();

		/// <summary>
		/// Prevent any user interacting with the interface if it is not tagged as 'tech'.
		/// </summary>
		void EnableTechOnlyLock();

		/// <summary>
		/// Removes the tech-only lock if it is currently active.
		/// </summary>
		void DisableTechOnlyLock();
	}
}
