// ReSharper disable SuspiciousTypeConversion.Global

using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using pkd_application_service;
using pkd_application_service.CameraControl;
using pkd_application_service.CustomEvents;
using pkd_application_service.LightingControl;
using pkd_application_service.VideoWallControl;
using pkd_common_utils.DataObjects;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_ui_service.Fusion;
using pkd_ui_service.Interfaces;
using pkd_ui_service.Utility;
// ReSharper disable MemberCanBePrivate.Global

namespace pkd_ui_service
{
	/// <summary>
	/// Root presentation implementation.
	/// </summary>
	public class PresentationService : IPresentationService, IDisposable
	{
		/// <summary>
		/// Root control system running the application.
		/// </summary>
		protected readonly CrestronControlSystem Control;
		
		/// <summary>
		/// Core application service for managing business logic.
		/// </summary>
		protected readonly IApplicationService AppService;
		
		/// <summary>
		/// All user interfaces in the system configuration.
		/// </summary>
		protected readonly List<IUserInterface> UiConnections;
		
		/// <summary>
		/// a Fusion room connection implementation.
		/// </summary>
		protected IFusionInterface? Fusion;
		
		/// <summary>
		/// timer for showing the powering on/off modal for interfaces that don't implement it locally. 
		/// </summary>
		protected CTimer? StateChangeTimer;
		
		/// <summary>
		/// true = object is disposed, false = not disposed.
		/// </summary>
		protected bool Disposed;
#if DEBUG
		/// <summary>
		/// The amount of time to display the state change modal for interfaces that don't implement it locally.
		/// </summary>
		protected const int TransitionTime = 3000;
#else
		/// <summary>
		/// The amount of time to display the state change modal for interfaces that don't implement it locally.
		/// </summary>
        protected const int TransitionTime = 20000;
#endif
		
		/// <param name="appService">The framework application implementation that handles state management.</param>
		/// <param name="control">the root Crestron control system object.</param>
		public PresentationService(IApplicationService appService, CrestronControlSystem control)
		{
			ParameterValidator.ThrowIfNull(appService, "Ctor", nameof(appService));
			ParameterValidator.ThrowIfNull(control, "Ctor", nameof(control));

			AppService = appService;
			Control = control;
			UiConnections = [];
			BuildInterfaces();
			SubscribeToAppService();
		}

		/// <inheritdoc />
		~PresentationService()
		{
			Dispose(false);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public void Initialize()
		{
			foreach (var uiDevice in UiConnections)
			{
				uiDevice.Connect();
			}

			Fusion?.Initialize();
		}

		/// <summary>
		/// disposes of all internal component objects if they are disposable.
		/// </summary>
		protected void Dispose(bool disposing)
		{
			if (Disposed) return;
			if (disposing)
			{
				UnsubscribeFromAppService();
				UnsubscribeFromInterfaces();
				foreach (var uiConn in UiConnections)
				{
					if (uiConn is IDisposable disposableUiConn)
					{
						disposableUiConn.Dispose();
					}
				}

				StateChangeTimer?.Dispose();

				if (Fusion != null)
				{
					Fusion.OnlineStatusChanged -= FusionConnectionHandler;
					Fusion.MicMuteChangeRequested -= FusionMicMuteHandler;
					Fusion.SystemStateChangeRequested -= FusionPowerHandler;
					Fusion.DisplayPowerChangeRequested -= FusionDisplayPowerHandler;
					Fusion.DisplayBlankChangeRequested -= FusionDisplayBlankHandler;
					Fusion.DisplayFreezeChangeRequested -= FusionDisplayFreezeHandler;
					Fusion.AudioMuteChangeRequested -= FusionAudioMuteHandler;
					Fusion.ProgramAudioChangeRequested -= FusionAudioLevelHandler;
					Fusion.SourceSelectRequested -= FusionRouteSourceHandler;
					Fusion.Dispose();
				}
			}

			Disposed = true;
		}

		/// <summary>
		/// Iterate through all user interface definitions in the application service and create the associated objects
		/// and event subscriptions.
		/// </summary>
		protected void BuildInterfaces()
		{
			foreach (var device in AppService.GetAllUserInterfaces())
			{
				Logger.Info("PresentationService - Building interface {0} with IP-ID {1}", device.Id, device.IpId);
				
				var uiObj = PresentationServiceFactory.CreateUserInterface(Control, device, AppService);
				if (uiObj == null) continue;
				UiConnections.Add(uiObj);
				if (!uiObj.IsInitialized)
				{
					uiObj.Initialize();
				}
				
				SubscribeToInterface(uiObj);
			}

			Fusion = PresentationServiceFactory.CreateFusionService(AppService, Control);
			Fusion.OnlineStatusChanged += FusionConnectionHandler;
			Fusion.MicMuteChangeRequested += FusionMicMuteHandler;
			Fusion.SystemStateChangeRequested += FusionPowerHandler;
			Fusion.DisplayPowerChangeRequested += FusionDisplayPowerHandler;
			Fusion.DisplayBlankChangeRequested += FusionDisplayBlankHandler;
			Fusion.DisplayFreezeChangeRequested += FusionDisplayFreezeHandler;
			Fusion.AudioMuteChangeRequested += FusionAudioMuteHandler;
			Fusion.ProgramAudioChangeRequested += FusionAudioLevelHandler;
			Fusion.SourceSelectRequested += FusionRouteSourceHandler;
		}

		/// <summary>
		/// Subscribe to all application service events for all implemented application interfaces.
		/// </summary>
		protected void SubscribeToAppService()
		{
			AppService.AudioDspConnectionStatusChanged += AppServiceDspConnectionHandler;
			AppService.AudioInputLevelChanged += AppServiceAudioInputLevelHandler;
			AppService.AudioInputMuteChanged += AppServiceAudioInputMuteHandler;
			AppService.AudioOutputLevelChanged += AppServiceAudioOutputLevelHandler;
			AppService.AudioOutputMuteChanged += AppServiceAudioOutputMuteHandler;
			AppService.AudioOutputRouteChanged += AppServiceAudioOutputRouteHandler;
			AppService.AudioZoneEnableChanged += AppServiceAudioZoneEnableHandler;
			AppService.DisplayBlankChange += AppServiceDisplayBlankHandler;
			AppService.DisplayFreezeChange += AppServiceDisplayFreezeHandler;
			AppService.DisplayConnectChange += AppServiceDisplayConnectionHandler;
			AppService.DisplayPowerChange += AppServiceDisplayPowerHandler;
			AppService.DisplayInputChanged += AppServiceDisplayInputChangedHandler;
			AppService.EndpointConnectionChanged += AppServiceEndpointConnectionHandler;
			AppService.EndpointRelayChanged += AppServiceEndpointChangedHandler;
			AppService.RouteChanged += AppServiceRouteHandler;
			AppService.RouterConnectChange += AppServiceRouterConnectionHandler;
			AppService.SystemStateChanged += AppServiceStateChangeHandler;
			AppService.GlobalVideoBlankChanged += AppServiceGlobalBlankHandler;
			AppService.GlobalVideoFreezeChanged += AppServiceGlobalFreezeHandler;
			AppService.LightingSceneChanged += AppServiceLightingSceneHandler;
			AppService.LightingLoadLevelChanged += AppServiceLightingLoadHandler;
			AppService.LightingControlConnectionChanged += AppServiceLightingConnectionHandler;

			if (AppService is CustomEventAppService customEventService)
			{
				customEventService.CustomEventStateChanged += AppServiceCustomEventHandler;
			}

			if (AppService is ITechAuthGroupAppService techService)
			{
				techService.NonTechLockoutStateChangeRequest += AppServiceTechLockoutHandler;
			}

			if (AppService is IVideoWallApp videoWallApp)
			{
				videoWallApp.VideoWallLayoutChanged += VideoWallAppLayoutChangedHandler;
				videoWallApp.VideoWallConnectionStatusChanged += VideoWallAppConnectionChangeHandler;
				videoWallApp.VideoWallCellRouteChanged += VideoWallAppRouteHandler;
			}

			if (AppService is ICameraControlApp cameraApp)
			{
				cameraApp.CameraControlConnectionChanged += CameraAppConnectionChangeHandler;
				cameraApp.CameraPowerStateChanged += CameraAppPowerChangeHandler;
			}
		}

		/// <summary>
		/// Unsubscribe from all application services that were subscribed through <see cref="SubscribeToAppService"/>.
		/// </summary>
		protected void UnsubscribeFromAppService()
		{
			AppService.AudioDspConnectionStatusChanged -= AppServiceDspConnectionHandler;
			AppService.AudioInputLevelChanged -= AppServiceAudioInputLevelHandler;
			AppService.AudioInputMuteChanged -= AppServiceAudioInputMuteHandler;
			AppService.AudioOutputLevelChanged -= AppServiceAudioOutputLevelHandler;
			AppService.AudioOutputMuteChanged -= AppServiceAudioOutputMuteHandler;
			AppService.AudioOutputRouteChanged -= AppServiceAudioOutputRouteHandler;
			AppService.AudioZoneEnableChanged -= AppServiceAudioZoneEnableHandler;
			AppService.DisplayBlankChange -= AppServiceDisplayBlankHandler;
			AppService.DisplayFreezeChange -= AppServiceDisplayFreezeHandler;
			AppService.DisplayConnectChange -= AppServiceDisplayConnectionHandler;
			AppService.EndpointConnectionChanged -= AppServiceEndpointConnectionHandler;
			AppService.EndpointRelayChanged -= AppServiceEndpointChangedHandler;
			AppService.RouteChanged -= AppServiceRouteHandler;
			AppService.RouterConnectChange -= AppServiceRouterConnectionHandler;
			AppService.SystemStateChanged -= AppServiceStateChangeHandler;
			AppService.LightingSceneChanged -= AppServiceLightingSceneHandler;
			AppService.LightingLoadLevelChanged -= AppServiceLightingLoadHandler;
			AppService.LightingControlConnectionChanged -= AppServiceLightingConnectionHandler;

			if (AppService is CustomEventAppService customEventService)
			{
				customEventService.CustomEventStateChanged -= AppServiceCustomEventHandler;
			}

			if (AppService is ITechAuthGroupAppService securityService)
			{
				securityService.NonTechLockoutStateChangeRequest -= AppServiceTechLockoutHandler;
			}

			if (AppService is IVideoWallApp videoWallApp)
			{
				videoWallApp.VideoWallLayoutChanged -= VideoWallAppLayoutChangedHandler;
				videoWallApp.VideoWallConnectionStatusChanged -= VideoWallAppConnectionChangeHandler;
				videoWallApp.VideoWallCellRouteChanged -= VideoWallAppRouteHandler;
			}
			
			if (AppService is ICameraControlApp cameraApp)
			{
				cameraApp.CameraControlConnectionChanged -= CameraAppConnectionChangeHandler;
				cameraApp.CameraPowerStateChanged -= CameraAppPowerChangeHandler;
			}
		}

		/// <summary>
		/// Subscribe to all user interface events for all implemented plugin interfaces.
		/// </summary>
		/// <param name="ui"></param>
		protected void SubscribeToInterface(IUserInterface ui)
		{
			ui.OnlineStatusChanged += UiConnectionHandler;
			ui.SystemStateChangeRequest += UiStatusChangeHandler;
			ui.GlobalBlankToggleRequest += UiGlobalBlankHandler;
			ui.GlobalFreezeToggleRequest += UiGlobalFreezeHandler;

			if (ui is IRoutingUserInterface routingInterface)
			{
				routingInterface.AvRouteChangeRequest += UiRouteChangeHandler;
			}

			if (ui is IDisplayUserInterface displayInterface)
			{
				displayInterface.DisplayBlankChangeRequest += UiDisplayBlankHandler;
				displayInterface.DisplayFreezeChangeRequest += UiDisplayFreezeHandler;
				displayInterface.DisplayPowerChangeRequest += UiDisplayPowerHandler;
				displayInterface.DisplayScreenDownRequest += UiDisplayScreenDownHandler;
				displayInterface.DisplayScreenUpRequest += UiDisplayScreenUpHandler;
				displayInterface.StationLecternInputRequest += UiSetStationLecternHandler;
				displayInterface.StationLocalInputRequest += UiSetStationLocalHandler;
			}

			if (ui is IAudioUserInterface audioUi)
			{
				audioUi.AudioInputLevelDownRequest += UiAudioInputLevelDownRequest;
				audioUi.AudioInputLevelUpRequest += UiAudioInputLevelUpRequest;
				audioUi.AudioInputMuteChangeRequest += UiAudioInputMuteRequest;
				audioUi.AudioOutputLevelDownRequest += UiAudioOutputLevelDownRequest;
				audioUi.AudioOutputLevelUpRequest += UiAudioOutputLevelUpRequest;
				audioUi.AudioOutputMuteChangeRequest += UiAudioOutputMuteRequest;
				audioUi.AudioOutputRouteRequest += UiAudioOutputRouteRequest;
				audioUi.AudioZoneEnableToggleRequest += UiAudioZoneToggleHandler;
			}

			if (ui is IAudioDiscreteLevelUserInterface discreteAudioUi)
			{
				discreteAudioUi.SetAudioInputLevelRequest += DiscreteAudioInputUiHandler;
				discreteAudioUi.SetAudioOutputLevelRequest += DiscreteAudioOutputUiHandler;
			}

			if (ui is ITransportControlUserInterface transportUi)
			{
				transportUi.TransportControlRequest += TransportUi_TransportControlRequest;
				transportUi.TransportDialFavoriteRequest += TransportUi_TransportDialFavoriteRequest;
				transportUi.TransportDialRequest += TransportUi_TransportDialRequest;
			}

			if (ui is ICustomEventUserInterface eventUi)
			{
				eventUi.CustomEventChangeRequest += EventUi_CustomEventStateChanged;
			}

			if (ui is ILightingUserInterface lightingUi)
			{
				lightingUi.LightingSceneRecallRequest += LightingUiSceneHandler;
				lightingUi.LightingLoadChangeRequest += LightingUiLoadHandler;
			}

			if (ui is IVideoWallUserInterface videoWallUi)
			{
				videoWallUi.VideoWallLayoutChangeRequest += VideoWallUiLayoutHandler;
				videoWallUi.VideoWallRouteRequest += VideoWallRouteHandler;
			}

			if (ui is ICameraUserInterface cameraUi)
			{
				cameraUi.CameraPanTiltRequest += CameraUiPanTiltHandler;
				cameraUi.CameraZoomRequest += CameraUiZoomHandler;
				cameraUi.CameraPowerChangeRequest += CameraUiPowerHandler;
				cameraUi.CameraPresetRecallRequest += CameraPresetRecallHandler;
				cameraUi.CameraPresetSaveRequest += CameraPresetSaveHandler;
			}
		}

		/// <summary>
		/// Unsubscribe from all events handlers added by <see cref="BuildInterfaces"/>.
		/// </summary>
		protected void UnsubscribeFromInterfaces()
		{
			foreach (var ui in UiConnections)
			{
				ui.OnlineStatusChanged -= UiConnectionHandler;
				ui.SystemStateChangeRequest -= UiStatusChangeHandler;
				ui.GlobalFreezeToggleRequest -= UiGlobalFreezeHandler;
				ui.GlobalBlankToggleRequest -= UiGlobalBlankHandler;

				if (ui is IRoutingUserInterface routingUserInterface)
				{
					routingUserInterface.AvRouteChangeRequest -= UiRouteChangeHandler;
				}

				if (ui is IDisplayUserInterface displayUi)
				{
					displayUi.DisplayBlankChangeRequest -= UiDisplayBlankHandler;
					displayUi.DisplayFreezeChangeRequest -= UiDisplayFreezeHandler;
					displayUi.DisplayPowerChangeRequest -= UiDisplayPowerHandler;
					displayUi.DisplayScreenDownRequest += UiDisplayScreenDownHandler;
					displayUi.DisplayScreenUpRequest += UiDisplayScreenUpHandler;
				}

				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.AudioInputLevelDownRequest -= UiAudioInputLevelDownRequest;
					audioUi.AudioInputLevelUpRequest -= UiAudioInputLevelUpRequest;
					audioUi.AudioInputMuteChangeRequest -= UiAudioInputMuteRequest;
					audioUi.AudioOutputLevelDownRequest -= UiAudioOutputLevelDownRequest;
					audioUi.AudioOutputLevelUpRequest -= UiAudioOutputLevelUpRequest;
					audioUi.AudioOutputMuteChangeRequest -= UiAudioOutputMuteRequest;
					audioUi.AudioZoneEnableToggleRequest -= UiAudioZoneToggleHandler;
				}

				if (ui is ITransportControlUserInterface transportUi)
				{
					transportUi.TransportControlRequest -= TransportUi_TransportControlRequest;
					transportUi.TransportDialFavoriteRequest -= TransportUi_TransportDialFavoriteRequest;
					transportUi.TransportDialRequest -= TransportUi_TransportDialRequest;
				}

				if (ui is ICustomEventUserInterface eventUi)
				{
					eventUi.CustomEventChangeRequest -= EventUi_CustomEventStateChanged;
				}
				
				if (ui is ICameraUserInterface cameraUi)
				{
					cameraUi.CameraPanTiltRequest -= CameraUiPanTiltHandler;
					cameraUi.CameraZoomRequest -= CameraUiZoomHandler;
					cameraUi.CameraPowerChangeRequest -= CameraUiPowerHandler;
					cameraUi.CameraPresetRecallRequest -= CameraPresetRecallHandler;
					cameraUi.CameraPresetSaveRequest -= CameraPresetSaveHandler;
				}
			}
		}

		/// <summary>
		/// Callback action that is triggered once <see cref="StateChangeTimer"/> expires.
 		/// </summary>
		/// <param name="obj">The user object provided when the timer is set.</param>
		protected void StateChangeTimerCallback(object? obj)
		{
			foreach (var conn in UiConnections)
			{
				conn.HideSystemStateChanging();
			}
		}

		/// <summary>
		/// Begin the timer for system state change events.
		/// </summary>
		protected void TriggerStateChangeTimer()
		{
			if (StateChangeTimer != null)
			{
				StateChangeTimer.Reset(TransitionTime);
			}
			else
			{
				StateChangeTimer = new CTimer(StateChangeTimerCallback, TransitionTime);
			}
		}

		#region AppService Handlers

		/// <summary>
		/// Handle camera connection change notifications from the application service.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">args.Arg = the id of the camera that updated.</param>
		protected void CameraAppConnectionChangeHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			if (sender is not ICameraControlApp cameraApp) return;
			var newState = cameraApp.QueryCameraConnectionStatus(args.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is not ICameraUserInterface cameraUi) continue;
				cameraUi.SetCameraConnectionStatus(args.Arg, newState);
			}
			
			
			if (newState)
			{
				Fusion?.ClearOfflineDevice(args.Arg);
			}
			else
			{
				var found = cameraApp.GetAllCameraDeviceInfo().First(x => x.Id.Equals(args.Arg));
				Fusion?.AddOfflineDevice(args.Arg, found.Label );
			}
		}

		/// <summary>
		/// Handle camera power status notifications from the application service.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">args.Arg = the id of the camera that updated.</param>
		protected void CameraAppPowerChangeHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			if (sender is not ICameraControlApp cameraApp) return;
			var newState = cameraApp.QueryCameraPowerStatus(args.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is not ICameraUserInterface cameraUi) continue;
				cameraUi.SetCameraPowerState(args.Arg, newState);
			}
		}
		
		/// <summary>
		/// Handle video wall layout change notifications from the application service.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">args.Arg = the id of the video wall that updated.</param>
		protected void VideoWallAppLayoutChangedHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			if (sender is not IVideoWallApp videoWallApp) return;
			var activeLayoutId = videoWallApp.QueryActiveVideoWallLayout(args.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is not IVideoWallUserInterface videoWallUi) continue;
				videoWallUi.UpdateActiveVideoWallLayout(args.Arg, activeLayoutId);
			}
		}

		/// <summary>
		/// Handle video wall device connection status notifications from the application service.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">args.Arg = the id of the video wall that updated.</param>
		protected void VideoWallAppConnectionChangeHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			if (sender is not IVideoWallApp videoWallApp) return;
			var onlineStatus = videoWallApp.QueryVideoWallConnectionStatus(args.Arg);
			if (onlineStatus)
				Fusion?.ClearOfflineDevice(args.Arg);
			else
				Fusion?.AddOfflineDevice(args.Arg, $"Video Wall Controller {args.Arg}");
			
			foreach (var ui in UiConnections)
			{
				if (ui is not IVideoWallUserInterface videoWallUi) continue;
				videoWallUi.UpdateVideoWallConnectionStatus(args.Arg, onlineStatus);
			}
		}

		/// <summary>
		/// Handle video wall device connection status notifications from the application service.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">args.Arg1 = the id of the video wall that updated., Arg2 = ID of the cell that updated.</param>
		protected void VideoWallAppRouteHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			if (sender is not IVideoWallApp videoWallApp) return;
			var newRoute = videoWallApp.QueryVideoWallCellSource(args.Arg1, args.Arg2);
			foreach (var ui in UiConnections)
			{
				if (ui is not IVideoWallUserInterface videoWallUi) continue;
				videoWallUi.UpdateCellRoutedSource(args.Arg1, args.Arg2, newRoute);
			}
		}
		
		/// <summary>
		/// Handle technician "lockout" state notifications from the application service.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">args.Arg = true = all non-tech UIs should lock, false = unlock.</param>
		protected void AppServiceTechLockoutHandler(object? sender, GenericSingleEventArgs<bool> e)
		{
			foreach (var ui in UiConnections)
			{
				if (ui is not ISecurityUserInterface secureUi) continue;
				if (e.Arg)
				{
					secureUi.EnableTechOnlyLock();
				}
				else
				{
					secureUi.DisableTechOnlyLock();
				}
			}
		}

		/// <summary>
		/// Handle technician "lockout" state notifications from the application service.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">args.Arg = true = all non-tech UIs should lock, false = unlock.</param>
		protected void AppServiceLightingLoadHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			Logger.Debug("PresentationService.AppServiceLightingLoadHandler({0}, {1})", e.Arg1, e.Arg2);
			if (sender is not ILightingControlApp lightingService) return;

			var loadLevel = lightingService.GetZoneLoad(e.Arg1, e.Arg2);
			foreach (var ui in UiConnections)
			{
				if (ui is ILightingUserInterface lightingUi)
				{
					lightingUi.UpdateLightingZoneLoad(e.Arg1, e.Arg2, loadLevel);
				}
			}
		}

		/// <summary>
		/// Handle lighing scene change notifications from the application service.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = the id of the lighting controller that updated.</param>
		protected void AppServiceLightingSceneHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			if (sender is not ILightingControlApp lightingService) return;
			var scene = lightingService.GetActiveScene(e.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is ILightingUserInterface lightingUi)
				{
					lightingUi.UpdateActiveLightingScene(e.Arg, scene);
				}
			}
		}

		/// <summary>
		/// Handle notifications from the application service for changes in event modes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = the id of the event that changed.</param>
		protected void AppServiceCustomEventHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			if (sender is not CustomEventAppService customAppService) return;
			bool state = customAppService.QueryCustomEventState(e.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is not ICustomEventUserInterface eventUi) continue;
				eventUi.UpdateCustomEvent(e.Arg, state);
			}
		}

		/// <summary>
		/// Handle notifications from the application service about an audio DSP changing connection status.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg = the id of the DSP that changed.</param>
		protected void AppServiceDspConnectionHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var isOnline = AppService.QueryAudioDspConnectionStatus(args.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioDeviceConnectionStatus(args.Arg, isOnline);
				}
			}
			
			if (isOnline)
			{
				Fusion?.ClearOfflineDevice(args.Arg);
			}
			else
			{
				var found = AppService.GetAllAudioDspDevices().First(x => x.Id.Equals(args.Arg, StringComparison.InvariantCulture));
				Fusion?.AddOfflineDevice(args.Arg, found.Label );
			}
		}

		/// <summary>
		/// Handle notifications from the application service about volume level changes on inputs.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg = the id of the audio channel that changed.</param>
		protected void AppServiceAudioInputLevelHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var level = AppService.QueryAudioInputLevel(args.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioInputLevel(args.Arg, level);
				}
			}
		}

		/// <summary>
		/// Handle notifications from the application service about mute state changes on inputs.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg = the id of the audio channel that changed.</param>
		protected void AppServiceAudioInputMuteHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var newState = AppService.QueryAudioInputMute(args.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioInputMute(args.Arg, newState);
				}
			}

			Fusion?.UpdateMicMute(args.Arg, newState);
		}

		/// <summary>
		/// Handle notifications from the application service about volume level changes on outputs.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg = the id of the audio channel that changed.</param>
		protected void AppServiceAudioOutputLevelHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var level = AppService.QueryAudioOutputLevel(args.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioOutputLevel(args.Arg, level);
				}
			}

			UpdateFusionAudioFeedback();
		}

		/// <summary>
		/// Handle notifications from the application service about mute state changes on outputs.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg = the id of the audio channel that changed.</param>
		protected void AppServiceAudioOutputMuteHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var newState = AppService.QueryAudioOutputMute(args.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioOutputMute(args.Arg, newState);
				}
			}

			UpdateFusionAudioFeedback();
		}

		/// <summary>
		/// Handle notifications from the application service about audio route changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg = the id of the audio output channel that changed.</param>
		protected void AppServiceAudioOutputRouteHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var sourceId = AppService.QueryAudioOutputRoute(args.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioOutputRoute(sourceId, args.Arg);
				}
			}
		}

		/// <summary>
		/// Handle notifications from the application service about zone enable/disable changes for microphones.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = the id of the audio channel being updated, Arg2 = the id of the zone that changed.</param>
		protected void AppServiceAudioZoneEnableHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			var newState = AppService.QueryAudioZoneState(args.Arg1, args.Arg2);
			foreach (var ui in UiConnections)
			{
				var audioUi = (ui as IAudioUserInterface);
				audioUi?.UpdateAudioZoneState(args.Arg1, args.Arg2, newState);
			}
		}

		/// <summary>
		/// Handle notifications from the application service about video display/projector connection status changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = the id of the display that changed. Arg2 = true is online, false is offline.</param>
		protected void AppServiceDisplayConnectionHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceDisplayConnectionHandler() - {0}, {1}", args.Arg1, args.Arg2);
			var display = AppService.GetAllDisplayInfo().FirstOrDefault(x => x.Id.Equals(args.Arg1, StringComparison.InvariantCulture));
			if (display == null) return;

			foreach (var ui in UiConnections)
			{
				if (ui is IDisplayUserInterface displayUi)
				{
					displayUi.UpdateDisplayConnectionStatus(args.Arg1, args.Arg2);
				}
			}
			
			if (args.Arg2)
			{
				Fusion?.ClearOfflineDevice(args.Arg1);
			}
			else
			{
				Fusion?.AddOfflineDevice(args.Arg1, display.Label);
			}
		}

		/// <summary>
		/// Handle notifications from the application service about video display/projector video blank status changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = the id of the display that changed. Arg2 = true is blanked, false is showing video.</param>
		protected void AppServiceDisplayBlankHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceDisplayBlankHandler() - {0}, {1}", args.Arg1, args.Arg2);
			foreach (var ui in UiConnections)
			{
				if (ui is IDisplayUserInterface displayUi)
				{
					displayUi.UpdateDisplayBlank(args.Arg1, args.Arg2);
				}
			}
		}

		/// <summary>
		/// Handle notifications from the application service about video display/projector video freeze status changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = the id of the display that changed. Arg2 = true is frozen, false is not frozen.</param>
		protected void AppServiceDisplayFreezeHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceDisplayFreezeHandler() - {0}, {1}", args.Arg1, args.Arg2);
			foreach (var ui in UiConnections)
			{
				if (ui is IDisplayUserInterface displayUi)
				{
					displayUi.UpdateDisplayFreeze(args.Arg1, args.Arg2);
				}
			}
		}

		/// <summary>
		/// Handle notifications from the application service about video display/projector power status changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = the id of the display that changed. Arg2 = true is on, false is off.</param>
		protected void AppServiceDisplayPowerHandler(object? sender, GenericDualEventArgs<string, bool> e)
		{
			Logger.Debug("PresentationService.AppServiceDisplayPowerHandler() - {0}, {1}", e.Arg1, e.Arg2);
			foreach (var ui in UiConnections)
			{
				if (ui is IDisplayUserInterface displayUi)
				{
					displayUi.UpdateDisplayPower(e.Arg1, e.Arg2);
				}
			}

			UpdateFusionDisplayPowerFeedback();
			if (e.Arg2)
			{
				Fusion?.StartDisplayUse(e.Arg1);
			}
			else
			{
				Fusion?.StopDisplayUse(e.Arg1);
			}
		}

		/// <summary>
		/// Handle notifications from the application service about video display/projector input selection changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = the id of the display that changed.</param>
		protected void AppServiceDisplayInputChangedHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.AppServiceDisplayInputChangedHandler({0})", e.Arg);

			var isLectern = AppService.DisplayInputLecternQuery(e.Arg);
			var isStation = AppService.DisplayInputStationQuery(e.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is not IDisplayUserInterface displayUi) continue;

				if (isLectern)
				{
					displayUi.SetStationLecternInput(e.Arg);
				}
				else if (isStation)
				{
					displayUi.SetStationLocalInput(e.Arg);
				}
			}
		}

		/// <summary>
		/// Handle notifications from the application service about relay or control expander endpoint connection status changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = the id of the endpoint device that changed. Arg2 = true is online, false is offline.</param>
		protected void AppServiceEndpointConnectionHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceEndpointConnectionHandler() - {0}, {1}", args.Arg1, args.Arg2);
			
			if (args.Arg2)
			{
				Fusion?.ClearOfflineDevice(args.Arg1);
				RemoveErrorFromUi(args.Arg1);
			}
			else
			{
				var label = $"Endpoint {args.Arg1}";
				Fusion?.AddOfflineDevice(args.Arg1, label);
				AddErrorToUi(args.Arg1, label);
			}
		}

		/// <summary>
		/// Handle notifications from the application service about endpoint state changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = the id of the endpoint that changed. Arg2 = the index on the endpoint device that changed.</param>
		protected void AppServiceEndpointChangedHandler(object? sender, GenericDualEventArgs<string, int> args)
		{
			Logger.Debug("PresentationService.AppServiceEndpointChangedHandler() - {0}", args.Arg1, args.Arg2);
		}

		/// <summary>
		/// Handle notifications from the application service about video routing events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg = the id of the destination that changed.</param>
		protected void AppServiceRouteHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var currentSrc = AppService.QueryCurrentRoute(args.Arg);
			foreach (var conn in UiConnections)
			{
				if (conn is IRoutingUserInterface routingUi)
				{
					routingUi.UpdateAvRoute(currentSrc, args.Arg);
				}
			}

			UpdateFusionRoutingFeedback();
		}

		/// <summary>
		/// Handle notifications from the application service about AVR connection status changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg = the id of the AVR that updated.</param>
		protected void AppServiceRouterConnectionHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			Logger.Debug("PresentationService.AppServiceRouterConnectionHandler() - {0}", args.Arg);
			
			var isOnline = AppService.QueryRouterConnectionStatus(args.Arg);
			foreach (var ui in UiConnections)
			{
				if (ui is IRoutingUserInterface routingUi)
				{
					routingUi.UpdateAvRouterConnectionStatus(args.Arg, isOnline);
				}
			}
			
			if (isOnline)
			{
				Fusion?.ClearOfflineDevice(args.Arg);
			}
			else
			{
				var label = $"Router {args.Arg} is offline";
				Fusion?.AddOfflineDevice(args.Arg, label);
			}
		}

		/// <summary>
		/// Handle notifications from the application service about lighting controller connection status changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = the id of the lighting controller that updated.</param>
		protected void AppServiceLightingConnectionHandler(object? sender, GenericDualEventArgs<string, bool> e)
		{
			Logger.Debug("PresentationService.AppServiceLightingConnectionHandler({0}, 1)", e.Arg1, e.Arg2);
			foreach (var ui in UiConnections)
			{
				if (ui is ILightingUserInterface lightingUi)
				{
					lightingUi.UpdateLightingControlConnectionStatus(e.Arg1, e.Arg2);
				}
			}
			
			if (e.Arg2)
			{
				Fusion?.ClearOfflineDevice(e.Arg1);
			}
			else
			{
				var label = $"Lighting controller {e.Arg1}";
				Fusion?.AddOfflineDevice(e.Arg1, label);
			}
		}

		/// <summary>
		/// Handle notifications from the application service about global/AVR video freeze changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = the id of the lighting controller that updated.</param>
		protected void AppServiceGlobalFreezeHandler(object? sender, EventArgs e)
		{
			bool freezeState = AppService.QueryGlobalVideoFreeze();
			foreach (var ui in UiConnections)
			{
				ui.SetGlobalFreezeState(freezeState);
			}

			Fusion?.UpdateDisplayFreeze(freezeState);
		}

		/// <summary>
		/// Handle notifications from the application service about global/AVR video blank events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Generic empty event args.</param>
		protected void AppServiceGlobalBlankHandler(object? sender, EventArgs e)
		{
			bool blankState = AppService.QueryGlobalVideoBlank();
			foreach (var ui in UiConnections)
			{
				ui.SetGlobalBlankState(blankState);
			}

			Fusion?.UpdateDisplayBlank(blankState);
		}

		/// <summary>
		/// Handle notifications from the application service about global/AVR video blank events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Generic empty event args.</param>
		protected void AppServiceStateChangeHandler(object? sender, EventArgs args)
		{
			bool state = AppService.CurrentSystemState;
			foreach (var conn in UiConnections)
			{
				conn.SetSystemState(state);
				conn.ShowSystemStateChanging(state);
			}

			TriggerStateChangeTimer();
			Fusion?.UpdateSystemState(state);

			// If system is powering off then stop recording usage, or start recording usage for the currently
			// selected input when powered on.
			if (state)
			{
				UpdateFusionRoutingFeedback();
			}
			else
			{
				foreach (var source in AppService.GetAllAvSources())
				{
					Fusion?.StopDeviceUse(source.Id);
				}
			}
		}

		#endregion

		#region Touchscreen Handlers

		/// <summary>
		/// Handle user interface triggered camera preset recall events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = camera id, Arg2 = preset id</param>
		protected void CameraPresetRecallHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			if (AppService is not ICameraControlApp cameraApp) return;
			cameraApp.SendCameraPresetRecall(args.Arg1, args.Arg2);
		}

		/// <summary>
		/// Handle user interface triggered camera preset save events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = camera id, Arg2 = preset id</param>
		protected void CameraPresetSaveHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			if (AppService is not ICameraControlApp cameraApp) return;
			cameraApp.SendCameraPresetSave(args.Arg1, args.Arg2);
		}
		
		/// <summary>
		/// Handle user interface triggered camera pan/tilt events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = camera id, Arg2 = the direction to move the camera</param>
		protected void CameraUiPanTiltHandler(object? sender, GenericDualEventArgs<string, Vector2D> args)
		{
			if (AppService is not ICameraControlApp cameraApp) return;
			cameraApp.SendCameraPanTilt(args.Arg1, args.Arg2);
		}

		/// <summary>
		/// Handle user interface triggered camera zoom events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = camera id, Arg2 = negative values for wide, positive for telephoto, 0 to stop.</param>
		protected void CameraUiZoomHandler(object? sender, GenericDualEventArgs<string, int> args)
		{
			if (AppService is not ICameraControlApp cameraApp) return;
			cameraApp.SendCameraZoom(args.Arg1, args.Arg2);
		}

		/// <summary>
		/// Handle user interface triggered camera power change events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = camera id, Arg2 = true is on, false is off </param>
		protected void CameraUiPowerHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			if (AppService is not ICameraControlApp cameraApp) return;
			cameraApp.SendCameraPowerChange(args.Arg1, args.Arg2);
		}
		
		/// <summary>
		/// Handle user interface triggered layout selection events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = controller id, Arg2 = layout id</param>
		protected void VideoWallUiLayoutHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			if (AppService is not IVideoWallApp videoWallApp) return;
			videoWallApp.SetActiveVideoWallLayout(args.Arg1, args.Arg2);
		}

		/// <summary>
		/// Handle user interface triggered video wall cell routing events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = control id, Arg2 = cell id, Arg3 = source id</param>
		protected void VideoWallRouteHandler(object? sender, GenericTrippleEventArgs<string, string, string> args)
		{
			Logger.Debug($"PresentationService.VideoWallRouteHandler(${args.Arg1}, {args.Arg2}, {args.Arg3})");
			
			if (AppService is not IVideoWallApp videoWallApp) return;
			videoWallApp.SetVideoWallCellRoute(args.Arg1, args.Arg2, args.Arg3);
		}
		
		/// <summary>
		/// Handle user interface connection changes.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = The id of the user interface that changed.</param>
		protected void UiConnectionHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var found = UiConnections.FirstOrDefault(x => x.Id.Equals(args.Arg, StringComparison.InvariantCulture));
			if (found == null) return;
			
			if (found is { IsOnline: false, IsXpanel: false })
			{
				Fusion?.AddOfflineDevice(found.Id, $"UI {found.Id}");
			}
			else
			{
				Fusion?.ClearOfflineDevice(found.Id);
				found.SetSystemState(AppService.CurrentSystemState);
				found.SetGlobalBlankState(AppService.QueryGlobalVideoBlank());
				found.SetGlobalFreezeState(AppService.QueryGlobalVideoFreeze());

				if (found is IAudioUserInterface audioUi)
				{
					foreach (var inChan in AppService.GetAudioInputChannels())
					{
						audioUi.UpdateAudioInputLevel(inChan.Id, AppService.QueryAudioInputLevel(inChan.Id));
						audioUi.UpdateAudioInputMute(inChan.Id, AppService.QueryAudioInputMute(inChan.Id));
					}

					foreach (var outChan in AppService.GetAudioOutputChannels())
					{
						audioUi.UpdateAudioOutputLevel(outChan.Id, AppService.QueryAudioOutputLevel(outChan.Id));
						audioUi.UpdateAudioOutputMute(outChan.Id, AppService.QueryAudioOutputMute(outChan.Id));
					}
				}

				if (found is ITransportControlUserInterface transportUi)
				{
					transportUi.SetCableBoxData(AppService.GetAllCableBoxes());
				}

				if (found is ICustomEventUserInterface customEventUi)
				{
					UpdateCustomEventUi(customEventUi);
				}

				if (found is ILightingUserInterface lightingUi)
				{
					lightingUi.SetLightingData(AppService.GetAllLightingDeviceInfo());
				}
			}
		}

		/// <summary>
		/// update a user interface with the current state of all event modes, if the ui supports event control.
		/// </summary>
		/// <param name="ui">the user interface to update.</param>
		protected void UpdateCustomEventUi(ICustomEventUserInterface ui)
		{
			if (AppService is not CustomEventAppService eventServiceApp) return;
			var events = eventServiceApp.QueryAllCustomEvents();
			foreach (var evt in events)
			{
				ui.UpdateCustomEvent(evt.Id, eventServiceApp.QueryCustomEventState(evt.Id));
			}
		}

		/// <summary>
		/// Handle user-interface triggers for changing the use state of the system.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg = true to set state active, false to set state standby.</param>
		protected void UiStatusChangeHandler(object? sender, GenericSingleEventArgs<bool> args)
		{
			if (args.Arg)
			{
				AppService.SetActive();
			}
			else
			{
				AppService.SetStandby();
			}
		}

		/// <summary>
		/// Handle user-interface triggers for video routing.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Arg1 = input id, Arg2 = output id.</param>
		protected void UiRouteChangeHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			Logger.Debug("PresentationService.UiRouteChangeHandler() - {0} -> {1}", args.Arg1, args.Arg2);
			
			if (args.Arg2.Equals("ALL", StringComparison.InvariantCulture))
			{
				AppService.RouteToAll(args.Arg1);
			}
			else
			{
				AppService.MakeRoute(args.Arg1, args.Arg2);
			}
		}

		/// <summary>
		/// Handle user-interface triggers for changing display/projector power state.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = id of the display to change, Arg2 = true for on, false for off.</param>
		protected void UiDisplayPowerHandler(object? sender, GenericDualEventArgs<string, bool> e)
		{
			Logger.Debug("PresentationService.UiDisplayPowerHandler() - {0} - {1}", e.Arg1, e.Arg2);
			AppService.SetDisplayPower(e.Arg1, e.Arg2);
		}

		/// <summary>
		/// Handle user-interface triggers for changing display freeze state.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = id of the display to change, Arg2 = true for freeze, false for disable freeze.</param>
		protected void UiDisplayFreezeHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			bool currenState = AppService.DisplayFreezeQuery(e.Arg);
			AppService.SetDisplayFreeze(e.Arg, !currenState);
		}

		/// <summary>
		/// Handle user-interface triggers for changing display video blank state.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = id of the display to change, Arg2 = true for blank, false for show video.</param>
		protected void UiDisplayBlankHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			bool currentState = AppService.DisplayBlankQuery(e.Arg);
			AppService.SetDisplayBlank(e.Arg, !currentState);
		}

		/// <summary>
		/// Handle user-interface triggers for changing global/AVR video blank.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = id of the display to change, Arg2 = true for blank, false for show video.</param>
		protected void UiGlobalBlankHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.UiGlobalBlankHandler()");
			bool currentState = AppService.QueryGlobalVideoBlank();
			AppService.SetGlobalVideoBlank(!currentState);
		}

		/// <summary>
		/// Handle user-interface triggers for changing global/AVR freeze state.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = id of the display to change, Arg2 = true for freeze, false for disable freeze.</param>
		protected void UiGlobalFreezeHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.UiGlobalFreezeHandler()");
			bool currentState = AppService.QueryGlobalVideoFreeze();
			AppService.SetGlobalVideoFreeze(!currentState);
		}

		/// <summary>
		/// Handle user-interface triggers for raising a screen associated with a projector.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = id of the display associated with the screen to raise.</param>
		protected void UiDisplayScreenUpHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			AppService.RaiseScreen(e.Arg);
		}

		/// <summary>
		/// Handle user-interface triggers for lowering a screen associated with a projector.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = id of the display associated with the screen to lower.</param>
		protected void UiDisplayScreenDownHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			AppService.LowerScreen(e.Arg);
		}

		/// <summary>
		/// Handle user-interface triggers for muting an audio output channel.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = id of the channel to mute.</param>
		protected void UiAudioOutputMuteRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputMuteRequest() - {0}", e.Arg);
			try
			{
				var current = AppService.QueryAudioOutputMute(e.Arg);
				AppService.SetAudioOutputMute(e.Arg, !current);
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "PresentationService.UiAudioOutputMuteRequest()");
			}
		}

		/// <summary>
		/// Handle user-interface triggers for increasing an audio output channel level.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = id of the channel to increase.</param>
		protected void UiAudioOutputLevelUpRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputLevelUpRequest() - {0}", e.Arg);
			var newLevel = AppService.QueryAudioOutputLevel(e.Arg) + 3;
			AppService.SetAudioOutputLevel(e.Arg, newLevel < 100 ? newLevel : 100);
		}

		/// <summary>
		/// Handle user-interface triggers for decreasing an audio output channel level.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = id of the channel to decrease.</param>
		protected void UiAudioOutputLevelDownRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputLevelDownRequest() - {0}", e.Arg);
			var newLevel = AppService.QueryAudioOutputLevel(e.Arg) - 3;
			AppService.SetAudioOutputLevel(e.Arg, newLevel > 0 ? newLevel : 0);
		}

		/// <summary>
		/// Handle user-interface triggers for audio routing events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = id of the input channel, Arg2 = id of the output channel.</param>
		protected void UiAudioOutputRouteRequest(object? sender, GenericDualEventArgs<string, string> e)
		{
			AppService.SetAudioOutputRoute(e.Arg1, e.Arg2);
		}

		/// <summary>
		/// Handle user-interface triggers for input mute events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = id of the input to mute.</param>
		protected void UiAudioInputMuteRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputMuteRequest() - {0}", e.Arg);
			bool current = AppService.QueryAudioInputMute(e.Arg);
			AppService.SetAudioInputMute(e.Arg, !current);
		}

		/// <summary>
		/// Handle user-interface triggers for decreasing an audio input channel level.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = id of the channel to decrease.</param>
		protected void UiAudioInputLevelDownRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputLevelDownRequest() - {0}", e.Arg);
			int newLevel = AppService.QueryAudioInputLevel(e.Arg) - 3;
			AppService.SetAudioInputLevel(e.Arg, newLevel > 0 ? newLevel : 0);
		}

		/// <summary>
		/// Handle user-interface triggers for increasing an audio input channel level.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = id of the channel to increase.</param>
		protected void UiAudioInputLevelUpRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputLevelUpRequest() - {0}", e.Arg);
			int newLevel = AppService.QueryAudioInputLevel(e.Arg) + 3;
			AppService.SetAudioInputLevel(e.Arg, newLevel < 100 ? newLevel : 100);
		}

		/// <summary>
		/// Handle user-interface triggers for toggling the input channel mix for a given output.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = id of the input channel. Arg2 = id of the output zone to toggle</param>
		protected void UiAudioZoneToggleHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			AppService.ToggleAudioZoneState(e.Arg1, e.Arg2);
		}

		/// <summary>
		/// Handle user-interface triggers for setting a level value for an audio output.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = id of the channel to adjust, Arg2 = the 0-100 value to set.</param>
		protected void DiscreteAudioOutputUiHandler(object? sender, GenericDualEventArgs<string, int> e)
		{
			var adjustedLevel = e.Arg2;
			if (adjustedLevel < 0)
			{
				adjustedLevel = 0;
			}
			else if (adjustedLevel > 100)
			{
				adjustedLevel = 100;
			}

			AppService.SetAudioOutputLevel(e.Arg1, adjustedLevel);
		}

		/// <summary>
		/// Handle user-interface triggers for setting a level value for an audio input.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = id of the channel to adjust, Arg2 = the 0-100 value to set.</param>
		protected void DiscreteAudioInputUiHandler(object? sender, GenericDualEventArgs<string, int> e)
		{
			var adjustedLevel = e.Arg2;
			if (adjustedLevel < 0)
			{
				adjustedLevel = 0;
			}
			else if (adjustedLevel > 100)
			{
				adjustedLevel = 100;
			}

			AppService.SetAudioInputLevel(e.Arg1, adjustedLevel);
		}

		/// <summary>
		/// Handle user-interface triggers for setting a load level for a lighting zone.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = id of the controller to adjust, Arg2 = id of the lighting zone, Arg3 = the 0-100 value to set.</param>
		protected void LightingUiLoadHandler(object? sender, GenericTrippleEventArgs<string, string, int> e)
		{
			AppService.SetLightingLoad(e.Arg1, e.Arg2, e.Arg3);
		}

		/// <summary>
		/// Handle user-interface triggers for setting a lighting scene on a given controller.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = id of the device to adjust, Arg2 = id of the scene to set.</param>
		protected void LightingUiSceneHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			AppService.RecallLightingScene(e.Arg1, e.Arg2);
		}

		/// <summary>
		/// Update all user interfaces that support error reporting.
		/// </summary>
		/// <param name="id">The id of the error, used when referencing the specific error.</param>
		/// <param name="label">The information to display on the user interface for the error.</param>
		protected void AddErrorToUi(string id, string label)
		{
			foreach (var ui in UiConnections)
			{
				if (ui is IErrorInterface errorUi)
				{
					errorUi.AddDeviceError(id, label);
				}
			}
		}

		/// <summary>
		/// Remove an existing error from all user interfaces.
		/// </summary>
		/// <param name="id">The id of the error that was added with <see cref="AddErrorToUi"/>.</param>
		protected void RemoveErrorFromUi(string id)
		{
			foreach (var ui in UiConnections)
			{
				if (ui is IErrorInterface errorUi)
				{
					errorUi.ClearDeviceError(id);
				}
			}
		}

		/// <summary>
		/// Handle user-interface triggers for selecting the "station" input on a display.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = id of the display.</param>
		protected void UiSetStationLecternHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			AppService.SetInputLectern(e.Arg);
		}

		/// <summary>
		/// Handle user-interface triggers for selecting the "lectern" input on a display.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = id of the display.</param>
		protected void UiSetStationLocalHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			AppService.SetInputStation(e.Arg);
		}
		
		/// <summary>
		/// Handle user interface requests to set a channel on a transport-controlled device (cable TV, DVR).
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = the id of the device to control, Arg2 = the channel to dial.</param>
		protected void TransportUi_TransportDialRequest(object? sender, GenericDualEventArgs<string, string> e)
		{
			AppService.TransportDial(e.Arg1, e.Arg2);
		}

		/// <summary>
		/// Handle user interface requests to dial a saved favorite on a transport-controlled device (cable TV, DVR).
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = the id of the device to control, Arg2 = the id of the channel to dial.</param>
		protected void TransportUi_TransportDialFavoriteRequest(object? sender, GenericDualEventArgs<string, string> e)
		{
			AppService.TransportDialFavorite(e.Arg1, e.Arg2);
		}

		/// <summary>
		/// Handle user interface requests to send a transport/navigation to a device (cable TV, DVR).
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = the id of the device to control, Arg2 = transport control to trigger.</param>
		protected void TransportUi_TransportControlRequest(object? sender, GenericDualEventArgs<string, TransportTypes> e)
		{
			TransportUtilities.SendCommand(AppService, e.Arg1, e.Arg2);
		}

		/// <summary>
		/// Handle user interface requests to set a custom event state.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg1 = the id of the event, Arg2 = true to set active, false to set inactive.</param>
		protected void EventUi_CustomEventStateChanged(object? sender, GenericDualEventArgs<string, bool> e)
		{
			if (AppService is ICustomEventAppService eventApp)
			{
				eventApp.ChangeCustomEventState(e.Arg1, e.Arg2);
			}
		}
		#endregion

		#region Fusion Handlers
		/// <summary>
		/// Handle route requests from a <see cref="IFusionInterface"/> ui.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = the id of the source to route to all video destinations</param>
		protected void FusionRouteSourceHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.FusionRouteSourceHandler()");
			AppService.RouteToAll(e.Arg);
		}

		/// <summary>
		/// Handle route requests from a <see cref="IFusionInterface"/> ui.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = the id of the source to route to all video destinations</param>
		protected void FusionAudioLevelHandler(object? sender, GenericSingleEventArgs<uint> e)
		{
			Logger.Debug("PresentationService.FusionAudioLevelHandler()");
			var pgmOut = AppService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmOut != null)
			{
				AppService.SetAudioOutputLevel(pgmOut.Id, (int)e.Arg);
			}
		}

		/// <summary>
		/// Handle program audio mute state change requests from a <see cref="IFusionInterface"/> ui.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Empty args package.</param>
		protected void FusionAudioMuteHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionAudioMuteHandler()");
			var pgmOut = AppService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmOut != null)
			{
				AppService.SetAudioOutputMute(pgmOut.Id, !AppService.QueryAudioOutputMute(pgmOut.Id));
			}
		}

		/// <summary>
		/// Handle global video freeze change requests from a <see cref="IFusionInterface"/> ui.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Empty args package.</param>
		protected void FusionDisplayFreezeHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionDisplayFreezeHandler()");
			AppService.SetGlobalVideoFreeze(!AppService.QueryGlobalVideoFreeze());
		}

		/// <summary>
		/// Handle global video blank change requests from a <see cref="IFusionInterface"/> ui.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Empty args package.</param>
		protected void FusionDisplayBlankHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionDisplayBlankHandler()");
			AppService.SetGlobalVideoBlank(!AppService.QueryGlobalVideoFreeze());
		}

		/// <summary>
		/// Handle display power change requests from a <see cref="IFusionInterface"/> ui.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = true for on, false for off.</param>
		protected void FusionDisplayPowerHandler(object? sender, GenericSingleEventArgs<bool> e)
		{
			Logger.Debug("PresentationService.FusionDisplayPowerHandler()");
			foreach (var display in AppService.GetAllDisplayInfo())
			{
				AppService.SetDisplayPower(display.Id, e.Arg);
			}
		}

		/// <summary>
		/// Handle system use state change requests from a <see cref="IFusionInterface"/> ui.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = true for set active, false for set standby.</param>
		protected void FusionPowerHandler(object? sender, GenericSingleEventArgs<bool> e)
		{
			Logger.Debug("PresentationService.FusionPowerHandler()");
			if (e.Arg)
			{
				AppService.SetActive();
			}
			else
			{
				AppService.SetStandby();
			}
		}

		/// <summary>
		/// Handle mic mute change requests from a <see cref="IFusionInterface"/> ui.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Arg = the id of the microphone to toggle.</param>
		protected void FusionMicMuteHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.FusionMicMuteHandler()");
			bool currentState = AppService.QueryAudioInputMute(e.Arg);
			AppService.SetAudioInputMute(e.Arg, !currentState);
		}

		/// <summary>
		/// Handle <see cref="IFusionInterface"/> device online/offline state change events.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Empy args package.</param>
		protected void FusionConnectionHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionConnectionHandler()");
			UpdateFusionFeedback();
		}

		/// <summary>
		/// update all <see cref="IFusionInterface"/> devices with the current system use state.
		/// </summary>
		protected void UpdateFusionDisplayPowerFeedback()
		{
			bool aDisplayOn = false;
			foreach (var display in AppService.GetAllDisplayInfo())
			{
				bool power = AppService.DisplayPowerQuery(display.Id);

				if (power)
				{
					aDisplayOn = power;
					break;
				}
			}

			Fusion?.UpdateDisplayPower(aDisplayOn);
		}

		/// <summary>
		/// Update all <see cref="IFusionInterface"/> devices with the current state of program audio level and mute.
		/// </summary>
		protected void UpdateFusionAudioFeedback()
		{
			var pgmAudio = AppService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmAudio == null) return;
			Fusion?.UpdateProgramAudioLevel((uint)AppService.QueryAudioOutputLevel(pgmAudio.Id));
			Fusion?.UpdateProgramAudioMute(AppService.QueryAudioOutputMute(pgmAudio.Id));
		}

		/// <summary>
		/// Update all <see cref="IFusionInterface"/> devices with the current video route status.
		/// </summary>
		protected void UpdateFusionRoutingFeedback()
		{
			var avDestinations = AppService.GetAllAvDestinations();
			if (avDestinations.Count > 0)
			{
				var currentRoute = AppService.QueryCurrentRoute(avDestinations[0].Id);
				Fusion?.UpdateSelectedSource(currentRoute.Id);
				foreach (var source in AppService.GetAllAvSources())
				{
					if (source.Id.Equals(currentRoute.Id, StringComparison.InvariantCulture))
					{
						Fusion?.StartDeviceUse(source.Id);
					}
					else
					{
						Fusion?.StopDeviceUse(source.Id);
					}
				}
			}
		}

		/// <summary>
		/// Update all <see cref="IFusionInterface"/> devices with all supported feedback.
		/// </summary>
		protected void UpdateFusionFeedback()
		{
			Fusion?.UpdateSystemState(AppService.CurrentSystemState);
			UpdateFusionDisplayPowerFeedback();
			UpdateFusionAudioFeedback();
			UpdateFusionRoutingFeedback();
			Fusion?.UpdateDisplayFreeze(AppService.QueryGlobalVideoFreeze());
			Fusion?.UpdateDisplayBlank(AppService.QueryGlobalVideoBlank());
		}
		#endregion
	}
}
