using System.Collections.ObjectModel;
using pkd_application_service.CameraControl;
using pkd_common_utils.DataObjects;
using pkd_common_utils.GenericEventArgs;

namespace pkd_ui_service.Interfaces;

/// <summary>
/// Events, properties, and methods for a user interface that supports camera control.
/// </summary>
public interface ICameraUserInterface
{
    /// <summary>
    /// Trigger when requesting a Pan/tilt control of a camera.
    /// </summary>
    /// <remarks>Arg1 = camera id, arg2 = direction</remarks>
    event EventHandler<GenericDualEventArgs<string, Vector2D>> CameraPanTiltRequest;
    
    /// <summary>
    /// Trigger when requesting zoom control of a camera. negative speed values are zoom wide, positive values are zoom telephoto.
    /// </summary>
    /// <remarks>Arg1 = camera id, Arg2 = speed.</remarks>
    event EventHandler<GenericDualEventArgs<string, int>> CameraZoomRequest;
    
    /// <summary>
    /// Args.Arg1 = id of the camera to adjust.
    /// Args.Arg2 = id of the preset to recall
    /// </summary>
    event EventHandler<GenericDualEventArgs<string, string>> CameraPresetRecallRequest;
    
    /// <summary>
    /// Args.Arg1 = id of the camera to adjust.
    /// Args.Arg2 = id of the preset to save.
    /// </summary>
    event EventHandler<GenericDualEventArgs<string, string>> CameraPresetSaveRequest;
    
    /// <summary>
    /// Args.Arg1 = id of the camera to adjust.
    /// Args.Arg2 = true = turn power on, false = turn power off.
    /// </summary>
    event EventHandler<GenericDualEventArgs<string, bool>> CameraPowerChangeRequest;
    
    /// <param name="cameras">A collection of all controllable cameras in the system.</param>
    void SetCameraData(ReadOnlyCollection<CameraInfoContainer> cameras);
    
    /// <param name="id">The id of the camera being updated.</param>
    /// <param name="newState">true = device power is now on, false = device power is now off.</param>
    void SetCameraPowerState(string id, bool newState);
    
    /// <param name="id">The id of the camera being updated.</param>
    /// <param name="newState">true = device is online, false = device is offline.</param>
    void SetCameraConnectionStatus(string id, bool newState);
}