namespace pkd_application_service.Base
{
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data;
	using pkd_hardware_service.BaseDevice;
	using System;
	using System.Collections.ObjectModel;
	using System.Linq;

	/// <summary>
	/// Data container object that assists in storing a data type and referencing it by ID.
	/// </summary>
	/// <typeparam name="TDevice">Defines what type of device will be stored in this container.</typeparam>
	/// <typeparam name="TDeviceData">Existing domain data objects representing the devices.</typeparam>
	public class BaseApp<TDevice, TDeviceData> : IDisposable
		where TDevice : IBaseDevice
		where TDeviceData : BaseData
	{
		protected readonly DeviceContainer<TDevice> devices;
		protected readonly ReadOnlyCollection<TDeviceData> data;

		private bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseApp{TDevice, TDeviceData}"/> class.
		/// </summary>
		/// <param name="devices">The collection of devices that will be controlled by the app.</param>
		/// <param name="data">The collection of config data associated with the devices being controlled.</param>
		public BaseApp(DeviceContainer<TDevice> devices, ReadOnlyCollection<TDeviceData> data)
		{
			ParameterValidator.ThrowIfNull(devices, "Ctor", "devices");
			ParameterValidator.ThrowIfNull(data, "Ctor", "data");
			this.devices = devices;
			this.data = data;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="BaseApp{TDevice}"/> class.
		/// </summary>
		~BaseApp()
		{
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Attempt to read the collection of devices and find the specific device.
		/// </summary>
		/// <param name="id">The unique ID of the device to find.</param>
		/// <returns>The device control object if found, otherwise null.</returns>
		protected TDevice GetDevice(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "FindDevice", "id");
			return this.devices.GetDevice(id);
		}

		/// <summary>
		/// Get the configuration information for the target device.
		/// </summary>
		/// <param name="id">The unique ID of the devices to look for.</param>
		/// <returns>The configuration info for the device, or null if the device cannot be found.</returns>
		protected TDeviceData GetDeviceInfo(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetDeviceInfo", "id");

			try
			{
				return this.data.First(x => x.Id == id);
			}
			catch (InvalidOperationException)
			{
				return default;
			}
		}

		/// <summary>
		/// Get all of the devices in the collection
		/// </summary>
		/// <returns>the collection of devices.</returns>
		protected ReadOnlyCollection<TDevice> GetAllDevices()
		{
			return this.devices.GetAllDevices();
		}

		protected void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					// TODO: dispose any managed objects.
				}

				this.disposed = true;
			}
		}
	}
}
