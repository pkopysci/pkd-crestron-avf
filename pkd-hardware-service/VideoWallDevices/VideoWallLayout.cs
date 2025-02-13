namespace pkd_hardware_service.VideoWallDevices;

/// <summary>
/// Data object representing a single video wall layout.
/// </summary>
public class VideoWallLayout
{
    /// <summary>
    /// The unique id of this layout used for internal referencing.
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// The human-friendly name of this layout.
    /// </summary>
    public string Label { get; init; } = string.Empty;
    
    /// <summary>
    /// A collection of all video cells and their positions within this layout.
    /// </summary>
    public List<VideoWallCell> Cells { get; init; } = [];
}