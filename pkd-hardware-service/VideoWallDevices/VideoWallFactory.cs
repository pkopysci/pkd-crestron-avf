using Crestron.SimplSharpPro;
using pkd_common_utils.FileOps;
using pkd_common_utils.Logging;
using pkd_domain_service.Data.VideoWallData;

namespace pkd_hardware_service.VideoWallDevices;

/// <summary>
/// Factory class used to build a hardware control interface for video wall controllers.
/// </summary>
public static class VideoWallFactory
{
    public static IVideoWallDevice? CreateVideoWallDevice(
        VideoWall data,
        CrestronControlSystem processor,
        IInfrastructureService hwService)
    {
        var path = DirectoryHelper.NormalizePath(data.Connection.Driver);
        var device = DriverLoader.LoadClassByInterface<IVideoWallDevice>(
            path,
            data.ClassName,
            "IVideoWallDevice");

        if (device == null)
        {
            Logger.Info("VideoWallFactory.CreateVideoWallDevice() - Failed to create device. See error log for details.");
            return null;
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (device is ICrestronDevice crestronDevice)
        {
            crestronDevice.SetControlSystem(processor);
        }
        
        device.Initialize(
            data.Connection.Host,
            data.Connection.Port,
            data.Id,
            data.Label,
            data.Connection.Authentication.UserName,
            data.Connection.Authentication.Password);
        
        return device;
    }
}