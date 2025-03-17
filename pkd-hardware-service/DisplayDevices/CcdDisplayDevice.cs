using Crestron.RAD.Common;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces;
using Crestron.SimplSharp;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_domain_service.Data.DisplayData;
using pkd_hardware_service.Routable;

namespace pkd_hardware_service.DisplayDevices
{
	/// <summary>
	/// Display control object that uses a Crestron certified driver for control.
	/// </summary>
	public class CcdDisplayDevice : BaseDevice.BaseDevice, IDisplayDevice, IVideoRoutable, IDisposable
	{
		private static readonly Dictionary<uint, VideoConnections> Inputs = new Dictionary<uint, VideoConnections>()
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

		private const int OfflineTimeout = 15000;

		private readonly IBasicVideoDisplay driver;
		private readonly Display config;
		private CTimer? offlineTimer;
		private bool disposed;

		/// <param name="driver">the Crestron Certified Driver object for controlling the device.</param>
		/// <param name="config">The device config data that was created during boot.</param>
		public CcdDisplayDevice(IBasicVideoDisplay driver, Display config)
		{
			ParameterValidator.ThrowIfNull(driver, "DisplayDevice.Ctor", nameof(driver));
			ParameterValidator.ThrowIfNull(config, "DisplayDevice.Ctor", nameof(config));

			this.config = config;
			this.driver = driver;
			Id = config.Id;
			Label = config.Label;
			FreezeState = false;
		}

		/// <inheritdoc />
		~CcdDisplayDevice()
		{
			Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? PowerChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? VideoBlankChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? VideoFreezeChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? HoursUsedChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, uint>>? VideoRouteChanged;

		/// <inheritdoc/>
		public bool PowerState => driver.PowerIsOn;

		/// <inheritdoc/>
		public bool BlankState => driver.VideoMuteIsOn;

		/// <inheritdoc/>
		public bool SupportsFreeze => !string.IsNullOrEmpty(config.CustomCommands.FreezeOnRx);

		/// <inheritdoc/>
		public bool FreezeState { get; private set; }

		/// <inheritdoc/>
		public uint HoursUsed => driver.LampHours.Count > 0 ? driver.LampHours[0] : 0;

		/// <inheritdoc/>
		public bool EnableReconnect { get; set; }

		/// <inheritdoc/>
		public void PowerOn()
		{
			if (!driver.Connected)
			{
				Logger.Warn($"DisplayDevice {Id} - sending power on command while disconnected.");
			}

			driver.PowerOn();
		}

		/// <inheritdoc/>
		public void PowerOff()
		{
			if (!driver.Connected)
			{
				Logger.Warn($"DisplayDevice {Id} - sending power off command while disconnected.");
			}

			driver.PowerOff();
		}

		/// <inheritdoc/>
		public void VideoBlankOn()
		{
			if (!driver.Connected)
			{
				Logger.Warn($"DisplayDevice {Id} - sending video blank on command while disconnected.");
			}

			driver.VideoMuteOn();
		}

		/// <inheritdoc/>
		public void VideoBlankOff()
		{
			if (!driver.Connected)
			{
				Logger.Warn($"DisplayDevice {Id} - sending video blank off command while disconnected.");
			}

			driver.VideoMuteOff();
		}

		/// <inheritdoc/>
		public void FreezeOn()
		{
			if (!driver.Connected)
			{
				Logger.Warn($"DisplayDevice {Id} - sending freeze on command while disconnected.");
			}

			driver.SendCustomCommand(config.CustomCommands.FreezeOnTx);
			FreezeState = true;
			NotifyEvent(VideoFreezeChanged);
		}

		/// <inheritdoc/>
		public void FreezeOff()
		{
			if (!driver.Connected)
			{
				Logger.Warn($"DisplayDevice {Id} - sending freeze off command while disconencted.");
			}

			driver.SendCustomCommand(config.CustomCommands.FreezeOffTx);
			FreezeState = false;
			NotifyEvent(VideoFreezeChanged);
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
			Label = label;
			Id = id;
			SubscribeToEvents();
		}

		/// <inheritdoc/>
		public override void Connect()
		{
			driver.Connect();
		}

		/// <inheritdoc/>
		public override void Disconnect()
		{
			driver.Disconnect();
		}

		/// <inheritdoc/>
		public uint GetCurrentVideoSource(uint output)
		{
			foreach (var kvp in Inputs)
			{
				if (kvp.Value == driver.InputSource.InputType)
				{
					return kvp.Key;
				}
			}

			Logger.Error(
				"CcdDisplayDevice {0}  GetCurrentVideoSource({0}) - Could not find a supported input.",
				Id,
				output);

			return 0;
		}

		/// <inheritdoc/>
		public void RouteVideo(uint source, uint output)
		{
			if (Inputs.TryGetValue(source, out var input))
			{
				Logger.Debug("CcdDisplayDevice {0}.RouteVideo({0}, {1}) - found = {2}", source, output, input);
				driver.SetInputSource(input);
			}
			else
			{
				Logger.Error(
					"CcdDisplayDevice {0} - RouteVideo({1},{2}) - input value not supported.",
					Id,
					source,
					output);
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// This is not supported by CCD devices.
		/// </summary>
		public void ClearVideoRoute(uint output) { }

		private void SubscribeToEvents()
		{
			Logger.Info($"DisplayDevice {Id} - Subscribing to events.");
			driver.StateChangeEvent += StateChangeHandler;
		}

		private void StateChangeHandler(DisplayStateObjects arg1, IBasicVideoDisplay arg2, byte arg3)
		{
			switch (arg1)
			{
				case DisplayStateObjects.Power:
					NotifyEvent(PowerChanged);
					break;

				case DisplayStateObjects.Connection:
					IsOnline = driver.Connected;
					if (IsOnline)
					{
						NotifyOnlineStatus();
					}
					else
					{
						StartOfflineTimer();
					}
					break;

				case DisplayStateObjects.LampHours:
					NotifyEvent(HoursUsedChanged);
					break;

				case DisplayStateObjects.VideoMute:
					NotifyEvent(VideoBlankChanged);
					break;

				case DisplayStateObjects.Authentication:
					Logger.Warn($"Display {Id} - Authentication event received.");
					break;

				case DisplayStateObjects.Input:
					ReportNewInput(driver.InputSource);
					break;
			}
		}

		private void ReportNewInput(InputDetail source)
		{
			foreach (var kvp in Inputs)
			{
				if (kvp.Value != source.InputType) continue;
				var temp = VideoRouteChanged;
				temp?.Invoke(this, new GenericDualEventArgs<string, uint>(Id, kvp.Key));
				break;
			}
		}

		private void NotifyEvent(EventHandler<GenericSingleEventArgs<string>>? handler)
		{
			handler?.Invoke(this, new GenericSingleEventArgs<string>(Id));
		}

		private void StartOfflineTimer()
		{
			if (offlineTimer == null)
			{
				offlineTimer = new CTimer(OfflineTimerTriggered, OfflineTimeout);
			}
			else
			{
				offlineTimer.Reset(OfflineTimeout);
			}
		}

		private void OfflineTimerTriggered(object? obj)
		{
			IsOnline = driver.Connected;
			if (!IsOnline)
			{
				NotifyOnlineStatus();
			}
		}

		private void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing)
			{
				if (offlineTimer != null)
				{
					offlineTimer.Stop();
					offlineTimer.Dispose();
					offlineTimer = null;
				}

				driver.StateChangeEvent -= StateChangeHandler;
				driver.Disconnect();
				driver.Dispose();
			}

			disposed = true;
		}
	}
}
