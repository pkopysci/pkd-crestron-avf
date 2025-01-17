namespace pkd_hardware_service.TransportDevices
{
	using Crestron.SimplSharpPro;
	using pkd_common_utils.FileOps;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.TransportDeviceData;
	using System;

	internal static class TransportDeviceFactory
	{
		public static ITransportDevice CreateCableBox(CableBox cableBoxData, CrestronControlSystem processor, IInfrastructureService hwService)
		{
			ParameterValidator.ThrowIfNull(cableBoxData, "TransportDeviceFactory.CreateCableBox", "cableBoxData");
			ParameterValidator.ThrowIfNull(processor, "TransportDeviceFactory.CreateCableBox", "processor");
			ParameterValidator.ThrowIfNull(hwService, "TransportDeviceFactory.CreateCableBox", "hwService");

			Logger.Info("TransportDeviceFactory.CreateCableBox() - creating device with ID {0}", cableBoxData.Id);

			string path = DirectoryHelper.NormalizePath(cableBoxData.Connection.Driver);
			ITransportDevice device = DriverLoader.LoadClassByInterface<ITransportDevice>(
				path,
				cableBoxData.Connection.Transport,
				"ITransportDevice");

			if (device == null)
			{
				Logger.Info("TransportDeviceFactory.CreateCableBox() - Failed to create device {0}. See error log.", cableBoxData.Id);
				return device;
			}

			try
			{
				device.Initialize(
					processor.IROutputPorts[(uint)cableBoxData.Connection.Port],
					cableBoxData.Id,
					cableBoxData.Label);

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
