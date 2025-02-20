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
    public string SourceId { get; set; } = string.Empty;
    
    /// <summary>
    /// The id of the routable video wall source that should be sent to this cell/window when the parent layout
    /// is selected.
    /// </summary>
    public string DefaultSourceId { get; set; } = string.Empty;
    
    /// <summary>
    /// The horizontal location of this cell/window
    /// </summary>
    public int XPosition { get; init; }
    
    /// <summary>
    /// The vertical location of this cell/window.
    /// </summary>
    public int YPosition { get; init;  }
}