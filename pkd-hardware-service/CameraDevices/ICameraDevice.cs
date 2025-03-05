using pkd_hardware_service.BaseDevice;

namespace pkd_hardware_service.CameraDevices;

/// <summary>
/// Properties, events, and methods required for any PTZ camera device.
/// </summary>
public interface ICameraDevice : IBaseDevice, IPtzDevice
{
    /// <param name="hostname">The IP Address or hostname used to connect to the hardware.</param>
    /// <param name="port">The port number used to connect to the hardware.</param>
    /// <param name="id">A unique id used to reference this device.</param>
    /// <param name="label">A human-friendly name of this device.</param>
    /// <param name="username">the authentication username used when connecting.</param>
    /// <param name="password">The authentication password used when connecting.</param>
    /// <param name="tags">Any custom behavior or description tags associated with the device.</param>
    void Initialize(string hostname, int port, string id, string label, string username, string password,
        List<string> tags);

    
    /// <param name="state">true = turn power on, false = turn power off.</param>
    void SetPowerState(bool state);
    
    /// <returns>True = power is on, false = power is off.</returns>
    bool QueryPowerState();
}