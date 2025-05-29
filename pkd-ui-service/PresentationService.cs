// ReSharper disable SuspiciousTypeConversion.Global

using Crestron.SimplSharpPro;
using pkd_application_service;
using pkd_application_service.CameraControl;
using pkd_application_service.VideoWallControl;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_ui_service.Fusion;
using pkd_ui_service.Interfaces;

// disabled because this class is meant to be extended
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
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
        /// true = object is disposed, false = not disposed.
        /// </summary>
        protected bool Disposed;

        /// <param name="appService">The framework application implementation that handles state management.</param>
        /// <param name="control">the root Crestron control system object.</param>
        public PresentationService(IApplicationService appService, CrestronControlSystem control)
        {
            ParameterValidator.ThrowIfNull(appService, "Ctor", nameof(appService));
            ParameterValidator.ThrowIfNull(control, "Ctor", nameof(control));

            AppService = appService;
            Control = control;
            UiConnections = [];
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
        public virtual void Initialize()
        {
            BuildInterfaces();
            SubscribeToAppService();
            foreach (var uiDevice in UiConnections)
            {
                uiDevice.Connect();
            }

            Fusion?.Initialize();
        }

        /// <summary>
        /// disposes of all internal component objects if they are disposable.
        /// </summary>
        protected virtual void Dispose(bool disposing)
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
        protected virtual void BuildInterfaces()
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
        protected virtual void SubscribeToAppService()
        {
            AppService.AudioDspConnectionStatusChanged += AppServiceDspConnectionHandler;
            AppService.AudioInputMuteChanged += AppServiceAudioInputMuteHandler;
            AppService.AudioOutputLevelChanged += AppServiceAudioOutputLevelHandler;
            AppService.AudioOutputMuteChanged += AppServiceAudioOutputMuteHandler;
            AppService.DisplayConnectChange += AppServiceDisplayConnectionHandler;
            AppService.DisplayPowerChange += AppServiceDisplayPowerHandler;
            AppService.EndpointConnectionChanged += AppServiceEndpointConnectionHandler;
            AppService.RouteChanged += AppServiceRouteHandler;
            AppService.RouterConnectChange += AppServiceRouterConnectionHandler;
            AppService.SystemStateChanged += AppServiceStateChangeHandler;
            AppService.GlobalVideoBlankChanged += AppServiceGlobalBlankHandler;
            AppService.GlobalVideoFreezeChanged += AppServiceGlobalFreezeHandler;
            AppService.LightingControlConnectionChanged += AppServiceLightingConnectionHandler;

            if (AppService is IVideoWallApp videoWallApp)
            {
                videoWallApp.VideoWallConnectionStatusChanged += VideoWallAppConnectionChangeHandler;
            }

            if (AppService is ICameraControlApp cameraApp)
            {
                cameraApp.CameraControlConnectionChanged += CameraAppConnectionChangeHandler;
            }
        }

        /// <summary>
        /// Unsubscribe from all application services that were subscribed through <see cref="SubscribeToAppService"/>.
        /// </summary>
        protected virtual void UnsubscribeFromAppService()
        {
            AppService.AudioDspConnectionStatusChanged -= AppServiceDspConnectionHandler;
            AppService.AudioInputMuteChanged -= AppServiceAudioInputMuteHandler;
            AppService.AudioOutputLevelChanged -= AppServiceAudioOutputLevelHandler;
            AppService.AudioOutputMuteChanged -= AppServiceAudioOutputMuteHandler;
            AppService.DisplayConnectChange -= AppServiceDisplayConnectionHandler;
            AppService.EndpointConnectionChanged -= AppServiceEndpointConnectionHandler;
            AppService.RouteChanged -= AppServiceRouteHandler;
            AppService.RouterConnectChange -= AppServiceRouterConnectionHandler;
            AppService.SystemStateChanged -= AppServiceStateChangeHandler;
            AppService.LightingControlConnectionChanged -= AppServiceLightingConnectionHandler;
            if (AppService is IVideoWallApp videoWallApp)
            {
                videoWallApp.VideoWallConnectionStatusChanged -= VideoWallAppConnectionChangeHandler;
            }

            if (AppService is ICameraControlApp cameraApp)
            {
                cameraApp.CameraControlConnectionChanged -= CameraAppConnectionChangeHandler;
            }
        }

        /// <summary>
        /// Subscribe to all user interface events for all implemented plugin interfaces.
        /// </summary>
        /// <param name="ui"></param>
        protected virtual void SubscribeToInterface(IUserInterface ui)
        {
            ui.OnlineStatusChanged += UiConnectionHandler;
        }

        /// <summary>
        /// Unsubscribe from all events handlers added by <see cref="BuildInterfaces"/>.
        /// </summary>
        protected virtual void UnsubscribeFromInterfaces()
        {
            foreach (var ui in UiConnections)
            {
                ui.OnlineStatusChanged -= UiConnectionHandler;
            }
        }
        
        #region AppService Handlers

        /// <summary>
        /// Handle camera connection change notifications from the application service.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">args.Arg = the id of the camera that updated.</param>
        protected virtual void CameraAppConnectionChangeHandler(object? sender, GenericSingleEventArgs<string> args)
        {
            if (sender is not ICameraControlApp cameraApp) return;
            var newState = cameraApp.QueryCameraConnectionStatus(args.Arg);
            if (newState)
            {
                Fusion?.ClearOfflineDevice(args.Arg);
            }
            else
            {
                var found = cameraApp.GetAllCameraDeviceInfo().First(x => x.Id.Equals(args.Arg));
                Fusion?.AddOfflineDevice(args.Arg, found.Label);
            }
        }


        /// <summary>
        /// Handle video wall device connection status notifications from the application service.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">args.Arg = the id of the video wall that updated.</param>
        protected virtual void VideoWallAppConnectionChangeHandler(object? sender, GenericSingleEventArgs<string> args)
        {
            if (sender is not IVideoWallApp videoWallApp) return;
            var wall = videoWallApp.GetAllVideoWalls().FirstOrDefault(x => x.Id.Equals(args.Arg));
            if (wall == null) return;
            if (wall.IsOnline)
                Fusion?.ClearOfflineDevice(args.Arg);
            else
                Fusion?.AddOfflineDevice(args.Arg, $"{wall.Model} ({wall.Id})");
        }

        /// <summary>
        /// Handle notifications from the application service about an audio DSP changing connection status.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">Arg = the id of the DSP that changed.</param>
        protected virtual void AppServiceDspConnectionHandler(object? sender, GenericSingleEventArgs<string> args)
        {
            var isOnline = AppService.QueryAudioDspConnectionStatus(args.Arg);
            if (isOnline)
            {
                Fusion?.ClearOfflineDevice(args.Arg);
            }
            else
            {
                var found = AppService.GetAllAudioDspDevices()
                    .First(x => x.Id.Equals(args.Arg, StringComparison.InvariantCulture));
                Fusion?.AddOfflineDevice(args.Arg, $"{found.Model} {found.Label}");
            }
        }

        /// <summary>
        /// Handle notifications from the application service about mute state changes on inputs.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">Arg = the id of the audio channel that changed.</param>
        protected virtual void AppServiceAudioInputMuteHandler(object? sender, GenericSingleEventArgs<string> args)
        {
            var newState = AppService.QueryAudioInputMute(args.Arg);
            Fusion?.UpdateMicMute(args.Arg, newState);
        }

        /// <summary>
        /// Handle notifications from the application service about volume level changes on outputs.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">Arg = the id of the audio channel that changed.</param>
        protected virtual void AppServiceAudioOutputLevelHandler(object? sender, GenericSingleEventArgs<string> args)
        {
            UpdateFusionAudioFeedback();
        }

        /// <summary>
        /// Handle notifications from the application service about mute state changes on outputs.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">Arg = the id of the audio channel that changed.</param>
        protected virtual void AppServiceAudioOutputMuteHandler(object? sender, GenericSingleEventArgs<string> args)
        {
            UpdateFusionAudioFeedback();
        }

        /// <summary>
        /// Handle notifications from the application service about video display/projector connection status changes.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">Arg1 = the id of the display that changed. Arg2 = true is online, false is offline.</param>
        protected virtual void AppServiceDisplayConnectionHandler(object? sender,
            GenericDualEventArgs<string, bool> args)
        {
            var display = AppService.GetAllDisplayInfo()
                .FirstOrDefault(x => x.Id.Equals(args.Arg1, StringComparison.InvariantCulture));
            if (display == null) return;
            if (args.Arg2)
            {
                Fusion?.ClearOfflineDevice(args.Arg1);
            }
            else
            {
                Fusion?.AddOfflineDevice(args.Arg1, $"{display.Model} {display.Label}");
            }
        }

        /// <summary>
        /// Handle notifications from the application service about video display/projector power status changes.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Arg1 = the id of the display that changed. Arg2 = true is on, false is off.</param>
        protected virtual void AppServiceDisplayPowerHandler(object? sender, GenericDualEventArgs<string, bool> e)
        {
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
        /// Handle notifications from the application service about relay or control expander endpoint connection status changes.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">Arg1 = the id of the endpoint device that changed. Arg2 = true is online, false is offline.</param>
        protected virtual void AppServiceEndpointConnectionHandler(object? sender,
            GenericDualEventArgs<string, bool> args)
        {
            if (args.Arg2)
            {
                Fusion?.ClearOfflineDevice(args.Arg1);
            }
            else
            {
                var label = $"Endpoint {args.Arg1}";
                Fusion?.AddOfflineDevice(args.Arg1, label);
            }
        }

        /// <summary>
        /// Handle notifications from the application service about video routing events.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">Arg = the id of the destination that changed.</param>
        protected virtual void AppServiceRouteHandler(object? sender, GenericSingleEventArgs<string> args)
        {
            UpdateFusionRoutingFeedback();
        }

        /// <summary>
        /// Handle notifications from the application service about AVR connection status changes.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">Arg = the id of the AVR that updated.</param>
        protected virtual void AppServiceRouterConnectionHandler(object? sender, GenericSingleEventArgs<string> args)
        {
            var avr = AppService.GetAllAvRouters().FirstOrDefault(x => x.Id.Equals(args.Arg));
            if (avr == null) return;
            if (avr.IsOnline)
            {
                Fusion?.ClearOfflineDevice(args.Arg);
            }
            else
            {
                var label = $"{avr.Model} ({args.Arg}) is offline";
                Fusion?.AddOfflineDevice(args.Arg, label);
            }
        }

        /// <summary>
        /// Handle notifications from the application service about lighting controller connection status changes.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Arg = the id of the lighting controller that updated.</param>
        protected virtual void AppServiceLightingConnectionHandler(object? sender, GenericDualEventArgs<string, bool> e)
        {
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
        protected virtual void AppServiceGlobalFreezeHandler(object? sender, EventArgs e)
        {
            var freezeState = AppService.QueryGlobalVideoFreeze();
            Fusion?.UpdateDisplayFreeze(freezeState);
        }

        /// <summary>
        /// Handle notifications from the application service about global/AVR video blank events.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Generic empty event args.</param>
        protected virtual void AppServiceGlobalBlankHandler(object? sender, EventArgs e)
        {
            var blankState = AppService.QueryGlobalVideoBlank();
            Fusion?.UpdateDisplayBlank(blankState);
        }

        /// <summary>
        /// Handle notifications from the application service about global/AVR video blank events.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">Generic empty event args.</param>
        protected virtual void AppServiceStateChangeHandler(object? sender, EventArgs args)
        {
            var state = AppService.CurrentSystemState;
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
        
        /// <summary>
        /// Handle user interface connection changes.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="args">Arg1 = The id of the user interface that changed.</param>
        protected virtual void UiConnectionHandler(object? sender, GenericSingleEventArgs<string> args)
        {
            var found = UiConnections.FirstOrDefault(x => x.Id.Equals(args.Arg, StringComparison.InvariantCulture));
            if (found == null) return;

            if (found is { IsOnline: false, IsXpanel: false })
            {
                Fusion?.AddOfflineDevice(found.Id, $"UI {found.Id}");
                foreach (var ui in UiConnections)
                {
                    if (ui is not IErrorInterface errUi) continue;
                    var device = AppService.GetAllUserInterfaces().First(x => x.Id == found.Id);
                    errUi.AddDeviceError(args.Arg, $"{device.Model} {args.Arg} is offline");
                }
            }
            else
            {
                Fusion?.ClearOfflineDevice(found.Id);
                foreach (var ui in UiConnections)
                {
                    if (ui is not IErrorInterface errUi) continue;
                    errUi.ClearDeviceError(args.Arg);
                }
            }
        }

        #region Fusion Handlers

        /// <summary>
        /// Handle route requests from a <see cref="IFusionInterface"/> ui.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Arg = the id of the source to route to all video destinations</param>
        protected virtual void FusionRouteSourceHandler(object? sender, GenericSingleEventArgs<string> e)
        {
            Logger.Debug("PresentationService.FusionRouteSourceHandler()");
            AppService.RouteToAll(e.Arg);
        }

        /// <summary>
        /// Handle route requests from a <see cref="IFusionInterface"/> ui.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Arg = the id of the source to route to all video destinations</param>
        protected virtual void FusionAudioLevelHandler(object? sender, GenericSingleEventArgs<uint> e)
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
        protected virtual void FusionAudioMuteHandler(object? sender, EventArgs e)
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
        protected virtual void FusionDisplayFreezeHandler(object? sender, EventArgs e)
        {
            Logger.Debug("PresentationService.FusionDisplayFreezeHandler()");
            AppService.SetGlobalVideoFreeze(!AppService.QueryGlobalVideoFreeze());
        }

        /// <summary>
        /// Handle global video blank change requests from a <see cref="IFusionInterface"/> ui.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Empty args package.</param>
        protected virtual void FusionDisplayBlankHandler(object? sender, EventArgs e)
        {
            Logger.Debug("PresentationService.FusionDisplayBlankHandler()");
            AppService.SetGlobalVideoBlank(!AppService.QueryGlobalVideoFreeze());
        }

        /// <summary>
        /// Handle display power change requests from a <see cref="IFusionInterface"/> ui.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Arg = true for on, false for off.</param>
        protected virtual void FusionDisplayPowerHandler(object? sender, GenericSingleEventArgs<bool> e)
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
        protected virtual void FusionPowerHandler(object? sender, GenericSingleEventArgs<bool> e)
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
        protected virtual void FusionMicMuteHandler(object? sender, GenericSingleEventArgs<string> e)
        {
            Logger.Debug("PresentationService.FusionMicMuteHandler()");
            bool currentState = AppService.QueryAudioInputMute(e.Arg);
            AppService.SetAudioInputMute(e.Arg, !currentState);
        }

        /// <summary>
        /// Handle <see cref="IFusionInterface"/> device online/offline state change events.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="e">Empty args package.</param>
        protected virtual void FusionConnectionHandler(object? sender, EventArgs e)
        {
            Logger.Debug("PresentationService.FusionConnectionHandler()");
            UpdateFusionFeedback();
        }

        /// <summary>
        /// update all <see cref="IFusionInterface"/> devices with the current system use state.
        /// </summary>
        protected virtual void UpdateFusionDisplayPowerFeedback()
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
        protected virtual void UpdateFusionAudioFeedback()
        {
            var pgmAudio = AppService.GetAudioOutputChannels().FirstOrDefault(x => x.Tags.Contains("pgm"));
            if (pgmAudio == null) return;
            Fusion?.UpdateProgramAudioLevel((uint)AppService.QueryAudioOutputLevel(pgmAudio.Id));
            Fusion?.UpdateProgramAudioMute(AppService.QueryAudioOutputMute(pgmAudio.Id));
        }

        /// <summary>
        /// Update all <see cref="IFusionInterface"/> devices with the current video route status.
        /// </summary>
        protected virtual void UpdateFusionRoutingFeedback()
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
        protected virtual void UpdateFusionFeedback()
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