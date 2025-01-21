namespace pkd_ui_service.Fusion.DeviceUse
{
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using Crestron.SimplSharpPro.Fusion;

	internal class FusionDeviceUse : IFusionDeviceUse
	{
		private const string DeviceTag = "Source";
		private const string DisplayTag = "DISPLAY";
		private const string TimeTag = "TIME";

		private readonly FusionRoom fusion;
		private readonly Dictionary<string, FusionDeviceData> devices;
		private readonly Dictionary<string, FusionDeviceData> displays;

		public FusionDeviceUse(FusionRoom fusion)
		{
			ParameterValidator.ThrowIfNull(fusion, "FusionDeviceUse.Ctor", "fusion");

			this.fusion = fusion;
			devices = new Dictionary<string, FusionDeviceData>();
			displays = new Dictionary<string, FusionDeviceData>();
		}

		///<inheritdoc/>
		public void AddDeviceToUseTracking(string id, string label)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "FusionDeviceUse.AddDeviceToUseTracking", "id");
			ParameterValidator.ThrowIfNullOrEmpty(label, "FusionDeviceUse.AddDeviceToUseTracking", "label");
			devices.Add(id, new FusionDeviceData()
			{
				Id = id,
				Label = label,
				TypeTag = DeviceTag,
				StartTime = default,
			});
		}

		///<inheritdoc/>
		public void StartDeviceUse(string id)
		{
			if (devices.TryGetValue(id, out var found))
			{
				found.StartTime = DateTime.Now;
			}
		}

		///<inheritdoc/>
		public void StopDeviceUse(string id)
		{
			if (!devices.TryGetValue(id, out var found)) return;
			
			var useMinutes = CalculateUseTime(found);
			if (useMinutes <= 0) return;
				
			SendCommand(found, useMinutes, DeviceTag);
			found.StartTime = default;
		}

		///<inheritdoc/>
		public void AddDisplayToUseTracking(string id, string label)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "FusionDeviceUse.AddDisplayToUseTracking", "id");
			ParameterValidator.ThrowIfNullOrEmpty(label, "FusionDeviceUse.AddDisplayToUseTracking", "label");
			displays.Add(id, new FusionDeviceData()
			{
				Id = id,
				Label = label,
				TypeTag = DisplayTag,
				StartTime = default
			});
		}

		///<inheritdoc/>
		public void StartDisplayUse(string id)
		{
			if (displays.TryGetValue(id, out var found))
			{
				found.StartTime = DateTime.Now;
			}
		}

		///<inheritdoc/>
		public void StopDisplayUse(string id)
		{
			if (!displays.TryGetValue(id, out var found)) return;
			
			var useMinutes = CalculateUseTime(found);
			if (useMinutes <= 0) return;
			
			SendCommand(found, useMinutes, DisplayTag);
			found.StartTime = default;
		}

		private void SendCommand(FusionDeviceData device, int minutesUsed, string devType)
		{
			// USAGE||current-date||current-time||TIME||Source||Device-name||-||minutes-used||-||-||-||
			var now = DateTime.Now;
			var builder = new StringBuilder();
			builder.Append("USAGE||");
			builder.Append(now.Date.Year)
				.Append('-')
				.Append(now.Date.Month)
				.Append('-')
				.Append(now.Date.Day)
				.Append("||");

			builder.Append(now.Hour)
				.Append(':')
				.Append(now.Minute)
				.Append(':')
				.Append(now.Second)
				.Append("||");

			builder.Append(TimeTag + "||");
			builder.Append(devType + "||");
			builder.Append(device.Label + "||");
			builder.Append("-||");
			builder.Append(minutesUsed);
			builder.Append("||-||-||-||");

			Logger.Debug("Sending command:\n{0}", builder.ToString());
			fusion.DeviceUsage.InputSig.StringValue = builder.ToString();
		}

		private static int CalculateUseTime(FusionDeviceData device)
		{
			if (device.StartTime == default)
			{
				return 0;
			}

			try
			{
				var delta = DateTime.Now - device.StartTime;
				return (delta.Hours * 60) + delta.Minutes + (delta.Seconds >= 30 ? 1 : 0);
			}
			catch (Exception e)
			{
				Logger.Error("Unable to calculate use time for device {0} : {1}", device.Id, e.StackTrace ?? string.Empty);
				return 0;
			}
		}
	}
}
