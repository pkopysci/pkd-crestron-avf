namespace pkd_hardware_service.DisplayDevices
{
	using Crestron.RAD.Common.Interfaces;
	using Crestron.SimplSharp;
	using Crestron.SimplSharpPro;
	using pkd_common_utils.FileOps;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.DisplayData;
	using pkd_domain_service.Data.DriverData;
	using System;
	using System.Collections.Generic;
	using System.Globalization;

	public static class DisplayDeviceFactory
	{
		private const int WARMUP_COODLOWN_TIME = 30;

		public static IDisplayDevice CreateDisplay(Display displayData, CrestronControlSystem processor, IInfrastructureService hwService)
		{
			ParameterValidator.ThrowIfNull(displayData, "CreateDisplay", "displayData");
			ParameterValidator.ThrowIfNull(processor, "CreateDisplay", "processor");
			ParameterValidator.ThrowIfNull(hwService, "CreateDisplay", "hwService");
			Logger.Info(string.Format("CreateDisplay() - Creating display with ID {0}", displayData.Id));


			if (displayData.Connection.Transport.ToUpper().Equals("TCP"))
			{
				return LoadCcdDisplay(displayData, processor, hwService);
			}
			else
			{
				return LoadPluginDisplay(displayData, processor, hwService);
			}
		}

		private static IDisplayDevice LoadCcdDisplay(Display displayData, CrestronControlSystem processor, IInfrastructureService hwService)
		{
			Logger.Info(string.Format(
				"DisplayDeviceFactory.CreateDisplay() - Creating Crestron certified driver for {0}.",
				displayData.Id));

			IBasicVideoDisplay device = null;
			string transport = DriverLoader.GetTransportType(displayData.Connection.Transport);
			string driverPath = DirectoryHelper.NormalizePath(displayData.Connection.Driver);

			device = DriverLoader.LoadDriverInstance<IBasicVideoDisplay>(
				driverPath,
				"IBasicVideoDisplay",
				transport);

			if (device is IBasicVideoDisplay2)
			{
				SetUserAttributes(device as IBasicVideoDisplay2, displayData.UserAttributes);
			}

			if (device is ITcp2)
			{
				var tcpDevice = device as ITcp2;
				tcpDevice.Initialize(displayData.Connection.Host, displayData.Connection.Port);
			}
			else if (device is ITcp)
			{
				var tcpDevice = device as ITcp;
				tcpDevice.Initialize(IPAddress.Parse(displayData.Connection.Host), displayData.Connection.Port);
			}
			else if (device is ISerialComport)
			{
				Logger.Warn(string.Format("DisplayDeviceFactory.CreateDisplay({0}) - Serial drivers are not supported.", displayData.Id));
			}

			device.WarmUpTime = WARMUP_COODLOWN_TIME;
			device.CoolDownTime = WARMUP_COODLOWN_TIME;
			var ccd = new CcdDisplayDevice(device, displayData);
			ccd.Initialize(
				displayData.Connection.Host,
				displayData.Connection.Port,
				displayData.Label,
				displayData.Id);

			return ccd;
		}

		private static void SetUserAttributes(IBasicVideoDisplay2 driver, IEnumerable<UserAttribute> data)
		{
			// Ignore displays with no attributes
			if (data == null)
			{
				return;
			}

			try
			{
				//IBasicVideoDisplay2 driver = device as IBasicVideoDisplay2;
				foreach (var attribute in data)
				{
					switch (attribute.DataType.ToUpper(CultureInfo.InvariantCulture))
					{
						case "STRING":
							driver.SetUserAttribute(attribute.Id, attribute.Value);
							break;

						case "NUMBER":
							driver.SetUserAttribute(attribute.Id, ushort.Parse(attribute.Value));
							break;

						case "HEX":
							driver.SetUserAttribute(attribute.Id, ushort.Parse(attribute.Value, NumberStyles.HexNumber));
							break;

						case "BOOLEAN":
							driver.SetUserAttribute(attribute.Id, bool.Parse(attribute.Value));
							break;

						default:
							Logger.Error(string.Format(
								"DisplayDeviceFactory.SetUserAttribute({0}) - Unknown data type encounterd: {1}",
								driver.GetType(),
								attribute.DataType));
							break;
					}
				}
			}
			catch (FormatException ex)
			{
				Logger.Error(ex, "DisplayDeviceFactory.SetUserAttributes()");
			}
		}

		private static IDisplayDevice LoadPluginDisplay(Display displayData, CrestronControlSystem processor, IInfrastructureService hwService)
		{
			Logger.Info(string.Format(
				"DisplayDeviceFactory.CreateDisplay() - Creating display from plugin library for {0}...",
				displayData.Id));

			string driverPath = DirectoryHelper.NormalizePath(displayData.Connection.Driver);
			IDisplayDevice device = DriverLoader.LoadClassByInterface<IDisplayDevice>(
				driverPath,
				displayData.Connection.Transport,
				"IDisplayDevice");

			if (device != null)
			{
				device.Initialize(
					displayData.Connection.Host,
					displayData.Connection.Port,
					displayData.Label,
					displayData.Id);
			}
			else
			{
				Logger.Warn("DisplayFactory.CreateDisplay() - Could not load display from library for display id {0}", displayData.Id);
			}

			return device;
		}
	}

}
