using System.Text;
using Crestron.SimplSharpPro.Fusion;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;

namespace pkd_ui_service.Fusion.DeviceUse
{
	/// <summary>
	/// Device use message object for sending data to the fusion server.
	/// </summary>
	public class FusionDeviceUse : IFusionDeviceUse
	{
		private const string DeviceTag = "Source";
		private const string DisplayTag = "DISPLAY";
		private const string TimeTag = "TIME";

		private readonly FusionRoom _fusion;
		private readonly Dictionary<string, FusionDeviceData> _devices;
		private readonly Dictionary<string, FusionDeviceData> _displays;

		
		/// <summary>
		/// Instantiates a new instance of <see cref="FusionDeviceUse"/>.
		/// </summary>
		/// <param name="fusion">The Crestron fusion connection object that will be used for communication.</param>
		public FusionDeviceUse(FusionRoom fusion)
		{
			ParameterValidator.ThrowIfNull(fusion, "FusionDeviceUse.Ctor", nameof(fusion));

			this._fusion = fusion;
			_devices = new Dictionary<string, FusionDeviceData>();
			_displays = new Dictionary<string, FusionDeviceData>();
		}

		///<inheritdoc/>
		public void AddDeviceToUseTracking(string id, string label)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "FusionDeviceUse.AddDeviceToUseTracking", nameof(id));
			ParameterValidator.ThrowIfNullOrEmpty(label, "FusionDeviceUse.AddDeviceToUseTracking", nameof(label));
			_devices.Add(id, new FusionDeviceData()
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
			if (_devices.TryGetValue(id, out var found))
			{
				found.StartTime = DateTime.Now;
			}
		}

		///<inheritdoc/>
		public void StopDeviceUse(string id)
		{
			if (!_devices.TryGetValue(id, out var found)) return;
			
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
			_displays.Add(id, new FusionDeviceData()
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
			if (_displays.TryGetValue(id, out var found))
			{
				found.StartTime = DateTime.Now;
			}
		}

		///<inheritdoc/>
		public void StopDisplayUse(string id)
		{
			if (!_displays.TryGetValue(id, out var found)) return;
			
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
			_fusion.DeviceUsage.InputSig.StringValue = builder.ToString();
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
