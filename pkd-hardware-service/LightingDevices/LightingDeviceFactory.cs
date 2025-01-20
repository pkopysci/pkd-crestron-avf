namespace pkd_hardware_service.LightingDevices
{
	using Crestron.SimplSharpPro;
	using pkd_common_utils.FileOps;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.LightingData;

	internal static class LightingDeviceFactory
	{
		public static ILightingDevice? CreateLightingDevice(
			LightingInfo data,
			CrestronControlSystem processor,
			IInfrastructureService hwService)
		{
			ParameterValidator.ThrowIfNull(data, "CreateLightingDevice", "data");
			ParameterValidator.ThrowIfNull(processor, "CreateLightingDevice", "processor");
			ParameterValidator.ThrowIfNull(hwService, "CreateLightingDevice", "hwService");

			string dllPath = DirectoryHelper.NormalizePath(data.Connection.Driver);
			ILightingDevice? device = DriverLoader.LoadClassByInterface<ILightingDevice>(dllPath, data.ClassName, "ILightingDevice");

			if (device == null)
			{
				Logger.Info("LightingDeviceFactory.CreateLightingDevice() - Failed to create device. See logs for details.");
				return null;
			}

			foreach (var scene in data.Scenes)
			{
				device.AddScene(scene.Id, scene.Label, scene.Index);
			}

			foreach (var zone in data.Zones)
			{
				device.AddZone(zone.Id, zone.Label, zone.Index);
			}

			device.Initialize(
				data.Connection.Host,
				data.Connection.Port,
				data.Id,
				data.Label,
				data.Connection.Authentication.UserName,
				data.Connection.Authentication.Password,
				data.Tags);

			return device;
		}
	}

}
