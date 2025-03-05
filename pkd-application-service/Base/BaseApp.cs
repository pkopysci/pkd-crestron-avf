using System.Collections.ObjectModel;
using pkd_common_utils.Validation;
using pkd_domain_service.Data;
using pkd_hardware_service.BaseDevice;

namespace pkd_application_service.Base
{
	/// <summary>
	/// Data container object that assists in storing a data type and referencing it by ID.
	/// </summary>
	/// <typeparam name="TDevice">Defines what type of device will be stored in this container.</typeparam>
	/// <typeparam name="TDeviceData">Existing domain data objects representing the devices.</typeparam>
	public class BaseApp<TDevice, TDeviceData> : IDisposable
		where TDevice : IBaseDevice
		where TDeviceData : BaseData
	{
		/// <summary>
		/// Container for managing the hardware interaction objects.
		/// </summary>
		protected readonly DeviceContainer<TDevice> Devices;
		
		/// <summary>
		/// Collection of configuration data representing the devices.
		/// </summary>
		protected readonly ReadOnlyCollection<TDeviceData> Data;
		private bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseApp{TDevice, TDeviceData}"/> class.
		/// </summary>
		/// <param name="devices">The collection of devices that will be controlled by the app.</param>
		/// <param name="data">The collection of config data associated with the devices being controlled.</param>
		public BaseApp(DeviceContainer<TDevice> devices, ReadOnlyCollection<TDeviceData> data)
		{
			ParameterValidator.ThrowIfNull(devices, "Ctor", nameof(devices));
			ParameterValidator.ThrowIfNull(data, "Ctor", nameof(data));
			Devices = devices;
			Data = data;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="BaseApp{TDevice, TDeviceData}"/> class.
		/// </summary>
		~BaseApp()
		{
			Dispose(false);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Attempt to read the collection of devices and find the specific device.
		/// </summary>
		/// <param name="id">The unique ID of the device to find.</param>
		/// <returns>The device control object if found, otherwise null.</returns>
		protected TDevice? GetDevice(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "FindDevice", "id");
			return Devices.GetDevice(id);
		}

		/// <summary>
		/// Get the configuration information for the target device.
		/// </summary>
		/// <param name="id">The unique ID of the devices to look for.</param>
		/// <returns>The configuration info for the device, or null if the device cannot be found.</returns>
		protected TDeviceData? GetDeviceInfo(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetDeviceInfo", "id");

			try
			{
				return Data.First(x => x.Id == id);
			}
			catch (InvalidOperationException)
			{
				return null;
			}
		}

		/// <summary>
		/// Get all the devices in the collection
		/// </summary>
		/// <returns>the collection of devices.</returns>
		protected ReadOnlyCollection<TDevice> GetAllDevices()
		{
			return Devices.GetAllDevices();
		}

		/// <summary>
		/// Dispose of all managed objects.
		/// </summary>
		/// <param name="disposing">true = this device is disposing, false = not disposing.</param>
		protected void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing)
			{
				// dispose any managed objects.
			}

			disposed = true;
		}
	}
}
