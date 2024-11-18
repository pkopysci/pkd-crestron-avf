namespace pkd_ui_service
{
	using Crestron.SimplSharp;
	using Crestron.SimplSharpPro;
	using pkd_application_service;
	using pkd_application_service.Base;
	using pkd_application_service.CustomEvents;
	using pkd_application_service.LightingControl;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_ui_service.Fusion;
	using pkd_ui_service.Interfaces;
	using pkd_ui_service.Utility;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class PresentationService : IPresentationService, IDisposable
	{
		private readonly CrestronControlSystem control;
		private readonly IApplicationService appService;
		private readonly List<IUserInterface> uiConnections;
		private IFusionInterface fusion;
		private CTimer stateChangeTimer;
		private bool disposed;
#if DEBUG
		private static readonly int TRANSITION_TIME = 3000;
#else
        private static readonly int TRANSITION_TIME = 20000;
#endif

		public PresentationService(IApplicationService appService, CrestronControlSystem control)
		{
			ParameterValidator.ThrowIfNull(appService, "Ctor", "appService");
			ParameterValidator.ThrowIfNull(control, "Ctor", "control");

			this.appService = appService;
			this.control = control;
			this.uiConnections = new List<IUserInterface>();
			this.BuildInterfaces();
			this.SubscribeToAppService();
		}

		~PresentationService()
		{
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public void Initialize()
		{
			foreach (var uiDevice in this.uiConnections)
			{
				uiDevice.Connect();
			}

			this.fusion.Initialize();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					this.UnsubscribeFromAppService();
					this.UnsubscribeFromInterfaces();
					foreach (var uiConn in this.uiConnections)
					{
						if (uiConn is IDisposable)
						{
							(uiConn as IDisposable).Dispose();
						}
					}

					this.stateChangeTimer?.Dispose();

					this.fusion = PresentationServiceFactory.CreateFusionService(this.appService, this.control);
					this.fusion.OnlineStatusChanged -= this.FusionConnectionHandler;
					this.fusion.MicMuteChangeRequested -= this.FusionMicMuteHandler;
					this.fusion.SystemStateChangeRequested -= this.FusionPowerHandler;
					this.fusion.DisplayPowerChangeRequested -= this.FusionDisplayPowerHandler;
					this.fusion.DisplayBlankChangeRequested -= this.FusionDisplayBlankHandler;
					this.fusion.DisplayFreezeChangeRequested -= this.FusionDisplayFreezeHandler;
					this.fusion.AudioMuteChangeRequested -= this.FusionAudioMuteHandler;
					this.fusion.ProgramAudioChangeRequested -= this.FusionAudioLevelHandler;
					this.fusion.SourceSelectRequested -= this.FusionRouteSourceHandler;
					this.fusion.Dispose();
				}

				this.disposed = true;
			}
		}

		private void BuildInterfaces()
		{
			foreach (var device in this.appService.GetAllUserInterfaces())
			{
				Logger.Info("PresentationService - Buidling interface {0} with IP-ID {1}", device.Id, device.IpId);
				var uiObj = PresentationServiceFactory.CreateUserInterface(this.control, device, this.appService);
				this.uiConnections.Add(uiObj);
				if (!uiObj.IsInitialized)
				{
					uiObj.Initialize();
				};
				
				this.SubscribeToInterface(uiObj);
			}

			this.fusion = PresentationServiceFactory.CreateFusionService(this.appService, this.control);
			this.fusion.OnlineStatusChanged += this.FusionConnectionHandler;
			this.fusion.MicMuteChangeRequested += this.FusionMicMuteHandler;
			this.fusion.SystemStateChangeRequested += this.FusionPowerHandler;
			this.fusion.DisplayPowerChangeRequested += this.FusionDisplayPowerHandler;
			this.fusion.DisplayBlankChangeRequested += this.FusionDisplayBlankHandler;
			this.fusion.DisplayFreezeChangeRequested += this.FusionDisplayFreezeHandler;
			this.fusion.AudioMuteChangeRequested += this.FusionAudioMuteHandler;
			this.fusion.ProgramAudioChangeRequested += this.FusionAudioLevelHandler;
			this.fusion.SourceSelectRequested += this.FusionRouteSourceHandler;
		}

		private void SubscribeToAppService()
		{
			this.appService.AudioDspConnectionStatusChanged += this.AppServiceDspConnectionHandler;
			this.appService.AudioInputLevelChanged += this.AppServiceAudioInputLevelHandler;
			this.appService.AudioInputMuteChanged += this.AppServiceAudioInputMuteHandler;
			this.appService.AudioOutputLevelChanged += this.AppServiceAudioOutputLevelHandler;
			this.appService.AudioOutputMuteChanged += this.AppServiceAudioOutputMuteHandler;
			this.appService.AudioOutputRouteChanged += this.AppServiceAudioOutputRouteHandler;
			this.appService.AudioZoneEnableChanged += this.AppServiceAudioZoneEnableHandler;
			this.appService.DisplayBlankChange += this.AppServiceDisplayBlankHandler;
			this.appService.DisplayFreezeChange += this.AppServiceDisplayFreezeHandler;
			this.appService.DisplayConnectChange += this.AppServiceDisplayConnectionHandler;
			this.appService.DisplayPowerChange += this.AppServiceDisplayPowerHandler;
			this.appService.DisplayInputChanged += this.AppServiceDisplayInputChangedHandler;
			this.appService.EndpointConnectionChanged += this.AppServiceEnpdpointConnectionHandler;
			this.appService.EndpointRelayChanged += this.AppServiceEndpointChangedHandler;
			this.appService.RouteChanged += this.AppServiceRouteHandler;
			this.appService.RouterConnectChange += this.AppServiceRouterConnectionHandler;
			this.appService.SystemStateChanged += this.AppServiceStateChangeHandler;
			this.appService.GlobalVideoBlankChanged += AppServiceGlobalBlankHandler;
			this.appService.GlobalVideoFreezeChanged += AppServiceGlobalFreezeHandler;
			this.appService.LightingSceneChanged += this.AppServiceLightingSceneHandler;
			this.appService.LightingLoadLevelChanged += this.AppServiceLightingLoadHandler;
			this.appService.LightingControlConnectionChanged += this.AppServiceLightingConnectionHandler;

			if (this.appService is CustomEventAppService customEventService)
			{
				customEventService.CustomEventStateChanged += this.AppServiceCustomEventHandler;
			}

			if (this.appService is ITechAuthGroupAppService techService)
			{
				techService.NonTechLockoutStateChangeRequest += AppServiceTechLockoutHandler;
			}
		}

		private void UnsubscribeFromAppService()
		{
			if (this.appService != null)
			{
				this.appService.AudioDspConnectionStatusChanged -= this.AppServiceDspConnectionHandler;
				this.appService.AudioInputLevelChanged -= this.AppServiceAudioInputLevelHandler;
				this.appService.AudioInputMuteChanged -= this.AppServiceAudioInputMuteHandler;
				this.appService.AudioOutputLevelChanged -= this.AppServiceAudioOutputLevelHandler;
				this.appService.AudioOutputMuteChanged -= this.AppServiceAudioOutputMuteHandler;
				this.appService.AudioOutputRouteChanged -= this.AppServiceAudioOutputRouteHandler;
				this.appService.AudioZoneEnableChanged -= this.AppServiceAudioZoneEnableHandler;
				this.appService.DisplayBlankChange -= this.AppServiceDisplayBlankHandler;
				this.appService.DisplayFreezeChange -= this.AppServiceDisplayFreezeHandler;
				this.appService.DisplayConnectChange -= this.AppServiceDisplayConnectionHandler;
				this.appService.EndpointConnectionChanged -= this.AppServiceEnpdpointConnectionHandler;
				this.appService.EndpointRelayChanged -= this.AppServiceEndpointChangedHandler;
				this.appService.RouteChanged -= this.AppServiceRouteHandler;
				this.appService.RouterConnectChange -= this.AppServiceRouterConnectionHandler;
				this.appService.SystemStateChanged -= this.AppServiceStateChangeHandler;
				this.appService.LightingSceneChanged -= this.AppServiceLightingSceneHandler;
				this.appService.LightingLoadLevelChanged -= this.AppServiceLightingLoadHandler;
				this.appService.LightingControlConnectionChanged -= this.AppServiceLightingConnectionHandler;

				if (this.appService is CustomEventAppService customEventService)
				{
					customEventService.CustomEventStateChanged -= this.AppServiceCustomEventHandler;
				}

				if (this.appService is ITechAuthGroupAppService securityService)
				{
					securityService.NonTechLockoutStateChangeRequest -= AppServiceTechLockoutHandler;
				}
			}
		}

		private void SubscribeToInterface(IUserInterface ui)
		{
			ui.OnlineStatusChanged += this.UiConnectionHandler;
			ui.SystemStateChangeRequest += this.UiStatusChangeHandler;
			ui.GlobalBlankToggleRequest += this.UiGlobalBlankHandler;
			ui.GlobalFreezeToggleRequest += this.UiGlobalFreezeHandler;

			if (ui is IRoutingUserInterface)
			{
				(ui as IRoutingUserInterface).AvRouteChangeRequest += this.UiRouteChangeHandler;
			}

			if (ui is IDisplayUserInterface)
			{
				var displayUi = ui as IDisplayUserInterface;
				displayUi.DisplayBlankChangeRequest += this.UiDisplayBlankHandler;
				displayUi.DisplayFreezeChangeRequest += this.UiDisplayFreezeHandler;
				displayUi.DisplayPowerChangeRequest += this.UiDisplayPowerHandler;
				displayUi.DisplayScreenDownRequest += this.UiDisplayScreenDownHandler;
				displayUi.DisplayScreenUpRequest += this.UiDisplayScreenUpHandler;
				displayUi.StationLecternInputRequest += this.UISetStationLecternHandler;
				displayUi.StationLocalInputRequest += this.UiSetStationLocalHandler;
			}

			if (ui is IAudioUserInterface)
			{
				var audioUi = ui as IAudioUserInterface;
				audioUi.AudioInputLevelDownRequest += this.UiAudioInputLevelDownRequest;
				audioUi.AudioInputLevelUpRequest += this.UiAudioInputLevelUpRequest;
				audioUi.AudioInputMuteChangeRequest += this.UiAudioInputMuteRequest;
				audioUi.AudioOutputLevelDownRequest += this.UiAudioOutputLevelDownRequest;
				audioUi.AudioOutputLevelUpRequest += this.UiAudioOutputLevelUpRequest;
				audioUi.AudioOutputMuteChangeRequest += this.UiAudioOutputMuteRequest;
				audioUi.AudioOutputRouteRequest += this.UiAudioOutputRouteRequest;
				audioUi.AudioZoneEnableToggleRequest += this.UiAudioZoneToggleHandler;
			}

			if (ui is IAudioDiscreteLevelUserInterface discreteAudioUi)
			{
				discreteAudioUi.SetAudioInputLevelRequest += DiscreteAudioInputUiHandler;
				discreteAudioUi.SetAudioOutputLevelRequest += DiscreteAudioOutputUiHandler;
			}

			if (ui is ITransportControlUserInterface)
			{
				var transportUi = ui as ITransportControlUserInterface;
				transportUi.TransportControlRequest += this.TransportUi_TransportControlRequest;
				transportUi.TransportDialFavoriteRequest += this.TransportUi_TransportDialFavoriteRequest;
				transportUi.TransportDialRequest += this.TransportUi_TransportDialRequest;
			}

			if (ui is ICustomEventUserInterface)
			{
				var eventUi = ui as ICustomEventUserInterface;
				eventUi.CustomEventChangeRequest += this.EventUi_CustomEventStateChanged;
			}

			if (ui is ILightingUserInterface)
			{
				var lightingUi = ui as ILightingUserInterface;
				lightingUi.LightingSceneRecallRequest += this.LightingUISceneHandler;
				lightingUi.LightingLoadChangeRequest += this.LightingUILoadHandler;
			}
		}

		private void UnsubscribeFromInterfaces()
		{
			foreach (var ui in this.uiConnections)
			{
				if (ui != null)
				{
					ui.OnlineStatusChanged -= this.UiConnectionHandler;
					ui.SystemStateChangeRequest -= this.UiStatusChangeHandler;
					ui.GlobalFreezeToggleRequest -= this.UiGlobalFreezeHandler;
					ui.GlobalBlankToggleRequest -= this.UiGlobalBlankHandler;

					if (ui is IRoutingUserInterface)
					{
						(ui as IRoutingUserInterface).AvRouteChangeRequest -= this.UiRouteChangeHandler;
					}

					if (ui is IDisplayUserInterface)
					{
						var displayUi = ui as IDisplayUserInterface;
						displayUi.DisplayBlankChangeRequest -= this.UiDisplayBlankHandler;
						displayUi.DisplayFreezeChangeRequest -= this.UiDisplayFreezeHandler;
						displayUi.DisplayPowerChangeRequest -= this.UiDisplayPowerHandler;
						displayUi.DisplayScreenDownRequest += this.UiDisplayScreenDownHandler;
						displayUi.DisplayScreenUpRequest += this.UiDisplayScreenUpHandler;
					}

					if (ui is IAudioUserInterface)
					{
						var audioUi = ui as IAudioUserInterface;
						audioUi.AudioInputLevelDownRequest -= this.UiAudioInputLevelDownRequest;
						audioUi.AudioInputLevelUpRequest -= this.UiAudioInputLevelUpRequest;
						audioUi.AudioInputMuteChangeRequest -= this.UiAudioInputMuteRequest;
						audioUi.AudioOutputLevelDownRequest -= this.UiAudioOutputLevelDownRequest;
						audioUi.AudioOutputLevelUpRequest -= this.UiAudioOutputLevelUpRequest;
						audioUi.AudioOutputMuteChangeRequest -= this.UiAudioOutputMuteRequest;
						audioUi.AudioZoneEnableToggleRequest -= this.UiAudioZoneToggleHandler;
					}

					if (ui is ITransportControlUserInterface)
					{
						var transportUi = ui as ITransportControlUserInterface;
						transportUi.TransportControlRequest -= this.TransportUi_TransportControlRequest;
						transportUi.TransportDialFavoriteRequest -= this.TransportUi_TransportDialFavoriteRequest;
						transportUi.TransportDialRequest -= this.TransportUi_TransportDialRequest;
					}

					if (ui is ICustomEventUserInterface)
					{
						var eventUi = ui as ICustomEventUserInterface;
						eventUi.CustomEventChangeRequest -= this.EventUi_CustomEventStateChanged;
					}
				}
			}
		}

		private void StateChangeTimerCallback(object obj)
		{
			foreach (var conn in this.uiConnections)
			{
				conn.HideSystemStateChanging();
			}
		}

		private void TriggerStateChangeTimer()
		{
			if (this.stateChangeTimer != null)
			{
				this.stateChangeTimer.Reset(TRANSITION_TIME);
			}
			else
			{
				this.stateChangeTimer = new CTimer(this.StateChangeTimerCallback, TRANSITION_TIME);
			}
		}

		#region AppService Handlers
		private void AppServiceTechLockoutHandler(object sender, GenericSingleEventArgs<bool> e)
		{
			foreach (var ui in this.uiConnections)
			{
				if (ui is ISecurityUserInterface secureUi)
				{
					if (e.Arg)
						secureUi.EnableTechOnlyLock();
					else
						secureUi.DisableTechOnlyLock();
				}
			}
		}

		private void AppServiceLightingLoadHandler(object sender, GenericDualEventArgs<string, string> e)
		{
			Logger.Debug("PresentationService.AppServiceLightingLoadHandler({0}, {1})", e.Arg1, e.Arg2);
			if (!(sender is ILightingControlApp lightingService))
			{
				return;
			}

			var loadLevel = lightingService.GetZoneLoad(e.Arg1, e.Arg2);
			foreach (var ui in this.uiConnections)
			{
				if (ui is ILightingUserInterface)
				{
					(ui as ILightingUserInterface).UpdateLightingZoneLoad(e.Arg1, e.Arg2, loadLevel);
				}
			}
		}

		private void AppServiceLightingSceneHandler(object sender, GenericSingleEventArgs<string> e)
		{
			if (!(sender is ILightingControlApp lightingService))
			{
				return;
			}

			var scene = lightingService.GetActiveScene(e.Arg);
			foreach (var ui in this.uiConnections)
			{
				if (ui is ILightingUserInterface)
				{
					(ui as ILightingUserInterface).UpdateActiveLightingScene(e.Arg, scene);
				}
			}
		}

		private void AppServiceCustomEventHandler(object sender, GenericSingleEventArgs<string> e)
		{
			if (sender is CustomEventAppService customAppService)
			{
				bool state = customAppService.QueryCustomEventState(e.Arg);
				foreach (var ui in this.uiConnections)
				{
					if (!(ui is ICustomEventUserInterface eventUi))
					{
						continue;
					}
					eventUi.UpdateCustomEvent(e.Arg, state);
				}
			}
		}

		private void AppServiceDspConnectionHandler(object sender, GenericSingleEventArgs<string> args)
		{
			Logger.Debug("Presentation.PresentationService.AppServiceDspConnectionHandler() - {0}", args.Arg);
			bool isOnline = this.appService.QueryAudioDspConnectionStatus(args.Arg);
			if (isOnline)
			{
				this.RemoveErrorFromUi(args.Arg);
				this.fusion?.ClearOfflineDevice(args.Arg);
			}
			else
			{
				var found = this.appService.GetAllAudioDspDevices().First(x => x.Id.Equals(args.Arg, StringComparison.InvariantCulture));
				string label = (found != null) ? found.Label : args.Arg;
				this.AddErrorToUi(args.Arg, label);
				fusion?.AddOfflineDevice(args.Arg, label);
			}
		}

		private void AppServiceAudioInputLevelHandler(object sender, GenericSingleEventArgs<string> args)
		{
			int level = this.appService.QueryAudioInputLevel(args.Arg);
			foreach (var ui in this.uiConnections)
			{
				if (ui is IAudioUserInterface)
				{
					(ui as IAudioUserInterface).UpdateAudioInputLevel(args.Arg, level);
				}
			}
		}

		private void AppServiceAudioInputMuteHandler(object sender, GenericSingleEventArgs<string> args)
		{
			bool newState = this.appService.QueryAudioInputMute(args.Arg);
			foreach (var ui in this.uiConnections)
			{
				if (ui is IAudioUserInterface)
				{
					(ui as IAudioUserInterface).UpdateAudioInputMute(args.Arg, newState);
				}
			}

			this.fusion?.UpdateMicMute(args.Arg, newState);
		}

		private void AppServiceAudioOutputLevelHandler(object sender, GenericSingleEventArgs<string> args)
		{
			int level = this.appService.QueryAudioOutputLevel(args.Arg);
			foreach (var ui in this.uiConnections)
			{
				if (ui is IAudioUserInterface)
				{
					(ui as IAudioUserInterface).UpdateAudioOutputLevel(args.Arg, level);
				}
			}

			if (this.fusion != null)
			{
				this.UpdateFusionAudioFeedback();
			}
		}

		private void AppServiceAudioOutputMuteHandler(object sender, GenericSingleEventArgs<string> args)
		{
			bool newState = this.appService.QueryAudioOutputMute(args.Arg);
			foreach (var ui in this.uiConnections)
			{
				if (ui is IAudioUserInterface)
				{
					(ui as IAudioUserInterface).UpdateAudioOutputMute(args.Arg, newState);
				}
			}

			if (this.fusion != null)
			{
				this.UpdateFusionAudioFeedback();
			}
		}

		private void AppServiceAudioOutputRouteHandler(object sender, GenericSingleEventArgs<string> args)
		{
			string sourceId = this.appService.QueryAudioOutputRoute(args.Arg);
			foreach (var ui in this.uiConnections)
			{
				if (ui is IAudioUserInterface)
				{
					(ui as IAudioUserInterface).UpdateAudioOutputRoute(sourceId, args.Arg);
				}
			}
		}

		private void AppServiceAudioZoneEnableHandler(object sender, GenericDualEventArgs<string, string> args)
		{
			bool newState = appService.QueryAudioZoneState(args.Arg1, args.Arg2);
			foreach (var ui in this.uiConnections)
			{
				var audioUi = (ui as IAudioUserInterface);
				audioUi?.UpdateAudioZoneState(args.Arg1, args.Arg2, newState);
			}
		}

		private void AppServiceDisplayConnectionHandler(object sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceDisplayConnectionHandler() - {0}, {1}", args.Arg1, args.Arg2);
			if (args.Arg2)
			{
				this.fusion?.ClearOfflineDevice(args.Arg1);
				this.RemoveErrorFromUi(args.Arg1);
			}
			else
			{
				var display = this.appService.GetAllDisplayInfo().First(x => x.Id.Equals(args.Arg1, StringComparison.InvariantCulture));
				this.fusion?.AddOfflineDevice(args.Arg1, display.Label);
				this.AddErrorToUi(args.Arg1, display.Label);
			}
		}

		private void AppServiceDisplayBlankHandler(object sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceDisplayBlankHandler() - {0}, {1}", args.Arg1, args.Arg2);
			foreach (var ui in this.uiConnections)
			{
				if (ui is IDisplayUserInterface)
				{
					(ui as IDisplayUserInterface).UpdateDisplayBlank(args.Arg1, args.Arg2);
				}
			}
		}

		private void AppServiceDisplayFreezeHandler(object sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceDisplayFreezeHandler() - {0}, {1}", args.Arg1, args.Arg2);
			foreach (var ui in this.uiConnections)
			{
				if (ui is IDisplayUserInterface)
				{
					(ui as IDisplayUserInterface).UpdateDisplayFreeze(args.Arg1, args.Arg2);
				}
			}
		}

		private void AppServiceDisplayPowerHandler(object sender, GenericDualEventArgs<string, bool> e)
		{
			Logger.Debug("PresentationService.AppServiceDisplayPowerHandler() - {0}, {1}", e.Arg1, e.Arg2);
			foreach (var ui in this.uiConnections)
			{
				if (ui is IDisplayUserInterface)
				{
					(ui as IDisplayUserInterface).UpdateDisplayPower(e.Arg1, e.Arg2);
				}
			}

			if (this.fusion != null)
			{
				this.UpdateFusionDisplayPowerFeedback();
				if (e.Arg2)
				{
					this.fusion.StartDisplayUse(e.Arg1);
				}
				else
				{
					this.fusion.StopDisplayUse(e.Arg1);
				}
			}
		}

		private void AppServiceDisplayInputChangedHandler(object sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.AppServiceDisplayInputChangedHandler({0})", e.Arg);

			bool isLectern = this.appService.DisplayInputLecternQuery(e.Arg);
			bool isStation = this.appService.DisplayInputStationQuery(e.Arg);

			foreach (var ui in uiConnections)
			{
				if (!(ui is IDisplayUserInterface displayUi))
				{
					continue;
				}

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

		private void AppServiceEnpdpointConnectionHandler(object sender, GenericDualEventArgs<string, bool> args)
		{
			Logger.Debug("PresentationService.AppServiceEnpdpointConnectionHandler() - {0}, {1}", args.Arg1, args.Arg2);
			if (args.Arg2)
			{
				this.fusion.ClearOfflineDevice(args.Arg1);
				this.RemoveErrorFromUi(args.Arg1);
			}
			else
			{
				string label = string.Format("Endpoint {0}", args.Arg1);
				this.fusion.AddOfflineDevice(args.Arg1, label);
				this.AddErrorToUi(args.Arg1, label);
			}
		}

		private void AppServiceEndpointChangedHandler(object sender, GenericDualEventArgs<string, int> args)
		{
			Logger.Debug("PresentationService.AppServiceEnpdpointConnectionHandler() - {0}", args.Arg1, args.Arg2);
		}

		private void AppServiceRouteHandler(object sender, GenericSingleEventArgs<string> args)
		{
			var currentSrc = this.appService.QueryCurrentRoute(args.Arg);
			foreach (var conn in this.uiConnections)
			{
				if (conn is IRoutingUserInterface)
				{
					(conn as IRoutingUserInterface).UpdateAvRoute(currentSrc, args.Arg);
				}
			}

			if (this.fusion != null)
			{
				this.UpdateFusionRoutingFeedback();
			}
		}

		private void AppServiceRouterConnectionHandler(object sender, GenericSingleEventArgs<string> args)
		{
			Logger.Debug("PresentationService.AppServiceRouterConnectionHandler() - {0}", args.Arg);
			bool isOnline = this.appService.QueryRouterConnectionStatus(args.Arg);
			if (isOnline)
			{
				this.RemoveErrorFromUi(args.Arg);
				this.fusion?.ClearOfflineDevice(args.Arg);
			}
			else
			{
				string label = string.Format("Router {0} is offline", args.Arg);
				this.AddErrorToUi(args.Arg, label);
				this.fusion?.AddOfflineDevice(args.Arg, label);
			}
		}

		private void AppServiceLightingConnectionHandler(object sender, GenericDualEventArgs<string, bool> e)
		{
			Logger.Debug("PresentationService.AppServiceLightingConnectionHandler({0}, 1)", e.Arg1, e.Arg2);
			if (e.Arg2)
			{
				this.RemoveErrorFromUi(e.Arg1);
				this.fusion?.ClearOfflineDevice(e.Arg1);
			}
			else
			{
				string label = string.Format("Lighting controller {0}", e.Arg1);
				this.AddErrorToUi(e.Arg1, label);
				this.fusion?.AddOfflineDevice(e.Arg1, label);
			}
		}

		private void AppServiceGlobalFreezeHandler(object sender, EventArgs e)
		{
			bool freezeState = this.appService.QueryGlobalVideoFreeze();
			foreach (var ui in this.uiConnections)
			{
				ui.SetGlobalFreezeState(freezeState);
			}

			this.fusion?.UpdateDisplayFreeze(freezeState);
		}

		private void AppServiceGlobalBlankHandler(object sender, EventArgs e)
		{
			bool blankState = this.appService.QueryGlobalVideoBlank();
			foreach (var ui in this.uiConnections)
			{
				ui.SetGlobalBlankState(blankState);
			}

			this.fusion?.UpdateDisplayBlank(blankState);
		}

		private void AppServiceStateChangeHandler(object sender, EventArgs args)
		{
			bool state = this.appService.CurrentSystemState;
			foreach (var conn in this.uiConnections)
			{
				conn.SetSystemState(state);
				conn.ShowSystemStateChanging(state);
			}

			this.TriggerStateChangeTimer();
			if (this.fusion != null)
			{
				this.fusion.UpdateSystemState(state);

				// If system is powering off then stop recording useage, or start recordingg useage for the currently
				// selected input when powered on.
				if (state)
				{
					this.UpdateFusionRoutingFeedback();
				}
				else
				{
					foreach (var source in this.appService.GetAllAvSources())
					{
						this.fusion.StopDeviceUse(source.Id);
					}
				}
			}
		}

		#endregion

		#region Touchscreen Handlers
		private void UiConnectionHandler(object sender, GenericSingleEventArgs<string> args)
		{
			var found = this.uiConnections.FirstOrDefault(x => x.Id.Equals(args.Arg, StringComparison.InvariantCulture));
			if (found != default(IUserInterface))
			{
				if (!found.IsOnline && !found.IsXpanel && this.fusion != null)
				{
					this.fusion.AddOfflineDevice(found.Id, string.Format("UI {0}", found.Id));
				}
				else
				{
					this.fusion?.ClearOfflineDevice(found.Id);
					found.SetSystemState(this.appService.CurrentSystemState);
					found.SetGlobalBlankState(this.appService.QueryGlobalVideoBlank());
					found.SetGlobalFreezeState(this.appService.QueryGlobalVideoFreeze());

					if (found is IAudioUserInterface audioUi)
					{
						foreach (var inChan in this.appService.GetAudioInputChannels())
						{
							audioUi.UpdateAudioInputLevel(inChan.Id, this.appService.QueryAudioInputLevel(inChan.Id));
							audioUi.UpdateAudioInputMute(inChan.Id, this.appService.QueryAudioInputMute(inChan.Id));
						}

						foreach (var outChan in this.appService.GetAudioOutputChannels())
						{
							audioUi.UpdateAudioOutputLevel(outChan.Id, this.appService.QueryAudioOutputLevel(outChan.Id));
							audioUi.UpdateAudioOutputMute(outChan.Id, this.appService.QueryAudioOutputMute(outChan.Id));
						}
					}

					if (found is ITransportControlUserInterface transportUi)
					{
						transportUi.SetCableBoxData(this.appService.GetAllCableBoxes());
					}

					if (found is ICustomEventUserInterface customEventUi)
					{
						this.UpdateCustomEventUi(customEventUi);
					}

					if (found is ILightingUserInterface lightingUi)
					{
						lightingUi.SetLightingData(this.appService.GetAllLightingDeviceInfo());
					}
				}
			}
		}

		private void UpdateCustomEventUi(ICustomEventUserInterface ui)
		{
			if (this.appService is CustomEventAppService eventServiceApp)
			{
				var events = eventServiceApp.QueryAllCustomEvents();
				foreach (var evt in events)
				{
					ui.UpdateCustomEvent(evt.Id, eventServiceApp.QueryCustomEventState(evt.Id));
				}
			}
		}

		private void UiStatusChangeHandler(object sender, GenericSingleEventArgs<bool> args)
		{
			if (args.Arg)
			{
				this.appService.SetActive();
			}
			else
			{
				this.appService.SetStandby();
			}
		}

		private void UiRouteChangeHandler(object sender, GenericDualEventArgs<string, string> args)
		{
			Logger.Debug("PresentationService.UiRouteChangeHandler() - {0} -> {1}", args.Arg1, args.Arg2);
			if (args.Arg2.Equals("ALL", StringComparison.InvariantCulture))
			{
				this.appService.RouteToAll(args.Arg1);
			}
			else
			{
				this.appService.MakeRoute(args.Arg1, args.Arg2);
			}
		}

		private void UiDisplayPowerHandler(object sender, GenericDualEventArgs<string, bool> e)
		{
			Logger.Debug("PresentationService.UiDisplayPowerHandler() - {0} - {1}", e.Arg1, e.Arg2);
			this.appService.SetDisplayPower(e.Arg1, e.Arg2);
		}

		private void UiDisplayFreezeHandler(object sender, GenericSingleEventArgs<string> e)
		{
			bool currenState = this.appService.DisplayFreezeQuery(e.Arg);
			this.appService.SetDisplayFreeze(e.Arg, !currenState);
		}

		private void UiDisplayBlankHandler(object sender, GenericSingleEventArgs<string> e)
		{
			bool currentState = this.appService.DisplayBlankQuery(e.Arg);
			this.appService.SetDisplayBlank(e.Arg, !currentState);
		}

		private void UiGlobalBlankHandler(object sender, EventArgs e)
		{
			Logger.Debug("PresentationService.UiGlobalBlankHandler()");
			bool currentState = this.appService.QueryGlobalVideoBlank();
			this.appService.SetGlobalVideoBlank(!currentState);
		}

		private void UiGlobalFreezeHandler(object sender, EventArgs e)
		{
			Logger.Debug("PresentationService.UiGLobalFreezeHandler()");
			bool currentState = this.appService.QueryGlobalVideoFreeze();
			this.appService.SetGlobalVideoFreeze(!currentState);
		}

		private void UiDisplayScreenUpHandler(object sender, GenericSingleEventArgs<string> e)
		{
			this.appService.RaiseScreen(e.Arg);
		}

		private void UiDisplayScreenDownHandler(object sender, GenericSingleEventArgs<string> e)
		{
			this.appService.LowerScreen(e.Arg);
		}

		private void UiAudioOutputMuteRequest(object sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputMuteRequest() - {0}", e.Arg);
			bool current = this.appService.QueryAudioOutputMute(e.Arg);
			this.appService.SetAudioOutputMute(e.Arg, !current);
		}

		private void UiAudioOutputLevelUpRequest(object sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputLevelUpRequest() - {0}", e.Arg);
			var newLevel = this.appService.QueryAudioOutputLevel(e.Arg) + 3;
			if (newLevel < 100)
			{
				this.appService.SetAudioOutputLevel(e.Arg, newLevel);
			}
			else
			{
				this.appService.SetAudioOutputLevel(e.Arg, 100);
			}
		}

		private void UiAudioOutputLevelDownRequest(object sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioOutputLevelDownRequest() - {0}", e.Arg);
			var newLevel = this.appService.QueryAudioOutputLevel(e.Arg) - 3;
			if (newLevel > 0)
			{
				this.appService.SetAudioOutputLevel(e.Arg, newLevel);
			}
			else
			{
				this.appService.SetAudioOutputLevel(e.Arg, 0);
			}
		}

		private void UiAudioOutputRouteRequest(object sender, GenericDualEventArgs<string, string> e)
		{
			this.appService.SetAudioOutputRoute(e.Arg1, e.Arg2);
		}

		private void UiAudioInputMuteRequest(object sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputMuteRequest() - {0}", e.Arg);
			bool current = this.appService.QueryAudioInputMute(e.Arg);
			this.appService.SetAudioInputMute(e.Arg, !current);
		}

		private void UiAudioInputLevelDownRequest(object sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputLevelDownRequest() - {0}", e.Arg);
			int newLevel = this.appService.QueryAudioInputLevel(e.Arg) - 3;
			if (newLevel > 0)
			{
				this.appService.SetAudioInputLevel(e.Arg, newLevel);
			}
			else
			{
				this.appService.SetAudioInputLevel(e.Arg, 0);
			}
		}

		private void UiAudioInputLevelUpRequest(object sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.UiAudioInputLevelUpRequest() - {0}", e.Arg);
			int newLevel = this.appService.QueryAudioInputLevel(e.Arg) + 3;
			if (newLevel < 100)
			{
				this.appService.SetAudioInputLevel(e.Arg, newLevel);
			}
			else
			{
				this.appService.SetAudioInputLevel(e.Arg, 100);
			}
		}

		private void UiAudioZoneToggleHandler(object sender, GenericDualEventArgs<string, string> e)
		{
			appService.ToggleAudioZoneState(e.Arg1, e.Arg2);
		}

		private void DiscreteAudioOutputUiHandler(object sender, GenericDualEventArgs<string, int> e)
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

		private void DiscreteAudioInputUiHandler(object sender, GenericDualEventArgs<string, int> e)
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

		private void LightingUILoadHandler(object sender, GenericTrippleEventArgs<string, string, int> e)
		{
			this.appService.SetLightingLoad(e.Arg1, e.Arg2, e.Arg3);
		}

		private void LightingUISceneHandler(object sender, GenericDualEventArgs<string, string> e)
		{
			this.appService.RecallLightingScene(e.Arg1, e.Arg2);
		}

		private void AddErrorToUi(string id, string label)
		{
			foreach (var ui in this.uiConnections)
			{
				if (ui is IErrorInterface errorUi)
				{
					errorUi.AddDeviceError(id, label);
				}
			}
		}

		private void RemoveErrorFromUi(string id)
		{
			foreach (var ui in this.uiConnections)
			{
				if (ui is IErrorInterface)
				{
					(ui as IErrorInterface).ClearDeviceError(id);
				}
			}
		}

		private void UISetStationLecternHandler(object sender, GenericSingleEventArgs<string> e)
		{
			this.appService.SetInputLectern(e.Arg);
		}

		private void UiSetStationLocalHandler(object sender, GenericSingleEventArgs<string> e)
		{
			this.appService.SetInputStation(e.Arg);
		}

		private void UiTransportDialHandler(object sender, GenericDualEventArgs<string, string> args)
		{
			this.appService.TransportDial(args.Arg1, args.Arg2);
		}

		private void TransportUi_TransportDialRequest(object sender, GenericDualEventArgs<string, string> e)
		{
			this.appService.TransportDial(e.Arg1, e.Arg2);
		}

		private void TransportUi_TransportDialFavoriteRequest(object sender, GenericDualEventArgs<string, string> e)
		{
			this.appService.TransportDialFavorite(e.Arg1, e.Arg2);
		}

		private void TransportUi_TransportControlRequest(object sender, GenericDualEventArgs<string, TransportTypes> e)
		{
			TransportUtilities.SendCommand(this.appService, e.Arg1, e.Arg2);
		}

		private void EventUi_CustomEventStateChanged(object sender, GenericDualEventArgs<string, bool> e)
		{
			if (this.appService is ICustomEventAppService eventApp)
			{
				eventApp.ChangeCustomEventState(e.Arg1, e.Arg2);
			}
		}
		#endregion

		#region Fusion Handlers
		private void FusionRouteSourceHandler(object sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.FusionRouteSourceHandler()");
			this.appService.RouteToAll(e.Arg);
		}

		private void FusionAudioLevelHandler(object sender, GenericSingleEventArgs<uint> e)
		{
			Logger.Debug("PresentationService.FusionAudioLevelHandler()");
			var pgmOut = this.appService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmOut != default(InfoContainer))
			{
				this.appService.SetAudioOutputLevel(pgmOut.Id, (int)e.Arg);
			}
		}

		private void FusionAudioMuteHandler(object sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionAudioMuteHandler()");
			var pgmOut = this.appService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmOut != default(InfoContainer))
			{
				this.appService.SetAudioOutputMute(pgmOut.Id, !this.appService.QueryAudioOutputMute(pgmOut.Id));
			}
		}

		private void FusionDisplayFreezeHandler(object sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionDisplayFreezeHandler()");
			this.appService.SetGlobalVideoFreeze(!this.appService.QueryGlobalVideoFreeze());
		}

		private void FusionDisplayBlankHandler(object sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionDisplayBlankHandler()");
			this.appService.SetGlobalVideoBlank(!this.appService.QueryGlobalVideoFreeze());
		}

		private void FusionDisplayPowerHandler(object sender, GenericSingleEventArgs<bool> e)
		{
			Logger.Debug("PresentationService.FusionDisplayPowerHandler()");
			foreach (var display in this.appService.GetAllDisplayInfo())
			{
				this.appService.SetDisplayPower(display.Id, e.Arg);
			}
		}

		private void FusionPowerHandler(object sender, GenericSingleEventArgs<bool> e)
		{
			Logger.Debug("PresentationService.FusionPowerHandler()");
			if (e.Arg)
			{
				this.appService.SetActive();
			}
			else
			{
				this.appService.SetStandby();
			}
		}

		private void FusionMicMuteHandler(object sender, GenericSingleEventArgs<string> e)
		{
			Logger.Debug("PresentationService.FusionMicMuteHandler()");
			bool currentState = this.appService.QueryAudioInputMute(e.Arg);
			this.appService.SetAudioInputMute(e.Arg, !currentState);
		}

		private void FusionConnectionHandler(object sender, EventArgs e)
		{
			Logger.Debug("PresentationService.FusionConnectionHandler()");
			this.UpdateFusionFeedback();
		}

		private void UpdateFusionDisplayPowerFeedback()
		{
			bool aDisplayOn = false;
			foreach (var display in this.appService.GetAllDisplayInfo())
			{
				bool power = this.appService.DisplayPowerQuery(display.Id);

				if (power)
				{
					aDisplayOn = power;
					break;
				}
			}

			this.fusion.UpdateDisplayPower(aDisplayOn);
		}

		private void UpdateFusionAudioFeedback()
		{
			var pgmAudio = this.appService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
			if (pgmAudio != default(InfoContainer))
			{
				this.fusion.UpdateProgramAudioLevel((uint)this.appService.QueryAudioOutputLevel(pgmAudio.Id));
				this.fusion.UpdateProgramAudioMute(this.appService.QueryAudioOutputMute(pgmAudio.Id));
			}
		}

		private void UpdateFusionRoutingFeedback()
		{
			var AvDests = this.appService.GetAllAvDestinations();
			if (AvDests.Count > 0)
			{
				var currentRoute = this.appService.QueryCurrentRoute(AvDests[0].Id);
				this.fusion.UpdateSelectedSource(currentRoute.Id);
				foreach (var source in this.appService.GetAllAvSources())
				{
					if (source.Id.Equals(currentRoute.Id, StringComparison.InvariantCulture))
					{
						this.fusion.StartDeviceUse(source.Id);
					}
					else
					{
						this.fusion.StopDeviceUse(source.Id);
					}
				}
			}
		}

		private void UpdateFusionFeedback()
		{
			this.fusion.UpdateSystemState(this.appService.CurrentSystemState);
			this.UpdateFusionDisplayPowerFeedback();
			this.UpdateFusionAudioFeedback();
			this.UpdateFusionRoutingFeedback();
			this.fusion.UpdateDisplayFreeze(this.appService.QueryGlobalVideoFreeze());
			this.fusion.UpdateDisplayBlank(this.appService.QueryGlobalVideoBlank());
		}
		#endregion
	}
}
