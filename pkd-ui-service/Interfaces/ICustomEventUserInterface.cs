namespace pkd_ui_service.Interfaces
{
	using pkd_common_utils.GenericEventArgs;
	using System;

	public interface ICustomEventUserInterface
	{
		/// <summary>
		/// Triggered when the UI sends a command to change an event state.
		/// Args package is [event tag], [new state].
		/// </summary>
		event EventHandler<GenericDualEventArgs<string, bool>> CustomEventChangeRequest;

		/// <summary>
		/// Update the internal collection of custom events. This will either add the event to the collection
		/// or update an existing event state. In either case, an update command is sent to the user interface project.
		/// </summary>
		/// <param name="eventTag">The unique tag of the event being added or updated.</param>
		/// <param name="state">The new or current state of the custom event.</param>
		void UpdateCustomEvent(string eventTag, bool state);

		/// <summary>
		/// Add a new event to the existing collection.
		/// </summary>
		/// <param name="eventTag">The unique ID of the event. Used for internal referencing</param>
		/// <param name="eventLabel">The user-friendly name of the event</param>
		/// <param name="currentState">true = event is active, false = event is not active</param>
		void AddCustomEvent(string eventTag, string eventLabel, bool currentState);

		/// <summary>
		/// Removes the target custom event from the internall collection if it exists.
		/// </summary>
		/// <param name="eventTag">The unique tag of the custom event to remove.</param>
		void RemoveCustomEvent(string eventTag);
	}
}
