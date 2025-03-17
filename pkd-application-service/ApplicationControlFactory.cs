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

namespace pkd_application_service;

/// <summary>
/// Helper class for creating an implementation of IApplicationService.
/// </summary>
public static class ApplicationControlFactory
{
	/// <summary>
	/// Creates the application service implementation and stores the hardware connections and control data.
	/// If there is no custom application service defined in the domain then the default application service will be created.
	/// </summary>
	/// <param name="hwService">The infrastructure implementation for controlling devices.</param>
	/// <param name="domain">The system configuration data provider.</param>
	/// <returns>a populated application service that is ready for initialization.</returns>
	/// <exception cref="ArgumentNullException">if hwService or domain is null.</exception>
	public static IApplicationService? CreateAppService(
		IInfrastructureService hwService,
		IDomainService domain)
	{
		ParameterValidator.ThrowIfNull(hwService, "CreateAppService", nameof(hwService));
		ParameterValidator.ThrowIfNull(domain, "CreateAppService", nameof(domain));
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
		ParameterValidator.ThrowIfNull(hwService, "CreateSystemPower", nameof(hwService));
		ParameterValidator.ThrowIfNull(domain, "CreateSystemPower", nameof(domain));
		ParameterValidator.ThrowIfNull(appService, "CreateSystemPower", nameof(appService));
		return new SystemPowerApp(hwService, domain);
	}

	internal static IDisplayControlApp CreateDisplayControl(IInfrastructureService hwService, IDomainService domain, IApplicationService appService)
	{
		ParameterValidator.ThrowIfNull(hwService, "CreateDisplayControl", nameof(hwService));
		ParameterValidator.ThrowIfNull(domain, "CreateDisplayControl", nameof(domain));
		ParameterValidator.ThrowIfNull(appService, "CreateDisplayControl", nameof(appService));
		return new DisplayControlApp(hwService.Displays, domain.Displays, appService);
	}

	internal static IEndpointControlApp CreateEndpointControl(IInfrastructureService hwService, IDomainService domain, IApplicationService appService)
	{
		ParameterValidator.ThrowIfNull(hwService, "CreateEndpointControl", nameof(hwService));
		ParameterValidator.ThrowIfNull(domain, "CreateEndpointControl", nameof(domain));
		ParameterValidator.ThrowIfNull(appService, "CreateEndpointControl", nameof(appService));
		return new EndpointControlApp(hwService.Endpoints, domain.Endpoints);
	}

	internal static IAudioControlApp CreateAudioControl(IInfrastructureService hwService, IDomainService domain, IApplicationService appService)
	{
		ParameterValidator.ThrowIfNull(hwService, "CreateAudioControl", nameof(hwService));
		ParameterValidator.ThrowIfNull(domain, "CreateAudioControl", nameof(domain));
		ParameterValidator.ThrowIfNull(appService, "CreateAudioControl", nameof(appService));

		List<AudioChannelInfoContainer> dspInputs = [];
		List<AudioChannelInfoContainer> dspOutputs = [];
		var allPresets = new Dictionary<string, List<InfoContainer>>();
		foreach (var dsp in domain.Dsps)
		{
			List<InfoContainer> presets = [];
			foreach (var item in dsp.Presets)
			{
				presets.Add(new InfoContainer(
					item.Id,
					$"{item.Bank}.{item.Index}",
					"",
					[dsp.Id])
				);
			}

			allPresets.Add(dsp.Id, presets);
		}

		foreach (var channel in domain.AudioChannels)
		{
			if (channel.Tags.Contains("input"))
			{
				List<InfoContainer> zoneInfo = [];
				foreach (var zone in channel.ZoneEnableToggles)
				{
					zoneInfo.Add(new InfoContainer(zone.ZoneId, zone.Label, string.Empty, [zone.Tag]));
				}

				dspInputs.Add(new AudioChannelInfoContainer(channel.Id, channel.Label, channel.Icon, channel.Tags, zoneInfo));
			}
			else if (channel.Tags.Contains("output"))
			{
				List<InfoContainer> zoneInfo = [];
				foreach (var zone in channel.ZoneEnableToggles)
				{
					zoneInfo.Add(new InfoContainer(zone.ZoneId, zone.Label, string.Empty, [zone.Tag]));
				}

				dspOutputs.Add(new AudioChannelInfoContainer(channel.Id, channel.Label, channel.Icon, channel.Tags, zoneInfo));
			}
		}

		return new AudioControlApp(hwService.Dsps, dspInputs, dspOutputs, allPresets);
	}

	internal static IAvRoutingApp CreateRoutingControl(IInfrastructureService hwService, IDomainService domain, IApplicationService appService)
	{
		ParameterValidator.ThrowIfNull(hwService, "CreateRoutingControl", nameof(hwService));
		ParameterValidator.ThrowIfNull(domain, "CreateRoutingControl", nameof(domain));
		ParameterValidator.ThrowIfNull(appService, "CreateRoutingControl", nameof(appService));

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
		ParameterValidator.ThrowIfNull(hwService, "CreateTransportControl", nameof(hwService));
		ParameterValidator.ThrowIfNull(domain, "CreateTransportControl", nameof(domain));
		ParameterValidator.ThrowIfNull(appService, "CreateTransportControl", nameof(appService));

		return new TransportControlApp(hwService.CableBoxes, domain.CableBoxes);
	}

	private static ApplicationService? LoadDefaultBehavior(IInfrastructureService hwService, IDomainService domain)
	{
		try
		{
			var appService = new ApplicationService();
			appService.Initialize(hwService, domain);
			return appService;
		}
		catch (Exception e)
		{
			Logger.Error(e, "ApplicationControlFactory.LoadDefaultBehavior() - Failed to create application service.");
			return null;
		}
	}

	private static IApplicationService? LoadPluginBehavior(IInfrastructureService hwService, IDomainService domain)
	{
		try
		{
			Logger.Info($"Creating application service {domain.RoomInfo.Logic.AppServiceClass} from  {domain.RoomInfo.Logic.AppServiceLibrary}...");
			
			var service = DriverLoader.LoadClassByInterface<IApplicationService>(
				domain.RoomInfo.Logic.AppServiceLibrary,
				domain.RoomInfo.Logic.AppServiceClass,
				"IApplicationService");

			if (service == null)
			{
				Logger.Error("ApplicationControlFactory.LoadPluginBehavior() - {0}.{1} not found. Loading default behavior.",
					domain.RoomInfo.Logic.AppServiceLibrary,
					domain.RoomInfo.Logic.AppServiceClass);

				return LoadDefaultBehavior(hwService, domain);
			}
				
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