namespace pkd_hardware_service.VideoWallDevices;

/// <summary>
/// Data object representing a single window/output cell in a video wall layout.
/// </summary>
public class VideoWallCell
{
    /// <summary>
    /// The unique id of this cell used for internal referencing.
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// The unique id of the currently routed video source.
    /// </summary>
    public string SourceId { get; init; } = string.Empty;
    
    /// <summary>
    /// The x-coordinate that this cell starts in the width of the parent layout.
    /// </summary>
    public int XStart { get; init; }
    
    /// <summary>
    /// The x-coordinate that this cell ends in the width of the parent layout.
    /// </summary>
    public int XEnd { get; init; }
    
    /// <summary>
    /// The y-coordinate that this cell starts in the height of the parent layout.
    /// </summary>
    public int YStart { get; init; }
    
    /// <summary>
    /// THe y-coordinate that this cell ends in the height of the parent layout.
    /// </summary>
    public int YEnd { get; init; }
}