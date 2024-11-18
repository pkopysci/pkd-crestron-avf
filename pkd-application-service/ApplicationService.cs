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
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_domain_service;
	using pkd_hardware_service;
	using pkd_hardware_service.DisplayDevices;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	/// <summary>
	/// Class for controlling interactions between the external control interfaces and the hardware service.
	/// A plugin can use this class to override functionality for a custom application service without needing to completely
	/// rewrite the IApplicationService implementation.
	/// </summary>
	public class ApplicationService : IApplicationService, IDisposable
	{
		protected readonly List<IDisposable> disposables = new List<IDisposable>();
		protected List<UserInterfaceDataContainer> interfaceData;
		protected IDisplayControlApp displayControl;
		protected ISystemPowerApp systemPowerControl;
		protected IEndpointControlApp endpointControl;
		protected IInfrastructureService hwService;
		protected IDomainService domain;
		protected IAudioControlApp audioControl;
		protected IAvRoutingApp routingControl;
		protected ITransportControlApp transportControl;
		protected ILightingControlApp lightingControl;
		private bool disposed;
		protected bool useAvrMuteFreeze;

		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationService"/> class.
		/// </summary>
		public ApplicationService() { }

		~ApplicationService()
		{
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public virtual void Initialize(IInfrastructureService hwService, IDomainService domain)
		{
			this.domain = domain;
			this.hwService = hwService;

			this.systemPowerControl = ApplicationControlFactory.CreateSystemPower(hwService, domain, this);
			if (this.systemPowerControl is IDisposable pwrd)
			{
				this.disposables.Add(pwrd);
			}

			this.displayControl = ApplicationControlFactory.CreateDisplayControl(hwService, domain, this);
			if (this.displayControl is IDisposable dispd)
			{
				this.disposables.Add(dispd);
			}

			this.endpointControl = ApplicationControlFactory.CreateEndpointControl(hwService, domain, this);
			if (this.endpointControl is IDisposable epd)
			{
				this.disposables.Add(epd);
			}

			this.audioControl = ApplicationControlFactory.CreateAudioControl(hwService, domain, this);
			if (this.audioControl is IDisposable acd)
			{
				this.disposables.Add(acd);
			}

			this.routingControl = ApplicationControlFactory.CreateRoutingControl(hwService, domain, this);
			if (this.routingControl is IDisposable)
			{
				this.disposables.Add(this.routingControl as IDisposable);
			}

			this.transportControl = ApplicationControlFactory.CreateTransportControl(hwService, domain, this);
			if (this.transportControl is IDisposable)
			{
				this.disposables.Add(this.transportControl as IDisposable);
			}

			this.lightingControl = ApplicationControlFactory.CreateLightingControl(hwService, domain, this);
			if (this.lightingControl is IDisposable)
			{
				this.disposables.Add(this.lightingControl as IDisposable);
			}

			this.interfaceData = ApplicationControlFactory.CreateUserInterfaceData(domain);
			this.SubscribeEvents();
		}

		/// <inheritdoc/>
		public event EventHandler SystemStateChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>> DisplayPowerChange;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>> DisplayBlankChange;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>> DisplayFreezeChange;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>> DisplayConnectChange;

		/// <inheritdoc />
		public event EventHandler<GenericSingleEventArgs<string>> DisplayInputChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, int>> EndpointRelayChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>> EndpointConnectionChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> RouteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioOutputRouteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> RouterConnectChange;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioOutputLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioOutputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioInputLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioInputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>> AudioZoneEnableChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioDspConnectionStatusChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>> LightingLoadLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> LightingSceneChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>> LightingControlConnectionChanged;

		/// <inheritdoc/>
		public event EventHandler GlobalVideoBlankChanged;

		/// <inheritdoc/>
		public event EventHandler GlobalVideoFreezeChanged;

		/// <inheritdoc/>
		public bool CurrentSystemState
		{
			get { return this.systemPowerControl.CurrentSystemState; }
		}

		/// <inheritdoc/>
		public bool AutoShutdownEnabled
		{
			get { return this.systemPowerControl.AutoShutdownEnabled; }
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<UserInterfaceDataContainer> GetAllUserInterfaces()
		{
			return new ReadOnlyCollection<UserInterfaceDataContainer>(this.interfaceData);
		}

		/// <inheritdoc/>
		public virtual UserInterfaceDataContainer GetFusionInterface()
		{
			return new UserInterfaceDataContainer(
				this.domain.Fusion.GUID,
				this.domain.Fusion.RoomName,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				string.Empty,
				this.domain.Fusion.IpId,
				new List<string>());
		}

		/// <inheritdoc/>
		public virtual RoomInfoContainer GetRoomInfo()
		{
			return new RoomInfoContainer(
				this.domain.RoomInfo.Id,
				this.domain.RoomInfo.RoomName,
				this.domain.RoomInfo.HelpContact,
				this.domain.RoomInfo.SystemType
			);
		}

		/// <inheritdoc/>
		public virtual void SetActive() => this.systemPowerControl.SetActive();

		/// <inheritdoc/>
		public virtual void SetStandby() => this.systemPowerControl.SetStandby();

		/// <inheritdoc/>
		public virtual void AutoShutdownEnable() => this.systemPowerControl.AutoShutdownEnable();

		/// <inheritdoc/>
		public virtual void AutoShutdownDisable() => this.systemPowerControl.AutoShutdownDisable();

		/// <inheritdoc/>
		public virtual void SetAutoShutdownTime(int hour, int minute) => this.systemPowerControl.SetAutoShutdownTime(hour, minute);

		/// <inheritdoc/>
		public virtual void SetDisplayPower(string id, bool newState) => this.displayControl.SetDisplayPower(id, newState);

		/// <inheritdoc/>
		public virtual bool DisplayPowerQuery(string id)
		{
			return this.displayControl.DisplayPowerQuery(id);
		}

		/// <inheritdoc/>
		public virtual void SetDisplayBlank(string id, bool newState) => this.displayControl.SetDisplayBlank(id, newState);

		/// <inheritdoc/>
		public virtual bool DisplayBlankQuery(string id)
		{
			return this.displayControl.DisplayBlankQuery(id);
		}

		/// <inheritdoc
		public virtual void SetDisplayFreeze(string id, bool state) => this.displayControl.SetDisplayFreeze(id, state);

		/// <inheritdoc/>
		public virtual bool DisplayFreezeQuery(string id)
		{
			return this.displayControl.DisplayFreezeQuery(id);
		}

		/// <inheritdoc/>
		public virtual void RaiseScreen(string displayId)
		{
			Logger.Debug("ApplicationService.RaiseScreen({0})", displayId);

			this.displayControl.RaiseScreen(displayId);
		}

		/// <inheritdoc/>
		public virtual void LowerScreen(string displayId)
		{
			Logger.Debug("ApplicationService.LowerScreen({0})", displayId);

			this.displayControl.LowerScreen(displayId);
		}

		/// <inheritdoc/>
		public virtual void SetInputLectern(string displayId) => this.displayControl.SetInputLectern(displayId);

		/// <inheritdoc/>
		public virtual void SetInputStation(string displayId) => this.displayControl.SetInputStation(displayId);

		/// <inheritdoc/>
		public virtual bool DisplayInputLecternQuery(string displayId)
		{
			return this.displayControl.DisplayInputLecternQuery(displayId);
		}

		/// <inheritdoc/>
		public virtual bool DisplayInputStationQuery(string displayId)
		{
			return this.displayControl.DisplayInputStationQuery(displayId);
		}

		/// <inheritdoc
		public virtual ReadOnlyCollection<DisplayInfoContainer> GetAllDisplayInfo()
		{
			return this.displayControl.GetAllDisplayInfo();
		}

		/// <inheritdoc/>
		public virtual void PulseEndpointRelay(string id, int index, int timeMs) => this.endpointControl.PulseEndpointRelay(id, index, timeMs);

		/// <inheritdoc/>
		public virtual void LatchRelayClosed(string id, int index) => this.endpointControl.LatchRelayClosed(id, index);

		/// <inheritdoc/>
		public virtual void LatchRelayOpen(string id, int index) => this.endpointControl.LatchRelayOpen(id, index);

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<AudioChannelInfoContainer> GetAudioInputChannels()
		{
			return this.audioControl.GetAudioInputChannels();
		}

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<AudioChannelInfoContainer> GetAudioOutputChannels()
		{
			return this.audioControl.GetAudioOutputChannels();
		}

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<InfoContainer> GetAllAudioDspDevices()
		{
			return this.audioControl.GetAllAudioDspDevices();
		}

		/// <inheritdoc/>
		public virtual bool QueryAudioDspConnectionStatus(string id)
		{
			return this.audioControl.QueryAudioDspConnectionStatus(id);
		}

		/// <inheritdoc/>
		public virtual int QueryAudioInputLevel(string id)
		{
			return this.audioControl.QueryAudioInputLevel(id);
		}

		/// <inheritdoc/>
		public virtual int QueryAudioOutputLevel(string id)
		{
			return this.audioControl.QueryAudioOutputLevel(id);
		}

		/// <inheritdoc/>
		public virtual bool QueryAudioInputMute(string id)
		{
			return this.audioControl.QueryAudioInputMute(id);
		}

		/// <inheritdoc/>
		public virtual bool QueryAudioOutputMute(string id)
		{
			return this.audioControl.QueryAudioOutputMute(id);
		}

		/// <inheritdoc/>
		public virtual string QueryAudioOutputRoute(string id)
		{
			return this.audioControl.QueryAudioOutputRoute(id);
		}

		/// <inheritdoc/>
		public virtual bool QueryAudioZoneState(string channelId, string zoneId)
		{
			return this.audioControl.QueryAudioZoneState(channelId, zoneId);
		}

		/// <inheritdoc/>
		public virtual void SetAudioInputLevel(string id, int level) => this.audioControl.SetAudioInputLevel(id, level);

		/// <inheritdoc/>
		public virtual void SetAudioInputMute(string id, bool mute) => this.audioControl.SetAudioInputMute(id, mute);

		/// <inheritdoc/>
		public virtual void SetAudioOutputLevel(string id, int level) => this.audioControl.SetAudioOutputLevel(id, level);

		/// <inheritdoc/>
		public virtual void SetAudioOutputMute(string id, bool mute) => this.audioControl.SetAudioOutputMute(id, mute);

		/// <inheritdoc/>
		public virtual void SetAudioOutputRoute(string srcId, string destId) => this.audioControl.SetAudioOutputRoute(srcId, destId);

		/// <inheritdoc/>
		public virtual void ToggleAudioZoneState(string channelId, string zoneId) => this.audioControl.ToggleAudioZoneState(channelId, zoneId);

		/// <inheritdoc/>
		public virtual bool QueryRouterConnectionStatus(string id)
		{
			return this.routingControl.QueryRouterConnectionStatus(id);
		}

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<AvSourceInfoContainer> GetAllAvSources() => routingControl.GetAllAvSources();

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<InfoContainer> GetAllAvDestinations() => routingControl.GetAllAvDestinations();

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<InfoContainer> GetAllAvRouters() => routingControl.GetAllAvRouters();

		/// <inheritdoc/>
		public virtual void MakeRoute(string inputId, string outputId) => this.routingControl.MakeRoute(inputId, outputId);

		/// <inheritdoc/>
		public virtual void RouteToAll(string inputId) => this.routingControl.RouteToAll(inputId);

		/// <inheritdoc/>
		public virtual void ReportGraph()
		{
			this.routingControl.ReportGraph();
		}

		/// <inheritdoc/>
		public virtual AvSourceInfoContainer QueryCurrentRoute(string outputId)
		{
			return this.routingControl.QueryCurrentRoute(outputId);
		}

		/// <inheritdoc/>
		public virtual void SetGlobalVideoFreeze(bool state)
		{
			if (this.useAvrMuteFreeze)
			{
				this.SetAvrVideoFreeze(state);
			}
			else
			{
				foreach (var dispInfo in this.displayControl.GetAllDisplayInfo())
				{
					this.displayControl.SetDisplayFreeze(dispInfo.Id, state);
				}
			}
		}

		/// <inheritdoc/>
		public virtual bool QueryGlobalVideoBlank()
		{
			bool result = false;

			if (this.useAvrMuteFreeze)
			{
				foreach (var avr in this.hwService.AvSwitchers.GetAllDevices())
				{
					if (avr is IVideoControllable)
					{
						result = (avr as IVideoControllable).BlankState;
					}
				}
			}
			else
			{
				foreach (var display in this.hwService.Displays.GetAllDevices())
				{
					if (display is IVideoControllable)
					{
						result = (display as IVideoControllable).BlankState;
					}
				}
			}

			return result;
		}

		/// <inheritdoc/>
		public virtual bool QueryGlobalVideoFreeze()
		{
			bool result = false;

			if (this.useAvrMuteFreeze)
			{
				foreach (var avr in this.hwService.AvSwitchers.GetAllDevices())
				{
					if (avr is IVideoControllable)
					{
						result = (avr as IVideoControllable).FreezeState;
					}
				}
			}
			else
			{
				foreach (var display in this.hwService.Displays.GetAllDevices())
				{
					if (display is IVideoControllable)
					{
						result = (display as IVideoControllable).FreezeState;
					}
				}
			}

			return result;
		}

		/// <inheritdoc/>
		public virtual void SetGlobalVideoBlank(bool state)
		{
			if (this.useAvrMuteFreeze)
			{
				SetAvrVideoBlank(state);
			}
			else
			{
				foreach (var dispInfo in this.displayControl.GetAllDisplayInfo())
				{
					this.displayControl.SetDisplayBlank(dispInfo.Id, state);
				}
			}
		}

		public virtual ReadOnlyCollection<TransportInfoContainer> GetAllCableBoxes()
		{
			return this.transportControl.GetAllCableBoxes();
		}

		/// <inheritdoc/>
		public virtual void TransportPowerOn(string id) => this.transportControl.TransportPowerOn(id);

		/// <inheritdoc/>
		public virtual void TransportPowerOff(string id) => this.transportControl.TransportPowerOff(id);

		/// <inheritdoc/>
		public virtual void TransportPowerToggle(string id) => this.transportControl.TransportPowerToggle(id);

		/// <inheritdoc/>
		public virtual void TransportDial(string id, string channel) => this.transportControl.TransportDial(id, channel);

		/// <inheritdoc/>
		public virtual void TransportDialFavorite(string id, string favId) => this.transportControl.TransportDialFavorite(id, favId);

		/// <inheritdoc/>
		public virtual void TransportDash(string id) => this.transportControl.TransportDash(id);

		/// <inheritdoc/>
		public virtual void TransportChannelUp(string id) => this.transportControl.TransportChannelUp(id);

		/// <inheritdoc/>
		public virtual void TransportChannelDown(string id) => this.transportControl.TransportChannelDown(id);

		/// <inheritdoc/>
		public virtual void TransportPageUp(string id) => this.transportControl.TransportPageUp(id);

		/// <inheritdoc/>
		public virtual void TransportPageDown(string id) => this.transportControl.TransportPageDown(id);

		/// <inheritdoc/>
		public virtual void TransportGuide(string id) => this.transportControl.TransportGuide(id);

		/// <inheritdoc/>
		public virtual void TransportMenu(string id) => this.transportControl.TransportMenu(id);

		/// <inheritdoc/>
		public virtual void TransportInfo(string id) => this.transportControl.TransportInfo(id);

		/// <inheritdoc/>
		public virtual void TransportExit(string id) => this.transportControl.TransportExit(id);

		/// <inheritdoc/>
		public virtual void TransportBack(string id) => this.transportControl.TransportBack(id);

		/// <inheritdoc/>
		public virtual void TransportPlay(string id) => this.transportControl.TransportPlay(id);

		/// <inheritdoc/>
		public virtual void TransportPause(string id) => this.transportControl.TransportPause(id);

		/// <inheritdoc/>
		public virtual void TransportStop(string id) => this.transportControl.TransportStop(id);

		/// <inheritdoc/>
		public virtual void TransportRecord(string id) => this.transportControl.TransportRecord(id);

		/// <inheritdoc/>
		public virtual void TransportScanForward(string id) => this.transportControl.TransportScanForward(id);

		/// <inheritdoc/>
		public virtual void TransportScanReverse(string id) => this.transportControl.TransportScanReverse(id);

		/// <inheritdoc/>
		public virtual void TransportSkipForward(string id) => this.transportControl.TransportSkipForward(id);

		/// <inheritdoc/>
		public virtual void TransportSelect(string id) => this.transportControl.TransportSelect(id);

		/// <inheritdoc/>
		public virtual void TransportSkipReverse(string id) => this.transportControl.TransportSkipReverse(id);

		/// <inheritdoc/>
		public virtual void TransportNavUp(string id) => this.transportControl.TransportNavUp(id);

		/// <inheritdoc/>
		public virtual void TransportNavDown(string id) => this.transportControl.TransportNavDown(id);

		/// <inheritdoc/>
		public virtual void TransportNavLeft(string id) => this.transportControl.TransportNavLeft(id);

		/// <inheritdoc/>
		public virtual void TransportNavRight(string id) => this.transportControl.TransportNavRight(id);

		/// <inheritdoc/>
		public virtual void TransportRed(string id) => this.transportControl.TransportRed(id);

		/// <inheritdoc/>
		public virtual void TransportGreen(string id) => this.transportControl.TransportGreen(id);

		/// <inheritdoc/>
		public virtual void TransportYellow(string id) => this.transportControl.TransportYellow(id);

		/// <inheritdoc/>
		public virtual void TransportBlue(string id) => this.transportControl.TransportBlue(id);

		/// <inheritdoc/>
		public virtual void RecallLightingScene(string deviceId, string sceneId) { this.lightingControl.RecallLightingScene(deviceId, sceneId); }

		/// <inheritdoc/>
		public virtual void SetLightingLoad(string deviceId, string sceneId, int level) { this.lightingControl.SetLightingLoad(deviceId, sceneId, level); }

		/// <inheritdoc/>
		public virtual string GetActiveScene(string deviceId) { return this.lightingControl.GetActiveScene(deviceId); }

		/// <inheritdoc/>
		public virtual ReadOnlyCollection<LightingControlInfoContainer> GetAllLightingDeviceInfo() { return this.lightingControl.GetAllLightingDeviceInfo(); }

		/// <inheritdoc/>
		public virtual int GetZoneLoad(string deviceId, string zoneId) { return this.lightingControl.GetZoneLoad(deviceId, zoneId); }

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					foreach (var item in this.disposables)
					{
						item.Dispose();
					}

					this.disposables.Clear();
				}

				this.disposed = true;
			}
		}

		protected virtual void HandleStartupShutdownPresets()
		{
			if (!(this.audioControl is IAudioPresetApp presetApp))
			{
				return;
			}

			string presetId = this.systemPowerControl.CurrentSystemState ? "STARTUP" : "SHUTDOWN";
			foreach (var dspInfo in this.audioControl.GetAllAudioDspDevices())
			{
				var allPresets = presetApp.QueryDspAudioPresets(dspInfo.Id);
				InfoContainer startupInfo = allPresets.FirstOrDefault(
					x => x.Id.ToUpper().Equals(presetId, StringComparison.InvariantCulture)
				);


				if (startupInfo != null)
				{
					presetApp.RecallAudioPreset(dspInfo.Id, startupInfo.Label);
				}
			}
		}

		protected virtual void HandleLightingStartupShutdown()
		{
			foreach (var control in this.lightingControl.GetAllLightingDeviceInfo())
			{
				if (this.systemPowerControl.CurrentSystemState && !string.IsNullOrEmpty(control.StartupSceneId))
				{
					this.lightingControl.RecallLightingScene(control.Id, control.StartupSceneId);
				}
				else if (!this.systemPowerControl.CurrentSystemState && !string.IsNullOrEmpty(control.ShutdownSceneId))
				{
					this.lightingControl.RecallLightingScene(control.Id, control.ShutdownSceneId);
				}
			}
		}

		protected virtual void OnSystemChange()
		{
			foreach (var destination in this.routingControl.GetAllAvDestinations())
			{
				this.routingControl.MakeRoute(this.domain.RoutingInfo.StartupSource, destination.Id);
			}

			this.HandleStartupShutdownPresets();
			this.HandleLightingStartupShutdown();
		}

		protected virtual void SubscribeEvents()
		{
			this.systemPowerControl.SystemStateChanged += (obj, evt) =>
			{
				var handler = this.SystemStateChanged;
				handler?.Invoke(this, evt);

				this.OnSystemChange();
			};

			this.displayControl.DisplayBlankChange += (obj, evt) =>
			{
				var handler = this.DisplayBlankChange;
				handler?.Invoke(this, evt);

				if (!this.useAvrMuteFreeze)
				{
					var globalHandler = this.GlobalVideoBlankChanged;
					globalHandler?.Invoke(this, EventArgs.Empty);
				}
			};

			this.displayControl.DisplayConnectChange += (obj, evt) =>
			{
				var handler = this.DisplayConnectChange;
				handler?.Invoke(this, evt);
			};

			this.displayControl.DisplayFreezeChange += (obj, evt) =>
			{
				var handler = this.DisplayFreezeChange;
				handler?.Invoke(this, evt);

				if (!this.useAvrMuteFreeze)
				{
					var globalHandler = this.GlobalVideoFreezeChanged;
					globalHandler?.Invoke(this, EventArgs.Empty);
				}
			};

			this.displayControl.DisplayPowerChange += (obj, evt) =>
			{
				Logger.Debug("ApplicationService.DisplayPowerChangeHandler");
				var handler = this.DisplayPowerChange;
				handler?.Invoke(this, evt);
			};

			this.displayControl.DisplayInputChanged += (obj, evt) =>
			{
				Logger.Debug("ApplicationService.DisplayInputChangedHandler");
				var handler = this.DisplayInputChanged;
				handler?.Invoke(this, evt);
			};


			this.endpointControl.EndpointConnectionChanged += (obj, evt) =>
			{
				var handler = this.EndpointConnectionChanged;
				handler?.Invoke(this, evt);
			};

			this.endpointControl.EndpointRelayChanged += (obj, evt) =>
			{
				var handler = this.EndpointRelayChanged;
				handler?.Invoke(this, evt);
			};

			this.audioControl.AudioDspConnectionStatusChanged += (obj, evt) =>
			{
				var handler = this.AudioDspConnectionStatusChanged;
				handler?.Invoke(this, evt);
			};

			this.audioControl.AudioInputLevelChanged += (obj, evt) =>
			{
				var handler = this.AudioInputLevelChanged;
				handler?.Invoke(this, evt);
			};

			this.audioControl.AudioInputMuteChanged += (obj, evt) =>
			{
				var handler = this.AudioInputMuteChanged;
				handler?.Invoke(this, evt);
			};

			this.audioControl.AudioOutputLevelChanged += (obj, evt) =>
			{
				var handler = this.AudioOutputLevelChanged;
				handler?.Invoke(this, evt);
			};

			this.audioControl.AudioOutputMuteChanged += (obj, evt) =>
			{
				var handler = this.AudioOutputMuteChanged;
				handler?.Invoke(this, evt);
			};

			this.audioControl.AudioOutputRouteChanged += (obj, evt) =>
			{
				var handler = this.AudioOutputRouteChanged;
				handler?.Invoke(this, evt);
			};

			this.audioControl.AudioZoneEnableChanged += (obj, evt) =>
			{
				var handler = this.AudioZoneEnableChanged;
				handler?.Invoke(this, evt);
			};

			this.routingControl.RouterConnectChange += (obj, evt) =>
			{
				var handler = this.RouterConnectChange;
				handler?.Invoke(this, evt);
			};

			this.routingControl.RouteChanged += (obj, evt) =>
			{
				var handler = this.RouteChanged;
				handler?.Invoke(this, evt);
			};

			foreach (var avr in this.hwService.AvSwitchers.GetAllDevices())
			{
				if (avr is IVideoControllable)
				{
					IVideoControllable avrVideo = avr as IVideoControllable;
					avrVideo.VideoBlankChanged += (obj, evt) =>
					{
						var temp = this.GlobalVideoBlankChanged;
						temp?.Invoke(this, EventArgs.Empty);
					};

					avrVideo.VideoFreezeChanged += (obj, evt) =>
					{
						var temp = this.GlobalVideoFreezeChanged;
						temp?.Invoke(this, EventArgs.Empty);
					};

					this.useAvrMuteFreeze = true;
				}
			}

			this.lightingControl.LightingLoadLevelChanged += (obj, evt) =>
			{
				var handler = this.LightingLoadLevelChanged;
				handler?.Invoke(this, evt);
			};

			this.lightingControl.LightingSceneChanged += (obj, evt) =>
			{
				var handler = this.LightingSceneChanged;
				handler?.Invoke(this, evt);
			};

			this.lightingControl.LightingControlConnectionChanged += (obj, evt) =>
			{
				var handler = this.LightingControlConnectionChanged;
				handler?.Invoke(this, evt);
			};
		}

		protected virtual void SetAvrVideoFreeze(bool state)
		{
			foreach (var avr in this.hwService.AvSwitchers.GetAllDevices())
			{
				if (avr is IVideoControllable)
				{
					if (state)
					{
						(avr as IVideoControllable).FreezeOn();
					}
					else
					{
						(avr as IVideoControllable).FreezeOff();
					}

				}
			}
		}

		protected virtual void SetAvrVideoBlank(bool state)
		{
			foreach (var avr in this.hwService.AvSwitchers.GetAllDevices())
			{
				if (avr is IVideoControllable)
				{
					if (state)
					{
						(avr as IVideoControllable).VideoBlankOn();
					}
					else
					{
						(avr as IVideoControllable).VideoBlankOff();
					}

				}
			}
		}
	}
}
