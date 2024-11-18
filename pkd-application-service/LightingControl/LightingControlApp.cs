namespace pkd_application_service.LightingControl
{
	using pkd_application_service.Base;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.LightingData;
	using pkd_hardware_service.BaseDevice;
	using pkd_hardware_service.LightingDevices;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;

	public class LightingControlApp : BaseApp<ILightingDevice, LightingInfo>, ILightingControlApp
	{
		private IApplicationService parent;
		private readonly ReadOnlyCollection<LightingControlInfoContainer> controllers;

		public LightingControlApp(
			DeviceContainer<ILightingDevice> devices,
			ReadOnlyCollection<LightingInfo> domainData,
			IApplicationService parent)
			: base(devices, domainData)
		{

			ParameterValidator.ThrowIfNull(parent, "LightingControlApp.Ctor", "parent");

			this.parent = parent;
			List<LightingControlInfoContainer> deviceList = new List<LightingControlInfoContainer>();
			foreach (var device in domainData)
			{
				List<LightingItemInfoContainer> zoneContainers = new List<LightingItemInfoContainer>();
				List<LightingItemInfoContainer> sceneContainers = new List<LightingItemInfoContainer>();
				foreach (var zone in device.Zones)
				{
					zoneContainers.Add(new LightingItemInfoContainer(zone.Id, zone.Label, "", new List<string>(), zone.Index));
				}

				foreach (var scene in device.Scenes)
				{
					sceneContainers.Add(new LightingItemInfoContainer(scene.Id, scene.Label, "", scene.Tags, scene.Index));
				}

				deviceList.Add(new LightingControlInfoContainer(
					device.Id,
					device.Label,
					"",
					device.StartupSceneId,
					device.ShutdownSceneId,
					device.Tags,
					zoneContainers,
					sceneContainers));
			}

			this.controllers = new ReadOnlyCollection<LightingControlInfoContainer>(deviceList);
			this.RegisterHandlers();
		}


		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>> LightingLoadLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> LightingSceneChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, bool>> LightingControlConnectionChanged;

		/// <inheritdoc/>
		public void RecallLightingScene(string deviceId, string sceneId)
		{
			Logger.Debug("Application.LightingControlApp.RecallLightingScene({0}, {1})", deviceId, sceneId);


			var found = this.devices.GetDevice(deviceId);
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
			var found = this.devices.GetDevice(deviceId);
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
			var found = this.devices.GetDevice(deviceId);
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
			var found = this.devices.GetDevice(deviceId);
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
			return this.controllers;
		}

		private void RegisterHandlers()
		{
			foreach (var device in this.GetAllDevices())
			{
				device.ActiveSceneChanged += this.DeviceActiveSceneChanged;
				device.ZoneLoadChanged += this.DeviceZoneLoadChanged;
				device.ConnectionChanged += this.DeviceConnectionChanged;
			}
		}

		private void DeviceConnectionChanged(object sender, GenericSingleEventArgs<string> e)
		{
			ILightingDevice lighting = sender as ILightingDevice;
			if (sender == null)
			{
				return;
			}

			var temp = this.LightingControlConnectionChanged;
			temp?.Invoke(this, new GenericDualEventArgs<string, bool>(e.Arg, lighting.IsOnline));
		}

		private void DeviceZoneLoadChanged(object sender, GenericSingleEventArgs<string> e)
		{
			if (!(sender is ILightingDevice device))
			{
				return;
			}

			var temp = this.LightingLoadLevelChanged;
			temp?.Invoke(this, new GenericDualEventArgs<string, string>(device.Id, e.Arg));
		}

		private void DeviceActiveSceneChanged(object sender, GenericSingleEventArgs<string> e)
		{
			if (!(sender is ILightingDevice device))
			{
				return;
			}

			try
			{

				Logger.Debug("LightingControlApp.DeviceActiveSceneChanged. Device ID = {0}", device.Id);
			}
			catch (Exception ex)
			{
				Logger.Debug("FAILED! {0}", ex);
				return;
			}

			var temp = this.LightingSceneChanged;
			temp?.Invoke(this, new GenericSingleEventArgs<string>(device.Id));
		}
	}

}
