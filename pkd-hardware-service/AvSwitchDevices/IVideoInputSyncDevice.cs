using pkd_common_utils.GenericEventArgs;

namespace pkd_hardware_service.AvSwitchDevices;

/// <summary>
/// Required events, methods, and properties for any AV switcher that supports video input sync monitoring.
/// </summary>
public interface IVideoInputSyncDevice
{
    /// <summary>
    /// Arg = the input number that is reporting a sync status changed.
    /// </summary>
    event EventHandler<GenericSingleEventArgs<uint>> VideoInputSyncStateChanged;
    
    /// <summary>
    /// Query the state of the input sync as last reported by the device.
    /// </summary>
    /// <param name="input">the index of the input to query</param>
    /// <returns>true if the input has sync, false otherwise.</returns>
    bool QueryVideoInputSyncState(uint input);
}