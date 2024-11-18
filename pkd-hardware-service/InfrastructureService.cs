namespace pkd_hardware_service
{
	using Crestron.SimplSharpPro;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.DisplayData;
	using pkd_domain_service.Data.DspData;
	using pkd_domain_service.Data.EndpointData;
	using pkd_domain_service.Data.LightingData;
	using pkd_domain_service.Data.RoutingData;
	using pkd_domain_service.Data.TransportDeviceData;
	using pkd_hardware_service.AudioDevices;
	using pkd_hardware_service.AvSwitchDevices;
	using pkd_hardware_service.BaseDevice;
	using pkd_hardware_service.DisplayDevices;
	using pkd_hardware_service.EndpointDevices;
	using pkd_hardware_service.LightingDevices;
	using pkd_hardware_service.TransportDevices;
	using System;

	/// <summary>
	/// Hardware managment service for controlling real-world hardware devices.
	/// </summary>
	public class InfrastructureService : IInfrastructureService
	{
		private readonly CrestronControlSystem controlSystem;
		private bool isDisposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="InfrastructureService"/> class.
		/// </summary>
		/// <param name="controlSystem">A reference to the Crestron control processor running the system.</param>
		public InfrastructureService(CrestronControlSystem controlSystem)
		{
			ParameterValidator.ThrowIfNull(controlSystem, "Ctor", "controlSystem");

			this.controlSystem = controlSystem;
			this.Dsps = new DeviceContainer<IAudioControl>();
			this.AvSwitchers = new DeviceContainer<IAvSwitcher>();
			this.Displays = new DeviceContainer<IDisplayDevice>();
			this.Endpoints = new DeviceContainer<IEndpointDevice>();
			this.CableBoxes = new DeviceContainer<ITransportDevice>();
			this.LightingDevices = new DeviceContainer<ILightingDevice>();
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="InfrastructureService"/> class.
		/// </summary>
		~InfrastructureService()
		{
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public DeviceContainer<IAudioControl> Dsps { get; private set; }

		/// <inheritdoc/>
		public DeviceContainer<IAvSwitcher> AvSwitchers { get; private set; }

		/// <inheritdoc/>
		public DeviceContainer<IDisplayDevice> Displays { get; private set; }

		/// <inheritdoc/>
		public DeviceContainer<IEndpointDevice> Endpoints { get; private set; }

		/// <inheritdoc/>
		public DeviceContainer<ITransportDevice> CableBoxes { get; private set; }

		public DeviceContainer<ILightingDevice> LightingDevices { get; private set; }

		/// <inheritdoc/>
		public void AddAvSwitch(MatrixData avSwitch, Routing routingData)
		{
			try
			{
				ParameterValidator.ThrowIfNull(avSwitch, "AddAvSwitch", "avSwitch");
				Logger.Info(string.Format("Adding Av Swtich {0} To collection.", avSwitch.Id));
				IAvSwitcher device = AvSwitchFactory.CreateAvSwitcher(
					routingData.Sources,
					routingData.Destinations,
					avSwitch,
					this.controlSystem,
					this);

				// If the switcher is also an audio control, add it to the collection of dsp devices.
				this.AvSwitchers.AddDevice(avSwitch.Id, device);
				if (device is IAudioControl dspDevice)
				{
					this.Dsps.AddDevice(avSwitch.Id, dspDevice);
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Infrastructureservice.AddAvSwitch()");
			}
		}

		/// <inheritdoc/>
		public void AddDisplay(Display display)
		{
			try
			{
				ParameterValidator.ThrowIfNull(display, "AddDisplay", "display");
				Logger.Info(string.Format("Adding display {0} to collection.", display.Id));
				var device = DisplayDeviceFactory.CreateDisplay(display, this.controlSystem, this);
				if (device != null && device != default(IDisplayDevice))
				{
					this.Displays.AddDevice(display.Id, device);
				}

				// If display is also an audio control, add it to the collection of dsp devices.
				if (device is IDsp dspDisplay)
				{
					this.Dsps.AddDevice(display.Id, dspDisplay);
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "InfrastructureService.AddDisplay()");
			}
		}

		/// <inheritdoc/>
		public void AddDsp(Dsp dsp)
		{
			try
			{
				ParameterValidator.ThrowIfNull(dsp, "AddDsp", "dsp");
				Logger.Info(string.Format("Adding DSP {0} to collection.", dsp.Id));
				var device = AudioDeviceFactory.CreateDspDevice(dsp, this.controlSystem, this);
				if (device != null)
				{
					this.Dsps.AddDevice(dsp.Id, device);
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "InfrastructureService.AddDsp()");
			}
		}

		/// <inheritdoc/>
		public void AddAudioChannel(Channel channel)
		{
			try
			{
				ParameterValidator.ThrowIfNull(channel, "AddChannel", "channel");
				Logger.Info("Adding Audio channel {0} to DSP {1}...", channel.Id, channel.DspId);
				IAudioControl found = this.Dsps.GetDevice(channel.DspId);
				if (found == null)
				{
					Logger.Error("No DSP with ID {0} found. Skipping channel {1}", channel.DspId, channel.Id);
					return;
				}

				if (channel.Tags.Contains("input"))
				{
					found.AddInputChannel(
						channel.Id,
						channel.LevelControlTag,
						channel.MuteControlTag,
						channel.BankIndex,
						channel.LevelMax,
						channel.LevelMin,
						channel.RouterIndex);
				}
				else if (channel.Tags.Contains("output"))
				{
					found.AddOutputChannel(
						channel.Id,
						channel.LevelControlTag,
						channel.MuteControlTag,
						channel.RouterControlTag,
						channel.RouterIndex,
						channel.BankIndex,
						channel.LevelMax,
						channel.LevelMin);
				}
				else
				{
					Logger.Error("Channel {0} does not contain 'input' or 'output' tags.", channel.Id);
				}

				var zoneController = found as IAudioZoneEnabler;
				if (found != null)
				{
					foreach (var zone in channel.ZoneEnableToggles)
					{
						zoneController.AddAudioZoneEnable(channel.Id, zone.ZoneId, zone.Tag);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "InfrastructureService.AddAudioChannel()");
			}
		}

		/// <inheritdoc/>
		public void AddEndpoint(Endpoint endpointData)
		{
			try
			{
				ParameterValidator.ThrowIfNull(endpointData, "AddEndpoint", "endpointData");
				Logger.Info(string.Format("Adding endpoint {0} to collection.", endpointData.Id));
				var endpoint = EndpointDeviceFactory.CreateEndpointDevice(endpointData, this.controlSystem);
				this.Endpoints.AddDevice(endpoint.Id, endpoint);
				endpoint.Register();
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "InfrastructureService.AddEndpoint()");
			}
		}

		/// <inheritdoc/>
		public void AddCableBox(CableBox cableBox)
		{
			try
			{
				ParameterValidator.ThrowIfNull(cableBox, "AddCableBox", "cableBox");
				Logger.Info("Adding cable box {0} to collection.", cableBox.Id);
				var cbox = TransportDeviceFactory.CreateCableBox(cableBox, this.controlSystem, this);
				if (cbox != null)
				{
					this.CableBoxes.AddDevice(cableBox.Id, cbox);
				}
			}
			catch (Exception e)
			{
				Logger.Error(e, "InfrastructureService.AddCableBox()");
			}
		}

		public void AddLightingDevice(LightingInfo lighting)
		{
			try
			{
				if (lighting == null)
				{
					Logger.Error("InfrastructureService.AddLightingDevice() - 'lighting' cannot be null.");
					return;
				}

				var lightingObj = LightingDeviceFactory.CreateLightingDevice(lighting, this.controlSystem, this);
				if (lighting != null)
				{
					this.LightingDevices.AddDevice(lighting.Id, lightingObj);
				}
			}
			catch (Exception e)
			{
				Logger.Error(e, "InfrastructureService.AddLightingDevice()");
			}
		}

		/// <inheritdoc/>
		public void ConnectAllDevices()
		{
			try
			{
				foreach (var device in this.Dsps.GetAllDevices())
				{
					if (device is IDsp dsp)
					{
						dsp.Connect();
					}
				}

				foreach (var disp in this.Displays.GetAllDevices())
				{
					disp.Connect();
				}

				foreach (var endpoint in this.Endpoints.GetAllDevices())
				{
					endpoint.Connect();
				}

				foreach (var rtr in this.AvSwitchers.GetAllDevices())
				{
					rtr.Connect();
				}

				foreach (var cbox in this.CableBoxes.GetAllDevices())
				{
					cbox.Connect();
				}

				foreach (var lightingControl in this.LightingDevices.GetAllDevices())
				{
					lightingControl.Connect();
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "InfrastructureService.ConnectAllDevices()");
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this.isDisposed)
			{
				if (disposing)
				{
					this.Dsps?.Dispose();
					this.Displays?.Dispose();
					this.AvSwitchers?.Dispose();
					this.Endpoints?.Dispose();
					this.LightingDevices?.Dispose();
				}

				this.isDisposed = true;
			}
		}
	}
}
