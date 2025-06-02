using Crestron.SimplSharpPro;
using pkd_common_utils.FileOps;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_domain_service.Data.CameraData;
// ReSharper disable SuspiciousTypeConversion.Global

namespace pkd_hardware_service.CameraDevices;

internal static class CameraDeviceFactory
{
    public static ICameraDevice? CreateCameraDevice(
    Camera cameraData,
    CrestronControlSystem controlSystem,
    IInfrastructureService hwService)
    {
        ParameterValidator.ThrowIfNull(cameraData, "CreateCameraDevice", nameof(cameraData));
        ParameterValidator.ThrowIfNull(controlSystem, "CreateCameraDevice", nameof(controlSystem));
        ParameterValidator.ThrowIfNull(hwService, "CreateCameraDevice", nameof(hwService));
        
        var dllPath = DirectoryHelper.NormalizePath(cameraData.Connection.Driver);
        var device = DriverLoader.LoadClassByInterface<ICameraDevice>(dllPath, cameraData.ClassName, "ICameraDevice");

        if (device == null)
        {
            Logger.Info("CameraDeviceFactory.CreateCameraDevice() - Failed to create device. See logs for details.");
            return null;
        }
        
        device.Initialize(cameraData.Connection.Host,
            cameraData.Connection.Port,
            cameraData.Id,
            cameraData.Label,
            cameraData.Connection.Authentication.UserName,
            cameraData.Connection.Authentication.Password);

        if (device is IPresetDevice presetCam)
        {
            List<CameraPreset> presets = [];
            foreach (var preset in cameraData.Presets)
            {
                presets.Add(new CameraPreset() {Id = preset.Id, Number = preset.Number});
            }
            presetCam.SetPresetData(presets);
        }
        
        if (!string.IsNullOrEmpty(device.Manufacturer))
        {
            device.Manufacturer = cameraData.Manufacturer;
        }

        if (!string.IsNullOrEmpty(device.Model))
        {
            device.Model = cameraData.Model;
        }

        return device;
    }
}