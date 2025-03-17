using pkd_common_utils.DataObjects;

namespace pkd_hardware_service.CameraDevices;

/// <summary>
/// Minimum required methods for a device that supports pant/tilt controls.
/// </summary>
public interface IPanTiltDevice
{
    /// <summary>
    /// pan / tilt the camera. sending a Vector2D.Zero will stop the movement.
    /// </summary>
    /// <param name="direction">The direction to pan and tilt. This may be normalized depending on the device implementation.</param>
    void SetPanTilt(Vector2D direction);
}