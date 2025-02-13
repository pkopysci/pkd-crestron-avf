#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System.Collections.ObjectModel;
using pkd_application_service.Base;

namespace pkd_application_service.VideoWallControl;

public class VideoWallCellInfo
{
    public string Id { get; init; } = string.Empty;
    public int XStart { get; init; }
    public int YStart { get; init; }
    public int XEnd { get; init; }
    public int YEnd { get; init; }
    public string SourceId { get; set; } = string.Empty;
}

public class VideoWallLayoutInfo
{
    public string VideoWallControlId { get; init; } = string.Empty;
    public string Id { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public List<VideoWallCellInfo> Cells { get; init; } = [];
}

public class VideoWallInfoContainer(string id, string label, string icon, List<string> tags, bool isOnline = false)
    : InfoContainer(id, label, icon, tags, isOnline)
{
    public ReadOnlyCollection<VideoWallLayoutInfo> Layouts { get; init; } = new([]);
}