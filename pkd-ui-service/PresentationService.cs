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

namespace pkd_ui_service
{
	/// <summary>
	/// Root presentation implementation.
	/// </summary>
	public class PresentationService : IPresentationService, IDisposable
	{
		private readonly CrestronControlSystem _control;
		private readonly IApplicationService _appService;
		private readonly List<IUserInterface> _uiConnections;
		private IFusionInterface? _fusion;
		private CTimer? _stateChangeTimer;
		private bool _disposed;
#if DEBUG
		private const int TransitionTime = 3000;
#else
        private const int TransitionTime = 20000;
#endif
		
		/// <param name="appService">The framework application implementation that handles state management.</param>
		/// <param name="control">the root Crestron control system object.</param>
		public PresentationService(IApplicationService appService, CrestronControlSystem control)
		{
			ParameterValidator.ThrowIfNull(appService, "Ctor", nameof(appService));
			ParameterValidator.ThrowIfNull(control, "Ctor", nameof(control));

			_appService = appService;
			_control = control;
			_uiConnections = [];
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
			foreach (var uiDevice in _uiConnections)
			{
				uiDevice.Connect();
			}

			_fusion?.Initialize();
		}

		/// <summary>
		/// disposes of all internal component objects if they are disposable.
		/// </summary>
		protected void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				UnsubscribeFromAppService();
				UnsubscribeFromInterfaces();
				foreach (var uiConn in _uiConnections)
				{
					if (uiConn is IDisposable disposableUiConn)
					{
						disposableUiConn.Dispose();
					}
				}

				_stateChangeTimer?.Dispose();

				if (_fusion != null)
				{
					_fusion.OnlineStatusChanged -= FusionConnectionHandler;
					_fusion.MicMuteChangeRequested -= FusionMicMuteHandler;
					_fusion.SystemStateChangeRequested -= FusionPowerHandler;
					_fusion.DisplayPowerChangeRequested -= FusionDisplayPowerHandler;
					_fusion.DisplayBlankChangeRequested -= FusionDisplayBlankHandler;
					_fusion.DisplayFreezeChangeRequested -= FusionDisplayFreezeHandler;
					_fusion.AudioMuteChangeRequested -= FusionAudioMuteHandler;
					_fusion.ProgramAudioChangeRequested -= FusionAudioLevelHandler;
					_fusion.SourceSelectRequested -= FusionRouteSourceHandler;
					_fusion.Dispose();
				}
			}

			_disposed = true;
		}

		private void BuildInterfaces()
		{
			foreach (var device in _appService.GetAllUserInterfaces())
			{
				Logger.Info("PresentationService - Building interface {0} with IP-ID {1}", device.Id, device.IpId);
				
				var uiObj = PresentationServiceFactory.CreateUserInterface(_control, device, _appService);
				if (uiObj == null) continue;
				_uiConnections.Add(uiObj);
				if (!uiObj.IsInitialized)
				{
					uiObj.Initialize();
				}
				
				SubscribeToInterface(uiObj);
			}

			_fusion = PresentationServiceFactory.CreateFusionService(_appService, _control);
			_fusion.OnlineStatusChanged += FusionConnectionHandler;
			_fusion.MicMuteChangeRequested += FusionMicMuteHandler;
			_fusion.SystemStateChangeRequested += FusionPowerHandler;
			_fusion.DisplayPowerChangeRequested += FusionDisplayPowerHandler;
			_fusion.DisplayBlankChangeRequested += FusionDisplayBlankHandler;
			_fusion.DisplayFreezeChangeRequested += FusionDisplayFreezeHandler;
			_fusion.AudioMuteChangeRequested += FusionAudioMuteHandler;
			_fusion.ProgramAudioChangeRequested += FusionAudioLevelHandler;
			_fusion.SourceSelectRequested += FusionRouteSourceHandler;
		}

		private void SubscribeToAppService()
		{
			_appService.AudioDspConnectionStatusChanged += AppServiceDspConnectionHandler;
			_appService.AudioInputLevelChanged += AppServiceAudioInputLevelHandler;
			_appService.AudioInputMuteChanged += AppServiceAudioInputMuteHandler;
			_appService.AudioOutputLevelChanged += AppServiceAudioOutputLevelHandler;
			_appService.AudioOutputMuteChanged += AppServiceAudioOutputMuteHandler;
			_appService.AudioOutputRouteChanged += AppServiceAudioOutputRouteHandler;
			_appService.AudioZoneEnableChanged += AppServiceAudioZoneEnableHandler;
			_appService.DisplayBlankChange += AppServiceDisplayBlankHandler;
			_appService.DisplayFreezeChange += AppServiceDisplayFreezeHandler;
			_appService.DisplayConnectChange += AppServiceDisplayConnectionHandler;
			_appService.DisplayPowerChange += AppServiceDisplayPowerHandler;
			_appService.DisplayInputChanged += AppServiceDisplayInputChangedHandler;
			_appService.EndpointConnectionChanged += AppServiceEndpointConnectionHandler;
			_appService.EndpointRelayChanged += AppServiceEndpointChangedHandler;
			_appService.RouteChanged += AppServiceRouteHandler;
			_appService.RouterConnectChange += AppServiceRouterConnectionHandler;
			_appService.SystemStateChanged += AppServiceStateChangeHandler;
			_appService.GlobalVideoBlankChanged += AppServiceGlobalBlankHandler;
			_appService.GlobalVideoFreezeChanged += AppServiceGlobalFreezeHandler;
			_appService.LightingSceneChanged += AppServiceLightingSceneHandler;
			_appService.LightingLoadLevelChanged += AppServiceLightingLoadHandler;
			_appService.LightingControlConnectionChanged += AppServiceLightingConnectionHandler;

			if (_appService is CustomEventAppService customEventService)
			{
				customEventService.CustomEventStateChanged += AppServiceCustomEventHandler;
			}

			if (_appService is ITechAuthGroupAppService techService)
			{
				techService.NonTechLockoutStateChangeRequest += AppServiceTechLockoutHandler;
			}

			if (_appService is IVideoWallApp videoWallApp)
			{
				videoWallApp.VideoWallLayoutChanged += VideoWallAppLayoutChangedHandler;
				videoWallApp.VideoWallConnectionStatusChanged += VideoWallAppConnectionChangeHandler;
				videoWallApp.VideoWallCellRouteChanged += VideoWallAppRouteHandler;
			}

			if (_appService is ICameraControlApp cameraApp)
			{
				cameraApp.CameraControlConnectionChanged += CameraAppConnectionChangeHandler;
				cameraApp.CameraPowerStateChanged += CameraAppPowerChangeHandler;
			}
		}

		private void UnsubscribeFromAppService()
		{
			_appService.AudioDspConnectionStatusChanged -= AppServiceDspConnectionHandler;
			_appService.AudioInputLevelChanged -= AppServiceAudioInputLevelHandler;
			_appService.AudioInputMuteChanged -= AppServiceAudioInputMuteHandler;
			_appService.AudioOutputLevelChanged -= AppServiceAudioOutputLevelHandler;
			_appService.AudioOutputMuteChanged -= AppServiceAudioOutputMuteHandler;
			_appService.AudioOutputRouteChanged -= AppServiceAudioOutputRouteHandler;
			_appService.AudioZoneEnableChanged -= AppServiceAudioZoneEnableHandler;
			_appService.DisplayBlankChange -= AppServiceDisplayBlankHandler;
			_appService.DisplayFreezeChange -= AppServiceDisplayFreezeHandler;
			_appService.DisplayConnectChange -= AppServiceDisplayConnectionHandler;
			_appService.EndpointConnectionChanged -= AppServiceEndpointConnectionHandler;
			_appService.EndpointRelayChanged -= AppServiceEndpointChangedHandler;
			_appService.RouteChanged -= AppServiceRouteHandler;
			_appService.RouterConnectChange -= AppServiceRouterConnectionHandler;
			_appService.SystemStateChanged -= AppServiceStateChangeHandler;
			_appService.LightingSceneChanged -= AppServiceLightingSceneHandler;
			_appService.LightingLoadLevelChanged -= AppServiceLightingLoadHandler;
			_appService.LightingControlConnectionChanged -= AppServiceLightingConnectionHandler;

			if (_appService is CustomEventAppService customEventService)
			{
				customEventService.CustomEventStateChanged -= AppServiceCustomEventHandler;
			}

			if (_appService is ITechAuthGroupAppService securityService)
			{
				securityService.NonTechLockoutStateChangeRequest -= AppServiceTechLockoutHandler;
			}

			if (_appService is IVideoWallApp videoWallApp)
			{
				videoWallApp.VideoWallLayoutChanged -= VideoWallAppLayoutChangedHandler;
				videoWallApp.VideoWallConnectionStatusChanged -= VideoWallAppConnectionChangeHandler;
				videoWallApp.VideoWallCellRouteChanged -= VideoWallAppRouteHandler;
			}
			
			if (_appService is ICameraControlApp cameraApp)
			{
				cameraApp.CameraControlConnectionChanged -= CameraAppConnectionChangeHandler;
				cameraApp.CameraPowerStateChanged -= CameraAppPowerChangeHandler;
			}
		}

		private void SubscribeToInterface(IUserInterface ui)
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
				cameraUi.CameraPowerChangeRequest += CameraUiPowerHandler;
			}
		}

		private void UnsubscribeFromInterfaces()
		{
			foreach (var ui in _uiConnections)
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
				}
			}
		}

		private void StateChangeTimerCallback(object? obj)
		{
			foreach (var conn in _uiConnections)
			{
				conn.HideSystemStateChanging();
			}
		}

		private void TriggerStateChangeTimer()
		{
			if (_stateChangeTimer != null)
			{
				_stateChangeTimer.Reset(TransitionTime);
			}
			else
			{
				_stateChangeTimer = new CTimer(StateChangeTimerCallback, TransitionTime);
			}
		}

		#region AppService Handlers

		private void CameraAppConnectionChangeHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			if (sender is not ICameraControlApp cameraApp) return;
			var newState = cameraApp.QueryCameraConnectionStatus(args.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is not ICameraUserInterface cameraUi) continue;
				cameraUi.SetCameraConnectionStatus(args.Arg, newState);
			}
			
			
			if (newState)
			{
				_fusion?.ClearOfflineDevice(args.Arg);
			}
			else
			{
				var found = cameraApp.GetAllCameraDeviceInfo().First(x => x.Id.Equals(args.Arg));
				_fusion?.AddOfflineDevice(args.Arg, found.Label );
			}
		}

		private void CameraAppPowerChangeHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			if (sender is not ICameraControlApp cameraApp) return;
			var newState = cameraApp.QueryCameraPowerStatus(args.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is not ICameraUserInterface cameraUi) continue;
				cameraUi.SetCameraPowerState(args.Arg, newState);
			}
		}
		
		private void VideoWallAppLayoutChangedHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			if (sender is not IVideoWallApp videoWallApp) return;
			var activeLayoutId = videoWallApp.QueryActiveVideoWallLayout(args.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is not IVideoWallUserInterface videoWallUi) continue;
				videoWallUi.UpdateActiveVideoWallLayout(args.Arg, activeLayoutId);
			}
		}

		private void VideoWallAppConnectionChangeHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			if (sender is not IVideoWallApp videoWallApp) return;
			var onlineStatus = videoWallApp.QueryVideoWallConnectionStatus(args.Arg);
			if (onlineStatus)
				_fusion?.ClearOfflineDevice(args.Arg);
			else
				_fusion?.AddOfflineDevice(args.Arg, $"Video Wall Controller {args.Arg}");
			
			foreach (var ui in _uiConnections)
			{
				if (ui is not IVideoWallUserInterface videoWallUi) continue;
				videoWallUi.UpdateVideoWallConnectionStatus(args.Arg, onlineStatus);
			}
		}

		private void VideoWallAppRouteHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			if (sender is not IVideoWallApp videoWallApp) return;
			var newRoute = videoWallApp.QueryVideoWallCellSource(args.Arg1, args.Arg2);
			foreach (var ui in _uiConnections)
			{
				if (ui is not IVideoWallUserInterface videoWallUi) continue;
				videoWallUi.UpdateCellRoutedSource(args.Arg1, args.Arg2, newRoute);
			}
		}
		
		private void AppServiceTechLockoutHandler(object? sender, GenericSingleEventArgs<bool> e)
		{
			foreach (var ui in _uiConnections)
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

		private void AppServiceLightingLoadHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			Logger.Debug("PresentationService.AppServiceLightingLoadHandler({0}, {1})", e.Arg1, e.Arg2);
			if (sender is not ILightingControlApp lightingService) return;

			var loadLevel = lightingService.GetZoneLoad(e.Arg1, e.Arg2);
			foreach (var ui in _uiConnections)
			{
				if (ui is ILightingUserInterface lightingUi)
				{
					lightingUi.UpdateLightingZoneLoad(e.Arg1, e.Arg2, loadLevel);
				}
			}
		}

		private void AppServiceLightingSceneHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			if (sender is not ILightingControlApp lightingService) return;
			var scene = lightingService.GetActiveScene(e.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is ILightingUserInterface lightingUi)
				{
					lightingUi.UpdateActiveLightingScene(e.Arg, scene);
				}
			}
		}

		private void AppServiceCustomEventHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			if (sender is not CustomEventAppService customAppService) return;
			bool state = customAppService.QueryCustomEventState(e.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is not ICustomEventUserInterface eventUi) continue;
				eventUi.UpdateCustomEvent(e.Arg, state);
			}
		}

		private void AppServiceDspConnectionHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var isOnline = _appService.QueryAudioDspConnectionStatus(args.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioDeviceConnectionStatus(args.Arg, isOnline);
				}
			}
			
			if (isOnline)
			{
				_fusion?.ClearOfflineDevice(args.Arg);
			}
			else
			{
				var found = _appService.GetAllAudioDspDevices().First(x => x.Id.Equals(args.Arg, StringComparison.InvariantCulture));
				_fusion?.AddOfflineDevice(args.Arg, found.Label );
			}
		}

		private void AppServiceAudioInputLevelHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			int level = _appService.QueryAudioInputLevel(args.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioInputLevel(args.Arg, level);
				}
			}
		}

		private void AppServiceAudioInputMuteHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			bool newState = _appService.QueryAudioInputMute(args.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioInputMute(args.Arg, newState);
				}
			}

			_fusion?.UpdateMicMute(args.Arg, newState);
		}

		private void AppServiceAudioOutputLevelHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			int level = _appService.QueryAudioOutputLevel(args.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioOutputLevel(args.Arg, level);
				}
			}

			UpdateFusionAudioFeedback();
		}

		private void AppServiceAudioOutputMuteHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			bool newState = _appService.QueryAudioOutputMute(args.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioOutputMute(args.Arg, newState);
				}
			}

			UpdateFusionAudioFeedback();
		}

		private void AppServiceAudioOutputRouteHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			string sourceId = _appService.QueryAudioOutputRoute(args.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioOutputRoute(sourceId, args.Arg);
				}
			}
		}

		private void AppServiceAudioZoneEnableHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			bool newState = _appService.QueryAudioZoneState(args.Arg1, args.Arg2);
			foreach (var ui in _uiConnections)
			{
				var audioUi = (ui as IAudioUserInterface);
				audioUi?.UpdateAudioZoneState(args.Arg1, args.Arg2, newState);
			}
		}

		private void AppServiceDisplayConnectionHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceDisplayConnectionHandler() - {0}, {1}", args.Arg1, args.Arg2);
			var display = _appService.GetAllDisplayInfo().FirstOrDefault(x => x.Id.Equals(args.Arg1, StringComparison.InvariantCulture));
			if (display == null) return;

			foreach (var ui in _uiConnections)
			{
				if (ui is IDisplayUserInterface displayUi)
				{
					displayUi.UpdateDisplayConnectionStatus(args.Arg1, args.Arg2);
				}
			}
			
			if (args.Arg2)
			{
				_fusion?.ClearOfflineDevice(args.Arg1);
			}
			else
			{
				_fusion?.AddOfflineDevice(args.Arg1, display.Label);
			}
		}

		private void AppServiceDisplayBlankHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceDisplayBlankHandler() - {0}, {1}", args.Arg1, args.Arg2);
			foreach (var ui in _uiConnections)
			{
				if (ui is IDisplayUserInterface displayUi)
				{
					displayUi.UpdateDisplayBlank(args.Arg1, args.Arg2);
				}
			}
		}

		private void AppServiceDisplayFreezeHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceDisplayFreezeHandler() - {0}, {1}", args.Arg1, args.Arg2);
			foreach (var ui in _uiConnections)
			{
				if (ui is IDisplayUserInterface displayUi)
				{
					displayUi.UpdateDisplayFreeze(args.Arg1, args.Arg2);
				}
			}
		}

		private void AppServiceDisplayPowerHandler(object? sender, GenericDualEventArgs<string, bool> e)
		{
			Logger.Debug("PresentationService.AppServiceDisplayPowerHandler() - {0}, {1}", e.Arg1, e.Arg2);
			foreach (var ui in _uiConnections)
			{
				if (ui is IDisplayUserInterface displayUi)
				{
					displayUi.UpdateDisplayPower(e.Arg1, e.Arg2);
				}
			}

			UpdateFusionDisplayPowerFeedback();
			if (e.Arg2)
			{
				_fusion?.StartDisplayUse(e.Arg1);
			}
			else
			{
				_fusion?.StopDisplayUse(e.Arg1);
			}
		}

		private void AppServiceDisplayInputChangedHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.AppServiceDisplayInputChangedHandler({0})", e.Arg);

			var isLectern = _appService.DisplayInputLecternQuery(e.Arg);
			var isStation = _appService.DisplayInputStationQuery(e.Arg);
			foreach (var ui in _uiConnections)
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

		private void AppServiceEndpointConnectionHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceEndpointConnectionHandler() - {0}, {1}", args.Arg1, args.Arg2);
			
			if (args.Arg2)
			{
				_fusion?.ClearOfflineDevice(args.Arg1);
				RemoveErrorFromUi(args.Arg1);
			}
			else
			{
				var label = $"Endpoint {args.Arg1}";
				_fusion?.AddOfflineDevice(args.Arg1, label);
				AddErrorToUi(args.Arg1, label);
			}
		}

		private void AppServiceEndpointChangedHandler(object? sender, GenericDualEventArgs<string, int> args)
		{
			Logger.Debug("PresentationService.AppServiceEndpointChangedHandler() - {0}", args.Arg1, args.Arg2);
		}

		private void AppServiceRouteHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var currentSrc = _appService.QueryCurrentRoute(args.Arg);
			foreach (var conn in _uiConnections)
			{
				if (conn is IRoutingUserInterface routingUi)
				{
					routingUi.UpdateAvRoute(currentSrc, args.Arg);
				}
			}

			UpdateFusionRoutingFeedback();
		}

		private void AppServiceRouterConnectionHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			Logger.Debug("PresentationService.AppServiceRouterConnectionHandler() - {0}", args.Arg);
			
			var isOnline = _appService.QueryRouterConnectionStatus(args.Arg);
			foreach (var ui in _uiConnections)
			{
				if (ui is IRoutingUserInterface routingUi)
				{
					routingUi.UpdateAvRouterConnectionStatus(args.Arg, isOnline);
				}
			}
			
			if (isOnline)
			{
				_fusion?.ClearOfflineDevice(args.Arg);
			}
			else
			{
				var label = $"Router {args.Arg} is offline";
				_fusion?.AddOfflineDevice(args.Arg, label);
			}
		}

		private void AppServiceLightingConnectionHandler(object? sender, GenericDualEventArgs<string, bool> e)
		{
			Logger.Debug("PresentationService.AppServiceLightingConnectionHandler({0}, 1)", e.Arg1, e.Arg2);
			foreach (var ui in _uiConnections)
			{
				if (ui is ILightingUserInterface lightingUi)
				{
					lightingUi.UpdateLightingControlConnectionStatus(e.Arg1, e.Arg2);
				}
			}
			
			if (e.Arg2)
			{
				_fusion?.ClearOfflineDevice(e.Arg1);
			}
			else
			{
				var label = $"Lighting controller {e.Arg1}";
				_fusion?.AddOfflineDevice(e.Arg1, label);
			}
		}

		private void AppServiceGlobalFreezeHandler(object? sender, EventArgs e)
		{
			bool freezeState = _appService.QueryGlobalVideoFreeze();
			foreach (var ui in _uiConnections)
			{
				ui.SetGlobalFreezeState(freezeState);
			}

			_fusion?.UpdateDisplayFreeze(freezeState);
		}

		private void AppServiceGlobalBlankHandler(object? sender, EventArgs e)
		{
			bool blankState = _appService.QueryGlobalVideoBlank();
			foreach (var ui in _uiConnections)
			{
				ui.SetGlobalBlankState(blankState);
			}

			_fusion?.UpdateDisplayBlank(blankState);
		}

		private void AppServiceStateChangeHandler(object? sender, EventArgs args)
		{
			bool state = _appService.CurrentSystemState;
			foreach (var conn in _uiConnections)
			{
				conn.SetSystemState(state);
				conn.ShowSystemStateChanging(state);
			}

			TriggerStateChangeTimer();
			_fusion?.UpdateSystemState(state);

			// If system is powering off then stop recording usage, or start recording usage for the currently
			// selected input when powered on.
			if (state)
			{
				UpdateFusionRoutingFeedback();
			}
			else
			{
				foreach (var source in _appService.GetAllAvSources())
				{
					_fusion?.StopDeviceUse(source.Id);
				}
			}
		}

		#endregion

		#region Touchscreen Handlers

		private void CameraUiPanTiltHandler(object? sender, GenericDualEventArgs<string, Vector2D> args)
		{
			if (_appService is not ICameraControlApp cameraApp) return;
			cameraApp.SendCameraPanTilt(args.Arg1, args.Arg2);
		}

		private void CameraUiZoomHandler(object? sender, GenericDualEventArgs<string, int> args)
		{
			if (_appService is not ICameraControlApp cameraApp) return;
			cameraApp.SendCameraZoom(args.Arg1, args.Arg2);
		}

		private void CameraUiPowerHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			if (_appService is not ICameraControlApp cameraApp) return;
			cameraApp.SendCameraPowerChange(args.Arg1, args.Arg2);
		}
		
		private void VideoWallUiLayoutHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			if (_appService is not IVideoWallApp videoWallApp) return;
			videoWallApp.SetActiveVideoWallLayout(args.Arg1, args.Arg2);
		}

		private void VideoWallRouteHandler(object? sender, GenericTrippleEventArgs<string, string, string> args)
		{
			Logger.Debug($"PresentationService.VideoWallRouteHandler(${args.Arg1}, {args.Arg2}, {args.Arg3})");
			
			if (_appService is not IVideoWallApp videoWallApp) return;
			videoWallApp.SetVideoWallCellRoute(args.Arg1, args.Arg2, args.Arg3);
		}
		
		private void UiConnectionHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var found = _uiConnections.FirstOrDefault(x => x.Id.Equals(args.Arg, StringComparison.InvariantCulture));
			if (found == null) return;
			
			if (found is { IsOnline: false, IsXpanel: false })
			{
				_fusion?.AddOfflineDevice(found.Id, $"UI {found.Id}");
			}
			else
			{
				_fusion?.ClearOfflineDevice(found.Id);
				found.SetSystemState(_appService.CurrentSystemState);
				found.SetGlobalBlankState(_appService.QueryGlobalVideoBlank());
				found.SetGlobalFreezeState(_appService.QueryGlobalVideoFreeze());

				if (found is IAudioUserInterface audioUi)
				{
					foreach (var inChan in _appService.GetAudioInputChannels())
					{
						audioUi.UpdateAudioInputLevel(inChan.Id, _appService.QueryAudioInputLevel(inChan.Id));
						audioUi.UpdateAudioInputMute(inChan.Id, _appService.QueryAudioInputMute(inChan.Id));
					}

					foreach (var outChan in _appService.GetAudioOutputChannels())
					{
						audioUi.UpdateAudioOutputLevel(outChan.Id, _appService.QueryAudioOutputLevel(outChan.Id));
						audioUi.UpdateAudioOutputMute(outChan.Id, _appService.QueryAudioOutputMute(outChan.Id));
					}
				}

				if (found is ITransportControlUserInterface transportUi)
				{
					transportUi.SetCableBoxData(_appService.GetAllCableBoxes());
				}

				if (found is ICustomEventUserInterface customEventUi)
				{
					UpdateCustomEventUi(customEventUi);
				}

				if (found is ILightingUserInterface lightingUi)
				{
					lightingUi.SetLightingData(_appService.GetAllLightingDeviceInfo());
				}
			}
		}

		private void UpdateCustomEventUi(ICustomEventUserInterface ui)
		{
			if (_appService is not CustomEventAppService eventServiceApp) return;
			var events = eventServiceApp.QueryAllCustomEvents();
			foreach (var evt in events)
			{
				ui.UpdateCustomEvent(evt.Id, eventServiceApp.QueryCustomEventState(evt.Id));
			}
		}

		private void UiStatusChangeHandler(object? sender, GenericSingleEventArgs<bool> args)
		{
			if (args.Arg)
			{
				_appService.SetActive();
			}
			else
			{
				_appService.SetStandby();
			}
		}

		private void UiRouteChangeHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			Logger.Debug("PresentationService.UiRouteChangeHandler() - {0} -> {1}", args.Arg1, args.Arg2);
			
			if (args.Arg2.Equals("ALL", StringComparison.InvariantCulture))
			{
				_appService.RouteToAll(args.Arg1);
			}
			else
			{
				_appService.MakeRoute(args.Arg1, args.Arg2);
			}
		}

		private void UiDisplayPowerHandler(object? sender, GenericDualEventArgs<string, bool> e)
		{
			Logger.Debug("PresentationService.UiDisplayPowerHandler() - {0} - {1}", e.Arg1, e.Arg2);
			_appService.SetDisplayPower(e.Arg1, e.Arg2);
		}

		private void UiDisplayFreezeHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			bool currenState = _appService.DisplayFreezeQuery(e.Arg);
			_appService.SetDisplayFreeze(e.Arg, !currenState);
		}

		private void UiDisplayBlankHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			bool currentState = _appService.DisplayBlankQuery(e.Arg);
			_appService.SetDisplayBlank(e.Arg, !currentState);
		}

		private void UiGlobalBlankHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.UiGlobalBlankHandler()");
			bool currentState = _appService.QueryGlobalVideoBlank();
			_appService.SetGlobalVideoBlank(!currentState);
		}

		private void UiGlobalFreezeHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.UiGlobalFreezeHandler()");
			bool currentState = _appService.QueryGlobalVideoFreeze();
			_appService.SetGlobalVideoFreeze(!currentState);
		}

		private void UiDisplayScreenUpHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			_appService.RaiseScreen(e.Arg);
		}

		private void UiDisplayScreenDownHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			_appService.LowerScreen(e.Arg);
		}

		private void UiAudioOutputMuteRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputMuteRequest() - {0}", e.Arg);
			bool current = _appService.QueryAudioOutputMute(e.Arg);
			_appService.SetAudioOutputMute(e.Arg, !current);
		}

		private void UiAudioOutputLevelUpRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputLevelUpRequest() - {0}", e.Arg);
			var newLevel = _appService.QueryAudioOutputLevel(e.Arg) + 3;
			_appService.SetAudioOutputLevel(e.Arg, newLevel < 100 ? newLevel : 100);
		}

		private void UiAudioOutputLevelDownRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputLevelDownRequest() - {0}", e.Arg);
			var newLevel = _appService.QueryAudioOutputLevel(e.Arg) - 3;
			_appService.SetAudioOutputLevel(e.Arg, newLevel > 0 ? newLevel : 0);
		}

		private void UiAudioOutputRouteRequest(object? sender, GenericDualEventArgs<string, string> e)
		{
			_appService.SetAudioOutputRoute(e.Arg1, e.Arg2);
		}

		private void UiAudioInputMuteRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputMuteRequest() - {0}", e.Arg);
			bool current = _appService.QueryAudioInputMute(e.Arg);
			_appService.SetAudioInputMute(e.Arg, !current);
		}

		private void UiAudioInputLevelDownRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputLevelDownRequest() - {0}", e.Arg);
			int newLevel = _appService.QueryAudioInputLevel(e.Arg) - 3;
			_appService.SetAudioInputLevel(e.Arg, newLevel > 0 ? newLevel : 0);
		}

		private void UiAudioInputLevelUpRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputLevelUpRequest() - {0}", e.Arg);
			int newLevel = _appService.QueryAudioInputLevel(e.Arg) + 3;
			_appService.SetAudioInputLevel(e.Arg, newLevel < 100 ? newLevel : 100);
		}

		private void UiAudioZoneToggleHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			_appService.ToggleAudioZoneState(e.Arg1, e.Arg2);
		}

		private void DiscreteAudioOutputUiHandler(object? sender, GenericDualEventArgs<string, int> e)
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

			_appService.SetAudioOutputLevel(e.Arg1, adjustedLevel);
		}

		private void DiscreteAudioInputUiHandler(object? sender, GenericDualEventArgs<string, int> e)
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

			_appService.SetAudioInputLevel(e.Arg1, adjustedLevel);
		}

		private void LightingUiLoadHandler(object? sender, GenericTrippleEventArgs<string, string, int> e)
		{
			_appService.SetLightingLoad(e.Arg1, e.Arg2, e.Arg3);
		}

		private void LightingUiSceneHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			_appService.RecallLightingScene(e.Arg1, e.Arg2);
		}

		private void AddErrorToUi(string id, string label)
		{
			foreach (var ui in _uiConnections)
			{
				if (ui is IErrorInterface errorUi)
				{
					errorUi.AddDeviceError(id, label);
				}
			}
		}

		private void RemoveErrorFromUi(string id)
		{
			foreach (var ui in _uiConnections)
			{
				if (ui is IErrorInterface errorUi)
				{
					errorUi.ClearDeviceError(id);
				}
			}
		}

		private void UiSetStationLecternHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			_appService.SetInputLectern(e.Arg);
		}

		private void UiSetStationLocalHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			_appService.SetInputStation(e.Arg);
		}
		
		private void TransportUi_TransportDialRequest(object? sender, GenericDualEventArgs<string, string> e)
		{
			_appService.TransportDial(e.Arg1, e.Arg2);
		}

		private void TransportUi_TransportDialFavoriteRequest(object? sender, GenericDualEventArgs<string, string> e)
		{
			_appService.TransportDialFavorite(e.Arg1, e.Arg2);
		}

		private void TransportUi_TransportControlRequest(object? sender, GenericDualEventArgs<string, TransportTypes> e)
		{
			TransportUtilities.SendCommand(_appService, e.Arg1, e.Arg2);
		}

		private void EventUi_CustomEventStateChanged(object? sender, GenericDualEventArgs<string, bool> e)
		{
			if (_appService is ICustomEventAppService eventApp)
			{
				eventApp.ChangeCustomEventState(e.Arg1, e.Arg2);
			}
		}
		#endregion

		#region Fusion Handlers
		private void FusionRouteSourceHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.FusionRouteSourceHandler()");
			_appService.RouteToAll(e.Arg);
		}

		private void FusionAudioLevelHandler(object? sender, GenericSingleEventArgs<uint> e)
		{
			Logger.Debug("PresentationService.FusionAudioLevelHandler()");
			var pgmOut = _appService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmOut != null)
			{
				_appService.SetAudioOutputLevel(pgmOut.Id, (int)e.Arg);
			}
		}

		private void FusionAudioMuteHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionAudioMuteHandler()");
			var pgmOut = _appService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmOut != null)
			{
				_appService.SetAudioOutputMute(pgmOut.Id, !_appService.QueryAudioOutputMute(pgmOut.Id));
			}
		}

		private void FusionDisplayFreezeHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionDisplayFreezeHandler()");
			_appService.SetGlobalVideoFreeze(!_appService.QueryGlobalVideoFreeze());
		}

		private void FusionDisplayBlankHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionDisplayBlankHandler()");
			_appService.SetGlobalVideoBlank(!_appService.QueryGlobalVideoFreeze());
		}

		private void FusionDisplayPowerHandler(object? sender, GenericSingleEventArgs<bool> e)
		{
			Logger.Debug("PresentationService.FusionDisplayPowerHandler()");
			foreach (var display in _appService.GetAllDisplayInfo())
			{
				_appService.SetDisplayPower(display.Id, e.Arg);
			}
		}

		private void FusionPowerHandler(object? sender, GenericSingleEventArgs<bool> e)
		{
			Logger.Debug("PresentationService.FusionPowerHandler()");
			if (e.Arg)
			{
				_appService.SetActive();
			}
			else
			{
				_appService.SetStandby();
			}
		}

		private void FusionMicMuteHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.FusionMicMuteHandler()");
			bool currentState = _appService.QueryAudioInputMute(e.Arg);
			_appService.SetAudioInputMute(e.Arg, !currentState);
		}

		private void FusionConnectionHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionConnectionHandler()");
			UpdateFusionFeedback();
		}

		private void UpdateFusionDisplayPowerFeedback()
		{
			bool aDisplayOn = false;
			foreach (var display in _appService.GetAllDisplayInfo())
			{
				bool power = _appService.DisplayPowerQuery(display.Id);

				if (power)
				{
					aDisplayOn = power;
					break;
				}
			}

			_fusion?.UpdateDisplayPower(aDisplayOn);
		}

		private void UpdateFusionAudioFeedback()
		{
			var pgmAudio = _appService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmAudio == null) return;
			_fusion?.UpdateProgramAudioLevel((uint)_appService.QueryAudioOutputLevel(pgmAudio.Id));
			_fusion?.UpdateProgramAudioMute(_appService.QueryAudioOutputMute(pgmAudio.Id));
		}

		private void UpdateFusionRoutingFeedback()
		{
			var avDestinations = _appService.GetAllAvDestinations();
			if (avDestinations.Count > 0)
			{
				var currentRoute = _appService.QueryCurrentRoute(avDestinations[0].Id);
				_fusion?.UpdateSelectedSource(currentRoute.Id);
				foreach (var source in _appService.GetAllAvSources())
				{
					if (source.Id.Equals(currentRoute.Id, StringComparison.InvariantCulture))
					{
						_fusion?.StartDeviceUse(source.Id);
					}
					else
					{
						_fusion?.StopDeviceUse(source.Id);
					}
				}
			}
		}

		private void UpdateFusionFeedback()
		{
			_fusion?.UpdateSystemState(_appService.CurrentSystemState);
			UpdateFusionDisplayPowerFeedback();
			UpdateFusionAudioFeedback();
			UpdateFusionRoutingFeedback();
			_fusion?.UpdateDisplayFreeze(_appService.QueryGlobalVideoFreeze());
			_fusion?.UpdateDisplayBlank(_appService.QueryGlobalVideoBlank());
		}
		#endregion
	}
}
