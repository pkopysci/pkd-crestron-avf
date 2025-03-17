using Crestron.SimplSharpPro;
using pkd_common_utils.FileOps;
using pkd_common_utils.Logging;
using pkd_domain_service.Data.DspData;

namespace pkd_hardware_service.AudioDevices
{
	internal static class AudioDeviceFactory
	{
		public static IDsp? CreateDspDevice(Dsp dspData, CrestronControlSystem parent, IInfrastructureService hwService)
		{
			Logger.Info("AudioDeviceFactory.CreateDspDevice() - Creating DSP from plugin for {0}", dspData.Id);
			var path = DirectoryHelper.NormalizePath(dspData.Connection.Driver);
			var device = DriverLoader.LoadClassByInterface<IDsp>(
				path,
				dspData.Connection.Transport,
				"IDsp");

			if (device != null)
			{
				device.Initialize(
					dspData.Id,
					dspData.CoreId,
					dspData.Connection.Host,
					dspData.Connection.Port,
					dspData.Connection.Authentication.UserName,
					dspData.Connection.Authentication.Password);

				foreach (var preset in dspData.Presets)
				{
					device.AddPreset(preset.Bank, preset.Index);
				}

				if (!string.IsNullOrEmpty(dspData.Manufacturer))
				{
					device.Manufacturer = dspData.Manufacturer;
				}

				if (!string.IsNullOrEmpty(dspData.Model))
				{
					device.Model = dspData.Model;
				}
			}
			else
			{
				Logger.Error("AudioDeviceFactory.CreateDspDevice() - Could not create DSP with ID {0}", dspData.Id);
			}

			return device;
		}
	}
}
