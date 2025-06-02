using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.GeneralIO;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_domain_service.Data.EndpointData;

namespace pkd_hardware_service.EndpointDevices
{
	/// <summary>
	/// Crestron C2N-IO relay controller via Cresnet.
	/// </summary>
	public class C2NIoRelayDevice : BaseDevice.BaseDevice, IEndpointDevice, IRelayDevice, IDisposable
	{
		private readonly C2nIo _device;
		private readonly CrestronControlSystem _processor;
		private bool _disposed;

		/// <summary>
		/// Creates an instance of <see cref="C2NIoRelayDevice"/>.
		/// </summary>
		/// <param name="data">The configuration data for this endpoint device.</param>
		/// <param name="controlSystem">The control system that's running the framework program.</param>
		public C2NIoRelayDevice(Endpoint data, CrestronControlSystem controlSystem)
		{
			ParameterValidator.ThrowIfNull(controlSystem, "Ctor", "controlSystem");
			ParameterValidator.ThrowIfNull(data, "Ctor", "data");
			
			_processor = controlSystem;
			Id = data.Id;
			Label = data.Id;
			_device = new C2nIo((uint)data.Port, _processor);
			_device.OnlineStatusChange += OnlineStatusChangeHandler;
			foreach (var relay in _device.RelayPorts)
			{
				if (relay != null) relay.StateChange += RelayStateChangeHandler;
			}

			Manufacturer = "Crestron";
			Model = "C2N-IO";
		}

		/// <inheritdoc />
		~C2NIoRelayDevice()
		{
			Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, int>>? RelayChanged;

		/// <inheritdoc/>
		public bool IsRegistered => CheckRegistered();

		/// <inheritdoc/>
		public bool SupportsRelays => true;

		/// <inheritdoc/>
		public bool SupportsIr => false;

		/// <inheritdoc/>
		public bool SupportsRs232 => false;

		/// <inheritdoc/>
		public void Register()
		{
			Logger.Debug("C2nIORelayDevice {0} - Register()");

			if (!CheckRegistered())
			{
				Logger.Error("C2nIoRelayDevice {0} is already registered.", Id);
				return;
			}

			if (_device.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Error("C2nIoRelayDevice {0} - failed to register with control system: {0}", _device.RegistrationFailureReason);
			}
		}

		/// <inheritdoc/>
		public bool? GetCurrentRelayState(int index)
		{
			if (!CheckRegistered())
			{
				return false;
			}

			return index <= _device.RelayPorts.Count && _device.RelayPorts[(uint)index] is { State: true };
		}

		/// <inheritdoc/>
		public void PulseRelay(int index, int timeMs)
		{
			Logger.Debug("C2nIORelayDevice {0} - PulseRelay({0}, {1})", index, timeMs);

			if (!CheckRegistered())
			{
				Logger.Error("C3nIoRelayDevice {0} - PulseRelay()- device is not registered.", Id);
				return;
			}

			if (index > _device.RelayPorts.Count)
			{
				Logger.Error("C3nIoRelayDevice {0} - PulseRelay() - index {1} more than the number of supported relays.", Id, index);
				return;
			}

			_device.RelayPorts[(uint)index]?.Close();
			CTimer t = new((obj) =>
			{
				if (!CheckRegistered())
				{
					return;
				}

				_device.RelayPorts[(uint)index]?.Open();
			}, timeMs);
		}

		/// <inheritdoc/>
		public void LatchRelayClosed(int index)
		{
			if (!CheckRegistered())
			{
				Logger.Error("C3nIoRelayDevice {0} - PulseRelay()- device is not registered.", Id);
				return;
			}

			if (index > _device.RelayPorts.Count)
			{
				Logger.Error("C3nIoRelayDevice {0} - LatchRelayClosed() - index {1} more than the number of supported relays.", Id, index);
				return;
			}

			_device.RelayPorts[(uint)index]?.Close();
		}

		/// <inheritdoc/>
		public void LatchRelayOpen(int index)
		{
			if (!CheckRegistered())
			{
				Logger.Error("C3nIoRelayDevice {0} - LatchRelayOpen()- device is not registered.", Id);
				return;
			}

			if (index > _device.RelayPorts.Count)
			{
				Logger.Error("C3nIoRelayDevice {0} - LatchRelayOpen() - index {1} more than the number of supported relays.", Id, index);
				return;
			}

			_device.RelayPorts[(uint)index]?.Open();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				if (CheckRegistered())
				{
					foreach (var relay in _device.RelayPorts)
					{
						relay?.Open();
					}

					_device.UnRegister();
					_device.Dispose();
				}
			}

			_disposed = true;
		}

		private void OnlineStatusChangeHandler(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			IsOnline = args.DeviceOnLine;
			NotifyOnlineStatus();
		}

		private void RelayStateChangeHandler(Relay relay, RelayEventArgs args)
		{
			Logger.Debug("C2N-IO {0} relay event: {0} -> {1}", relay.ID, args.State);

			var temp = RelayChanged;
			temp?.Invoke(this, new GenericDualEventArgs<string, int>(Id, (int)relay.ID));
		}

		private bool CheckRegistered()
		{
			return _device.Registered;
		}
	}

}
