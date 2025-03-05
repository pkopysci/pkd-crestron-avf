namespace pkd_hardware_service.CameraDevices;

/**
 * Data type for a single preset element.
 */
public struct CameraPreset
{
    /// <summary>
    /// A unique id used to reference this preset.
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// A human-friendly name of this preset.
    /// </summary>
    public string Label { get; set; }
}