#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.ObjectModel;
using pkd_application_service.AvRouting;
using pkd_application_service.Base;
// disabling because this is used by implementing plugins.
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace pkd_application_service.VideoWallControl;

public record VideoWallCellInfo(
    string Id,
    string Label,
    string Icon,
    int XPosition,
    int YPosition,
    string SourceId);

public record VideoWallLayoutInfo(
    string VideoWallControlId,
    int Width,
    int Height,
    string Id,
    string Label,
    string Icon,
    List<VideoWallCellInfo> Cells);

public record VideoWallCanvasInfo(
    string Id,
    string Label,
    string StartupLayoutId,
    int MaxWidth,
    int MaxHeight,
    List<VideoWallLayoutInfo> Layouts);

public class VideoWallInfoContainer(string id, string label, string icon, List<string> tags, bool isOnline = false)
    : InfoContainer(id, label, icon, tags, isOnline)
{
    public ReadOnlyCollection<VideoWallCanvasInfo> Canvases { get; init; } = new([]);
    public ReadOnlyCollection<AvSourceInfoContainer> Sources { get; init; } = new([]);
}