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
    /// An icon tag used to display an associated image on the ui.
    /// </summary>
    public string Icon { get; init; } = string.Empty;
    
    /// <summary>
    /// The number of cells in the layout on the x-axis.
    /// </summary>
    public int Width { get; init; } = 0;
    
    /// <summary>
    /// The number of cells in the layout on the y-axis.
    /// </summary>
    public int Height { get; init; } = 0;
    
    /// <summary>
    /// A collection of all video cells and their positions within this layout.
    /// </summary>
    public List<VideoWallCell> Cells { get; init; } = [];

    /// <summary>
    /// A collection of tags used internally for additional behavior.
    /// </summary>
    public List<string> Tags { get; init; } = [];
}