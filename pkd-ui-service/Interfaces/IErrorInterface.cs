namespace pkd_ui_service.Interfaces;

/// <summary>
/// required events, methods, and properties for implementing a user interface that supports adding errors externally.
/// </summary>
public interface IErrorInterface
{
    /// <summary>
    /// Add an external error to the user interface. This is used when adding error notices that are thrown by the Presentation Service
    /// of the framework and not the Application Service.
    /// </summary>
    /// <param name="id">the unique id of the error to add.</param>
    /// <param name="message">The error message to display on the UI.</param>
    void AddDeviceError(string id, string message);

    /// <summary>
    /// Removes an existing error from the ui.
    /// </summary>
    /// <param name="id">the unique id of the error to remove.</param>
    void ClearDeviceError(string id);
}