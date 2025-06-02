using System.Collections.ObjectModel;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;

namespace pkd_hardware_service.BaseDevice
{
	/// <summary>
	/// Manager class for adding, removing, and finding a device of the given type.
	/// </summary>
	/// <typeparam name="T">The device type that will be managed by this container.</typeparam>
	public class DeviceContainer<T> : IDisposable
	{
		private readonly Dictionary<string, T> devices = [];
		private bool disposed;

		/// <summary>
		/// Finalizes an instance of the <see cref="DeviceContainer{T}"/> class.
		/// </summary>
		~DeviceContainer()
		{
			Dispose(false);
		}

		/// <summary>
		/// Attempt to retrieve the device with the given ID.
		/// Writes an error to the logging system if no device is found at the given ID.
		/// </summary>
		/// <param name="id">The unique ID of the device to get.</param>
		/// <returns>The device object if found, otherwise the default object of that type.</returns>
		public T? GetDevice(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetDevices()", nameof(id));
			if (devices.TryGetValue(id, out T? value))
			{
				return value;
			}
			else
			{
                Logger.Error(string.Format(
                    "DeviceContainer<{0}>.GetDevice() - No device with ID {1}",
                    typeof(T),
                    id));

                return default;
            }
		}

		/// <summary>
		/// Gets a collection of all devices currently stored in this container.
		/// </summary>
		/// <returns>A collection of all currently stored devices.</returns>
		public ReadOnlyCollection<T> GetAllDevices()
		{
			return devices.Values.ToList().AsReadOnly();
		}

		/// <summary>
		/// Checks to see if a device exists with the given ID.
		/// </summary>
		/// <param name="id">The unique ID of the device to check.</param>
		/// <returns>true if the device is found, false otherwise.</returns>
		public bool ContainsDevice(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "ContainsDevice", nameof(id));
			return devices.ContainsKey(id);
		}

		/// <summary>
		/// Adds a new device to the container. If a device with the matching id already exists
		/// then it will be replaced.
		/// </summary>
		/// <param name="id">The unique ID used to reference the device.</param>
		/// <param name="device">The device that will be added to the container.</param>
		public void AddDevice(string id, T device)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "ContainsDevice()", nameof(id));
			ParameterValidator.ThrowIfNull(device, "AddDevice()", nameof(device));

			if (!devices.TryAdd(id, device))
			{
                devices[id] = device;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose stored objects.
		/// </summary>
		/// <param name="disposing">Flag indicating disposing state.</param>
		private void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing)
			{
				foreach (var dev in GetAllDevices())
				{
					if (dev is IDisposable disposable)
					{
						disposable.Dispose();
					}	
				}
			}

			disposed = true;
		}
	}
}
