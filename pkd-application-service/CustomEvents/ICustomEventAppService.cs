using System.Collections.ObjectModel;
using pkd_common_utils.GenericEventArgs;

namespace pkd_application_service.CustomEvents
{
	/// <summary>
	/// Common properties and methods associated with a custom event application service.
	/// </summary>
	public interface ICustomEventAppService
	{
		/// <summary>
		/// Triggered when a supported custom behavior changes state.
		/// </summary>
		event EventHandler<GenericSingleEventArgs<string>> CustomEventStateChanged;

		/// <summary>
		/// Queries the application service instance for custom behavior tags. These tags indicate any
		/// non-standard behavior that is supported by the currently loaded application service.
		/// </summary>
		/// <returns>A collection of tags that represent any custom actions supported by the application service.</returns>
		ReadOnlyCollection<CustomEventInfoContainer> QueryAllCustomEvents();

		/// <summary>
		/// Trigger a non-standard behavior sequence that is supported by the Application Service implementation.
		/// Logs a warning if no behavior with the supplied tag is found.
		/// </summary>
		/// <param name="tag">The unique tag for the behavior to trigger. Supported tags are provided by the QueryAllCustomBehaviors method.</param>
		/// <param name="state"></param>
		void ChangeCustomEventState(string tag, bool state);

		/// <summary>
		/// Query the application service for the current state of a target custom behavior.
		/// </summary>
		/// <param name="tag">The unique tag of the behavior to query.</param>
		/// <returns>True if the behavior is active, false otherwise.</returns>
		bool QueryCustomEventState(string tag);
	}
}
