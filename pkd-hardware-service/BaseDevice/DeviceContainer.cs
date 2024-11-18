namespace pkd_hardware_service.BaseDevice
{
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	/// <summary>
	/// Manager class for adding, removing, and finding a device of the given type.
	/// </summary>
	/// <typeparam name="T">The device type that will be managed by this container.</typeparam>
	public class DeviceContainer<T> : IDisposable
	{
		private readonly Dictionary<string, T> devices = new Dictionary<string, T>();
		private bool disposed;

		/// <summary>
		/// Finalizes an instance of the <see cref="DeviceContainer{T}"/> class.
		/// </summary>
		~DeviceContainer()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// Attempt to retrieve the device with the given ID.
		/// Writes an error to the logging system if no device is found at the given ID.
		/// </summary>
		/// <param name="id">The unique ID of the device to get.</param>
		/// <returns>The device object if found, otherwise the default object of that type.</returns>
		public T GetDevice(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetDevices()", "id");

			if (!this.devices.ContainsKey(id))
			{
				Logger.Error(string.Format(
					"DeviceContainer<{0}>.GetDevice() - No device with ID {1}",
					typeof(T),
					id));

				return default;
			}

			return this.devices[id];
		}

		/// <summary>
		/// Gets a collection of all devices currently stored in this container.
		/// </summary>
		/// <returns>A collection of all currently stored devices.</returns>
		public ReadOnlyCollection<T> GetAllDevices()
		{
			return this.devices.Values.ToList().AsReadOnly();
		}

		/// <summary>
		/// Checks to see if a device exists with the given ID.
		/// </summary>
		/// <param name="id">The unique ID of the device to check.</param>
		/// <returns>true if the device is found, false otherwise.</returns>
		public bool ContainsDevice(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "ContainsDevice", "id");
			return this.devices.ContainsKey(id);
		}

		/// <summary>
		/// Adds a new device to the container. If a device with the matching id already exists
		/// then it will be replaced.
		/// </summary>
		/// <param name="id">The unique ID used to reference the device.</param>
		/// <param name="device">The device that will be added to the container.</param>
		public void AddDevice(string id, T device)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "ContainsDevice()", "id");
			ParameterValidator.ThrowIfNull(device, "AddDevice()", "device");

			if (this.devices.ContainsKey(id))
			{
				this.devices[id] = device;
			}
			else
			{
				this.devices.Add(id, device);
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose stored objects.
		/// </summary>
		/// <param name="disposing">Flag indicating disposing state.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					foreach (var dev in this.GetAllDevices())
					{
						if (dev is IDisposable)
						{
							(dev as IDisposable).Dispose();
						}
					}
				}

				this.disposed = true;
			}
		}
	}

}
