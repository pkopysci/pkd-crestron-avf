using System.Globalization;
using Crestron.RAD.Common.Interfaces;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using pkd_common_utils.FileOps;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_domain_service.Data.DisplayData;
using pkd_domain_service.Data.DriverData;

namespace pkd_hardware_service.DisplayDevices
{
	/// <summary>
	/// Factory class for creating display control objects from configuration data.
	/// </summary>
	public static class DisplayDeviceFactory
	{
		private const int WarmupCooldownTime = 30;
		
		/// <param name="displayData">configuration data for the display to create.</param>
		/// <param name="processor">the root Crestron control system object.</param>
		/// <param name="hwService">the hardware service that will manage the device after creation.</param>
		/// <returns>a control object for the display, or false if an error occurs.</returns>
		public static IDisplayDevice? CreateDisplay(Display displayData, CrestronControlSystem processor, IInfrastructureService hwService)
		{
			ParameterValidator.ThrowIfNull(displayData, "CreateDisplay", nameof(displayData));
			ParameterValidator.ThrowIfNull(processor, "CreateDisplay", nameof(processor));
			ParameterValidator.ThrowIfNull(hwService, "CreateDisplay", nameof(hwService));
			Logger.Info($"CreateDisplay() - Creating display with ID {displayData.Id}");


			return displayData.Connection.Transport.ToUpper().Equals("TCP") ?
				LoadCcdDisplay(displayData, processor, hwService) : 
				LoadPluginDisplay(displayData, processor, hwService);
		}

		private static CcdDisplayDevice? LoadCcdDisplay(Display displayData, CrestronControlSystem processor, IInfrastructureService hwService)
		{
			var transport = DriverLoader.GetTransportType(displayData.Connection.Transport);
			var driverPath = DirectoryHelper.NormalizePath(displayData.Connection.Driver);

			var device = DriverLoader.LoadDriverInstance<IBasicVideoDisplay>(
				driverPath,
				"IBasicVideoDisplay",
				transport);

			if (device == null)
			{
				Logger.Error("DisplayDeviceFactory.LoadCcdDisplay() - failed to load driver for display {0}", displayData.Id);
				return null;
			}

			if (device is IBasicVideoDisplay2 basicDisplayDevice)
			{
				SetUserAttributes(basicDisplayDevice, displayData.UserAttributes);
			}

			switch (device)
			{
				// warning disabled due to external CCD implementations being able to implement ITcp2 and IBasicVideoDisplay
				// ReSharper disable once SuspiciousTypeConversion.Global
				case ITcp2 tcpDevice2:
					tcpDevice2.Initialize(displayData.Connection.Host, displayData.Connection.Port);
					break;
				case ITcp tcpDevice:
					tcpDevice.Initialize(IPAddress.Parse(displayData.Connection.Host), displayData.Connection.Port);
					break;
				case ISerialComport:
					Logger.Warn(
						$"DisplayDeviceFactory.CreateDisplay({displayData.Id}) - Serial drivers are not supported yet.");
					break;
			}

			device.WarmUpTime = WarmupCooldownTime;
			device.CoolDownTime = WarmupCooldownTime;
			var ccd = new CcdDisplayDevice(device, displayData);
			ccd.Initialize(
				displayData.Connection.Host,
				displayData.Connection.Port,
				displayData.Label,
				displayData.Id);

			if (!string.IsNullOrEmpty(displayData.Manufacturer))
			{
				ccd.Manufacturer = displayData.Manufacturer;
			}

			if (!string.IsNullOrEmpty(displayData.Model))
			{
				ccd.Model = displayData.Model;
			}
			
			return ccd;
		}

		private static void SetUserAttributes(IBasicVideoDisplay2 driver, IEnumerable<UserAttribute> data)
		{
			try
			{
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
							Logger.Error(
								$"DisplayDeviceFactory.SetUserAttribute({driver.GetType()}) - Unknown data type encountered: {attribute.DataType}");
							break;
					}
				}
			}
			catch (FormatException ex)
			{
				Logger.Error(ex, "DisplayDeviceFactory.SetUserAttributes()");
			}
		}

		private static IDisplayDevice? LoadPluginDisplay(Display displayData, CrestronControlSystem processor, IInfrastructureService hwService)
		{
			var driverPath = DirectoryHelper.NormalizePath(displayData.Connection.Driver);
			var device = DriverLoader.LoadClassByInterface<IDisplayDevice>(
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

				if (!string.IsNullOrEmpty(displayData.Manufacturer))
				{
					device.Manufacturer = displayData.Manufacturer;
				}

				if (!string.IsNullOrEmpty(displayData.Model))
				{
					device.Model = displayData.Model;
				}
			}
			else
			{
				Logger.Warn("DisplayFactory.CreateDisplay() - Could not load display from library for display id {0}", displayData.Id);
			}

			return device;
		}
	}
}
