using Crestron.SimplSharpPro;
using pkd_common_utils.FileOps;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_domain_service.Data.TransportDeviceData;

namespace pkd_hardware_service.TransportDevices
{
	internal static class TransportDeviceFactory
	{
		public static ITransportDevice? CreateCableBox(
			CableBox cableBoxData,
			CrestronControlSystem processor,
			IInfrastructureService hwService)
		{
			ParameterValidator.ThrowIfNull(cableBoxData, "TransportDeviceFactory.CreateCableBox", nameof(cableBoxData));
			ParameterValidator.ThrowIfNull(processor, "TransportDeviceFactory.CreateCableBox", nameof(processor));
			ParameterValidator.ThrowIfNull(hwService, "TransportDeviceFactory.CreateCableBox", nameof(hwService));

			Logger.Info("TransportDeviceFactory.CreateCableBox() - creating device with ID {0}", cableBoxData.Id);

			var path = DirectoryHelper.NormalizePath(cableBoxData.Connection.Driver);
			var device = DriverLoader.LoadClassByInterface<ITransportDevice>(
				path,
				cableBoxData.Connection.Transport,
				"ITransportDevice");

			if (device == null)
			{
				Logger.Info("TransportDeviceFactory.CreateCableBox() - Failed to create device {0}. See error log.", cableBoxData.Id);
				return device;
			}

			if (!processor.SupportsIROut)
			{
				Logger.Error("TransportDeviceFactory.CreateCableBox() - provided CrestronControlSystem does not support IROut");
				return device;
			}
			
			try
			{
				var port = processor.IROutputPorts[(uint)cableBoxData.Connection.Port];
				if (port == null)
				{
					Logger.Error($"TransportDeviceFactory.CreateCableBox() - Unable to get port at index {(uint)cableBoxData.Connection.Port}.");
					return device;
				}
				
				device.Initialize(
					port,
					cableBoxData.Id,
					cableBoxData.Label);

				if (!string.IsNullOrEmpty(cableBoxData.Manufacturer))
				{
					device.Manufacturer = cableBoxData.Manufacturer;
				}

				if (!string.IsNullOrEmpty(cableBoxData.Model))
				{
					device.Model = cableBoxData.Model;
				}

				return device;
			}
			catch (Exception e)
			{
				Logger.Error(e, "TransportDeviceFactory.CreateCableBox()");
				return null;
			}
		}
	}
}
