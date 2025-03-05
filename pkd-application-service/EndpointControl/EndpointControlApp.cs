using System.Collections.ObjectModel;
using pkd_application_service.Base;
using pkd_common_utils.GenericEventArgs;
using pkd_domain_service.Data.EndpointData;
using pkd_hardware_service.BaseDevice;
using pkd_hardware_service.EndpointDevices;

namespace pkd_application_service.EndpointControl
{
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
			RegisterHandlers();
		}

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, int>>? EndpointRelayChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>>? EndpointConnectionChanged;

		/// <inheritdoc/>
		public void LatchRelayClosed(string id, int index)
		{
			var found = GetDevice(id);
			if (found is IRelayDevice relayDev)
			{
				relayDev.LatchRelayClosed(index);
			}
		}

		/// <inheritdoc/>
		public void LatchRelayOpen(string id, int index)
		{
			var found = GetDevice(id);
			if (found is IRelayDevice relayDev)
			{
				relayDev.LatchRelayOpen(index);
			}
		}

		/// <inheritdoc/>
		public void PulseEndpointRelay(string id, int index, int timeMs)
		{
			var found = GetDevice(id);
			if (found is IRelayDevice relayDev)
			{
				relayDev.PulseRelay(index, timeMs);
			}
		}

		private void RegisterHandlers()
		{
			foreach (var device in GetAllDevices())
			{
				device.ConnectionChanged += Device_ConnectionChanged;
				if (device is IRelayDevice relayDev)
				{
					// TODO: Support other endpoint device types
					relayDev.RelayChanged += RelayDev_RelayChanged;
				}
			}
		}

		private void RelayDev_RelayChanged(object? sender, GenericDualEventArgs<string, int> e)
		{
			var temp = EndpointRelayChanged;
			temp?.Invoke(this, e);
		}

		private void Device_ConnectionChanged(object? sender, GenericSingleEventArgs<string> e)
		{
			if (sender is not IEndpointDevice endpointDev) return;
			var temp = EndpointConnectionChanged;
			temp?.Invoke(
				this,
				new GenericDualEventArgs<string, bool>(endpointDev.Id, endpointDev.IsOnline));
		}
	}
}
