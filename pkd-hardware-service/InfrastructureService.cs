// ReSharper disable SuspiciousTypeConversion.Global

using pkd_domain_service.Data.VideoWallData;
using pkd_hardware_service.VideoWallDevices;

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
	using AudioDevices;
	using AvSwitchDevices;
	using BaseDevice;
	using DisplayDevices;
	using EndpointDevices;
	using LightingDevices;
	using TransportDevices;
	using System;

	/// <summary>
	/// Hardware management service for controlling real-world hardware devices.
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
			ParameterValidator.ThrowIfNull(controlSystem, "Ctor", nameof(controlSystem));

			this.controlSystem = controlSystem;
			Dsps = new DeviceContainer<IAudioControl>();
			AvSwitchers = new DeviceContainer<IAvSwitcher>();
			Displays = new DeviceContainer<IDisplayDevice>();
			Endpoints = new DeviceContainer<IEndpointDevice>();
			CableBoxes = new DeviceContainer<ITransportDevice>();
			LightingDevices = new DeviceContainer<ILightingDevice>();
			VideoWallDevices = new DeviceContainer<IVideoWallDevice>();
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="InfrastructureService"/> class.
		/// </summary>
		~InfrastructureService()
		{
			Dispose(false);
		}

		/// <inheritdoc/>
		public DeviceContainer<IAudioControl> Dsps { get; }

		/// <inheritdoc/>
		public DeviceContainer<IAvSwitcher> AvSwitchers { get; }

		/// <inheritdoc/>
		public DeviceContainer<IDisplayDevice> Displays { get; }

		/// <inheritdoc/>
		public DeviceContainer<IEndpointDevice> Endpoints { get; }

		/// <inheritdoc/>
		public DeviceContainer<ITransportDevice> CableBoxes { get; }

		/// <inheritdoc/>
		public DeviceContainer<ILightingDevice> LightingDevices { get; }
		
		/// <inheritdoc/>
		public DeviceContainer<IVideoWallDevice> VideoWallDevices { get; }

		/// <inheritdoc/>
		public void AddAvSwitch(MatrixData avSwitch, Routing routingData)
		{
			try
			{
				ParameterValidator.ThrowIfNull(avSwitch, "AddAvSwitch", nameof(avSwitch));
				Logger.Info($"Adding Av Switch {avSwitch.Id} To collection.");
				var device = AvSwitchFactory.CreateAvSwitcher(
					routingData.Sources,
					routingData.Destinations,
					avSwitch,
					controlSystem,
					this);

				if (device == null) return;
				AvSwitchers.AddDevice(avSwitch.Id, device);
				
				// If the switcher is also an audio control, add it to the collection of dsp devices.
				if (device is IAudioControl dspDevice)
				{
					Dsps.AddDevice(avSwitch.Id, dspDevice);
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "InfrastructureService.AddAvSwitch()");
			}
		}

		/// <inheritdoc/>
		public void AddDisplay(Display display)
		{
			try
			{
				ParameterValidator.ThrowIfNull(display, "AddDisplay", nameof(display));
				Logger.Info("Adding display {0} to collection.", display.Id);
				var device = DisplayDeviceFactory.CreateDisplay(display, controlSystem, this);
				if (device != null)
				{
					Displays.AddDevice(display.Id, device);
				}

				// If display is also an audio control, add it to the collection of dsp devices.
				if (device is IDsp dspDisplay)
				{
					Dsps.AddDevice(display.Id, dspDisplay);
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
				ParameterValidator.ThrowIfNull(dsp, "AddDsp", nameof(dsp));
				Logger.Info("Adding DSP {0} to collection.", dsp.Id);
				var device = AudioDeviceFactory.CreateDspDevice(dsp, controlSystem, this);
				if (device != null)
				{
					Dsps.AddDevice(dsp.Id, device);
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
				ParameterValidator.ThrowIfNull(channel, "AddChannel", nameof(channel));
				Logger.Info("Adding Audio channel {0} to DSP {1}...", channel.Id, channel.DspId);
				var found = Dsps.GetDevice(channel.DspId);
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

                if (found is IAudioZoneEnabler zoneController)
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
				ParameterValidator.ThrowIfNull(endpointData, "AddEndpoint", nameof(endpointData));
				Logger.Info("Adding endpoint {0} to collection.", endpointData.Id);
				var endpoint = EndpointDeviceFactory.CreateEndpointDevice(endpointData, controlSystem);
				if (endpoint == null) return;
				Endpoints.AddDevice(endpoint.Id, endpoint);
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
				ParameterValidator.ThrowIfNull(cableBox, "AddCableBox", nameof(cableBox));
				Logger.Info("Adding cable box {0} to collection.", cableBox.Id);
				var device = TransportDeviceFactory.CreateCableBox(cableBox, controlSystem, this);
				if (device == null) return;
				CableBoxes.AddDevice(cableBox.Id, device);
			}
			catch (Exception e)
			{
				Logger.Error(e, "InfrastructureService.AddCableBox()");
			}
		}

		/// <inheritdoc/>
		public void AddLightingDevice(LightingInfo lighting)
		{
			try
			{
				var lightingObj = LightingDeviceFactory.CreateLightingDevice(lighting, controlSystem, this);
				if (lightingObj == null) return;
				LightingDevices.AddDevice(lighting.Id, lightingObj);
			}
			catch (Exception e)
			{
				Logger.Error(e, "InfrastructureService.AddLightingDevice()");
			}
		}

		/// <inheritdoc/>
		public void AddVideoWall(VideoWall videoWall)
		{
			var videoWallObj = VideoWallFactory.CreateVideoWallDevice(videoWall, controlSystem, this);
			if (videoWallObj == null) return;
			VideoWallDevices.AddDevice(videoWallObj.Id, videoWallObj);
		}

		/// <inheritdoc/>
		public void ConnectAllDevices()
		{
			try
			{
				foreach (var device in Dsps.GetAllDevices())
				{
					if (device is IDsp dsp)
					{
						dsp.Connect();
					}
				}

				foreach (var display in Displays.GetAllDevices())
				{
					display.Connect();
				}

				foreach (var endpoint in Endpoints.GetAllDevices())
				{
					endpoint.Connect();
				}

				foreach (var rtr in AvSwitchers.GetAllDevices())
				{
					rtr.Connect();
				}

				foreach (var cableBox in CableBoxes.GetAllDevices())
				{
					cableBox.Connect();
				}

				foreach (var lightingControl in LightingDevices.GetAllDevices())
				{
					lightingControl.Connect();
				}

				foreach (var videoWallControl in VideoWallDevices.GetAllDevices())
				{
					videoWallControl.Connect();
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
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (isDisposed) return;
			if (disposing)
			{
				Dsps.Dispose();
				Displays.Dispose();
				AvSwitchers.Dispose();
				Endpoints.Dispose();
				LightingDevices.Dispose();
			}

			isDisposed = true;
		}
	}
}
