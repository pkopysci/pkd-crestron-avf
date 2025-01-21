// ReSharper disable SuspiciousTypeConversion.Global
namespace pkd_ui_service
{
	using Crestron.SimplSharp;
	using Crestron.SimplSharpPro;
	using pkd_application_service;
	using pkd_application_service.CustomEvents;
	using pkd_application_service.LightingControl;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using Fusion;
	using Interfaces;
	using Utility;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class PresentationService : IPresentationService, IDisposable
	{
		private readonly CrestronControlSystem control;
		private readonly IApplicationService appService;
		private readonly List<IUserInterface> uiConnections;
		private IFusionInterface fusion;
		private CTimer? stateChangeTimer;
		private bool disposed;
#if DEBUG
		private const int TransitionTime = 3000;
#else
        private const int TransitionTime = 20000;
#endif

		public PresentationService(IApplicationService appService, CrestronControlSystem control)
		{
			ParameterValidator.ThrowIfNull(appService, "Ctor", nameof(appService));
			ParameterValidator.ThrowIfNull(control, "Ctor", nameof(control));

			this.appService = appService;
			this.control = control;
			uiConnections = [];
			BuildInterfaces();
			SubscribeToAppService();
		}

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
			foreach (var uiDevice in uiConnections)
			{
				uiDevice.Connect();
			}

			fusion.Initialize();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing)
			{
				UnsubscribeFromAppService();
				UnsubscribeFromInterfaces();
				foreach (var uiConn in uiConnections)
				{
					if (uiConn is IDisposable disposableUiConn)
					{
						disposableUiConn.Dispose();
					}
				}

				stateChangeTimer?.Dispose();

				fusion = PresentationServiceFactory.CreateFusionService(appService, control);
				fusion.OnlineStatusChanged -= FusionConnectionHandler;
				fusion.MicMuteChangeRequested -= FusionMicMuteHandler;
				fusion.SystemStateChangeRequested -= FusionPowerHandler;
				fusion.DisplayPowerChangeRequested -= FusionDisplayPowerHandler;
				fusion.DisplayBlankChangeRequested -= FusionDisplayBlankHandler;
				fusion.DisplayFreezeChangeRequested -= FusionDisplayFreezeHandler;
				fusion.AudioMuteChangeRequested -= FusionAudioMuteHandler;
				fusion.ProgramAudioChangeRequested -= FusionAudioLevelHandler;
				fusion.SourceSelectRequested -= FusionRouteSourceHandler;
				fusion.Dispose();
			}

			disposed = true;
		}

		private void BuildInterfaces()
		{
			foreach (var device in appService.GetAllUserInterfaces())
			{
				Logger.Info("PresentationService - Building interface {0} with IP-ID {1}", device.Id, device.IpId);
				
				var uiObj = PresentationServiceFactory.CreateUserInterface(control, device, appService);
				if (uiObj == null) continue;
				uiConnections.Add(uiObj);
				if (!uiObj.IsInitialized)
				{
					uiObj.Initialize();
				}
				
				SubscribeToInterface(uiObj);
			}

			fusion = PresentationServiceFactory.CreateFusionService(appService, control);
			fusion.OnlineStatusChanged += FusionConnectionHandler;
			fusion.MicMuteChangeRequested += FusionMicMuteHandler;
			fusion.SystemStateChangeRequested += FusionPowerHandler;
			fusion.DisplayPowerChangeRequested += FusionDisplayPowerHandler;
			fusion.DisplayBlankChangeRequested += FusionDisplayBlankHandler;
			fusion.DisplayFreezeChangeRequested += FusionDisplayFreezeHandler;
			fusion.AudioMuteChangeRequested += FusionAudioMuteHandler;
			fusion.ProgramAudioChangeRequested += FusionAudioLevelHandler;
			fusion.SourceSelectRequested += FusionRouteSourceHandler;
		}

		private void SubscribeToAppService()
		{
			appService.AudioDspConnectionStatusChanged += AppServiceDspConnectionHandler;
			appService.AudioInputLevelChanged += AppServiceAudioInputLevelHandler;
			appService.AudioInputMuteChanged += AppServiceAudioInputMuteHandler;
			appService.AudioOutputLevelChanged += AppServiceAudioOutputLevelHandler;
			appService.AudioOutputMuteChanged += AppServiceAudioOutputMuteHandler;
			appService.AudioOutputRouteChanged += AppServiceAudioOutputRouteHandler;
			appService.AudioZoneEnableChanged += AppServiceAudioZoneEnableHandler;
			appService.DisplayBlankChange += AppServiceDisplayBlankHandler;
			appService.DisplayFreezeChange += AppServiceDisplayFreezeHandler;
			appService.DisplayConnectChange += AppServiceDisplayConnectionHandler;
			appService.DisplayPowerChange += AppServiceDisplayPowerHandler;
			appService.DisplayInputChanged += AppServiceDisplayInputChangedHandler;
			appService.EndpointConnectionChanged += AppServiceEndpointConnectionHandler;
			appService.EndpointRelayChanged += AppServiceEndpointChangedHandler;
			appService.RouteChanged += AppServiceRouteHandler;
			appService.RouterConnectChange += AppServiceRouterConnectionHandler;
			appService.SystemStateChanged += AppServiceStateChangeHandler;
			appService.GlobalVideoBlankChanged += AppServiceGlobalBlankHandler;
			appService.GlobalVideoFreezeChanged += AppServiceGlobalFreezeHandler;
			appService.LightingSceneChanged += AppServiceLightingSceneHandler;
			appService.LightingLoadLevelChanged += AppServiceLightingLoadHandler;
			appService.LightingControlConnectionChanged += AppServiceLightingConnectionHandler;

			if (appService is CustomEventAppService customEventService)
			{
				customEventService.CustomEventStateChanged += AppServiceCustomEventHandler;
			}

			if (appService is ITechAuthGroupAppService techService)
			{
				techService.NonTechLockoutStateChangeRequest += AppServiceTechLockoutHandler;
			}
		}

		private void UnsubscribeFromAppService()
		{
			appService.AudioDspConnectionStatusChanged -= AppServiceDspConnectionHandler;
			appService.AudioInputLevelChanged -= AppServiceAudioInputLevelHandler;
			appService.AudioInputMuteChanged -= AppServiceAudioInputMuteHandler;
			appService.AudioOutputLevelChanged -= AppServiceAudioOutputLevelHandler;
			appService.AudioOutputMuteChanged -= AppServiceAudioOutputMuteHandler;
			appService.AudioOutputRouteChanged -= AppServiceAudioOutputRouteHandler;
			appService.AudioZoneEnableChanged -= AppServiceAudioZoneEnableHandler;
			appService.DisplayBlankChange -= AppServiceDisplayBlankHandler;
			appService.DisplayFreezeChange -= AppServiceDisplayFreezeHandler;
			appService.DisplayConnectChange -= AppServiceDisplayConnectionHandler;
			appService.EndpointConnectionChanged -= AppServiceEndpointConnectionHandler;
			appService.EndpointRelayChanged -= AppServiceEndpointChangedHandler;
			appService.RouteChanged -= AppServiceRouteHandler;
			appService.RouterConnectChange -= AppServiceRouterConnectionHandler;
			appService.SystemStateChanged -= AppServiceStateChangeHandler;
			appService.LightingSceneChanged -= AppServiceLightingSceneHandler;
			appService.LightingLoadLevelChanged -= AppServiceLightingLoadHandler;
			appService.LightingControlConnectionChanged -= AppServiceLightingConnectionHandler;

			if (appService is CustomEventAppService customEventService)
			{
				customEventService.CustomEventStateChanged -= AppServiceCustomEventHandler;
			}

			if (appService is ITechAuthGroupAppService securityService)
			{
				securityService.NonTechLockoutStateChangeRequest -= AppServiceTechLockoutHandler;
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
		}

		private void UnsubscribeFromInterfaces()
		{
			foreach (var ui in uiConnections)
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
			}
		}

		private void StateChangeTimerCallback(object? obj)
		{
			foreach (var conn in uiConnections)
			{
				conn.HideSystemStateChanging();
			}
		}

		private void TriggerStateChangeTimer()
		{
			if (stateChangeTimer != null)
			{
				stateChangeTimer.Reset(TransitionTime);
			}
			else
			{
				stateChangeTimer = new CTimer(StateChangeTimerCallback, TransitionTime);
			}
		}

		#region AppService Handlers
		private void AppServiceTechLockoutHandler(object? sender, GenericSingleEventArgs<bool> e)
		{
			foreach (var ui in uiConnections)
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
			foreach (var ui in uiConnections)
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
			foreach (var ui in uiConnections)
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
			foreach (var ui in uiConnections)
			{
				if (ui is not ICustomEventUserInterface eventUi) continue;
				eventUi.UpdateCustomEvent(e.Arg, state);
			}
		}

		private void AppServiceDspConnectionHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			bool isOnline = appService.QueryAudioDspConnectionStatus(args.Arg);
			if (isOnline)
			{
				RemoveErrorFromUi(args.Arg);
				fusion.ClearOfflineDevice(args.Arg);
			}
			else
			{
				var found = appService.GetAllAudioDspDevices().First(x => x.Id.Equals(args.Arg, StringComparison.InvariantCulture));
				AddErrorToUi(args.Arg, found.Label );
				fusion.AddOfflineDevice(args.Arg, found.Label );
			}
		}

		private void AppServiceAudioInputLevelHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			int level = appService.QueryAudioInputLevel(args.Arg);
			foreach (var ui in uiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioInputLevel(args.Arg, level);
				}
			}
		}

		private void AppServiceAudioInputMuteHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			bool newState = appService.QueryAudioInputMute(args.Arg);
			foreach (var ui in uiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioInputMute(args.Arg, newState);
				}
			}

			fusion.UpdateMicMute(args.Arg, newState);
		}

		private void AppServiceAudioOutputLevelHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			int level = appService.QueryAudioOutputLevel(args.Arg);
			foreach (var ui in uiConnections)
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
			bool newState = appService.QueryAudioOutputMute(args.Arg);
			foreach (var ui in uiConnections)
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
			string sourceId = appService.QueryAudioOutputRoute(args.Arg);
			foreach (var ui in uiConnections)
			{
				if (ui is IAudioUserInterface audioUi)
				{
					audioUi.UpdateAudioOutputRoute(sourceId, args.Arg);
				}
			}
		}

		private void AppServiceAudioZoneEnableHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			bool newState = appService.QueryAudioZoneState(args.Arg1, args.Arg2);
			foreach (var ui in uiConnections)
			{
				var audioUi = (ui as IAudioUserInterface);
				audioUi?.UpdateAudioZoneState(args.Arg1, args.Arg2, newState);
			}
		}

		private void AppServiceDisplayConnectionHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceDisplayConnectionHandler() - {0}, {1}", args.Arg1, args.Arg2);
			if (args.Arg2)
			{
				fusion.ClearOfflineDevice(args.Arg1);
				RemoveErrorFromUi(args.Arg1);
			}
			else
			{
				var display = appService.GetAllDisplayInfo().First(x => x.Id.Equals(args.Arg1, StringComparison.InvariantCulture));
				fusion.AddOfflineDevice(args.Arg1, display.Label);
				AddErrorToUi(args.Arg1, display.Label);
			}
		}

		private void AppServiceDisplayBlankHandler(object? sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceDisplayBlankHandler() - {0}, {1}", args.Arg1, args.Arg2);
			foreach (var ui in uiConnections)
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
			foreach (var ui in uiConnections)
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
			foreach (var ui in uiConnections)
			{
				if (ui is IDisplayUserInterface displayUi)
				{
					displayUi.UpdateDisplayPower(e.Arg1, e.Arg2);
				}
			}

			UpdateFusionDisplayPowerFeedback();
			if (e.Arg2)
			{
				fusion.StartDisplayUse(e.Arg1);
			}
			else
			{
				fusion.StopDisplayUse(e.Arg1);
			}
		}

		private void AppServiceDisplayInputChangedHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.AppServiceDisplayInputChangedHandler({0})", e.Arg);

			bool isLectern = appService.DisplayInputLecternQuery(e.Arg);
			bool isStation = appService.DisplayInputStationQuery(e.Arg);
			foreach (var ui in uiConnections)
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
				fusion.ClearOfflineDevice(args.Arg1);
				RemoveErrorFromUi(args.Arg1);
			}
			else
			{
				string label = $"Endpoint {args.Arg1}";
				fusion.AddOfflineDevice(args.Arg1, label);
				AddErrorToUi(args.Arg1, label);
			}
		}

		private void AppServiceEndpointChangedHandler(object? sender, GenericDualEventArgs<string, int> args)
		{
			Logger.Debug("PresentationService.AppServiceEndpointChangedHandler() - {0}", args.Arg1, args.Arg2);
		}

		private void AppServiceRouteHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var currentSrc = appService.QueryCurrentRoute(args.Arg);
			foreach (var conn in uiConnections)
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
			
			bool isOnline = appService.QueryRouterConnectionStatus(args.Arg);
			if (isOnline)
			{
				RemoveErrorFromUi(args.Arg);
				fusion.ClearOfflineDevice(args.Arg);
			}
			else
			{
				string label = $"Router {args.Arg} is offline";
				AddErrorToUi(args.Arg, label);
				fusion.AddOfflineDevice(args.Arg, label);
			}
		}

		private void AppServiceLightingConnectionHandler(object? sender, GenericDualEventArgs<string, bool> e)
		{
			Logger.Debug("PresentationService.AppServiceLightingConnectionHandler({0}, 1)", e.Arg1, e.Arg2);
			if (e.Arg2)
			{
				RemoveErrorFromUi(e.Arg1);
				fusion.ClearOfflineDevice(e.Arg1);
			}
			else
			{
				string label = $"Lighting controller {e.Arg1}";
				AddErrorToUi(e.Arg1, label);
				fusion.AddOfflineDevice(e.Arg1, label);
			}
		}

		private void AppServiceGlobalFreezeHandler(object? sender, EventArgs e)
		{
			bool freezeState = appService.QueryGlobalVideoFreeze();
			foreach (var ui in uiConnections)
			{
				ui.SetGlobalFreezeState(freezeState);
			}

			fusion.UpdateDisplayFreeze(freezeState);
		}

		private void AppServiceGlobalBlankHandler(object? sender, EventArgs e)
		{
			bool blankState = appService.QueryGlobalVideoBlank();
			foreach (var ui in uiConnections)
			{
				ui.SetGlobalBlankState(blankState);
			}

			fusion.UpdateDisplayBlank(blankState);
		}

		private void AppServiceStateChangeHandler(object? sender, EventArgs args)
		{
			bool state = appService.CurrentSystemState;
			foreach (var conn in uiConnections)
			{
				conn.SetSystemState(state);
				conn.ShowSystemStateChanging(state);
			}

			TriggerStateChangeTimer();
			fusion.UpdateSystemState(state);

			// If system is powering off then stop recording usage, or start recording usage for the currently
			// selected input when powered on.
			if (state)
			{
				UpdateFusionRoutingFeedback();
			}
			else
			{
				foreach (var source in appService.GetAllAvSources())
				{
					fusion.StopDeviceUse(source.Id);
				}
			}
		}

		#endregion

		#region Touchscreen Handlers
		private void UiConnectionHandler(object? sender, GenericSingleEventArgs<string> args)
		{
			var found = uiConnections.FirstOrDefault(x => x.Id.Equals(args.Arg, StringComparison.InvariantCulture));
			if (found == null) return;
			
			if (found is { IsOnline: false, IsXpanel: false })
			{
				fusion.AddOfflineDevice(found.Id, $"UI {found.Id}");
			}
			else
			{
				fusion.ClearOfflineDevice(found.Id);
				found.SetSystemState(appService.CurrentSystemState);
				found.SetGlobalBlankState(appService.QueryGlobalVideoBlank());
				found.SetGlobalFreezeState(appService.QueryGlobalVideoFreeze());

				if (found is IAudioUserInterface audioUi)
				{
					foreach (var inChan in appService.GetAudioInputChannels())
					{
						audioUi.UpdateAudioInputLevel(inChan.Id, appService.QueryAudioInputLevel(inChan.Id));
						audioUi.UpdateAudioInputMute(inChan.Id, appService.QueryAudioInputMute(inChan.Id));
					}

					foreach (var outChan in appService.GetAudioOutputChannels())
					{
						audioUi.UpdateAudioOutputLevel(outChan.Id, appService.QueryAudioOutputLevel(outChan.Id));
						audioUi.UpdateAudioOutputMute(outChan.Id, appService.QueryAudioOutputMute(outChan.Id));
					}
				}

				if (found is ITransportControlUserInterface transportUi)
				{
					transportUi.SetCableBoxData(appService.GetAllCableBoxes());
				}

				if (found is ICustomEventUserInterface customEventUi)
				{
					UpdateCustomEventUi(customEventUi);
				}

				if (found is ILightingUserInterface lightingUi)
				{
					lightingUi.SetLightingData(appService.GetAllLightingDeviceInfo());
				}
			}
		}

		private void UpdateCustomEventUi(ICustomEventUserInterface ui)
		{
			if (appService is not CustomEventAppService eventServiceApp) return;
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
				appService.SetActive();
			}
			else
			{
				appService.SetStandby();
			}
		}

		private void UiRouteChangeHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			Logger.Debug("PresentationService.UiRouteChangeHandler() - {0} -> {1}", args.Arg1, args.Arg2);
			
			if (args.Arg2.Equals("ALL", StringComparison.InvariantCulture))
			{
				appService.RouteToAll(args.Arg1);
			}
			else
			{
				appService.MakeRoute(args.Arg1, args.Arg2);
			}
		}

		private void UiDisplayPowerHandler(object? sender, GenericDualEventArgs<string, bool> e)
		{
			Logger.Debug("PresentationService.UiDisplayPowerHandler() - {0} - {1}", e.Arg1, e.Arg2);
			appService.SetDisplayPower(e.Arg1, e.Arg2);
		}

		private void UiDisplayFreezeHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			bool currenState = appService.DisplayFreezeQuery(e.Arg);
			appService.SetDisplayFreeze(e.Arg, !currenState);
		}

		private void UiDisplayBlankHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			bool currentState = appService.DisplayBlankQuery(e.Arg);
			appService.SetDisplayBlank(e.Arg, !currentState);
		}

		private void UiGlobalBlankHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.UiGlobalBlankHandler()");
			bool currentState = appService.QueryGlobalVideoBlank();
			appService.SetGlobalVideoBlank(!currentState);
		}

		private void UiGlobalFreezeHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.UiGlobalFreezeHandler()");
			bool currentState = appService.QueryGlobalVideoFreeze();
			appService.SetGlobalVideoFreeze(!currentState);
		}

		private void UiDisplayScreenUpHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			appService.RaiseScreen(e.Arg);
		}

		private void UiDisplayScreenDownHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			appService.LowerScreen(e.Arg);
		}

		private void UiAudioOutputMuteRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputMuteRequest() - {0}", e.Arg);
			bool current = appService.QueryAudioOutputMute(e.Arg);
			appService.SetAudioOutputMute(e.Arg, !current);
		}

		private void UiAudioOutputLevelUpRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputLevelUpRequest() - {0}", e.Arg);
			var newLevel = appService.QueryAudioOutputLevel(e.Arg) + 3;
			appService.SetAudioOutputLevel(e.Arg, newLevel < 100 ? newLevel : 100);
		}

		private void UiAudioOutputLevelDownRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputLevelDownRequest() - {0}", e.Arg);
			var newLevel = appService.QueryAudioOutputLevel(e.Arg) - 3;
			appService.SetAudioOutputLevel(e.Arg, newLevel > 0 ? newLevel : 0);
		}

		private void UiAudioOutputRouteRequest(object? sender, GenericDualEventArgs<string, string> e)
		{
			appService.SetAudioOutputRoute(e.Arg1, e.Arg2);
		}

		private void UiAudioInputMuteRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputMuteRequest() - {0}", e.Arg);
			bool current = appService.QueryAudioInputMute(e.Arg);
			appService.SetAudioInputMute(e.Arg, !current);
		}

		private void UiAudioInputLevelDownRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputLevelDownRequest() - {0}", e.Arg);
			int newLevel = appService.QueryAudioInputLevel(e.Arg) - 3;
			appService.SetAudioInputLevel(e.Arg, newLevel > 0 ? newLevel : 0);
		}

		private void UiAudioInputLevelUpRequest(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputLevelUpRequest() - {0}", e.Arg);
			int newLevel = appService.QueryAudioInputLevel(e.Arg) + 3;
			appService.SetAudioInputLevel(e.Arg, newLevel < 100 ? newLevel : 100);
		}

		private void UiAudioZoneToggleHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			appService.ToggleAudioZoneState(e.Arg1, e.Arg2);
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

			appService.SetAudioOutputLevel(e.Arg1, adjustedLevel);
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

			appService.SetAudioInputLevel(e.Arg1, adjustedLevel);
		}

		private void LightingUiLoadHandler(object? sender, GenericTrippleEventArgs<string, string, int> e)
		{
			appService.SetLightingLoad(e.Arg1, e.Arg2, e.Arg3);
		}

		private void LightingUiSceneHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			appService.RecallLightingScene(e.Arg1, e.Arg2);
		}

		private void AddErrorToUi(string id, string label)
		{
			foreach (var ui in uiConnections)
			{
				if (ui is IErrorInterface errorUi)
				{
					errorUi.AddDeviceError(id, label);
				}
			}
		}

		private void RemoveErrorFromUi(string id)
		{
			foreach (var ui in uiConnections)
			{
				if (ui is IErrorInterface errorUi)
				{
					errorUi.ClearDeviceError(id);
				}
			}
		}

		private void UiSetStationLecternHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			appService.SetInputLectern(e.Arg);
		}

		private void UiSetStationLocalHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			appService.SetInputStation(e.Arg);
		}

		private void UiTransportDialHandler(object? sender, GenericDualEventArgs<string, string> args)
		{
			appService.TransportDial(args.Arg1, args.Arg2);
		}

		private void TransportUi_TransportDialRequest(object? sender, GenericDualEventArgs<string, string> e)
		{
			appService.TransportDial(e.Arg1, e.Arg2);
		}

		private void TransportUi_TransportDialFavoriteRequest(object? sender, GenericDualEventArgs<string, string> e)
		{
			appService.TransportDialFavorite(e.Arg1, e.Arg2);
		}

		private void TransportUi_TransportControlRequest(object? sender, GenericDualEventArgs<string, TransportTypes> e)
		{
			TransportUtilities.SendCommand(appService, e.Arg1, e.Arg2);
		}

		private void EventUi_CustomEventStateChanged(object? sender, GenericDualEventArgs<string, bool> e)
		{
			if (appService is ICustomEventAppService eventApp)
			{
				eventApp.ChangeCustomEventState(e.Arg1, e.Arg2);
			}
		}
		#endregion

		#region Fusion Handlers
		private void FusionRouteSourceHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.FusionRouteSourceHandler()");
			appService.RouteToAll(e.Arg);
		}

		private void FusionAudioLevelHandler(object? sender, GenericSingleEventArgs<uint> e)
		{
			Logger.Debug("PresentationService.FusionAudioLevelHandler()");
			var pgmOut = appService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmOut != null)
			{
				appService.SetAudioOutputLevel(pgmOut.Id, (int)e.Arg);
			}
		}

		private void FusionAudioMuteHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionAudioMuteHandler()");
			var pgmOut = appService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmOut != null)
			{
				appService.SetAudioOutputMute(pgmOut.Id, !appService.QueryAudioOutputMute(pgmOut.Id));
			}
		}

		private void FusionDisplayFreezeHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionDisplayFreezeHandler()");
			appService.SetGlobalVideoFreeze(!appService.QueryGlobalVideoFreeze());
		}

		private void FusionDisplayBlankHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionDisplayBlankHandler()");
			appService.SetGlobalVideoBlank(!appService.QueryGlobalVideoFreeze());
		}

		private void FusionDisplayPowerHandler(object? sender, GenericSingleEventArgs<bool> e)
		{
			Logger.Debug("PresentationService.FusionDisplayPowerHandler()");
			foreach (var display in appService.GetAllDisplayInfo())
			{
				appService.SetDisplayPower(display.Id, e.Arg);
			}
		}

		private void FusionPowerHandler(object? sender, GenericSingleEventArgs<bool> e)
		{
			Logger.Debug("PresentationService.FusionPowerHandler()");
			if (e.Arg)
			{
				appService.SetActive();
			}
			else
			{
				appService.SetStandby();
			}
		}

		private void FusionMicMuteHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.FusionMicMuteHandler()");
			bool currentState = appService.QueryAudioInputMute(e.Arg);
			appService.SetAudioInputMute(e.Arg, !currentState);
		}

		private void FusionConnectionHandler(object? sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionConnectionHandler()");
			UpdateFusionFeedback();
		}

		private void UpdateFusionDisplayPowerFeedback()
		{
			bool aDisplayOn = false;
			foreach (var display in appService.GetAllDisplayInfo())
			{
				bool power = appService.DisplayPowerQuery(display.Id);

				if (power)
				{
					aDisplayOn = power;
					break;
				}
			}

			fusion.UpdateDisplayPower(aDisplayOn);
		}

		private void UpdateFusionAudioFeedback()
		{
			var pgmAudio = appService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmAudio == null) return;
			fusion.UpdateProgramAudioLevel((uint)appService.QueryAudioOutputLevel(pgmAudio.Id));
			fusion.UpdateProgramAudioMute(appService.QueryAudioOutputMute(pgmAudio.Id));
		}

		private void UpdateFusionRoutingFeedback()
		{
			var avDestinations = appService.GetAllAvDestinations();
			if (avDestinations.Count > 0)
			{
				var currentRoute = appService.QueryCurrentRoute(avDestinations[0].Id);
				fusion.UpdateSelectedSource(currentRoute.Id);
				foreach (var source in appService.GetAllAvSources())
				{
					if (source.Id.Equals(currentRoute.Id, StringComparison.InvariantCulture))
					{
						fusion.StartDeviceUse(source.Id);
					}
					else
					{
						fusion.StopDeviceUse(source.Id);
					}
				}
			}
		}

		private void UpdateFusionFeedback()
		{
			fusion.UpdateSystemState(appService.CurrentSystemState);
			UpdateFusionDisplayPowerFeedback();
			UpdateFusionAudioFeedback();
			UpdateFusionRoutingFeedback();
			fusion.UpdateDisplayFreeze(appService.QueryGlobalVideoFreeze());
			fusion.UpdateDisplayBlank(appService.QueryGlobalVideoBlank());
		}
		#endregion
	}
}
