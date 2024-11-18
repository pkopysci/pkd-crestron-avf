namespace pkd_application_service.DisplayControl
{
	using pkd_application_service.Base;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.DisplayData;
	using pkd_hardware_service.BaseDevice;
	using pkd_hardware_service.DisplayDevices;
	using pkd_hardware_service.Routable;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	/// <summary>
	/// Display device control app.
	/// </summary>
	internal sealed class DisplayControlApp : BaseApp<IDisplayDevice, Display>, IDisplayControlApp
	{
		private readonly IApplicationService parent;
		private readonly ReadOnlyCollection<DisplayInfoContainer> displayInfo;

		/// <summary>
		/// Initializes a new instance of the <see cref="DisplayControlApp"/> class.
		/// </summary>
		/// <param name="devices">System collection of display control objects.</param>
		/// <param name="data">Configuration data for all displays in the system.</param>
		/// <param name="parent">The application control service for the system.</param>
		public DisplayControlApp(
			DeviceContainer<IDisplayDevice> devices,
			ReadOnlyCollection<Display> data,
			IApplicationService parent)
			: base(devices, data)
		{
			ParameterValidator.ThrowIfNull(parent, "Ctor", "parent");

			this.parent = parent;
			List<DisplayInfoContainer> info = new List<DisplayInfoContainer>();
			foreach (var display in data)
			{
				var device = devices.GetDevice(display.Id);
				List<DisplayInput> inputs = new List<DisplayInput>()
				{
					new DisplayInput() { Id = string.Format("{0}-in{1}", device.Id, display.LecternInput), InputNumber = display.LecternInput, Label = "Lectern", Tags = new List<string> () { "lectern" } },
					new DisplayInput() { Id = string.Format("{0}-in{1}", device.Id, display.StationInput), InputNumber = display.StationInput, Label = "Station", Tags = new List<string> () { "station" } },
				};

				info.Add(new DisplayInfoContainer(display.Id, display.Label, display.Icon, display.Tags, display.HasScreen, device.IsOnline)
				{
					Inputs = inputs,
				});
			}

			this.displayInfo = new ReadOnlyCollection<DisplayInfoContainer>(info);
			this.RegisterHandlers();
		}

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
		public bool DisplayBlankQuery(string id)
		{
			var found = this.GetDevice(id);
			return (found != null) && found.BlankState;
		}

		/// <inheritdoc/>
		public bool DisplayFreezeQuery(string id)
		{
			var found = this.GetDevice(id);
			return (found != null) && found.FreezeState;
		}

		/// <inheritdoc/>
		public bool DisplayPowerQuery(string id)
		{
			var found = this.GetDevice(id);
			return (found != null) && found.PowerState;
		}

		/// <inheritdoc/>
		public bool DisplayInputLecternQuery(string id)
		{
			var found = this.GetDevice(id);
			var foundData = this.GetDeviceInfo(id);
			if (found == null || foundData == null)
			{
				Logger.Error("DisplayControlApp.DisplayInputLecternQuery({0}) - no device with given ID.", id);
				return false;
			}

			if (!(found is IVideoRoutable routable))
			{
				Logger.Error("DisplayControlApp.DisplayInputLecternQuery({0}) - display is not IVideoRoutable.", id);
				return false;
			}

			return routable.GetCurrentVideoSource(1) == foundData.LecternInput;
		}

		/// <inheritdoc/>
		public bool DisplayInputStationQuery(string id)
		{
			var found = this.GetDevice(id);
			var foundData = this.GetDeviceInfo(id);
			if (found == null || foundData == null)
			{
				Logger.Error("DisplayControlApp.DisplayInputStationQuery({0}) - no device with given ID.", id);
				return false;
			}

			if (!(found is IVideoRoutable routable))
			{
				Logger.Error("DisplayControlApp.DisplayInputStationQuery({0}) - display is not IVideoRoutable.", id);
				return false;
			}

			return routable.GetCurrentVideoSource(1) == foundData.StationInput;
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<DisplayInfoContainer> GetAllDisplayInfo()
		{
			return this.displayInfo;
		}

		/// <inheritdoc/>
		public void LowerScreen(string displayId)
		{
			var found = this.data.FirstOrDefault(x => x.Id == displayId);
			if (found != default(Display))
			{
				this.parent.PulseEndpointRelay(found.RelayController, found.ScreenDownRelay, 1800);
			}
		}

		/// <inheritdoc/>
		public void RaiseScreen(string displayId)
		{
			var found = this.data.FirstOrDefault(x => x.Id == displayId);
			if (found != default(Display))
			{
				this.parent.PulseEndpointRelay(found.RelayController, found.ScreenUpRelay, 1800);
			}
		}

		/// <inheritdoc/>
		public void SetDisplayBlank(string id, bool newState)
		{
			var found = this.devices.GetDevice(id);
			if (found != default(IDisplayDevice))
			{
				if (newState)
				{
					found.VideoBlankOn();
				}
				else
				{
					found.VideoBlankOff();
				}
			}
		}

		/// <inheritdoc/>
		public void SetDisplayFreeze(string id, bool state)
		{
			var found = this.devices.GetDevice(id);
			if (found != default(IDisplayDevice))
			{
				if (state)
				{
					found.FreezeOn();
				}
				else
				{
					found.FreezeOff();
				}
			}
		}

		/// <inheritdoc/>
		public void SetDisplayPower(string id, bool newState)
		{
			var found = this.devices.GetDevice(id);
			if (found != null)
			{
				if (newState)
				{
					found.PowerOn();
				}
				else
				{
					found.PowerOff();
				}
			}
		}

		/// <inheritdoc/>
		public void SetInputLectern(string displayId)
		{
			var found = this.GetDevice(displayId);
			if (found == null)
			{
				Logger.Error("DisplayControlApp.SetInputLectern({0}) - display not found.", displayId);
				return;
			}

			IVideoRoutable routable = found as IVideoRoutable;
			if (found == null)
			{
				Logger.Error("DisplayControlApp.SetInputLectern({0}) - display Does not support input selection.", displayId);
				return;
			}

			Logger.Debug("ApplicationService.DisplayControlAPp.SetInputLectern({0}) - number = {1}", displayId, this.GetDeviceInfo(displayId).LecternInput);
			routable.RouteVideo((uint)this.GetDeviceInfo(displayId).LecternInput, 1);
		}

		/// <inheritdoc/>
		public void SetInputStation(string displayId)
		{
			Logger.Debug("ApplicationService.DisplayControlAPp.SetInputStation({0})", displayId);

			var found = this.GetDevice(displayId);
			if (found == null)
			{
				Logger.Error("DisplayControlApp.SetInputStation({0}) - display not found.", displayId);
				return;
			}

			IVideoRoutable routable = found as IVideoRoutable;
			if (found == null)
			{
				Logger.Error("DisplayControlApp.SetInputStation({0}) - display Does not support input selection.", displayId);
				return;
			}

			routable.RouteVideo((uint)this.GetDeviceInfo(displayId).StationInput, 1);
		}

		private void RegisterHandlers()
		{
			foreach (var display in this.GetAllDevices())
			{
				display.EnableReconnect = true;
				display.ConnectionChanged += this.Display_ConnectionChanged;
				display.VideoFreezeChanged += this.Display_VideoFreezeChanged;
				display.VideoBlankChanged += this.Display_VideoBlankChanged;
				display.PowerChanged += this.Display_PowerChanged;

				if (display is IVideoRoutable routable)
				{
					routable.VideoRouteChanged += this.Display_RouteChanged;
				}
			}
		}

		private void Display_RouteChanged(object sender, GenericDualEventArgs<string, uint> e)
		{
			var temp = this.DisplayInputChanged;
			temp?.Invoke(this, new GenericSingleEventArgs<string>(e.Arg1));
		}

		private void Display_PowerChanged(object sender, EventArgs e)
		{
			if (sender is IDisplayDevice display)
			{
				this.Notify(this.DisplayPowerChange, display.Id, display.PowerState);
			}
		}

		private void Display_VideoBlankChanged(object sender, EventArgs e)
		{
			if (sender is IDisplayDevice display)
			{
				this.Notify(this.DisplayBlankChange, display.Id, display.FreezeState);
			}
		}

		private void Display_VideoFreezeChanged(object sender, EventArgs e)
		{
			if (sender is IDisplayDevice display)
			{
				this.Notify(this.DisplayFreezeChange, display.Id, display.FreezeState);
			}
		}

		private void Display_ConnectionChanged(object sender, GenericSingleEventArgs<string> e)
		{
			if (sender is IDisplayDevice display)
			{
				this.Notify(this.DisplayConnectChange, display.Id, display.IsOnline);
				if (display.IsOnline)
				{
					display.EnablePolling();
				}
			}
		}

		private void Notify(EventHandler<GenericDualEventArgs<string, bool>> evt, string id, bool data)
		{
			var temp = evt;
			temp?.Invoke(this, new GenericDualEventArgs<string, bool>(id, data));
		}
	}
}
