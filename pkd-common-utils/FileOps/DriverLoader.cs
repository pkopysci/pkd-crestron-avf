using System.Globalization;
using System.Reflection;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;

namespace pkd_common_utils.FileOps
{
	/// <summary>
    /// Helper class used to load a Crestron Certified Driver from a DLL using reflection.
    /// </summary>
    public static class DriverLoader
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
			ParameterValidator.ThrowIfNullOrEmpty(assemblyName, "LoadDriverInstance", nameof(assemblyName));
			ParameterValidator.ThrowIfNullOrEmpty(interfaceName, "LoadDriverInstance", nameof(interfaceName));
			ParameterValidator.ThrowIfNullOrEmpty(transportName, "LoadDriverInstance", nameof(transportName));

			try
			{
				var dllPath = DirectoryHelper.NormalizePath($@"{DirectoryHelper.GetUserFolder()}\net8-plugins\{assemblyName}");

				Logger.Debug($"DriverLoader.LoadDriverInstance() - Attempting to load driver from location {dllPath}...");

				var dll = Assembly.LoadFrom(dllPath);
				foreach (var type in dll.GetTypes())
				{
					var interfaces = type.GetInterfaces();
					if (interfaces.Any(x => x.Name.Equals(interfaceName))
					    && interfaces.Any(x => x.Name.Equals(transportName)))
					{
						return type.FullName != null ? (T?)dll.CreateInstance(type.FullName) : default;
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error(e, $"DriverLoader.LoadDriverInstance({assemblyName},{interfaceName},{transportName})");
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
		public static T? LoadClassByInterface<T>(string assemblyName, string className, string interfaceName)
		{
			ParameterValidator.ThrowIfNullOrEmpty(assemblyName, "LoadClassByInterface", nameof(assemblyName));
			ParameterValidator.ThrowIfNullOrEmpty(interfaceName, "LoadClassByInterface", nameof(interfaceName));

			T? device = default;
			try
			{
				var dllPath =
					DirectoryHelper.NormalizePath($@"{DirectoryHelper.GetUserFolder()}\net8-plugins\{assemblyName}");

				Logger.Debug("Attempting to load driver from file {0}...", dllPath);
				var dll = Assembly.LoadFrom(dllPath);

				// Add event handler to resolve dependencies
				AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver;

				foreach (var type in dll.GetTypes())
				{
					if (!type.Name.Equals(className, StringComparison.InvariantCulture)) continue;
					if (!type.GetInterfaces()
						    .Any(x => x.Name.Equals(interfaceName, StringComparison.InvariantCulture))) continue;

					device = type.FullName != null ? (T?)dll.CreateInstance(type.FullName) : default;
					break;
				}

				Logger.Debug("Done!");
			}
			catch (Exception e)
			{
				Logger.Error(e, $"DriverLoader.LoadClassByInterface({assemblyName},{interfaceName})");
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolver;
			}
			
			return device;
		}

		/// <summary>
		/// Return an instance of a class based on the defined interface name. This version of the method allows for
		/// constructor arguments.
		/// </summary>
		/// <typeparam name="T">The expected return type.</typeparam>
		/// <param name="assemblyName">The full directory and file name with extension of the target assembly.</param>
		/// <param name="className">The class to search for in the assembly dll.</param>
		/// <param name="interfaceName">The interface to search for in the assembly dll.</param>
		/// <param name="constructorArgs">The constructor arguments to pass when creating the object.</param>
		/// <returns>The target T object, if found in the assembly, or T default if no matching driver is found.</returns>
		public static T? LoadClassByInterface<T>(string assemblyName, string className, string interfaceName,
			object[] constructorArgs)
		{
			ParameterValidator.ThrowIfNullOrEmpty(assemblyName, "LoadClassByInterface", nameof(assemblyName));
			ParameterValidator.ThrowIfNullOrEmpty(interfaceName, "LoadClassByInterface", nameof(interfaceName));

			T? device = default;
			try
			{
				var dllPath =
					DirectoryHelper.NormalizePath($@"{DirectoryHelper.GetUserFolder()}\net8-plugins\{assemblyName}");

				Logger.Debug("Attempting to load driver from file {0}...", dllPath);
				var dll = Assembly.LoadFrom(dllPath);

				// Add event handler to resolve dependencies
				AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolver;

				foreach (var type in dll.GetTypes())
				{
					if (!type.Name.Equals(className, StringComparison.InvariantCulture)) continue;
					if (!type.GetInterfaces()
						    .Any(x => x.Name.Equals(interfaceName, StringComparison.InvariantCulture))) continue;

					var argTypes = new Type[constructorArgs.Length];
					for (var i = 0; i < constructorArgs.Length; i++)
					{
						argTypes[i] = constructorArgs[i].GetType();
					}
					
					//(T?)dll.CreateInstance(type.FullName) : default;
					device = type.FullName != null ? (T?)type.GetConstructor(argTypes)?.Invoke(constructorArgs) : default; 
					break;
				}

				Logger.Debug("Done!");
			}
			catch (Exception e)
			{
				Logger.Error(e, $"DriverLoader.LoadClassByInterface({assemblyName},{interfaceName})");
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= AssemblyResolver;
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
			ParameterValidator.ThrowIfNullOrEmpty(connectionTag, "GetTransportType", nameof(connectionTag));

			switch (connectionTag.ToUpper(CultureInfo.InvariantCulture))
			{
				case "TCP":
					return nameof(ITcp);

				case "HTTP":
					return nameof(IHttp);

				case "TELNET":
					return nameof(ITelnet);

				case "REST":
					return nameof(IRestable);

				case "SERIAL":
					return nameof(ISerial);

				case "IR":
					return nameof(IIr);

				default:
					Logger.Error($"GetTransportType() - Unsupported transport in config: {connectionTag}");
					break;
			}

			return string.Empty;
		}

		/// <summary>
		/// Convert a configuration baud rate argument to a CCD baud rate value.
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
					Logger.Warn($"GetBaudRate - {baudRate} is an unsupported bad rate. Setting to 9600.");
					goto case 9600;
			}
		}

		/// <summary>
		/// Convert a configuration data bits number to a CCD data bits definition.
		/// Writes an error to the logging system if an unrecognized argument is given.
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
					Logger.Error($"GetDataBits() - undefined data bits encountered: {data}");
					return eComDataBits.NotSpecified;
			}
		}

		/// <summary>
		/// Convert a configuration data bits number to a CCD stop bits definition.
		/// Writes an error to the logging system if an unrecognized argument is given.
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
					Logger.Error($"GetStopBits() - Unknown stop bit value: {data}, setting to NotSpecified.");
					return eComStopBits.NotSpecified;
			}
		}

		/// <summary>
		/// Convert a configuration Hardware Handshake argument to a CCD hardware handshake definition.
		/// Writes an error to the logging system if an unrecognized argument is given.
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
					Logger.Error($"GetHwHandshake() - Unknown argument given: {data}, setting to NotSpecified.");
					return eComHardwareHandshakeType.NotSpecified;
			}
		}

		/// <summary>
		/// Convert a configuration Software Handshake argument to a CCD software handshake definition.
		/// Writes an error to the logging system if an unrecognized argument is given.
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
					Logger.Error($"GetSwHandshake() - Unknown argument given: {data}, setting to NotSpecified.");
					return eComSoftwareHandshakeType.NotSpecified;
			}
		}

		/// <summary>
		/// Convert a configuration parity argument to a CCD parity definition.
		/// Writes an error to the logging system if an unrecognized argument is given.
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
					Logger.Error($"GetParity() - Unknown argument given: {data}, setting to NotSpecified.");
					return eComParityType.NotSpecified;
			}
		}

		/// <summary>
		/// Convert a configuration serial protocol argument to a CCD protocol definition.
		/// Writes an error to the logging system if an unrecognized argument is given.
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
					Logger.Error($"GetProtocol() - Unknown argument given: {data}, setting to NotSpecified.");
					return eComProtocolType.NotSpecified;
			}
		}

		private static Assembly? AssemblyResolver(object? sender, ResolveEventArgs args)
		{
			var dependencyName = new AssemblyName(args.Name).Name;
			var dependencyPath = DirectoryHelper.NormalizePath($@"{DirectoryHelper.GetUserFolder()}\net8-plugins\{dependencyName}.dll");
			return File.Exists(dependencyPath) ? Assembly.LoadFrom(dependencyPath) : null;
		}
	}
}
