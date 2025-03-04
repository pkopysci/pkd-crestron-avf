namespace pkd_hardware_service.AvSwitchDevices
{
	using Crestron.SimplSharpPro;
	using pkd_common_utils.FileOps;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.RoutingData;
	using AvIpMatrix;
	using DmMd400;
	using DmMd8x1;
	using System.Collections.Generic;

	/// <summary>
	/// Factory class used to build a hardware control interface for all AV switching devices.
	/// </summary>
	internal static class AvSwitchFactory
	{
		/// <summary>
		/// Attempts to create an Audio/video switcher hardware control based on the given configuration data.
		/// If the Model defined in the configuration data is not supported then null is returned.
		/// </summary>
		/// <param name="sources">The domain data containing all routable sources in the system.</param>
		/// <param name="destinations">The domain data containing all routable destinations in the system.</param>
		/// <param name="switchData">the deserialized AV switch configuration data.</param>
		/// <param name="processor">The host control system running this program.</param>
		/// <param name="hwService">The hardware control service that will contain the final control object.</param>
		/// <returns>The AV switch control if supported, null otherwise.</returns>
		public static IAvSwitcher? CreateAvSwitcher(
			List<Source> sources,
			List<Destination> destinations,
			MatrixData switchData,
			CrestronControlSystem processor,
			IInfrastructureService hwService)
		{
			ParameterValidator.ThrowIfNull(switchData, "CreateAvSwitcher", "displayData");
			ParameterValidator.ThrowIfNull(processor, "CreateAvSwitcher", "processor");
			ParameterValidator.ThrowIfNull(hwService, "CreateAvSwitcher", "hwService");

			switch (switchData.ClassName.ToUpper())
			{
				case "DMMD8X1":
					return Get8X1Switch(switchData, processor, hwService);

				case "HDMD400":
					return GetMd400Switch(switchData, processor, hwService);

				default:
					return LoadSwitchDriver(sources, destinations, switchData, processor, hwService);
			}
		}

		private static DmMd8X1AvSwitch Get8X1Switch(
			MatrixData switchData,
			CrestronControlSystem processor,
#pragma warning disable IDE0060 // Remove unused parameter
			IInfrastructureService hwService)
#pragma warning restore IDE0060 // Remove unused parameter
		{
			Logger.Info("Create Dm-MD-8x1 switch with device ID {0}", switchData.Id);
			return new DmMd8X1AvSwitch(switchData, processor);
		}

		private static DmMd400AvSwitch GetMd400Switch(
			MatrixData switchData,
			CrestronControlSystem processor,
#pragma warning disable IDE0060 // Remove unused parameter
			IInfrastructureService hwService)
#pragma warning restore IDE0060 // Remove unused parameter
		{
			Logger.Info("Create DM-MD-400 switch with device ID {0}", switchData.Id);
			return new DmMd400AvSwitch(switchData, processor);
		}

		private static IAvSwitcher? LoadSwitchDriver(
			List<Source> sources,
			List<Destination> destinations,
			MatrixData switchData,
			CrestronControlSystem processor,
#pragma warning disable IDE0060 // Remove unused parameter
			IInfrastructureService hwService)
#pragma warning restore IDE0060 // Remove unused parameter
		{
			var path = DirectoryHelper.NormalizePath(switchData.Connection.Driver);

			var device = DriverLoader.LoadClassByInterface<IAvSwitcher>(
				path,
				switchData.ClassName,
				"IAvSwitcher");

			if (device == null)
			{
				Logger.Info("LoadSwitchDriver() - Failed to create device. See error log for details.");
				return null;
			}

			// disabling warning since plugins may implement both interfaces and the framework uses this factory
			// to create both objects.
			// ReSharper disable once SuspiciousTypeConversion.Global
			if (device is IAvIpMatrix deviceAsIpMatrix)
			{
				foreach (var source in sources)
				{
					deviceAsIpMatrix.AddEndpoint(
						source.Id,
						source.Tags,
						source.Input,
						AvIpEndpointTypes.Encoder,
						processor);
				}

				foreach (var dest in destinations)
				{
					deviceAsIpMatrix.AddEndpoint(
						dest.Id,
						dest.Tags,
						dest.Output,
						AvIpEndpointTypes.Decoder,
						processor);
				}
			}

			if (!string.IsNullOrEmpty(switchData.Manufacturer))
			{
				device.Manufacturer = switchData.Manufacturer;
			}

			if (!string.IsNullOrEmpty(switchData.Model))
			{
				device.Model = switchData.Model;
			}
			
			device.Initialize(
				switchData.Connection.Host,
				switchData.Connection.Port,
				switchData.Id,
				switchData.Label,
				switchData.Inputs,
				switchData.Outputs);

			return device;
		}
	}
}
