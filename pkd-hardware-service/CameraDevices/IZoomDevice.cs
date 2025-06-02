namespace pkd_hardware_service.CameraDevices;

/// <summary>
/// Minimum required methods for a device that supports zoom controls.
/// </summary>
public interface IZoomDevice
{
    /// <summary>
    /// Zoom in (telephoto) or out (wide angle). Sending 0 (zero) will stop the zoom.
    /// </summary>
    /// <param name="speed">negative values for zoom wide, positive values for zoom telephoto.</param>
    void SetZoom(int speed);
}