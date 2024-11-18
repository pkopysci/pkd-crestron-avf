namespace pkd_application_service.EndpointControl
{
	using pkd_application_service.Base;
	using pkd_common_utils.GenericEventArgs;
	using pkd_domain_service.Data.EndpointData;
	using pkd_hardware_service.BaseDevice;
	using pkd_hardware_service.EndpointDevices;
	using System;
	using System.Collections.ObjectModel;

	/// <summary>
	/// Endpoint device control application.
	/// </summary>
	internal sealed class EndpointControlApp : BaseApp<IEndpointDevice, Endpoint>, IEndpointControlApp
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EndpointControlApp"/> class.
		/// </summary>
		/// <param name="devices">System collection of endpoint control objects.</param>
		/// <param name="data">Configuration data for all endpoints in the system.</param>
		public EndpointControlApp(DeviceContainer<IEndpointDevice> devices, ReadOnlyCollection<Endpoint> data)
			: base(devices, data)
		{
			this.ReigsterHandlers();
		}

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, int>> EndpointRelayChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>> EndpointConnectionChanged;

		/// <inheritdoc/>
		public void LatchRelayClosed(string id, int index)
		{
			var found = this.GetDevice(id);
			if (found != default(IEndpointDevice) && found is IRelayDevice relayDev)
			{
				relayDev.LatchRelayClosed(index);
			}
		}

		/// <inheritdoc/>
		public void LatchRelayOpen(string id, int index)
		{
			var found = this.GetDevice(id);
			if (found != default(IEndpointDevice) && found is IRelayDevice relayDev)
			{
				relayDev.LatchRelayOpen(index);
			}
		}

		/// <inheritdoc/>
		public void PulseEndpointRelay(string id, int index, int timeMs)
		{
			var found = this.GetDevice(id);
			if (found != default(IEndpointDevice) && found is IRelayDevice relayDev)
			{
				relayDev.PulseRelay(index, timeMs);
			}
		}

		private void ReigsterHandlers()
		{
			foreach (var device in this.GetAllDevices())
			{
				device.ConnectionChanged += this.Device_ConnectionChanged;
				if (device is IRelayDevice relayDev)
				{
					// TODO: Support other endpoint device types
					relayDev.RelayChanged += this.RelayDev_RelayChanged;
				}
			}
		}

		private void RelayDev_RelayChanged(object sender, GenericDualEventArgs<string, int> e)
		{
			var temp = this.EndpointRelayChanged;
			temp?.Invoke(this, e);
		}

		private void Device_ConnectionChanged(object sender, GenericSingleEventArgs<string> e)
		{
			if (sender is IEndpointDevice endpointDev)
			{
				var temp = this.EndpointConnectionChanged;
				temp?.Invoke(
						this,
						new GenericDualEventArgs<string, bool>(endpointDev.Id, endpointDev.IsOnline));
			}
		}
	}
}
