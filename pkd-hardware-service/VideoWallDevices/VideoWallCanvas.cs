namespace pkd_hardware_service.VideoWallDevices;

/// <summary>
/// Data object representing a single video wall canvas that contains multiple layouts.
/// </summary>
public class VideoWallCanvas
{
    /// <summary>
    /// the unique id of the canvas, used for runtime referencing
    /// </summary>
    public string Id { get; init; } = string.Empty;
    
    /// <summary>
    /// A human-friendly name of the canvas.
    /// </summary>
    public string Label { get; init; } = string.Empty;
    
    /// <summary>
    /// The unique id of the layout that should be triggered on startup.
    /// </summary>
    public string StartupLayoutId { get; init; } = string.Empty;
    
    /// <summary>
    /// A collection of selectable <see cref="VideoWallLayout"/>s associated with this canvas.
    /// </summary>
    public List<VideoWallLayout> Layouts { get; init; } = [];
    
    /// <summary>
    /// The total number of possible cells in the X axis.
    /// </summary>
    public int MaxHeight { get; init; }
    
    /// <summary>
    /// The total number of possible cells in the Y axis.
    /// </summary>
    public int MaxWidth { get; init; }

    /// <summary>
    /// A collection of tags used internally for additional behavior.
    /// </summary>
    public List<string> Tags { get; init; } = [];
}