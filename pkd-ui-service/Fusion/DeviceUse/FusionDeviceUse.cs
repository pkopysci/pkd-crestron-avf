namespace pkd_ui_service.Fusion.DeviceUse
{
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Linq;
	using Crestron.SimplSharpPro.Fusion;

	internal class FusionDeviceUse : IFusionDeviceUse
	{
		private static readonly string DEVICE_TAG = "Source";
		private static readonly string DISPLAY_TAG = "DISPLAY";
		private static readonly string TIME_TAG = "TIME";

		private readonly FusionRoom fusion;
		private readonly Dictionary<string, FusionDeviceData> devices;
		private readonly Dictionary<string, FusionDeviceData> displays;

		public FusionDeviceUse(FusionRoom fusion)
		{
			ParameterValidator.ThrowIfNull(fusion, "FusionDeviceUse.Ctor", "fusion");

			this.fusion = fusion;
			this.devices = new Dictionary<string, FusionDeviceData>();
			this.displays = new Dictionary<string, FusionDeviceData>();
		}

		///<inheritdoc/>
		public void AddDeviceToUseTracking(string id, string label)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "FusionDeviceUse.AddDeviceToUseTracking", "id");
			ParameterValidator.ThrowIfNullOrEmpty(label, "FusionDeviceUse.AddDeviceToUseTracking", "label");
			this.devices.Add(id, new FusionDeviceData()
			{
				Id = id,
				Label = label,
				TypeTag = DEVICE_TAG,
				StartTime = default,
			});
		}

		///<inheritdoc/>
		public void StartDeviceUse(string id)
		{
			if (this.devices.TryGetValue(id, out FusionDeviceData found))
			{
				found.StartTime = DateTime.Now;
			}
		}

		///<inheritdoc/>
		public void StopDeviceUse(string id)
		{
			if (this.devices.TryGetValue(id, out FusionDeviceData found))
			{
				int useMins = this.CalculateUseTime(found);
				if (useMins > 0)
				{
					this.SendCommand(found, useMins, DEVICE_TAG);
					found.StartTime = default;
				}
			}
		}

		///<inheritdoc/>
		public void AddDisplayToUseTracking(string id, string label)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "FusionDeviceUse.AddDisplayToUseTracking", "id");
			ParameterValidator.ThrowIfNullOrEmpty(label, "FusionDeviceUse.AddDisplayToUseTracking", "label");
			this.displays.Add(id, new FusionDeviceData()
			{
				Id = id,
				Label = label,
				TypeTag = DISPLAY_TAG,
				StartTime = default
			});
		}

		///<inheritdoc/>
		public void StartDisplayUse(string id)
		{
			if (this.displays.TryGetValue(id, out FusionDeviceData found))
			{
				found.StartTime = DateTime.Now;
			}
		}

		///<inheritdoc/>
		public void StopDisplayUse(string id)
		{
			if (this.displays.TryGetValue(id, out FusionDeviceData found))
			{
				int useMins = this.CalculateUseTime(found);
				if (useMins > 0)
				{
					this.SendCommand(found, useMins, DISPLAY_TAG);
					found.StartTime = default;
				}
			}
		}

		private void SendCommand(FusionDeviceData device, int minutesUsed, string devType)
		{
			// USAGE||current-date||current-time||TIME||Source||Device-name||-||minutes-used||-||-||-||
			DateTime now = DateTime.Now;
			StringBuilder builder = new StringBuilder();
			builder.Append("USAGE||");
			builder.Append(now.Date.Year)
				.Append("-")
				.Append(now.Date.Month)
				.Append("-")
				.Append(now.Date.Day)
				.Append("||");

			builder.Append(now.Hour)
				.Append(":")
				.Append(now.Minute)
				.Append(":")
				.Append(now.Second)
				.Append("||");

			builder.Append(TIME_TAG + "||");
			builder.Append(devType + "||");
			builder.Append(device.Label + "||");
			builder.Append("-||");
			builder.Append(minutesUsed);
			builder.Append("||-||-||-||");

			Logger.Debug("Sending command:\n{0}", builder.ToString());
			this.fusion.DeviceUsage.InputSig.StringValue = builder.ToString();
		}

		private int CalculateUseTime(FusionDeviceData device)
		{
			if (device.StartTime == default)
			{
				return 0;
			}

			try
			{
				TimeSpan delta = DateTime.Now - device.StartTime;
				return (delta.Hours * 60) + delta.Minutes + (delta.Seconds >= 30 ? 1 : 0);
			}
			catch (Exception e)
			{
				Logger.Error("Unable to calculate use time for device {0} : {1}", device.Id, e.StackTrace);
				return 0;
			}
		}
	}
}
