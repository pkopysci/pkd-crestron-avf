namespace pkd_hardware_service.DisplayDevices
{
	using Crestron.RAD.Common;
	using Crestron.RAD.Common.Enums;
	using Crestron.RAD.Common.Interfaces;
	using Crestron.SimplSharp;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.DisplayData;
	using pkd_hardware_service.BaseDevice;
	using pkd_hardware_service.Routable;
	using System;
	using System.Collections.Generic;

	public class CcdDisplayDevice : BaseDevice, IDisplayDevice, IVideoRoutable, IDisposable
	{
		private static readonly Dictionary<uint, VideoConnections> inputs = new Dictionary<uint, VideoConnections>()
		{
			{ 1, VideoConnections.Hdmi1 },
			{ 2, VideoConnections.Hdmi2 },
			{ 3, VideoConnections.Hdmi3 },
			{ 4, VideoConnections.Hdmi4 },
			{ 5, VideoConnections.Hdmi5 },
			{ 6, VideoConnections.Hdmi6 },
			{ 7, VideoConnections.DisplayPort1 },
			{ 8, VideoConnections.DisplayPort2 },
			{ 9, VideoConnections.Vga1 },
		};

		private const int OFFLINE_TIMEOUT = 15000;

		private readonly IBasicVideoDisplay driver;
		private readonly Display config;
		private bool freezeActive;
		private CTimer offlineTimer;
		private bool disposed;

		public CcdDisplayDevice(IBasicVideoDisplay driver, Display config)
		{
			ParameterValidator.ThrowIfNull(driver, "DisplayDevice.Ctor", "driver");
			ParameterValidator.ThrowIfNull(config, "DisplayDevice.Ctor", "config");

			this.Id = config.Id;
			this.Label = config.Label;
			this.driver = driver;
			this.config = config;
			this.freezeActive = false;
		}

		~CcdDisplayDevice()
		{
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> PowerChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> VideoBlankChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> VideoFreezeChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> HoursUsedChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, uint>> VideoRouteChanged;

		/// <inheritdoc/>
		public bool PowerState
		{
			get
			{
				return this.driver.PowerIsOn;
			}
		}

		/// <inheritdoc/>
		public bool BlankState
		{
			get
			{
				return this.driver.VideoMuteIsOn;
			}
		}

		/// <inheritdoc/>
		public bool SupportsFreeze
		{
			get
			{
				return !string.IsNullOrEmpty(this.config.CustomCommands.FreezeOnRx);
			}
		}

		/// <inheritdoc/>
		public bool FreezeState
		{
			get
			{
				return this.freezeActive;
			}
		}

		/// <inheritdoc/>
		public uint HoursUsed
		{
			get
			{
				return this.driver.LampHours.Count > 0 ? this.driver.LampHours[0] : 0;
			}
		}

		/// <inheritdoc/>
		public bool EnableReconnect { get; set; }

		/// <inheritdoc/>
		public void PowerOn()
		{
			if (!this.driver.Connected)
			{
				Logger.Warn(string.Format("DisplayDevice {0} - sending power on command while disconnected.", this.Id));
			}

			this.driver.PowerOn();
		}

		/// <inheritdoc/>
		public void PowerOff()
		{
			if (!this.driver.Connected)
			{
				Logger.Warn(string.Format("DisplayDevice {0} - sending power off command while disconnected.", this.Id));
			}

			this.driver.PowerOff();
		}

		/// <inheritdoc/>
		public void VideoBlankOn()
		{
			if (!this.driver.Connected)
			{
				Logger.Warn(string.Format("DisplayDevice {0} - sending video blank on command while disconnected.", this.Id));
			}

			this.driver.VideoMuteOn();
		}

		/// <inheritdoc/>
		public void VideoBlankOff()
		{
			if (!this.driver.Connected)
			{
				Logger.Warn(string.Format("DisplayDevice {0} - sending video blank off command while disconnected.", this.Id));
			}

			this.driver.VideoMuteOff();
		}

		/// <inheritdoc/>
		public void FreezeOn()
		{
			if (!this.driver.Connected)
			{
				Logger.Warn(string.Format("DisplayDevice {0} - sending freeze on command while disconnected.", this.Id));
			}

			this.driver.SendCustomCommand(this.config.CustomCommands.FreezeOnTx);
			this.freezeActive = true;
			this.NotifyEvent(this.VideoFreezeChanged);
		}

		/// <inheritdoc/>
		public void FreezeOff()
		{
			if (!this.driver.Connected)
			{
				Logger.Warn(string.Format("DisplayDevice {0} - sending freeze off command while disconencted.", this.Id));
			}

			this.driver.SendCustomCommand(this.config.CustomCommands.FreezeOffTx);
			this.freezeActive = false;
			this.NotifyEvent(this.VideoFreezeChanged);
		}

		/// <inheritdoc/>
		public void EnablePolling()
		{
			Logger.Warn("CcdDisplayDevice.EnablePolling() - Cannot control polling functions on CCD.");
		}

		/// <inheritdoc/>
		public void DisablePolling()
		{
			Logger.Warn("CcdDisplayDevice.DisablePolling() - Cannot control polling functions on CCD.");
		}

		/// <inheritdoc/>
		public void Initialize(string host, int port, string label, string id)
		{
			this.Label = label;
			this.Id = id;
			this.SubscribeToEvents();
		}

		/// <inheritdoc/>
		public override void Connect()
		{
			this.driver.Connect();
		}

		/// <inheritdoc/>
		public override void Disconnect()
		{
			this.driver.Disconnect();
		}

		/// <inheritdoc/>
		public uint GetCurrentVideoSource(uint output)
		{
			foreach (var kvp in inputs)
			{
				if (kvp.Value == this.driver.InputSource.InputType)
				{
					return kvp.Key;
				}
			}

			Logger.Error(
				"CcdDisplayDevice {0}  GetCurrentVideoSource({0}) - Could not find a supported input.",
				this.Id,
				output);

			return 0;
		}

		/// <inheritdoc/>
		public void RouteVideo(uint source, uint output)
		{
			if (inputs.TryGetValue(source, out VideoConnections input))
			{
				Logger.Debug("CcdDisplayDevice {0}.RouteVideo({0}, {1}) - found = {2}", source, output, VideoConnections.None);
				this.driver.SetInputSource(VideoConnections.None);
			}
			else
			{
				Logger.Error(
					"CcdDisplayDevice {0} - RouteVideo({1},{2}) - input value not supported.",
					this.Id,
					source,
					output);
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void ClearVideoRoute(uint output) { }

		private void SubscribeToEvents()
		{
			Logger.Info(string.Format("DisplayDevice {0} - Subscribing to events.", this.Id));
			this.driver.StateChangeEvent += this.StateChangeHandler;
		}

		private void StateChangeHandler(DisplayStateObjects arg1, IBasicVideoDisplay arg2, byte arg3)
		{
			switch (arg1)
			{
				case DisplayStateObjects.Power:
					this.NotifyEvent(this.PowerChanged);
					break;

				case DisplayStateObjects.Connection:
					this.IsOnline = this.driver.Connected;
					if (this.IsOnline)
					{
						this.NotifyOnlineStatus();
					}
					else
					{
						this.StartOfflineTimer();
					}
					break;

				case DisplayStateObjects.LampHours:
					this.NotifyEvent(this.HoursUsedChanged);
					break;

				case DisplayStateObjects.VideoMute:
					this.NotifyEvent(this.VideoBlankChanged);
					break;

				case DisplayStateObjects.Authentication:
					Logger.Warn(string.Format("Display {0} - Authentication event received.", this.Id));
					break;

				case DisplayStateObjects.Input:
					this.ReportNewInput(this.driver.InputSource);
					break;

				default:
					break;
			}
		}

		private void ReportNewInput(InputDetail source)
		{
			foreach (var kvp in inputs)
			{
				if (kvp.Value == source.InputType)
				{
					var temp = this.VideoRouteChanged;
					if (temp != null)
					{
						temp.Invoke(this, new GenericDualEventArgs<string, uint>(this.Id, kvp.Key));
						break;
					}
				}
			}
		}

		private void NotifyEvent(EventHandler<GenericSingleEventArgs<string>> handler)
		{
			var temp = handler;
			temp?.Invoke(this, new GenericSingleEventArgs<string>(this.Id));
		}

		private void StartOfflineTimer()
		{
			if (this.offlineTimer == null)
			{
				this.offlineTimer = new CTimer(OfflineTimerTriggered, OFFLINE_TIMEOUT);
			}
			else
			{
				this.offlineTimer.Reset(OFFLINE_TIMEOUT);
			}
		}

		private void OfflineTimerTriggered(object obj)
		{
			this.IsOnline = this.driver.Connected;
			if (!IsOnline)
			{
				this.NotifyOnlineStatus();
			}
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this.offlineTimer != null)
					{
						this.offlineTimer.Stop();
						this.offlineTimer.Dispose();
						this.offlineTimer = null;
					}

					this.driver.StateChangeEvent -= this.StateChangeHandler;
					this.driver.Disconnect();
					this.driver.Dispose();
				}

				this.disposed = true;
			}
		}
	}

}
