﻿namespace pkd_hardware_service.EndpointDevices
{
	using Crestron.SimplSharp;
	using Crestron.SimplSharpPro;
	using Crestron.SimplSharpPro.GeneralIO;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.EndpointData;
	using BaseDevice;
	using System;


	/// <summary>
	/// Crestron CEN-IO-RY-401 relay controller via ethernet.
	/// </summary>

	public class CenIoRy401RelayDevice : BaseDevice, IEndpointDevice, IRelayDevice, IDisposable
	{
		private readonly CenIoRy104 device;
		private bool disposed;
		
		/// <param name="data">configuration data for the relay device.</param>
		/// <param name="controlSystem">root crestron control system.</param>
		public CenIoRy401RelayDevice(Endpoint data, CrestronControlSystem controlSystem)
		{
			ParameterValidator.ThrowIfNull(controlSystem, "Ctor", nameof(controlSystem));
			ParameterValidator.ThrowIfNull(data, "Ctor", nameof(data));

			Id = data.Id;
			Label = data.Id;
			device = new CenIoRy104((uint)data.Port, controlSystem);
			device.OnlineStatusChange += OnlineStatusChangeHandler;
			
			foreach (var relay in device.RelayPorts)
			{
				if (relay == null) continue;
				relay.StateChange += RelayStateChangeHandler;
			}
		}

		/// <inheritdoc />
		~CenIoRy401RelayDevice()
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
			Logger.Debug("CenIoRy401RelayDevice {0} - Register()");

			if (!CheckRegistered())
			{
				Logger.Error("CenIoRy401RelayDevice {0} is already registered.", Id);
				return;
			}

			if (device.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Error("CenIoRy401RelayDevice {0} - failed to register with control system: {0}", device.RegistrationFailureReason);
			}
		}

		/// <inheritdoc/>
		public bool? GetCurrentRelayState(int index)
		{
			if (!CheckRegistered())
			{
				return false;
			}

			return index <= device.RelayPorts.Count ? device.RelayPorts[(uint)index]?.State : false;
		}

		/// <inheritdoc/>
		public void PulseRelay(int index, int timeMs)
		{
			Logger.Debug("CenIoRy401RelayDevice {0} - PulseRelay({0}, {1})", index, timeMs);

			if (!CheckRegistered())
			{
				Logger.Error("CenIoRy401RelayDevice {0} - PulseRelay()- device is not registered.", Id);
				return;
			}

			if (index > device.RelayPorts.Count)
			{
				Logger.Error("CenIoRy401RelayDevice {0} - PulseRelay() - index {1} more than the number of supported relays.", Id, index);
				return;
			}

			device.RelayPorts[(uint)index]?.Close();
			CTimer t = new CTimer((obj) =>
			{
				if (!CheckRegistered())
				{
					return;
				}

				device.RelayPorts[(uint)index]?.Open();
			}, timeMs);
		}

		/// <inheritdoc/>
		public void LatchRelayClosed(int index)
		{
			if (!CheckRegistered())
			{
				Logger.Error("CenIoRy401RelayDevice {0} - PulseRelay()- device is not registered.", Id);
				return;
			}

			if (index > device.RelayPorts.Count)
			{
				Logger.Error("CenIoRy401RelayDevice {0} - LatchRelayClosed() - index {1} more than the number of supported relays.", Id, index);
				return;
			}

			device.RelayPorts[(uint)index]?.Close();
		}

		/// <inheritdoc/>
		public void LatchRelayOpen(int index)
		{
			if (!CheckRegistered())
			{
				Logger.Error("CenIoRy401RelayDevice {0} - LatchRelayOpen()- device is not registered.", Id);
				return;
			}

			if (index > device.RelayPorts.Count)
			{
				Logger.Error("CenIoRy401RelayDevice {0} - LatchRelayOpen() - index {1} more than the number of supported relays.", Id, index);
				return;
			}

			device.RelayPorts[(uint)index]?.Open();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				if (CheckRegistered())
				{
					foreach (var relay in device.RelayPorts)
					{
						relay?.Open();
					}

					device.UnRegister();
					device.Dispose();
				}
			}

			disposed = true;
		}

		private void OnlineStatusChangeHandler(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			IsOnline = args.DeviceOnLine;
			NotifyOnlineStatus();
		}

		private void RelayStateChangeHandler(Relay relay, RelayEventArgs args)
		{
			Logger.Debug("CenIoRy401RelayDevice {0} relay event: {0} -> {1}", relay.ID, args.State);

			var temp = RelayChanged;
			temp?.Invoke(this, new GenericDualEventArgs<string, int>(Id, (int)relay.ID));
		}

		private bool CheckRegistered()
		{
			return device.Registered;
		}
	}

}
