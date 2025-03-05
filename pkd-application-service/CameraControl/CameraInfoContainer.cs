using pkd_application_service.Base;

namespace pkd_application_service.CameraControl;

/// <summary>
/// DaTa object representing a single controllable camera device.
/// </summary>
public class CameraInfoContainer : InfoContainer
{

    /// <param name="id">The unique ID of the device. Used for internal referencing.</param>
    /// <param name="label">The user-friendly name of the device.</param>
    /// <param name="icon">The image tag used for referencing the UI icon.</param>
    /// <param name="tags">A collection of custom tags used by the subscribed service.</param>
    /// <param name="isOnline">true = the device is currently connected for communication, false = device offline (defaults to false)</param>
    public CameraInfoContainer(string id, string label, string icon, List<string> tags, bool isOnline)
        : base(id, label, icon, tags, isOnline)
    {
    }

    /// <summary>
    /// Collection of user-selectable presets.
    /// </summary>
    public List<InfoContainer> Presets { get; init; } = [];
    
    /// <summary>
    /// True = preset states can be saved as well as recalled. False = cannot save preset changes.
    /// </summary>
    public bool SupportsSavingPresets { get; init; }
    
    /// <summary>
    /// True = device implements zoom in/out controls, false = zoom not supported.
    /// </summary>
    public bool SupportsZoom { get; init; }
    
    /// <summary>
    /// True = device implements pan/tilt controls, false = not supported.
    /// </summary>
    public bool SupportsPanTilt { get; init; }
    
    /// <summary>
    /// True = device power is on, false = device power is off.
    /// </summary>
    public bool PowerState { get; set; }
}