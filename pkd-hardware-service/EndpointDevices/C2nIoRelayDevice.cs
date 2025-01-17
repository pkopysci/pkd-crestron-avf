namespace pkd_hardware_service.EndpointDevices
{
	using Crestron.SimplSharp;
	using Crestron.SimplSharpPro;
	using Crestron.SimplSharpPro.GeneralIO;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.EndpointData;
	using pkd_hardware_service.BaseDevice;
	using System;

	/// <summary>
	/// Crestron C2N-IO relay controller via Cresnet.
	/// </summary>
	public class C2nIoRelayDevice : BaseDevice, IEndpointDevice, IRelayDevice, IDisposable
	{
		private C2nIo device;
		private readonly Endpoint data;
		private readonly CrestronControlSystem processor;
		private bool disposed;

		/// <summary>
		/// Creates an instance of <see cref="C2nIoRelayDevice"/>.
		/// </summary>
		/// <param name="data">The configuration data for this endpoint device.</param>
		/// <param name="controlSystem">The control system that's running the framework program.</param>
		public C2nIoRelayDevice(Endpoint data, CrestronControlSystem controlSystem)
		{
			ParameterValidator.ThrowIfNull(controlSystem, "Ctor", "controlSystem");
			ParameterValidator.ThrowIfNull(data, "Ctor", "data");

			this.data = data;
			this.processor = controlSystem;
			this.Id = data.Id;
			this.Label = data.Id;
			this.device = new C2nIo((uint)this.data.Port, this.processor);
			this.device.OnlineStatusChange += new OnlineStatusChangeEventHandler(OnlineStatusChangeHandler);
			foreach (var relay in this.device.RelayPorts)
			{
				relay.StateChange += new RelayEventHandler(RelayStateChangeHandler);
			}
		}

		~C2nIoRelayDevice()
		{
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, int>> RelayChanged;

		/// <inheritdoc/>
		public bool IsRegistered
		{
			get { return this.CheckRegistered(); }
		}

		/// <inheritdoc/>
		public bool SupportsRelays
		{
			get { return true; }
		}

		/// <inheritdoc/>
		public bool SupportsIr
		{
			get { return false; }
		}

		/// <inheritdoc/>
		public bool SupportsRs232
		{
			get { return false; }
		}

		/// <inheritdoc/>
		public void Register()
		{
			Logger.Debug("C2nIORelayDevice {0} - Register()");

			if (!this.CheckRegistered())
			{
				Logger.Error("C2nIoRelayDevice {0} is already registered.", this.Id);
				return;
			}

			if (this.device.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Error("C2nIoRelayDevice {0} - failed to register with control system: {0}", this.device.RegistrationFailureReason);
			}
		}

		/// <inheritdoc/>
		public bool GetCurrentRelayState(int index)
		{
			if (!this.CheckRegistered())
			{
				return false;
			}

			if (index <= this.device.RelayPorts.Count)
			{
				return this.device.RelayPorts[(uint)index].State;
			}
			else
			{
				return false;
			}
		}

		/// <inheritdoc/>
		public void PulseRelay(int index, int timeMs)
		{
			Logger.Debug("C2nIORelayDevice {0} - PulseRelay({0}, {1})", index, timeMs);

			if (!this.CheckRegistered())
			{
				Logger.Error("C3nIoRelayDevice {0} - PulseRelay()- device is not registered.", this.Id);
				return;
			}

			if (index > this.device.RelayPorts.Count)
			{
				Logger.Error("C3nIoRelayDevice {0} - PulseRelay() - index {1} more than the number of supported relays.", this.Id, index);
				return;
			}

			this.device.RelayPorts[(uint)index].Close();
			CTimer t = new CTimer((obj) =>
			{
				if (!this.CheckRegistered())
				{
					return;
				}

				this.device.RelayPorts[(uint)index].Open();
			}, timeMs);
		}

		/// <inheritdoc/>
		public void LatchRelayClosed(int index)
		{
			if (!this.CheckRegistered())
			{
				Logger.Error("C3nIoRelayDevice {0} - PulseRelay()- device is not registered.", this.Id);
				return;
			}

			if (index > this.device.RelayPorts.Count)
			{
				Logger.Error("C3nIoRelayDevice {0} - LatchRelayClosed() - index {1} more than the number of supported relays.", this.Id, index);
				return;
			}

			this.device.RelayPorts[(uint)index].Close();
		}

		/// <inheritdoc/>
		public void LatchRelayOpen(int index)
		{
			if (!this.CheckRegistered())
			{
				Logger.Error("C3nIoRelayDevice {0} - LatchRelayOpen()- device is not registered.", this.Id);
				return;
			}

			if (index > this.device.RelayPorts.Count)
			{
				Logger.Error("C3nIoRelayDevice {0} - LatchRelayOpen() - index {1} more than the number of supported relays.", this.Id, index);
				return;
			}

			this.device.RelayPorts[(uint)index].Open();
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}

			if (disposing)
			{
				if (this.CheckRegistered())
				{
					foreach (var relay in this.device.RelayPorts)
					{
						relay.Open();
					}

					this.device.UnRegister();
					this.device.Dispose();
					this.device = null;
				}
			}

			this.disposed = true;
		}

		private void OnlineStatusChangeHandler(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			this.IsOnline = args.DeviceOnLine;
			this.NotifyOnlineStatus();
		}

		private void RelayStateChangeHandler(Relay relay, RelayEventArgs args)
		{
			Logger.Debug("C2N-IO {0} relay event: {0} -> {1}", relay.ID, args.State);

			var temp = this.RelayChanged;
			temp?.Invoke(this, new GenericDualEventArgs<string, int>(this.Id, (int)relay.ID));
		}

		private bool CheckRegistered()
		{
			return this.device != null || this.device.Registered;
		}
	}

}
