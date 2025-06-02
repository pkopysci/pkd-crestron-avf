using pkd_common_utils.GenericEventArgs;
using pkd_hardware_service.BaseDevice;

namespace pkd_hardware_service.CameraDevices;

/// <summary>
/// Properties, events, and methods required for any PTZ camera device.
/// </summary>
public interface ICameraDevice : IBaseDevice
{
    /// <param name="hostname">The IP Address or hostname used to connect to the hardware.</param>
    /// <param name="port">The port number used to connect to the hardware.</param>
    /// <param name="id">A unique id used to reference this device.</param>
    /// <param name="label">A human-friendly name of this device.</param>
    /// <param name="username">the authentication username used when connecting.</param>
    /// <param name="password">The authentication password used when connecting.</param>
    void Initialize(string hostname, int port, string id, string label, string username, string password);
}