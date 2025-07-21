// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable SuspiciousTypeConversion.Global

using System.Collections.ObjectModel;
using pkd_application_service.AudioControl;
using pkd_application_service.AvRouting;
using pkd_application_service.Base;
using pkd_application_service.DisplayControl;
using pkd_application_service.EndpointControl;
using pkd_application_service.LightingControl;
using pkd_application_service.SystemPower;
using pkd_application_service.TransportControl;
using pkd_application_service.UserInterface;
using pkd_common_utils.GenericEventArgs;
using pkd_domain_service;
using pkd_hardware_service;
using pkd_hardware_service.DisplayDevices;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
namespace pkd_application_service
{
	/// <summary>
	/// Class for controlling interactions between the external control interfaces and the hardware service.
	/// A plugin can use this class to override functionality for a custom application service without needing to completely
	/// rewrite the IApplicationService implementation.
	/// </summary>
	public class ApplicationService : IApplicationService, IDisposable
	{
		/// <summary>
		/// Collection of objects that need to be disposed when this object is disposed.
		/// </summary>
		protected readonly List<IDisposable> Disposables = [];
		
		/// <summary>
		/// Collection of all user interfaces defined in the configuration.
		/// </summary>
		protected List<UserInterfaceDataContainer> InterfaceData;
		
		/// <summary>
		/// internal control object for managing display control state.
		/// </summary>
		protected IDisplayControlApp DisplayControl;
		
		/// <summary>
		/// Internal control object for managing system power state and requests
		/// </summary>
		protected ISystemPowerApp SystemPowerControl;
		
		/// <summary>
		/// Internal control object for managing relay endpoint control requests.
		/// </summary>
		protected IEndpointControlApp EndpointControl;
		
		/// <summary>
		/// The hardware device interface manager part of the AV framework.
		/// </summary>
		protected IInfrastructureService HwService;
		
		/// <summary>
		/// The system configuration representation of the AV Framework.
		/// </summary>
		protected IDomainService Domain;
		
		/// <summary>
		/// Internal control object for managing audio state and requests.
		/// </summary>
		protected IAudioControlApp AudioControl;
		
		/// <summary>
		/// Internal control object for managing video routing state and requests.
		/// </summary>
		protected IAvRoutingApp RoutingControl;
		
		/// <summary>
		/// Internal control object for managing dvd, cable tv, and other transport-based requests.
		/// </summary>
		protected ITransportControlApp TransportControl;
		
		/// <summary>
		/// Internal control object for managing all lighting states and requests.
		/// </summary>
		protected ILightingControlApp LightingControl;
		
		/// <summary>
		/// Flag to indicate whether to use an AV Router in the configuration for video mute and freeze or if these controls
		/// should be sent to the individual displays.
		/// </summary>
		protected bool UseAvrMuteFreeze;
		private bool _disposed;

		/// <summary>
		/// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
		/// </summary>
		~ApplicationService()
		{
			Dispose(false);
		}

		/// <inheritdoc/>
		public virtual void Initialize(IInfrastructureService hwService, IDomainService domain)
		{
			Domain = domain;
			HwService = hwService;

			SystemPowerControl = ApplicationControlFactory.CreateSystemPower(hwService, domain, this);
			if (SystemPowerControl is IDisposable disposablePower)
			{
				Disposables.Add(disposablePower);
			}

			DisplayControl = ApplicationControlFactory.CreateDisplayControl(hwService, domain, this);
			if (DisplayControl is IDisposable disposableDisplay)
			{
				Disposables.Add(disposableDisplay);
			}

			EndpointControl = ApplicationControlFactory.CreateEndpointControl(hwService, domain, this);
			if (EndpointControl is IDisposable disposableEndpoint)
			{
				Disposables.Add(disposableEndpoint);
			}

			AudioControl = ApplicationControlFactory.CreateAudioControl(hwService, domain, this);
			if (AudioControl is IDisposable disposableAudio)
			{
				Disposables.Add(disposableAudio);
			}

			RoutingControl = ApplicationControlFactory.CreateRoutingControl(hwService, domain, this);
			if (RoutingControl is IDisposable disposableRouting)
			{
				Disposables.Add(disposableRouting);
			}

			TransportControl = ApplicationControlFactory.CreateTransportControl(hwService, domain, this);
			if (TransportControl is IDisposable disposableTransport)
			{
				Disposables.Add(disposableTransport);
			}

			LightingControl = ApplicationControlFactory.CreateLightingControl(hwService, domain, this);
			if (LightingControl is IDisposable disposableLighting)
			{
				Disposables.Add(disposableLighting);
			}

			InterfaceData = ApplicationControlFactory.CreateUserInterfaceData(domain);
			SubscribeEvents();
		}

		/// <inheritdoc/>
		public event EventHandler? SystemStateChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>>? DisplayPowerChange;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>>? DisplayBlankChange;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>>? DisplayFreezeChange;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>>? DisplayConnectChange;

		/// <inheritdoc />
		public event EventHandler<GenericSingleEventArgs<string>>? DisplayInputChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, int>>? EndpointRelayChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>>? EndpointConnectionChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? RouteChanged;
		
		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? VideoInputSyncChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioOutputRouteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? RouterConnectChange;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioOutputLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioOutputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioInputLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioInputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>>? AudioZoneEnableChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioDspConnectionStatusChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>>? LightingLoadLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? LightingSceneChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>>? LightingControlConnectionChanged;

		/// <inheritdoc/>
		public event EventHandler? GlobalVideoBlankChanged;

		/// <inheritdoc/>
		public event EventHandler? GlobalVideoFreezeChanged;

		/// <inheritdoc/>
		public bool CurrentSystemState => SystemPowerControl.CurrentSystemState;

		/// <inheritdoc/>
		public bool AutoShutdownEnabled => SystemPowerControl.AutoShutdownEnabled;

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<UserInterfaceDataContainer> GetAllUserInterfaces()
		{
			return new ReadOnlyCollection<UserInterfaceDataContainer>(InterfaceData);
		}

		/// <inheritdoc/>
		public virtual UserInterfaceDataContainer GetFusionInterface()
		{
			return new UserInterfaceDataContainer(
				Domain.Fusion.GUID,
				Domain.Fusion.RoomName,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				Domain.Fusion.IpId,
				[]);
		}

		/// <inheritdoc/>
		public virtual RoomInfoContainer GetRoomInfo()
		{
			return new RoomInfoContainer(
				Domain.RoomInfo.Id,
				Domain.RoomInfo.RoomName,
				Domain.RoomInfo.HelpContact,
				Domain.RoomInfo.SystemType
			)
			{
				PresentationServiceClass = Domain.RoomInfo.Logic.PresentationServiceClass,
				PresentationServiceLibrary = Domain.RoomInfo.Logic.PresentationServiceLibrary,
			};
		}

		/// <inheritdoc/>
		public virtual void SetActive() => SystemPowerControl.SetActive();

		/// <inheritdoc/>
		public virtual void SetStandby() => SystemPowerControl.SetStandby();

		/// <inheritdoc/>
		public virtual void AutoShutdownEnable() => SystemPowerControl.AutoShutdownEnable();

		/// <inheritdoc/>
		public virtual void AutoShutdownDisable() => SystemPowerControl.AutoShutdownDisable();

		/// <inheritdoc/>
		public virtual void SetAutoShutdownTime(int hour, int minute) => SystemPowerControl.SetAutoShutdownTime(hour, minute);

		/// <inheritdoc/>
		public virtual void SetDisplayPower(string id, bool newState) => DisplayControl.SetDisplayPower(id, newState);

		/// <inheritdoc/>
		public virtual bool DisplayPowerQuery(string id) => DisplayControl.DisplayPowerQuery(id);

		/// <inheritdoc/>
		public virtual void SetDisplayBlank(string id, bool newState) => DisplayControl.SetDisplayBlank(id, newState);

		/// <inheritdoc/>
		public virtual bool DisplayBlankQuery(string id) => DisplayControl.DisplayBlankQuery(id);

		/// <inheritdoc/>
		public virtual void SetDisplayFreeze(string id, bool state) => DisplayControl.SetDisplayFreeze(id, state);

		/// <inheritdoc/>
		public virtual bool DisplayFreezeQuery(string id) => DisplayControl.DisplayFreezeQuery(id);

		/// <inheritdoc/>
		public virtual void RaiseScreen(string displayId) => DisplayControl.RaiseScreen(displayId);

		/// <inheritdoc/>
		public virtual void LowerScreen(string displayId) => DisplayControl.LowerScreen(displayId);

		/// <inheritdoc/>
		public virtual void SetInputLectern(string displayId) => DisplayControl.SetInputLectern(displayId);

		/// <inheritdoc/>
		public virtual void SetInputStation(string displayId) => DisplayControl.SetInputStation(displayId);

		/// <inheritdoc/>
		public virtual bool DisplayInputLecternQuery(string displayId) => DisplayControl.DisplayInputLecternQuery(displayId);

		/// <inheritdoc/>
		public virtual bool DisplayInputStationQuery(string displayId) => DisplayControl.DisplayInputStationQuery(displayId);

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<DisplayInfoContainer> GetAllDisplayInfo() => DisplayControl.GetAllDisplayInfo();

		/// <inheritdoc/>
		public virtual void PulseEndpointRelay(string id, int index, int timeMs) => EndpointControl.PulseEndpointRelay(id, index, timeMs);

		/// <inheritdoc/>
		public virtual void LatchRelayClosed(string id, int index) => EndpointControl.LatchRelayClosed(id, index);

		/// <inheritdoc/>
		public virtual void LatchRelayOpen(string id, int index) => EndpointControl.LatchRelayOpen(id, index);

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<AudioChannelInfoContainer> GetAudioInputChannels() => AudioControl.GetAudioInputChannels();

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<AudioChannelInfoContainer> GetAudioOutputChannels() => AudioControl.GetAudioOutputChannels();

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<InfoContainer> GetAllAudioDspDevices() => AudioControl.GetAllAudioDspDevices();

		/// <inheritdoc/>
		public virtual bool QueryAudioDspConnectionStatus(string id) => AudioControl.QueryAudioDspConnectionStatus(id);

		/// <inheritdoc/>
		public virtual int QueryAudioInputLevel(string id) => AudioControl.QueryAudioInputLevel(id);

		/// <inheritdoc/>
		public virtual int QueryAudioOutputLevel(string id) => AudioControl.QueryAudioOutputLevel(id);

		/// <inheritdoc/>
		public virtual bool QueryAudioInputMute(string id) => AudioControl.QueryAudioInputMute(id);

		/// <inheritdoc/>
		public virtual bool QueryAudioOutputMute(string id) => AudioControl.QueryAudioOutputMute(id);

		/// <inheritdoc/>
		public virtual string QueryAudioOutputRoute(string id) => AudioControl.QueryAudioOutputRoute(id);

		/// <inheritdoc/>
		public virtual bool QueryAudioZoneState(string channelId, string zoneId) => AudioControl.QueryAudioZoneState(channelId, zoneId);

		/// <inheritdoc/>
		public virtual void SetAudioInputLevel(string id, int level) => AudioControl.SetAudioInputLevel(id, level);

		/// <inheritdoc/>
		public virtual void SetAudioInputMute(string id, bool mute) => AudioControl.SetAudioInputMute(id, mute);

		/// <inheritdoc/>
		public virtual void SetAudioOutputLevel(string id, int level) => AudioControl.SetAudioOutputLevel(id, level);

		/// <inheritdoc/>
		public virtual void SetAudioOutputMute(string id, bool mute) => AudioControl.SetAudioOutputMute(id, mute);

		/// <inheritdoc/>
		public virtual void SetAudioOutputRoute(string srcId, string destId) => AudioControl.SetAudioOutputRoute(srcId, destId);

		/// <inheritdoc/>
		public virtual void ToggleAudioZoneState(string channelId, string zoneId) => AudioControl.ToggleAudioZoneState(channelId, zoneId);

		/// <inheritdoc/>
		public void SetAudioZoneState(string channelId, string zoneId, bool state) =>  AudioControl.SetAudioZoneState(channelId, zoneId, state);

		/// <inheritdoc/>
		public virtual bool QueryRouterConnectionStatus(string id) => RoutingControl.QueryRouterConnectionStatus(id);

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<AvSourceInfoContainer> GetAllAvSources() => RoutingControl.GetAllAvSources();

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<InfoContainer> GetAllAvDestinations() => RoutingControl.GetAllAvDestinations();

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<InfoContainer> GetAllAvRouters() => RoutingControl.GetAllAvRouters();

		/// <inheritdoc/>
		public virtual void MakeRoute(string inputId, string outputId) => RoutingControl.MakeRoute(inputId, outputId);

		/// <inheritdoc/>
		public virtual void RouteToAll(string inputId) => RoutingControl.RouteToAll(inputId);

		/// <inheritdoc/>
		public virtual void ReportGraph() => RoutingControl.ReportGraph();

		/// <inheritdoc/>
		public bool QueryVideoInputSyncStatus(string id) => RoutingControl.QueryVideoInputSyncStatus(id);

		/// <inheritdoc/>
		public virtual AvSourceInfoContainer QueryCurrentRoute(string outputId) => RoutingControl.QueryCurrentRoute(outputId);

		/// <inheritdoc/>
		public virtual void SetGlobalVideoFreeze(bool state)
		{
			if (UseAvrMuteFreeze)
			{
				SetAvrVideoFreeze(state);
			}
			else
			{
				foreach (var displayInfo in DisplayControl.GetAllDisplayInfo())
				{
					DisplayControl.SetDisplayFreeze(displayInfo.Id, state);
				}
			}
		}

		/// <inheritdoc/>
		public virtual bool QueryGlobalVideoBlank()
		{
			bool result = false;

			if (UseAvrMuteFreeze)
			{
				foreach (var avr in HwService.AvSwitchers.GetAllDevices())
				{
					if (avr is IVideoControllable videoApp)
					{
						result = videoApp.BlankState;
					}
				}
			}
			else
			{
				foreach (var display in HwService.Displays.GetAllDevices())
				{
					if (display is IVideoControllable videoControllable)
					{
						result = videoControllable.BlankState;
					}
				}
			}

			return result;
		}

		/// <inheritdoc/>
		public virtual bool QueryGlobalVideoFreeze()
		{
			var result = false;
			if (UseAvrMuteFreeze)
			{
				foreach (var avr in HwService.AvSwitchers.GetAllDevices())
				{
					if (avr is IVideoControllable videoControllable)
					{
						result = videoControllable.FreezeState;
					}
				}
			}
			else
			{
				foreach (var display in HwService.Displays.GetAllDevices())
				{
					if (display is IVideoControllable videoControllable)
					{
						result = videoControllable.FreezeState;
					}
				}
			}

			return result;
		}

		/// <inheritdoc/>
		public virtual void SetGlobalVideoBlank(bool state)
		{
			if (UseAvrMuteFreeze)
			{
				SetAvrVideoBlank(state);
			}
			else
			{
				foreach (var displayInfo in DisplayControl.GetAllDisplayInfo())
				{
					DisplayControl.SetDisplayBlank(displayInfo.Id, state);
				}
			}
		}

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<TransportInfoContainer> GetAllCableBoxes()
		{
			return TransportControl.GetAllCableBoxes();
		}

		/// <inheritdoc/>
		public virtual void TransportPowerOn(string id) => TransportControl.TransportPowerOn(id);

		/// <inheritdoc/>
		public virtual void TransportPowerOff(string id) => TransportControl.TransportPowerOff(id);

		/// <inheritdoc/>
		public virtual void TransportPowerToggle(string id) => TransportControl.TransportPowerToggle(id);

		/// <inheritdoc/>
		public virtual void TransportDial(string id, string channel) => TransportControl.TransportDial(id, channel);

		/// <inheritdoc/>
		public virtual void TransportDialFavorite(string id, string favId) => TransportControl.TransportDialFavorite(id, favId);

		/// <inheritdoc/>
		public virtual void TransportDash(string id) => TransportControl.TransportDash(id);

		/// <inheritdoc/>
		public virtual void TransportChannelUp(string id) => TransportControl.TransportChannelUp(id);

		/// <inheritdoc/>
		public virtual void TransportChannelDown(string id) => TransportControl.TransportChannelDown(id);

		/// <inheritdoc/>
		public virtual void TransportPageUp(string id) => TransportControl.TransportPageUp(id);

		/// <inheritdoc/>
		public virtual void TransportPageDown(string id) => TransportControl.TransportPageDown(id);

		/// <inheritdoc/>
		public virtual void TransportGuide(string id) => TransportControl.TransportGuide(id);

		/// <inheritdoc/>
		public virtual void TransportMenu(string id) => TransportControl.TransportMenu(id);

		/// <inheritdoc/>
		public virtual void TransportInfo(string id) => TransportControl.TransportInfo(id);

		/// <inheritdoc/>
		public virtual void TransportExit(string id) => TransportControl.TransportExit(id);

		/// <inheritdoc/>
		public virtual void TransportBack(string id) => TransportControl.TransportBack(id);

		/// <inheritdoc/>
		public virtual void TransportPlay(string id) => TransportControl.TransportPlay(id);

		/// <inheritdoc/>
		public virtual void TransportPause(string id) => TransportControl.TransportPause(id);

		/// <inheritdoc/>
		public virtual void TransportStop(string id) => TransportControl.TransportStop(id);

		/// <inheritdoc/>
		public virtual void TransportRecord(string id) => TransportControl.TransportRecord(id);

		/// <inheritdoc/>
		public virtual void TransportScanForward(string id) => TransportControl.TransportScanForward(id);

		/// <inheritdoc/>
		public virtual void TransportScanReverse(string id) => TransportControl.TransportScanReverse(id);

		/// <inheritdoc/>
		public virtual void TransportSkipForward(string id) => TransportControl.TransportSkipForward(id);

		/// <inheritdoc/>
		public virtual void TransportSelect(string id) => TransportControl.TransportSelect(id);

		/// <inheritdoc/>
		public virtual void TransportSkipReverse(string id) => TransportControl.TransportSkipReverse(id);

		/// <inheritdoc/>
		public virtual void TransportNavUp(string id) => TransportControl.TransportNavUp(id);

		/// <inheritdoc/>
		public virtual void TransportNavDown(string id) => TransportControl.TransportNavDown(id);

		/// <inheritdoc/>
		public virtual void TransportNavLeft(string id) => TransportControl.TransportNavLeft(id);

		/// <inheritdoc/>
		public virtual void TransportNavRight(string id) => TransportControl.TransportNavRight(id);

		/// <inheritdoc/>
		public virtual void TransportRed(string id) => TransportControl.TransportRed(id);

		/// <inheritdoc/>
		public virtual void TransportGreen(string id) => TransportControl.TransportGreen(id);

		/// <inheritdoc/>
		public virtual void TransportYellow(string id) => TransportControl.TransportYellow(id);

		/// <inheritdoc/>
		public virtual void TransportBlue(string id) => TransportControl.TransportBlue(id);

		/// <inheritdoc/>
		public virtual void RecallLightingScene(string deviceId, string sceneId) { LightingControl.RecallLightingScene(deviceId, sceneId); }

		/// <inheritdoc/>
		public virtual void SetLightingLoad(string deviceId, string sceneId, int level) { LightingControl.SetLightingLoad(deviceId, sceneId, level); }

		/// <inheritdoc/>
		public virtual string GetActiveScene(string deviceId) { return LightingControl.GetActiveScene(deviceId); }

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<LightingControlInfoContainer> GetAllLightingDeviceInfo() { return LightingControl.GetAllLightingDeviceInfo(); }

		/// <inheritdoc/>
		public virtual int GetZoneLoad(string deviceId, string zoneId) { return LightingControl.GetZoneLoad(deviceId, zoneId); }

		/// <summary>
		/// Dispose all objects in <see cref="Disposables"/>.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				foreach (var item in Disposables)
				{
					item.Dispose();
				}

				Disposables.Clear();
			}

			_disposed = true;
		}

		/// <summary>
		/// recalls any startup or shutdown DSP presets that exist in the system configuration.
		/// </summary>
		protected virtual void HandleStartupShutdownPresets()
		{
			if (!(AudioControl is IAudioPresetApp presetApp))
			{
				return;
			}

			var presetId = SystemPowerControl.CurrentSystemState ? "STARTUP" : "SHUTDOWN";
			foreach (var dspInfo in AudioControl.GetAllAudioDspDevices())
			{
				var allPresets = presetApp.QueryDspAudioPresets(dspInfo.Id);
				var startupInfo = allPresets.FirstOrDefault(
					x => x.Id.ToUpper().Equals(presetId, StringComparison.InvariantCulture)
				);


				if (startupInfo != null)
				{
					presetApp.RecallAudioPreset(dspInfo.Id, startupInfo.Label);
				}
			}
		}
		
		/// <summary>
		/// Trigger any startup or shutdown lighting presets if they exist in the system configuration.
		/// </summary>
		protected virtual void HandleLightingStartupShutdown()
		{
			foreach (var control in LightingControl.GetAllLightingDeviceInfo())
			{
				if (SystemPowerControl.CurrentSystemState && !string.IsNullOrEmpty(control.StartupSceneId))
				{
					LightingControl.RecallLightingScene(control.Id, control.StartupSceneId);
				}
				else if (!SystemPowerControl.CurrentSystemState && !string.IsNullOrEmpty(control.ShutdownSceneId))
				{
					LightingControl.RecallLightingScene(control.Id, control.ShutdownSceneId);
				}
			}
		}

		/// <summary>
		/// Triggers any AV routes flagged for the startup or shutdown events. Also recalls <see cref="HandleStartupShutdownPresets"/>
		/// and <see cref="HandleLightingStartupShutdown"/>.
		/// </summary>
		protected virtual void OnSystemChange()
		{
			foreach (var destination in RoutingControl.GetAllAvDestinations())
			{
				RoutingControl.MakeRoute(Domain.RoutingInfo.StartupSource, destination.Id);
			}

			HandleStartupShutdownPresets();
			HandleLightingStartupShutdown();
		}

		/// <summary>
		/// subscribes to all events triggered by the internal subject-specific control objects.
		/// </summary>
		protected virtual void SubscribeEvents()
		{
			SystemPowerControl.SystemStateChanged += (_, evt) =>
			{
				var handler = SystemStateChanged;
				handler?.Invoke(this, evt);

				OnSystemChange();
			};

			DisplayControl.DisplayBlankChange += (_, evt) =>
			{
				var handler = DisplayBlankChange;
				handler?.Invoke(this, evt);

				if (!UseAvrMuteFreeze)
				{
					var globalHandler = GlobalVideoBlankChanged;
					globalHandler?.Invoke(this, EventArgs.Empty);
				}
			};

			DisplayControl.DisplayConnectChange += (_, evt) =>
			{
				var handler = DisplayConnectChange;
				handler?.Invoke(this, evt);
			};

			DisplayControl.DisplayFreezeChange += (_, evt) =>
			{
				var handler = DisplayFreezeChange;
				handler?.Invoke(this, evt);

				if (!UseAvrMuteFreeze)
				{
					var globalHandler = GlobalVideoFreezeChanged;
					globalHandler?.Invoke(this, EventArgs.Empty);
				}
			};

			DisplayControl.DisplayPowerChange += (_, evt) =>
			{
				var handler = DisplayPowerChange;
				handler?.Invoke(this, evt);
			};

			DisplayControl.DisplayInputChanged += (_, evt) =>
			{
				var handler = DisplayInputChanged;
				handler?.Invoke(this, evt);
			};


			EndpointControl.EndpointConnectionChanged += (_, evt) =>
			{
				var handler = EndpointConnectionChanged;
				handler?.Invoke(this, evt);
			};

			EndpointControl.EndpointRelayChanged += (_, evt) =>
			{
				var handler = EndpointRelayChanged;
				handler?.Invoke(this, evt);
			};

			AudioControl.AudioDspConnectionStatusChanged += (_, evt) =>
			{
				var handler = AudioDspConnectionStatusChanged;
				handler?.Invoke(this, evt);
			};

			AudioControl.AudioInputLevelChanged += (_, evt) =>
			{
				var handler = AudioInputLevelChanged;
				handler?.Invoke(this, evt);
			};

			AudioControl.AudioInputMuteChanged += (_, evt) =>
			{
				var handler = AudioInputMuteChanged;
				handler?.Invoke(this, evt);
			};

			AudioControl.AudioOutputLevelChanged += (_, evt) =>
			{
				var handler = AudioOutputLevelChanged;
				handler?.Invoke(this, evt);
			};

			AudioControl.AudioOutputMuteChanged += (_, evt) =>
			{
				var handler = AudioOutputMuteChanged;
				handler?.Invoke(this, evt);
			};

			AudioControl.AudioOutputRouteChanged += (_, evt) =>
			{
				var handler = AudioOutputRouteChanged;
				handler?.Invoke(this, evt);
			};

			AudioControl.AudioZoneEnableChanged += (_, evt) =>
			{
				var handler = AudioZoneEnableChanged;
				handler?.Invoke(this, evt);
			};

			RoutingControl.RouterConnectChange += (_, evt) =>
			{
				var handler = RouterConnectChange;
				handler?.Invoke(this, evt);
			};

			RoutingControl.RouteChanged += OnRoutingControlRouteChange;
			RoutingControl.VideoInputSyncChanged += (_, args) =>
			{
				var handler = VideoInputSyncChanged;
				handler?.Invoke(this, args);
			};

			foreach (var avr in HwService.AvSwitchers.GetAllDevices())
			{
				if (avr is IVideoControllable avrVideo)
				{
					avrVideo.VideoBlankChanged += (_, _) =>
					{
						var temp = GlobalVideoBlankChanged;
						temp?.Invoke(this, EventArgs.Empty);
					};

					avrVideo.VideoFreezeChanged += (_, _) =>
					{
						var temp = GlobalVideoFreezeChanged;
						temp?.Invoke(this, EventArgs.Empty);
					};

					UseAvrMuteFreeze = true;
				}
			}

			LightingControl.LightingLoadLevelChanged += (_, evt) =>
			{
				var handler = LightingLoadLevelChanged;
				handler?.Invoke(this, evt);
			};

			LightingControl.LightingSceneChanged += (_, evt) =>
			{
				var handler = LightingSceneChanged;
				handler?.Invoke(this, evt);
			};

			LightingControl.LightingControlConnectionChanged += (_, evt) =>
			{
				var handler = LightingControlConnectionChanged;
				handler?.Invoke(this, evt);
			};
		}

		/// <summary>
		/// Event handler for when the routing control component indicates a route was changed.
		/// </summary>
		/// <param name="sender">The RoutingComponent that triggered the change</param>
		/// <param name="args">Arg1 = ID of the destination that changed.</param>
		protected virtual void OnRoutingControlRouteChange(object? sender, GenericSingleEventArgs<string> args)
		{
			var handler = RouteChanged;
			handler?.Invoke(this, args);
		}

		/// <summary>
		/// Iterate through all AVRs in the system configuration and set their video freeze state to the given value, if
		/// they support this feature.
		/// </summary>
		/// <param name="state">true = freeze active, false = freeze inactive.</param>
		protected virtual void SetAvrVideoFreeze(bool state)
		{
			foreach (var avr in HwService.AvSwitchers.GetAllDevices())
			{
				if (avr is IVideoControllable avrVideo)
				{
					if (state)
					{
						avrVideo.FreezeOn();
					}
					else
					{
						avrVideo.FreezeOff();
					}

				}
			}
		}

		/// <summary>
		/// Iterate through all AVRs in the system configuration and set their video blank state to the given value, if
		/// they support this feature.
		/// </summary>
		/// <param name="state">true = blank active, false = blank inactive.</param>
		protected virtual void SetAvrVideoBlank(bool state)
		{
			foreach (var avr in HwService.AvSwitchers.GetAllDevices())
			{
				if (avr is IVideoControllable avrVideo)
				{
					if (state)
					{
						avrVideo.VideoBlankOn();
					}
					else
					{
						avrVideo.VideoBlankOff();
					}

				}
			}
		}
	}
}
