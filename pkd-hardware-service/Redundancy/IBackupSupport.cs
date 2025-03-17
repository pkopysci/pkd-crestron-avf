using pkd_common_utils.GenericEventArgs;

namespace pkd_hardware_service.Redundancy;

/// <summary>
/// required events, properties, and methods for a device that supports back/redundant fail-over to a separate device.
/// </summary>
public interface IRedundancySupport
{
    /// <summary>
    /// Trigger when the implementation switches between primary and backup/redundant connections.
    /// </summary>
    /// <remarks>Arg package is a unique ID of the implementing object.</remarks>
    public event EventHandler<GenericSingleEventArgs<string>> RedundancyStateChanged;

    /// <summary>
    /// Trigger whenever the backup/redundant device loses or establishes a connection.
    /// </summary>
    public event EventHandler<GenericSingleEventArgs<string>> BackupDeviceConnectionChanged;
    
    /// <summary>
    /// True = the main/primary connection in use, false otherwise.
    /// </summary>
    bool PrimaryDeviceActive { get; }
    
    /// <summary>
    /// True = the backup/redundant device is in use.
    /// </summary>
    bool BackupDeviceActive { get; }
    
    /// <summary>
    /// True = the backup device connection is established, false = no connection.
    /// </summary>
    bool BackupDeviceOnline { get; }
    
    /// <summary>
    /// Assign the back/redundant device TCP connection information. This method is called by the framework after Initialize()
    /// and before Connect().
    /// </summary>
    /// <param name="hostname">The hostname or IP address used to connect to the redundant device.</param>
    /// <param name="port">the port number used to connect to the device.</param>
    void SetBackupDeviceConnection(string hostname, int port);
}