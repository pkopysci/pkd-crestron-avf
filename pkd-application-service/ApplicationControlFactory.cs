namespace pkd_application_service
{
	using pkd_application_service.AudioControl;
	using pkd_application_service.AvRouting;
	using pkd_application_service.Base;
	using pkd_application_service.DisplayControl;
	using pkd_application_service.EndpointControl;
	using pkd_application_service.LightingControl;
	using pkd_application_service.SystemPower;
	using pkd_application_service.TransportControl;
	using pkd_application_service.UserInterface;
	using pkd_common_utils.FileOps;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service;
	using pkd_hardware_service;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Helper class for creating an implementation of IApplicationService.
	/// </summary>
	public static class ApplicationControlFactory
	{
		/// <summary>
		/// Creates the application service implementation and stores the hardware connections and control data.
		/// If there is no custom application service defined in the domain then the defaul application service will be created.
		/// </summary>
		/// <param name="hwService">The infrastructure implementation for controlling devices.</param>
		/// <param name="domain">The system configuration data provider.</param>
		/// <returns>a poplulated application service that is ready for initialization.</returns>
		/// <exception cref="ArgumentNullException">if hwService or domian is null.</exception>
		public static IApplicationService CreateAppService(
			IInfrastructureService hwService,
			IDomainService domain)
		{
			ParameterValidator.ThrowIfNull(hwService, "CreateAppService", "hwService");
			ParameterValidator.ThrowIfNull(domain, "CreateAppService", "domain");
			if (string.IsNullOrEmpty(domain.RoomInfo.Logic.AppServiceLibrary)
				|| string.IsNullOrEmpty(domain.RoomInfo.Logic.AppServiceClass))
			{
				Logger.Warn("No Application plugin found - loading default classroom behavior.");
				return LoadDefaultBehavior(hwService, domain);
			}
			else
			{
				return LoadPluginBehavior(hwService, domain);
			}
		}

		internal static ISystemPowerApp CreateSystemPower(IInfrastructureService hwService, IDomainService domain, IApplicationService appService)
		{
			ParameterValidator.ThrowIfNull(hwService, "CreateSystemPower", "hwService");
			ParameterValidator.ThrowIfNull(domain, "CreateSystemPower", "domain");
			ParameterValidator.ThrowIfNull(appService, "CreateSystemPower", "appService");
			return new SystemPowerApp(hwService, domain);
		}

		internal static IDisplayControlApp CreateDisplayControl(IInfrastructureService hwService, IDomainService domain, IApplicationService appService)
		{
			ParameterValidator.ThrowIfNull(hwService, "CreateDisplayControl", "hwService");
			ParameterValidator.ThrowIfNull(domain, "CreateDisplayControl", "domain");
			ParameterValidator.ThrowIfNull(appService, "CreateDisplayControl", "appService");
			return new DisplayControlApp(hwService.Displays, domain.Displays, appService);
		}

		internal static IEndpointControlApp CreateEndpointControl(IInfrastructureService hwService, IDomainService domain, IApplicationService appService)
		{
			ParameterValidator.ThrowIfNull(hwService, "CreateEndpointControl", "hwService");
			ParameterValidator.ThrowIfNull(domain, "CreateEndpointControl", "domain");
			ParameterValidator.ThrowIfNull(appService, "CreateEndpointControl", "appService");
			return new EndpointControlApp(hwService.Endpoints, domain.Endpoints);
		}

		internal static IAudioControlApp CreateAudioControl(IInfrastructureService hwService, IDomainService domain, IApplicationService appService)
		{
			ParameterValidator.ThrowIfNull(hwService, "CreateAudioControl", "hwService");
			ParameterValidator.ThrowIfNull(domain, "CreateAudioControl", "domain");
			ParameterValidator.ThrowIfNull(appService, "CreateAudioControl", "appService");

			List<AudioChannelInfoContainer> dspInputs = new List<AudioChannelInfoContainer>();
			List<AudioChannelInfoContainer> dspOutputs = new List<AudioChannelInfoContainer>();
			Dictionary<string, List<InfoContainer>> allPresets = new Dictionary<string, List<InfoContainer>>();
			foreach (var dsp in domain.Dsps)
			{
				List<InfoContainer> presets = new List<InfoContainer>();
				foreach (var item in dsp.Presets)
				{
					presets.Add(new InfoContainer(
						item.Id,
						string.Format("{0}.{1}", item.Bank, item.Index),
						"",
						new List<string>() { dsp.Id })
					);
				}

				allPresets.Add(dsp.Id, presets);
			}

			foreach (var channel in domain.AudioChannels)
			{
				if (channel.Tags.Contains("input"))
				{
					List<InfoContainer> zoneInfo = new List<InfoContainer>();
					foreach (var zone in channel.ZoneEnableToggles)
					{
						zoneInfo.Add(new InfoContainer(zone.ZoneId, zone.Label, string.Empty, new List<string>() { zone.Tag }));
					}

					dspInputs.Add(new AudioChannelInfoContainer(channel.Id, channel.Label, channel.Icon, channel.Tags, zoneInfo));
				}
				else if (channel.Tags.Contains("output"))
				{
					List<InfoContainer> zoneInfo = new List<InfoContainer>();
					foreach (var zone in channel.ZoneEnableToggles)
					{
						zoneInfo.Add(new InfoContainer(zone.ZoneId, zone.Label, string.Empty, new List<string>() { zone.Tag }));
					}

					dspOutputs.Add(new AudioChannelInfoContainer(channel.Id, channel.Label, channel.Icon, channel.Tags, zoneInfo));
				}
			}

			return new AudioControlApp(hwService.Dsps, dspInputs, dspOutputs, allPresets);
		}

		internal static IAvRoutingApp CreateRoutingControl(IInfrastructureService hwService, IDomainService domain, IApplicationService appService)
		{
			ParameterValidator.ThrowIfNull(hwService, "CreateRoutingControl", "hwService");
			ParameterValidator.ThrowIfNull(domain, "CreateRoutingControl", "domain");
			ParameterValidator.ThrowIfNull(appService, "CreateRoutingControl", "appService");

			return new AvRoutingApp(hwService.AvSwitchers, domain);
		}

		internal static ILightingControlApp CreateLightingControl(IInfrastructureService hwService, IDomainService domain, IApplicationService appService)
		{
			ParameterValidator.ThrowIfNull(hwService, "CreateLightingControl", "hwService");
			ParameterValidator.ThrowIfNull(domain, "CreateLightingControl", "domain");
			ParameterValidator.ThrowIfNull(appService, "CreateLightingControl", "appService");

			return new LightingControlApp(hwService.LightingDevices, domain.Lighting, appService);
		}

		internal static List<UserInterfaceDataContainer> CreateUserInterfaceData(IDomainService domain)
		{
			List<UserInterfaceDataContainer> interfaces = new List<UserInterfaceDataContainer>();
			foreach (var item in domain.UserInterfaces)
			{
				var ui = new UserInterfaceDataContainer(
					item.Id,
					domain.RoomInfo.RoomName,
					domain.RoomInfo.HelpContact,
					item.Id,
					item.Model,
					item.ClassName,
					item.Library,
					item.Sgd,
					item.DefaultActivity,
					item.IpId,
					item.Tags);

				foreach (var menuItem in item.Menu)
				{
					ui.AddMenuItem(new MenuItemDataContainer(
						menuItem.Id,
						menuItem.Label,
						menuItem.Icon,
						menuItem.Control,
						menuItem.SourceSelect,
						menuItem.Tags));
				}

				interfaces.Add(ui);
			}

			return interfaces;
		}

		internal static ITransportControlApp CreateTransportControl(IInfrastructureService hwService, IDomainService domain, IApplicationService appService)
		{
			ParameterValidator.ThrowIfNull(hwService, "CreateTransportControl", "hwService");
			ParameterValidator.ThrowIfNull(domain, "CreateTransportControl", "domain");
			ParameterValidator.ThrowIfNull(appService, "CreateTransportControl", "appService");

			return new TransportControlApp(hwService.CableBoxes, domain.CableBoxes);
		}

		private static IApplicationService LoadDefaultBehavior(IInfrastructureService hwService, IDomainService domain)
		{
			try
			{
				var appService = new ApplicationService();
				appService.Initialize(hwService, domain);
				return appService;
			}
			catch (Exception e)
			{
				Logger.Error(e, "ApplicationControLFactory.LoadDefaultBehavior() - Failed to create application service.");
				return null;
			}
		}

		private static IApplicationService LoadPluginBehavior(IInfrastructureService hwService, IDomainService domain)
		{
			try
			{
				IApplicationService service = DriverLoader.LoadClassByInterface<IApplicationService>(
					domain.RoomInfo.Logic.AppServiceLibrary,
					domain.RoomInfo.Logic.AppServiceClass,
					"IApplicationService");

				service.Initialize(hwService, domain);
				return service;
			}
			catch (Exception e)
			{
				Logger.Error(e, "ApplicationControlFactory.LoadPluginBehavior() - Failed to load application plugin. Loading defaults.");
				return LoadDefaultBehavior(hwService, domain);
			}
		}
	}
}
