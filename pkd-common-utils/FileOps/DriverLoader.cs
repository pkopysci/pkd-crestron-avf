namespace pkd_common_utils.FileOps
{
	using Crestron.RAD.Common.Enums;
	using Crestron.RAD.Common.Interfaces;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using System;
	using System.Globalization;
	using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Helper class used to load a Crestron Certified Driver from a DLL using reflection.
    /// </summary>
    public class DriverLoader
	{

		/// <summary>
		/// Return an instance of a Crestron Certified Driver.
		/// </summary>
		/// <typeparam name="T">The expected return type.</typeparam>
		/// <param name="assemblyName">The full directory and file name with extension of the target assembly.</param>
		/// <param name="interfaceName">The CCD interface to search for in the driver dll.</param>
		/// <param name="transportName">The CCD transport type to search for in the driver dll.</param>
		/// <returns>The target T object, if found in the assembly, or T default if no matching driver is found.</returns>
		/// <exception cref="Exception">Will propagate exceptions from System.Reflection.</exception>
		public static T? LoadDriverInstance<T>(string assemblyName, string interfaceName, string transportName)
		{
			ParameterValidator.ThrowIfNullOrEmpty(assemblyName, "LoadDriverInstance", "assemblyName");
			ParameterValidator.ThrowIfNullOrEmpty(interfaceName, "LoadDriverInstance", "interfaceName");
			ParameterValidator.ThrowIfNullOrEmpty(transportName, "LoadDriverInstance", "transportName");

			try
			{
				string dllPath = DirectoryHelper.NormalizePath(string.Format(
					"{0}\\{1}",
					DirectoryHelper.GetUserFolder(),
					assemblyName));

				Logger.Info(string.Format("DriverLoader.LoadDriverInstance() - Attempting to load driver from location {0}...", dllPath));

				Assembly dll = Assembly.LoadFrom(dllPath);
				if (dll == null)
				{
					Logger.Error(string.Format("DriverLoader.LoadDriverInstance() - Failed to load driver. DLL {0} returned NULL when loading.", dllPath));
				}
				else
				{
					foreach (Type type in dll.GetTypes())
					{
						Type[] interfaces = type.GetInterfaces();

						if (interfaces.Any(x => x.Name.Equals(interfaceName))
							&& interfaces.Any(x => x.Name.Equals(transportName)))
						{
							return type.FullName != null ? (T?)dll.CreateInstance(type.FullName) : default;
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error(e, string.Format("DriverLoader.LoadDriverInstance({0},{1},{2})", assemblyName, interfaceName, transportName));
			}

			return default;
		}

		/// <summary>
		/// Return an instance of a class based on the defined interface name.
		/// </summary>
		/// <typeparam name="T">The expected return type.</typeparam>
		/// <param name="assemblyName">The full directory and file name with extension of the target assembly.</param>
		/// <param name="className">The class to search for in the assembly dll.</param>
		/// <param name="interfaceName">The interface to search for in the assembly dll.</param>
		/// <returns>The target T object, if found in the assembly, or T default if no matching driver is found.</returns>
		/// <exception cref="Exception">Will propagate exceptions from System.Reflection.</exception>
		public static T? LoadClassByInterface<T>(string assemblyName, string className, string interfaceName)
		{
			ParameterValidator.ThrowIfNullOrEmpty(assemblyName, "LoadClassByInterface", "assemblyName");
			ParameterValidator.ThrowIfNullOrEmpty(interfaceName, "LoadClassByInterface", "interfaceName");

			T? device = default;
			try
			{
				string dllPath = DirectoryHelper.NormalizePath(string.Format(
					"{0}\\{1}",
					DirectoryHelper.GetUserFolder(),
					assemblyName));

				Logger.Info("Attempting to load driver from file {0}...", dllPath);
				Assembly dll = Assembly.LoadFrom(dllPath);


				if (dll == null)
				{
					Logger.Error(string.Format("LoadClassByInterface() - Unable to find assembly {0}", assemblyName));
					return device;
				}

				foreach (var type in dll.GetTypes())
				{
					if (type.Name.Equals(className, StringComparison.InvariantCulture))
					{
						if (type.GetInterfaces().Any(x => x.Name.Equals(interfaceName, StringComparison.InvariantCulture)))
						{
							device = type.FullName != null ? (T?)dll.CreateInstance(type.FullName) : default;
							break;
                        }
					}
				}

				Logger.Info("Done!");
			}
			catch (Exception e)
			{
				Logger.Error(e, string.Format("DriverLoader.LoadClassByInterface({0},{1})", assemblyName, interfaceName));
			}

			return device;
		}

		/// <summary>
		/// Resolved a configuration service tag to a Crestron Certified Driver transport interface type.
		/// Writes an error to the logging system if the lookup fails.
		/// </summary>
		/// <param name="connectionTag">The tag to evaluate. Cannot be null or empty.</param>
		/// <returns>The name of the CCD transport type, or the empty string if the give tag is not supported.</returns>
		public static string GetTransportType(string connectionTag)
		{
			ParameterValidator.ThrowIfNullOrEmpty(connectionTag, "GetTransportType", "connectionTag");

			switch (connectionTag.ToUpper(CultureInfo.InvariantCulture))
			{
				case "TCP":
					return typeof(ITcp).Name;

				case "HTTP":
					return typeof(IHttp).Name;

				case "TELNET":
					return typeof(ITelnet).Name;

				case "REST":
					return typeof(IRestable).Name;

				case "SERIAL":
					return typeof(ISerial).Name;

				case "IR":
					return typeof(IIr).Name;

				case "HTTPS":
				case "SSL":
				case "CEC":
				default:
					Logger.Error(string.Format("GetTransportType() - Unsupported transport in config: {0}", connectionTag));
					break;
			}

			return string.Empty;
		}

		/// <summary>
		/// Convert a configuration buad rate argument to a CCD buad rate value.
		/// Writes a warning to the logging system if an unrecognized value is encountered.
		/// </summary>
		/// <param name="baudRate">The config-defined baud rate to convert.</param>
		/// <returns>the CCD baud rate value. Defaults to 9600 if unable to parse baudRate.</returns>
		public static eComBaudRates GetBaudRate(int baudRate)
		{
			switch (baudRate)
			{
				case 300:
					return eComBaudRates.ComspecBaudRate300;

				case 600:
					return eComBaudRates.ComspecBaudRate600;

				case 1200:
					return eComBaudRates.ComspecBaudRate1200;

				case 1800:
					return eComBaudRates.ComspecBaudRate1800;

				case 2400:
					return eComBaudRates.ComspecBaudRate2400;

				case 3600:
					return eComBaudRates.ComspecBaudRate3600;

				case 4800:
					return eComBaudRates.ComspecBaudRate4800;

				case 7200:
					return eComBaudRates.ComspecBaudRate7200;

				case 14400:
					return eComBaudRates.ComspecBaudRate14400;

				case 19200:
					return eComBaudRates.ComspecBaudRate19200;

				case 28800:
					return eComBaudRates.ComspecBaudRate28800;

				case 38400:
					return eComBaudRates.ComspecBaudRate38400;

				case 57600:
					return eComBaudRates.ComspecBaudRate57600;

				case 115200:
					return eComBaudRates.ComspecBaudRate115200;

				case 9600:
					return eComBaudRates.ComspecBaudRate9600;

				default:
					Logger.Warn(string.Format("GetBaudRate - {0} is an unsupported buad rate. Setting to 9600.", baudRate));
					goto case 9600;
			}
		}

		/// <summary>
		/// Convert a configuration data bits number to a CCD data bits definition.
		/// Writes an error to the logging system if an unrecongized argument is given.
		/// </summary>
		/// <param name="data">The data bits number set in the configuration.</param>
		/// <returns>The CCD data bits equivalent to 'data', or NotSpecified if unable to parse.</returns>
		public static eComDataBits GetDataBits(int data)
		{
			switch (data)
			{
				case 7:
					return eComDataBits.ComspecDataBits7;

				case 8:
					return eComDataBits.ComspecDataBits8;

				default:
					Logger.Error(string.Format("GetBaudeRate() - undefined data bits encountered: {0}", data));
					return eComDataBits.NotSpecified;
			}
		}

		/// <summary>
		/// Convert a configuration data bits number to a CCD stop bits definition.
		/// Writes an error to the logging system if an unrecongized argument is given.
		/// </summary>
		/// <param name="data">The stop bits number set in the configuration.</param>
		/// <returns>The CCD stop bits equivalent to 'data', or NotSpecified if unable to parse.</returns>
		public static eComStopBits GetStopBits(int data)
		{
			switch (data)
			{
				case 1:
					return eComStopBits.ComspecStopBits1;

				case 2:
					return eComStopBits.ComspecStopBits2;

				default:
					Logger.Error(string.Format("GetStopBits() - Unknown stop bit value: {0}, setting to NotSpecified.", data));
					return eComStopBits.NotSpecified;
			}
		}

		/// <summary>
		/// Convert a configuration Hardware Handshake argument to a CCD hardware handshake definition.
		/// Writes an error to the logging system if an unrecongized argument is given.
		/// </summary>
		/// <param name="data">The Hardware Handshake argument set in the configuration.</param>
		/// <returns>The CCD Hardware Handshake equivalent to 'data', or NotSpecified if unable to parse.</returns>
		public static eComHardwareHandshakeType GetHwHandshake(string data)
		{
			switch (data.ToUpper(CultureInfo.InvariantCulture))
			{
				case "NONE":
					return eComHardwareHandshakeType.ComspecHardwareHandshakeNone;

				case "RTS":
					return eComHardwareHandshakeType.ComspecHardwareHandshakeRTS;

				case "CTS":
					return eComHardwareHandshakeType.ComspecHardwareHandshakeCTS;

				case "CTS/RTS":
					return eComHardwareHandshakeType.ComspecHardwareHandshakeRTSCTS;

				default:
					Logger.Error(string.Format("GetHwHandshake() - Unknown argument given: {0}, setting to NotSpecified.", data));
					return eComHardwareHandshakeType.NotSpecified;
			}
		}

		/// <summary>
		/// Convert a configuration Software Handshake argument to a CCD software handshake definition.
		/// Writes an error to the logging system if an unrecongized argument is given.
		/// </summary>
		/// <param name="data">The Software Handshake argument set in the configuration.</param>
		/// <returns>The CCD Software Handshake equivalent to 'data', or NotSpecified if unable to parse.</returns>
		public static eComSoftwareHandshakeType GetSwHandshake(string data)
		{
			switch (data.ToUpper(CultureInfo.InvariantCulture))
			{
				case "NONE":
					return eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone;

				case "XON":
					return eComSoftwareHandshakeType.ComspecSoftwareHandshakeXON;

				case "XONT":
					return eComSoftwareHandshakeType.ComspecSoftwareHandshakeXONT;

				case "XONR":
					return eComSoftwareHandshakeType.ComspecSoftwareHandshakeXONR;

				default:
					Logger.Error(string.Format("GetSwHandshake() - Unknown argument given: {0}, setting to NotSpecified.", data));
					return eComSoftwareHandshakeType.NotSpecified;
			}
		}

		/// <summary>
		/// Convert a configuration parity argument to a CCD parity definition.
		/// Writes an error to the logging system if an unrecongized argument is given.
		/// </summary>
		/// <param name="data">The parity argument set in the configuration.</param>
		/// <returns>The CCD parity equivalent to 'data', or NotSpecified if unable to parse.</returns>
		public static eComParityType GetParity(string data)
		{
			switch (data.ToUpper(CultureInfo.InvariantCulture))
			{
				case "NONE":
					return eComParityType.ComspecParityNone;

				case "EVEN":
					return eComParityType.ComspecParityEven;

				case "ODD":
					return eComParityType.ComspecParityOdd;

				default:
					Logger.Error(string.Format("GetParity() - Unknown argument given: {0}, setting to NotSpecified.", data));
					return eComParityType.NotSpecified;
			}
		}

		/// <summary>
		/// Convert a configuration serial protocol argument to a CCD protocol definition.
		/// Writes an error to the logging system if an unrecongized argument is given.
		/// </summary>
		/// <param name="data">The serial protocol argument set in the configuration.</param>
		/// <returns>The CCD serial protocol equivalent to 'data', or NotSpecified if unable to parse.</returns>
		public static eComProtocolType GetProtocol(string data)
		{
			switch (data.ToUpper(CultureInfo.InvariantCulture))
			{
				case "RS232":
					return eComProtocolType.ComspecProtocolRS232;

				case "RS422":
					return eComProtocolType.ComspecProtocolRS422;

				case "RS485":
					return eComProtocolType.ComspecProtocolRS485;

				default:
					Logger.Error(string.Format("GetProtocol() - Unknown argument given: {0}, setting to NotSpecified.", data));
					return eComProtocolType.NotSpecified;
			}
		}

		// .NET 4.0+ support only. Commented out for 3-series support.
		//private static Assembly CurrentDomain_AssemblyResolve()
		//{
		//    Logger.Warn(string.Format(
		//        "{0} contains dependencies. Attempting to load {1}...",
		//        args.RequestingAssembly.FullName,
		//        args.Name));

		//    foreach (var reference in args.RequestingAssembly.GetReferencedAssemblies())
		//    {
		//        if (reference.FullName.Equals(args.Name))
		//        {
		//            try
		//            {
		//                Assembly resolved = Assembly.LoadFrom(string.Format("{0}/{1}.dll", DirectoryHelper.GetUserFolder(), reference.Name));
		//                return resolved;
		//            }
		//            catch (Exception e)
		//            {
		//                Logger.Error(e, "DriverLoader Assembly resolve handler failed.");
		//            }
		//        }
		//    }

		//    return null;
		//}
	}
}
