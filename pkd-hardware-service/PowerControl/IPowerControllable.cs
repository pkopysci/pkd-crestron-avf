using pkd_common_utils.GenericEventArgs;

namespace pkd_hardware_service.PowerControl;

/// <summary>
/// Events, properties, and methods for devices that allow for power on/off control.
/// </summary>
public interface IPowerControllable
{
    /// <summary>
    /// Trigger when the power state for the devices changes.
    /// </summary>
    /// <remarks>Arg = the id of the device that changed.</remarks>
    event EventHandler<GenericSingleEventArgs<string>> PowerChanged;
    
    /// <summary>
    /// True = device is powered on, false = device is powered off or in standby.
    /// </summary>
    bool PowerState { get; }
    
    /// <summary>
    /// Send a command to turn the device on.
    /// </summary>
    void PowerOn();
    
    /// <summary>
    /// Send a command to turn the device off/standby.
    /// </summary>
    void PowerOff();
}