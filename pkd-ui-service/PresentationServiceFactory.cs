// ReSharper disable SuspiciousTypeConversion.Global

using System.Collections.ObjectModel;
using pkd_application_service.VideoWallControl;

namespace pkd_ui_service
{
	using Crestron.SimplSharpPro;
	using pkd_application_service;
	using pkd_application_service.CustomEvents;
	using pkd_application_service.UserInterface;
	using pkd_common_utils.FileOps;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using Fusion;
	using Interfaces;


	/// <summary>
	/// Helper class for creating presentation service objects.
	/// </summary>
	public static class PresentationServiceFactory
	{
		/// <summary>
		/// Creates a full presentation service object that hooks into the application service events.
		/// </summary>
		/// <param name="appService">The base application service used to control business logic.</param>
		/// <param name="control">The control system entry point for this program.</param>
		/// <returns>A Presentation service that can be initialized for interacting with user interface hardware.</returns>
		public static IPresentationService CreatePresentationService(IApplicationService appService, CrestronControlSystem control)
		{
			ParameterValidator.ThrowIfNull(appService, "CreatePresentationService", "appService");
			ParameterValidator.ThrowIfNull(control, "CreatePresentationService", "control");
			return new PresentationService(appService, control);
		}

		internal static IUserInterface? CreateUserInterface(
			CrestronControlSystem parent,
			UserInterfaceDataContainer uiData,
			IApplicationService appService)
		{
			if (string.IsNullOrEmpty(uiData.ClassName) || string.IsNullOrEmpty(uiData.Library))
			{
				Logger.Error("pkd_ui_service.PresentationServiceFactory.CreateUserInterface() - UiData argument must contain ClassName and Library entries.");
				return null;
			}
			else
			{
				Logger.Debug($"PresentationServiceFactory.CreateUserInterface() - Loading {uiData.Library}.{uiData.ClassName}");
				return CreateInterfaceFromPlugin(parent, uiData, appService);
			}
		}

		internal static IFusionInterface CreateFusionService(
			IApplicationService appService,
			CrestronControlSystem parent)
		{
			var data = appService.GetFusionInterface();
			var fusion = new FusionInterface((uint)data.IpId, parent, data.Label, data.Id);
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

		private static IUserInterface? CreateInterfaceFromPlugin(
			CrestronControlSystem parent,
			UserInterfaceDataContainer uiData,
			IApplicationService appService)
		{
			var device = DriverLoader.LoadClassByInterface<IUserInterface>(
				uiData.Library,
				uiData.ClassName,
				"IUserInterface");

			if (device == null)
			{
				Logger.Error("PresentationServiceFactory.CreateInterfaceFromPlugin() - Failed to create device. See logs for details.");
				return null;
			}

			(device as IHtmlUserInterface)?.SetSystemType(appService.GetRoomInfo().SystemType);
			device.SetUiData(uiData);
			(device as ICrestronUserInterface)?.SetCrestronControl(parent, uiData.IpId);
			(device as IDisplayUserInterface)?.SetDisplayData(appService.GetAllDisplayInfo());
			(device as IRoutingUserInterface)?.SetRoutingData(appService.GetAllAvSources(), appService.GetAllAvDestinations(), appService.GetAllAvRouters());
			(device as IAudioUserInterface)?.SetAudioData(
				appService.GetAudioInputChannels(),
				appService.GetAudioOutputChannels(),
				appService.GetAllAudioDspDevices());
			(device as ITransportControlUserInterface)?.SetCableBoxData(appService.GetAllCableBoxes());
			(device as ILightingUserInterface)?.SetLightingData(appService.GetAllLightingDeviceInfo());

			if (device is ICustomEventUserInterface eventUi && appService is ICustomEventAppService eventApp)
			{
				foreach (var item in eventApp.QueryAllCustomEvents())
				{
					eventUi.AddCustomEvent(item.Id, item.Label, item.IsActive);
				}
			}

			if (device is IVideoWallUserInterface videoWallUi)
			{
				var wallDevices = (appService as IVideoWallApp)?.GetAllVideoWalls() ??
				                  ReadOnlyCollection<VideoWallInfoContainer>.Empty;
				videoWallUi.SetVideoWallData(wallDevices);
			}

			return device;
		}
	}
}
