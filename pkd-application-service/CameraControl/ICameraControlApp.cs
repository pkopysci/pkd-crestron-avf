using System.Collections.ObjectModel;
using pkd_common_utils.DataObjects;
using pkd_common_utils.GenericEventArgs;

namespace pkd_application_service.CameraControl;

/// <summary>
/// Events, properties and methods required for an application service that supports PTZ cameras.
/// </summary>
public interface ICameraControlApp
{
    /// <summary>
    /// Trigger when a camera device reports a change in connection status.
    /// </summary>
    /// <remarks>Args.Arg = id of the camera that changed.</remarks>
    event EventHandler<GenericSingleEventArgs<string>> CameraControlConnectionChanged;

    /// <summary>
    /// Trigger when a camera device reports a change in its power state.
    /// </summary>
    /// <remarks>Args.Arg = id of the camera that changed.</remarks>
    event EventHandler<GenericSingleEventArgs<string>> CameraPowerStateChanged;
    
    /// <returns>A collection of all controllable cameras in the system.</returns>
    ReadOnlyCollection<CameraInfoContainer> GetAllCameraDeviceInfo();
    
    /// <summary>
    /// directional pan/tilt command to the target camera.
    /// </summary>
    /// <param name="cameraId">The id of the camera to adjust.</param>
    /// <param name="direction">the direction to pan/tilt the camera.</param>
    void SendCameraPanTilt(string cameraId, Vector2D direction);
    
    /// <summary>
    /// Send a zoom command to the camera. negative values will zoom the camera out (wide), positive values will
    /// zoom the camera in (telephoto).
    /// </summary>
    /// <param name="cameraId">The id of the camera to control</param>
    /// <param name="speed">A value indicating whether to zoom in or out. actual speed depends on the hardware.</param>
    void SendCameraZoom(string cameraId, int speed);
    
    /// <summary>
    /// Recall a camera position preset.
    /// </summary>
    /// <param name="cameraId">The id of the camera to control</param>
    /// <param name="presetId">The id of the preset to recall.</param>
    void SendCameraPresetRecall(string cameraId, string presetId);
    
    /// <summary>
    /// Send a command to save the current camera position as the target preset, if saving is supported.
    /// </summary>
    /// <param name="cameraId">The id of the camera to control.</param>
    /// <param name="presetId">The id of the preset to save.</param>
    void SendCameraPresetSave(string cameraId, string presetId);
    
    /// <param name="id">The id of the camera to query.</param>
    /// <returns>true = device is online, false = device is offline.</returns>
    bool QueryCameraConnectionStatus(string id);
    
    /// <param name="id">The id of the camera to query.</param>
    /// <returns>true = power is on, false = power is off.</returns>
    bool QueryCameraPowerStatus(string id);
    
    /// <param name="id">The id of the camera to change.</param>
    /// <param name="newState">true = set power on, false = set power off.</param>
    void SendCameraPowerChange(string id, bool newState);
}