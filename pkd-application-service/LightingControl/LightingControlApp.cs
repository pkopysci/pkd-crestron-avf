using System.Collections.ObjectModel;
using pkd_application_service.Base;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_domain_service.Data.LightingData;
using pkd_hardware_service.BaseDevice;
using pkd_hardware_service.LightingDevices;

namespace pkd_application_service.LightingControl
{
	/// <summary>
	/// Application control class for handling lighting requests and events.
	/// </summary>
	public class LightingControlApp : BaseApp<ILightingDevice, LightingInfo>, ILightingControlApp
	{
		private IApplicationService? parent;
		private readonly ReadOnlyCollection<LightingControlInfoContainer> controllers;

		/// <summary>
		/// Instantiates a new instance of <see cref="LightingControlApp"/>.
		/// </summary>
		/// <param name="devices">Hardware control objects for all lighting controllers in the system.</param>
		/// <param name="domainData">Configuration data for all lighting devices in the system.</param>
		/// <param name="parent">The root application service associated with this lighting manager.</param>
		public LightingControlApp(
			DeviceContainer<ILightingDevice> devices,
			ReadOnlyCollection<LightingInfo> domainData,
			IApplicationService parent)
			: base(devices, domainData)
		{
			ParameterValidator.ThrowIfNull(parent, "LightingControlApp.Ctor", nameof(parent));

			this.parent = parent;
			List<LightingControlInfoContainer> deviceList = [];
			foreach (var device in domainData)
			{
				List<LightingItemInfoContainer> zoneContainers = [];
				List<LightingItemInfoContainer> sceneContainers = [];
				foreach (var zone in device.Zones)
				{
					zoneContainers.Add(new LightingItemInfoContainer(zone.Id, zone.Label, "", [], zone.Index));
				}

				foreach (var scene in device.Scenes)
				{
					sceneContainers.Add(new LightingItemInfoContainer(scene.Id, scene.Label, "", scene.Tags, scene.Index));
				}

				deviceList.Add(new LightingControlInfoContainer(
					device.Id,
					device.Label,
					string.Empty,
					device.StartupSceneId,
					device.ShutdownSceneId,
					device.Tags,
					zoneContainers,
					sceneContainers)
				{
					Manufacturer = device.Manufacturer,
					Model = device.Model,
				});
			}

			controllers = new ReadOnlyCollection<LightingControlInfoContainer>(deviceList);
			RegisterHandlers();
		}


		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>>? LightingLoadLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? LightingSceneChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>>? LightingControlConnectionChanged;

		/// <inheritdoc/>
		public void RecallLightingScene(string deviceId, string sceneId)
		{
			Logger.Debug("Application.LightingControlApp.RecallLightingScene({0}, {1})", deviceId, sceneId);


			var found = Devices.GetDevice(deviceId);
			if (found == null)
			{
				Logger.Warn("LightingControlApp.RecallLightingScene() - No lighting device with id {0} found.", deviceId);
				return;
			}

			found.RecallScene(sceneId);
		}

		/// <inheritdoc/>
		public void SetLightingLoad(string deviceId, string zoneId, int level)
		{
			var found = Devices.GetDevice(deviceId);
			if (found == null)
			{
				Logger.Warn("LightingControlApp.SetLightingLoad() - No lighting device with id {0} found.", deviceId);
				return;
			}

			found.SetZoneLoad(zoneId, level);
		}

		/// <inheritdoc/>
		public string GetActiveScene(string deviceId)
		{
			var found = Devices.GetDevice(deviceId);
			if (found == null)
			{
				Logger.Warn("LightingControlApp.GetActiveScene() - No lighting device with id {0} found.", deviceId);
				return string.Empty;
			}

			return found.ActiveSceneId;
		}

		/// <inheritdoc/>
		public int GetZoneLoad(string deviceId, string zoneId)
		{
			var found = Devices.GetDevice(deviceId);
			if (found == null)
			{
				Logger.Warn("LightingControlApp.GetZoneLoad() - No lighting device with id {0} found.", deviceId);
				return 0;
			}

			return found.GetZoneLoad(zoneId);
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<LightingControlInfoContainer> GetAllLightingDeviceInfo()
		{
			return controllers;
		}

		private void RegisterHandlers()
		{
			foreach (var device in GetAllDevices())
			{
				device.ActiveSceneChanged += DeviceActiveSceneChanged;
				device.ZoneLoadChanged += DeviceZoneLoadChanged;
				device.ConnectionChanged += DeviceConnectionChanged;
			}
		}

		private void DeviceConnectionChanged(object? sender, GenericSingleEventArgs<string> e)
		{
			if (sender is not ILightingDevice lighting) return;
			var temp = LightingControlConnectionChanged;
			temp?.Invoke(this, new GenericDualEventArgs<string, bool>(e.Arg, lighting.IsOnline));
		}

		private void DeviceZoneLoadChanged(object? sender, GenericSingleEventArgs<string> e)
		{
			if (sender is not ILightingDevice lighting) return;
			var temp = LightingLoadLevelChanged;
			temp?.Invoke(this, new GenericDualEventArgs<string, string>(lighting.Id, e.Arg));
		}

		private void DeviceActiveSceneChanged(object? sender, GenericSingleEventArgs<string> e)
		{
			if (sender is not ILightingDevice lighting) return;
			var temp = LightingSceneChanged;
			temp?.Invoke(this, new GenericSingleEventArgs<string>(lighting.Id));
		}
	}
}
