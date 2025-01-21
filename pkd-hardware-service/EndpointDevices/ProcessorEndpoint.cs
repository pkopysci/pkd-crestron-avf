namespace pkd_hardware_service.EndpointDevices
{
	using Crestron.SimplSharp;
	using Crestron.SimplSharpPro;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.EndpointData;
	using BaseDevice;
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
			processor = controlSystem;
			Id = data.Id;
			Label = Id;
		}

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, int>>? RelayChanged;

		/// <inheritdoc/>
		public bool IsRegistered { get; private set; }

		/// <inheritdoc/>
		public bool SupportsRelays => processor.SupportsRelay;

		/// <inheritdoc/>
		public bool SupportsIr => processor.SupportsIROut;

		/// <inheritdoc/>
		public bool SupportsRs232 => processor.SupportsComPort;

		/// <inheritdoc/>
		public void Register()
		{
			IsRegistered = false;

			RegisterRelays();
			RegisterComPorts();
			RegisterIrPorts();

			IsRegistered = true;
			IsOnline = true;
			NotifyOnlineStatus();
		}

		/// <inheritdoc/>
		public bool? GetCurrentRelayState(int index)
		{
			return ValidateRelayIndex(index) ? processor.RelayPorts[(uint)index]?.State : false;
		}

		/// <inheritdoc/>
		public void LatchRelayClosed(int index)
		{

			if (ValidateRelayIndex(index) && CheckRegistered("LatchRelayClosed"))
			{
				processor.RelayPorts[(uint)index]?.Close();
			}
		}

		/// <inheritdoc/>
		public void LatchRelayOpen(int index)
		{
			if (ValidateRelayIndex(index) && CheckRegistered("LatchRelayOpen"))
			{
				processor.RelayPorts[(uint)index]?.Open();
			}
		}

		/// <inheritdoc/>
		public void PulseRelay(int index, int timeMs)
		{
			if (!ValidateRelayIndex(index) || !CheckRegistered("PulseRelay")) return;
			processor.RelayPorts[(uint)index]?.Close();
			var t = new CTimer(
				sender =>
				{
					processor.RelayPorts[(uint)index]?.Open();
				}, timeMs);
		}

		private void RelayStateHandler(Relay relay, RelayEventArgs args)
		{
			Logger.Debug("ProcessorEndpoint {0} - RelayStateHandler() - {1}", Id, relay.ID);
			var temp = RelayChanged;
			temp?.Invoke(this, new GenericDualEventArgs<string, int>(Id, (int)relay.ID));
		}

		private void RegisterRelays()
		{
			if (!SupportsRelays) return;
			foreach (var relay in data.Relays)
			{
				if (relay <= 0 || relay > processor.RelayPorts.Count) continue;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
				processor.RelayPorts[(uint)relay].StateChange += RelayStateHandler;
				processor.RelayPorts[(uint)relay].Register();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
			}
		}

		private void RegisterComPorts()
		{
			if (!SupportsRs232) return;
			foreach (var comPort in data.Comports)
			{
				if (comPort > 0 && comPort <= processor.ComPorts.Count)
				{
					// TODO: Subscribe events and register comport
				}
			}
		}

		private void RegisterIrPorts()
		{
			if (!SupportsIr) return;
			foreach (var irp in data.IrPorts)
			{
				if (irp > 0 && irp <= processor.IROutputPorts.Count)
				{
					// TODO: Subscribe events and register IR ports
				}
			}
		}

		private bool CheckRegistered(string methodName)
		{
			if (!IsRegistered)
			{
				Logger.Error(
					"Endpoint {0}.{1} - Device not yet registered.",
					Id,
					methodName);
			}

			return IsRegistered;
		}

		private bool ValidateRelayIndex(int index)
		{
			if (!data.Relays.Contains(index))
			{
				Logger.Error(
					"Endpoint {Id}.GetCurrentRelayState() - relay {0} is not registered to this device.",
					index);
				return false;
			}

			return true;
		}
	}
}
