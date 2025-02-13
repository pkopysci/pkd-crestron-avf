using pkd_domain_service.Data.ConnectionData;

namespace pkd_domain_service.Data.VideoWallData;

public class VideoWall : BaseData
{
    public string Label { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public Connection Connection { get; set; } = new();
}