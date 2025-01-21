namespace pkd_application_service
{
	using pkd_common_utils.GenericEventArgs;
	using System;

	/// <summary>
	/// Interface for adding technician-level access during runtime, such as broadcasting requests to lock all interfaces that are not
	/// tagged as a technician control point.
	/// </summary>
	public interface ITechAuthGroupAppService
	{
		// will be triggered when the application service requires non-tech interfaces to be locked/unlocked.
		event EventHandler<GenericSingleEventArgs<bool>>? NonTechLockoutStateChangeRequest;
	}
}
