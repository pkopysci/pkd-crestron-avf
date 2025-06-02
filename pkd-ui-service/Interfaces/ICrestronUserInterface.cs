using Crestron.SimplSharpPro;

namespace pkd_ui_service.Interfaces;

/// <summary>
/// Events, methods, and properties for an user interface that requires access to the root control system and connection
/// IP-ID.
/// </summary>
public interface ICrestronUserInterface
{
    /// <summary>
    /// Sets the plugin connection information.
    /// </summary>
    /// <param name="control">the root entry point object that is used to establish a control connection.</param>
    public void SetCrestronControl(CrestronControlSystem control);
}