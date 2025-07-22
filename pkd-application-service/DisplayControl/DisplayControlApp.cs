using System.Collections.ObjectModel;
using pkd_application_service.Base;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_domain_service.Data.DisplayData;
using pkd_hardware_service.BaseDevice;
using pkd_hardware_service.DisplayDevices;
using pkd_hardware_service.Routable;

namespace pkd_application_service.DisplayControl
{
	/// <summary>
	/// Display device control app.
	/// </summary>
	internal sealed class DisplayControlApp : BaseApp<IDisplayDevice, Display>, IDisplayControlApp
	{
		private readonly IApplicationService _parent;
		private readonly List<DisplayInfoContainer> _displayInfo;

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

			this._parent = parent;
			List<DisplayInfoContainer> info = [];
			foreach (var display in data)
			{
				var device = devices.GetDevice(display.Id);
				if (device == null) continue;
				// TODO: Remove statically adding display routing behavior.
				List<DisplayInput> inputs =
				[
					new()
					{
						Id = $"{device.Id}-in{display.LecternInput}",
						InputNumber = display.LecternInput, Label = "Lectern", Tags = ["lectern"]
					},
					new()
					{
						Id = $"{device.Id}-in{display.StationInput}",
						InputNumber = display.StationInput, Label = "Station", Tags = ["station"]
					}
				];

				info.Add(new DisplayInfoContainer(display.Id, display.Label, display.Icon, display.Tags, display.HasScreen, device.IsOnline)
				{
					Inputs = inputs,
					Manufacturer = display.Manufacturer,
					Model = display.Model
				});
			}

			_displayInfo = info;
			RegisterHandlers();
		}

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
		public bool DisplayBlankQuery(string id)
		{
			var found = GetDevice(id);
			return found is { BlankState: true };
		}

		/// <inheritdoc/>
		public bool DisplayFreezeQuery(string id)
		{
			var found = GetDevice(id);
			return found is { FreezeState: true };
		}

		/// <inheritdoc/>
		public bool DisplayPowerQuery(string id)
		{
			var found = GetDevice(id);
			return found is { PowerState: true };
		}

		/// <inheritdoc/>
		public bool DisplayInputLecternQuery(string id)
		{
			var found = GetDevice(id);
			var foundData = GetDeviceInfo(id);
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
			var found = GetDevice(id);
			var foundData = GetDeviceInfo(id);
			if (found == null || foundData == null)
			{
				Logger.Error("DisplayControlApp.DisplayInputStationQuery({0}) - no device with given ID.", id);
				return false;
			}

			if (found is not IVideoRoutable routable)
			{
				Logger.Error("DisplayControlApp.DisplayInputStationQuery({0}) - display is not IVideoRoutable.", id);
				return false;
			}

			return routable.GetCurrentVideoSource(1) == foundData.StationInput;
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<DisplayInfoContainer> GetAllDisplayInfo()
		{
			return new ReadOnlyCollection<DisplayInfoContainer>(_displayInfo);
		}

		/// <inheritdoc/>
		public void LowerScreen(string displayId)
		{
			var found = Data.FirstOrDefault(x => x.Id == displayId);
			if (found != null)
			{
				_parent.PulseEndpointRelay(found.RelayController, found.ScreenDownRelay, 1800);
			}
		}

		/// <inheritdoc/>
		public void RaiseScreen(string displayId)
		{
			var found = Data.FirstOrDefault(x => x.Id == displayId);
			if (found != null)
			{
				_parent.PulseEndpointRelay(found.RelayController, found.ScreenUpRelay, 1800);
			}
		}

		/// <inheritdoc/>
		public void SetDisplayBlank(string id, bool newState)
		{
			var found = Devices.GetDevice(id);
			if (found != null)
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
			var found = Devices.GetDevice(id);
			if (found != null)
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
			var found = Devices.GetDevice(id);
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
			var found = GetDevice(displayId);
			var info = GetDeviceInfo(displayId);
			if (found == null || info == null)
			{
				Logger.Error("DisplayControlApp.SetInputLectern({0}) - display device not found.", displayId);
				return;
			}

			if (found is IVideoRoutable routable)
			{

				Logger.Debug("ApplicationService.DisplayControlAPp.SetInputLectern({0}) - number = {1}", displayId, info.LecternInput);
				routable.RouteVideo((uint)info.LecternInput, 1);
			}
			else
			{
				Logger.Error("DisplayControlApp.SetInputLectern({0}) - display Does not support input selection.", displayId);
			}
		}

		/// <inheritdoc/>
		public void SetInputStation(string displayId)
		{
			Logger.Debug("ApplicationService.DisplayControlAPp.SetInputStation({0})", displayId);

			var found = GetDevice(displayId);
			var info = GetDeviceInfo(displayId);
			if (found == null || info == null)
			{
				Logger.Error("DisplayControlApp.SetInputStation({0}) - display not found.", displayId);
				return;
			}

			if (found is IVideoRoutable routable)
			{
				routable.RouteVideo((uint)info.StationInput, 1);
			}
			else
			{
				Logger.Error("DisplayControlApp.SetInputStation({0}) - display Does not support input selection.", displayId);
			}
		}

		private void RegisterHandlers()
		{
			foreach (var display in GetAllDevices())
			{
				display.EnableReconnect = true;
				display.ConnectionChanged += Display_ConnectionChanged;
				display.VideoFreezeChanged += Display_VideoFreezeChanged;
				display.VideoBlankChanged += Display_VideoBlankChanged;
				display.PowerChanged += Display_PowerChanged;

				if (display is IVideoRoutable routable)
				{
					routable.VideoRouteChanged += Display_RouteChanged;
				}
			}
		}

		private void Display_RouteChanged(object? sender, GenericDualEventArgs<string, uint> e)
		{
			var temp = DisplayInputChanged;
			temp?.Invoke(this, new GenericSingleEventArgs<string>(e.Arg1));
		}

		private void Display_PowerChanged(object? sender, GenericSingleEventArgs<string> genericSingleEventArgs)
		{
			if (sender is IDisplayDevice display)
			{
				Notify(DisplayPowerChange, display.Id, display.PowerState);
			}
		}

		private void Display_VideoBlankChanged(object? sender, GenericSingleEventArgs<string> genericSingleEventArgs)
		{
			if (sender is IDisplayDevice display)
			{
				Notify(DisplayBlankChange, display.Id, display.FreezeState);
			}
		}

		private void Display_VideoFreezeChanged(object? sender, GenericSingleEventArgs<string> genericSingleEventArgs)
		{
			if (sender is IDisplayDevice display)
			{
				Notify(DisplayFreezeChange, display.Id, display.FreezeState);
			}
		}

		private void Display_ConnectionChanged(object? sender, GenericSingleEventArgs<string> e)
		{
			if (sender is not IDisplayDevice display) return;

			var target = _displayInfo.FirstOrDefault(x => x.Id.Equals(display.Id));
			if (target != null) target.IsOnline = display.IsOnline;
			
			Notify(DisplayConnectChange, display.Id, display.IsOnline);
			if (display.IsOnline)
			{
				display.EnablePolling();
			}
		}

		private void Notify(EventHandler<GenericDualEventArgs<string, bool>>? evt, string id, bool data)
		{
			var temp = evt;
			temp?.Invoke(this, new GenericDualEventArgs<string, bool>(id, data));
		}
	}
}
