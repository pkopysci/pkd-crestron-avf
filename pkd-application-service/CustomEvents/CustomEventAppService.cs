using System.Collections.ObjectModel;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;

namespace pkd_application_service.CustomEvents
{
	/// <summary>
	/// An extension of the base ApplicationService class. This adds custom event features, such as entering and exiting a "theater mode" or other non-standard
	/// system states.
	/// </summary>
	public abstract class CustomEventAppService : ApplicationService, ICustomEventAppService
	{
		/// <summary>
		/// A collection of custom event tags and the action to invoke associated with them.
		/// </summary>
		protected Dictionary<string, Action<bool>> customEvents = new Dictionary<string, Action<bool>>();

		/// <summary>
		/// A collection of event data objects associated with all supported event tags.
		/// </summary>
		protected Dictionary<string, CustomEventInfoContainer> events = new Dictionary<string, CustomEventInfoContainer>();

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? CustomEventStateChanged;

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<CustomEventInfoContainer> QueryAllCustomEvents()
		{
			return new ReadOnlyCollection<CustomEventInfoContainer>(events.Values.ToList());
		}

		/// <inheritdoc/>
		public virtual void ChangeCustomEventState(string tag, bool state)
		{
			if (!customEvents.TryGetValue(tag, out var action))
			{
				Logger.Error("CustomEventAppService.ChangeCustomEventState({0}, {1}) - No matching tag found.", tag, state);
				return;
			}

			action.Invoke(state);
		}

		/// <inheritdoc/>
		public virtual bool QueryCustomEventState(string tag)
		{
			if (events.TryGetValue(tag, out var eventData))
			{
				return eventData.IsActive;
			}
			
			Logger.Error("CustomEventAppService.QueryCustomEventState({0}) - No matching tag found.", tag);
			return false;
		}

		/// <summary>
		/// Triggers the CustomEventStateChanged event with the given tag.
		/// </summary>
		/// <param name="tag">The tag that will be sent to subscribers with the state change notice.</param>
		protected virtual void NotifyStateChange(string tag)
		{
			var temp = CustomEventStateChanged;
			temp?.Invoke(this, new GenericSingleEventArgs<string>(tag));
		}
	}
}
