using Crestron.SimplSharpPro;

namespace pkd_hardware_service;

/// <summary>
/// Required methods for implementing a hardware plugin that requires direct hooks into the root control system object.
/// </summary>
public interface ICrestronDevice
{
    /// <summary>
    /// Assign a Crestron control system to the device control plugin.
    /// </summary>
    /// <param name="controlSystem">The root control system object.</param>
    void SetControlSystem(CrestronControlSystem controlSystem);
}