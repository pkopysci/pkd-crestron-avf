namespace pkd_hardware_service.EndpointDevices
{
	using Crestron.SimplSharp;
	using Crestron.SimplSharpPro;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.EndpointData;
	using pkd_hardware_service.BaseDevice;
	using System;
	using System.Linq;

	/// <summary>
	/// Endpoint control wrapper for accessing relay, RS-232, and IR controls on a crestron control
	/// processor.
	/// </summary>
	public class ProcessorEndpoint : BaseDevice, IEndpointDevice, IRelayDevice
	{
		private readonly CrestronControlSystem processor;
		private readonly Endpoint data;

		/// <summary>
		/// Initializes a new instance of the <see cref="ProcessorEndpoint"/> class.
		/// </summary>
		/// <param name="data">The config object used to create the device.</param>
		/// <param name="controlSystem">The Crestron control system that will be used control.</param>
		/// <exception cref="ArgumentNullException"> if any argument is null.</exception>
		public ProcessorEndpoint(Endpoint data, CrestronControlSystem controlSystem)
		{
			ParameterValidator.ThrowIfNull(controlSystem, "Ctor", "controlSystem");
			ParameterValidator.ThrowIfNull(data, "Ctor", "data");

			this.data = data;
			this.processor = controlSystem;
			this.Id = data.Id;
			this.Label = this.Id;
		}

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, int>> RelayChanged;

		/// <inheritdoc/>
		public bool IsRegistered { get; private set; }

		/// <inheritdoc/>
		public bool SupportsRelays
		{
			get
			{
				return this.processor.SupportsRelay;
			}
		}

		/// <inheritdoc/>
		public bool SupportsIr
		{
			get
			{
				return this.processor.SupportsIROut;
			}
		}

		/// <inheritdoc/>
		public bool SupportsRs232
		{
			get
			{
				return this.processor.SupportsComPort;
			}
		}

		/// <inheritdoc/>
		public void Register()
		{
			this.IsRegistered = false;

			this.RegisterRelays();
			this.RegisterComPorts();
			this.RegisterIrPorts();

			this.IsRegistered = true;
			this.IsOnline = true;
			this.NotifyOnlineStatus();
		}

		/// <inheritdoc/>
		public bool GetCurrentRelayState(int index)
		{
			if (this.ValidateRelayIndex(index))
			{
				return this.processor.RelayPorts[(uint)index].State;
			}

			return false;
		}

		/// <inheritdoc/>
		public void LatchRelayClosed(int index)
		{

			if (this.ValidateRelayIndex(index) && this.CheckRegistered("LatchRelayClosed"))
			{
				this.processor.RelayPorts[(uint)index].Close();
			}
		}

		/// <inheritdoc/>
		public void LatchRelayOpen(int index)
		{
			if (this.ValidateRelayIndex(index) && this.CheckRegistered("LatchRelayOpen"))
			{
				this.processor.RelayPorts[(uint)index].Open();
			}
		}

		/// <inheritdoc/>
		public void PulseRelay(int index, int timeMs)
		{
			if (this.ValidateRelayIndex(index) && this.CheckRegistered("PulseRelay"))
			{
				this.processor.RelayPorts[(uint)index].Close();
				CTimer t = new CTimer(
					(object sender) =>
					{
						this.processor.RelayPorts[(uint)index].Open();
					}, timeMs);
			}
		}

		private void RelayStateHandler(Relay relay, RelayEventArgs args)
		{
			Logger.Debug("ProcessorEndpoint {0} - RelayStateHandler() - {1}", this.Id, relay.ID);
			var temp = this.RelayChanged;
			temp?.Invoke(this, new GenericDualEventArgs<string, int>(this.Id, (int)relay.ID));
		}

		private void RegisterRelays()
		{
			if (this.SupportsRelays)
			{
				foreach (var relay in this.data.Relays)
				{
					if (relay > 0 && relay <= this.processor.RelayPorts.Count)
					{
						this.processor.RelayPorts[(uint)relay].StateChange += this.RelayStateHandler;
						this.processor.RelayPorts[(uint)relay].Register();
					}
				}
			}
		}

		private void RegisterComPorts()
		{
			if (this.SupportsRs232)
			{
				foreach (var comPort in this.data.Comports)
				{
					if (comPort > 0 && comPort <= this.processor.ComPorts.Count)
					{
						// TODO: Subscribe events and register comport
					}
				}
			}
		}

		private void RegisterIrPorts()
		{
			if (this.SupportsIr)
			{
				foreach (var irp in this.data.IrPorts)
				{
					if (irp > 0 && irp <= this.processor.IROutputPorts.Count)
					{
						// TODO: Subscribe events and register IR ports
					}
				}
			}
		}

		private bool CheckRegistered(string methodName)
		{
			if (!this.IsRegistered)
			{
				Logger.Error(
					"Endpoint {0}.{1} - Device not yet registered.",
					this.Id,
					methodName);
			}

			return this.IsRegistered;
		}

		private bool ValidateRelayIndex(int index)
		{
			if (!this.data.Relays.Contains(index))
			{
				Logger.Error(
					"Endpoing {this.Id}.GetCurrentRelayState() - relay {0} is not registered to this device.",
					index);
				return false;
			}

			return true;
		}
	}
}
