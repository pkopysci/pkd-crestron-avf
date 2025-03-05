using pkd_common_utils.DataObjects;

namespace pkd_hardware_service.CameraDevices;

/// <summary>
/// Minimum required methods for a device that supports pant/tilt controls.
/// </summary>
public interface IPtzDevice
{
    /// <summary>
    /// pan / tilt the camera. sending a Vector2D.Zero will stop the movement.
    /// </summary>
    /// <param name="direction">The direction to pan and tilt. This may be normalized depending on the device implementation.</param>
    void SetPanTilt(Vector2D direction);

    /// <summary>
    /// Zoom in (telephoto) or out (wide angle). Sending 0 (zero) will stop the zoom.
    /// </summary>
    /// <param name="speed">negative values for zoom wide, positive values for zoom telephoto.</param>
    void SetZoom(int speed);
}