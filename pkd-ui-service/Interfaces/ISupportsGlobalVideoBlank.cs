namespace pkd_ui_service.Interfaces;

/// <summary>
/// required events, methods, and properties for an interface that supports system-wide video blank controls.
/// </summary>
public interface ISupportsGlobalVideoBlank
{
    /// <summary>
    /// Triggered when the user requests to change the global video blank state.
    /// </summary>
    event EventHandler GlobalBlankToggleRequest;
    
    /// <summary>
    /// Update the user interface with the current status of the global video blank.
    /// </summary>
    /// <param name="state">true = blank is active, false = normal video operation.</param>
    void SetGlobalBlankState(bool state);
}