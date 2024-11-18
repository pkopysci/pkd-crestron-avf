namespace pkd_ui_service
{
	using Crestron.SimplSharpPro;
	using pkd_application_service;
	using pkd_application_service.CustomEvents;
	using pkd_application_service.UserInterface;
	using pkd_common_utils.FileOps;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_ui_service.Fusion;
	using pkd_ui_service.Interfaces;


	/// <summary>
	/// Helper class for creating presentation service objects.
	/// </summary>
	public static class PresentationServiceFactory
	{
		/// <summary>
		/// Creates a full presentation service object that hooks into the application service evetns.
		/// </summary>
		/// <param name="appService">The base application service used to control business logic.</param>
		/// <param name="control">The control system entry point for this program.</param>
		/// <returns>A Presentation service that can be initilized for interacting with user interface hardware.</returns>
		public static IPresentationService CreatePresentationService(IApplicationService appService, CrestronControlSystem control)
		{
			ParameterValidator.ThrowIfNull(appService, "CreatePresentationService", "appService");
			ParameterValidator.ThrowIfNull(control, "CreatePresentationService", "control");
			return new PresentationService(appService, control);
		}

		internal static IUserInterface CreateUserInterface(
			CrestronControlSystem parent,
			UserInterfaceDataContainer UiData,
			IApplicationService appService)
		{
			if (string.IsNullOrEmpty(UiData.ClassName) || string.IsNullOrEmpty(UiData.Library))
			{
				Logger.Error("pkd_ui_service.PresentationServiceFactory.CreateUserInterface() - UiData argument must contain ClassName and Library entries.");
				return null;
			}
			else
			{
				Logger.Debug($"PresentationServiceFactory.CreateUserInterface() - Loading {UiData.Library}.{UiData.ClassName}");
				return CreateInterfaceFromPlugin(parent, UiData, appService);
			}
		}

		internal static IFusionInterface CreateFusionService(
			IApplicationService appService,
			CrestronControlSystem parent)
		{
			var data = appService.GetFusionInterface();
			FusionInterface fusion = new FusionInterface((uint)data.IpId, parent, data.Label, data.Id);
			foreach (var mic in appService.GetAudioInputChannels())
			{
				fusion.AddMicrophone(mic.Id, mic.Label, mic.Tags.ToArray());
			}

			foreach (var source in appService.GetAllAvSources())
			{
				fusion.AddAvSource(source.Id, source.Label, source.Tags.ToArray());
				fusion.AddDeviceToUseTracking(source.Id, source.Label);
			}

			foreach (var display in appService.GetAllDisplayInfo())
			{
				fusion.AddDisplayToUseTracking(display.Id, display.Label);
			}

			return fusion;
		}

		private static IUserInterface CreateInterfaceFromPlugin(
			CrestronControlSystem parent,
			UserInterfaceDataContainer UiData,
			IApplicationService appService)
		{
			IUserInterface device = DriverLoader.LoadClassByInterface<IUserInterface>(
				UiData.Library,
				UiData.ClassName,
				"IUserInterface");

			if (device == null)
			{
				Logger.Error("PresentationServiceFactory.CreateInterfaceFromPlugin() - Failed to create device. See logs for details.");
				return null;
			}

			device.SetUiData(UiData);
			
			if (device is IHtmlUserInterface htmlInterface)
			{
				htmlInterface.SetSystemType(appService.GetRoomInfo().SystemType);
			}

			if (device is ICrestronUserInterface crestronInterface)
			{
				crestronInterface.SetCrestronControl(parent, UiData.IpId);
			}

			if (device is IDisplayUserInterface displayUI)
			{
				displayUI.SetDisplayData(appService.GetAllDisplayInfo());
			}

			if (device is IRoutingUserInterface routingUI)
			{
				routingUI.SetRoutingData(appService.GetAllAvSources(), appService.GetAllAvDestinations(), appService.GetAllAvRouters());
			}

			if (device is IAudioUserInterface audioUI)
			{
				audioUI.SetAudioData(appService.GetAudioInputChannels(), appService.GetAudioOutputChannels());
			}

			if (device is ITransportControlUserInterface transportControlUI)
			{
				transportControlUI.SetCableBoxData(appService.GetAllCableBoxes());
			}

			if (device is ILightingUserInterface lightingUI)
			{
				lightingUI.SetLightingData(appService.GetAllLightingDeviceInfo());
			}

			if (device is ICustomEventUserInterface && appService is ICustomEventAppService)
			{
				var eventApp = appService as ICustomEventAppService;
				var eventUi = device as ICustomEventUserInterface;
				foreach (var item in eventApp.QueryAllCustomEvents())
				{
					eventUi.AddCustomEvent(item.Id, item.Label, item.IsActive);
				}
			}

			return device;
		}
	}
}
