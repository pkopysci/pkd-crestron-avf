using pkd_common_utils.GenericEventArgs;

namespace pkd_ui_service.Interfaces;

/// <summary>
/// required events, methods, and properties for an interface that supports system-wide state changes.
/// </summary>
public interface ISupportsStateChangeControls
{
    /// <summary>
    /// Triggered when the user requests to start or end the current session.
    /// True = set system to in-use/on, false = set system to standby/off
    /// </summary>
    event EventHandler<GenericSingleEventArgs<bool>> SystemStateChangeRequest;
    
    /// <summary>
    /// Update the UI to show either the in-use/active pages or the standby pages.
    /// </summary>
    /// <param name="state">true = the system is currently being used.
    /// false = the system is currently in standby mode.</param>
    void SetSystemState(bool state);
    
    /// <summary>
    /// Display a notice on the UI indicating that the system is transitioning
    /// from standby to active or active to standby.
    /// </summary>
    /// <param name="state">True = show changing to active, false = show changing to standby.</param>
    void ShowSystemStateChanging(bool state);
    
    /// <summary>
    /// Hide any notifications that indicate the system is changing state and notify the user that
    /// the change is complete.
    /// </summary>
    void HideSystemStateChanging();
}