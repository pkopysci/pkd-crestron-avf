namespace pkd_ui_service.Interfaces;

/// <summary>
/// required events, methods, and properties for an interface that supports system-wide video freeze controls.
/// </summary>
public interface ISupportsGlobalVideoFreeze
{
    /// <summary>
    /// Triggered when the user requests to change the global video freeze state.
    /// </summary>
    event EventHandler GlobalFreezeToggleRequest;
    
    /// <summary>
    /// Update the user interface with the current status of the global video freeze.
    /// </summary>
    /// <param name="state">true = freeze active, false = normal video streaming.</param>
    void SetGlobalFreezeState(bool state);
}