namespace pkd_hardware_service.AvSwitchDevices.DmMd400
{
	using System;
	using System.Collections.Generic;
	using Crestron.SimplSharpPro;
	using Crestron.SimplSharpPro.DM;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.RoutingData;
	using AudioDevices;
	using BaseDevice;

	/// <summary>
	/// Wrapper class for controlling a Crestron DM-MD-400 tx/rx pair.
	/// </summary>
	public class DmMd400AvSwitch : BaseDevice, IAvSwitcher, IDisposable, IAudioControl
	{
		private const int VolMax = 200;
		private const int VolMin = -800;
		private readonly HdMdNxMHdmiOutput audioOutput;
		private readonly HdMd400CE md400;
		private bool isDisposed;
		private readonly List<string> inputIds;
		private readonly List<string> outputIds;

		/// <summary>
		/// Initializes a new instance of the <see cref="DmMd400AvSwitch"/> class.
		/// </summary>
		/// <param name="config">The configuration data used to control the device.</param>
		/// <param name="parent">The root control system that is running the program.</param>
		public DmMd400AvSwitch(MatrixData config, CrestronControlSystem parent)
		{
			ParameterValidator.ThrowIfNull(config, "CTor", nameof(config));
			ParameterValidator.ThrowIfNull(parent, "CTor", nameof(parent));

			inputIds = [];
			outputIds = [];
			Id = config.Id;
			Label = config.Label;

			md400 = new HdMd400CE((uint)config.Connection.Port, config.Connection.Host, parent);
			audioOutput = (HdMdNxMHdmiOutput)md400.Outputs[1]!;
			md400.OnlineStatusChange += Md400_OnlineStatusChange;
			md400.DMOutputChange += Md400_DMOutputChange;
			md400.DMSystemChange += Md400_DMSystemChange;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="DmMd400AvSwitch"/> class.
		/// </summary>
		~DmMd400AvSwitch()
		{
			Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, uint>>? VideoRouteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>>? AudioOutputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>>? AudioOutputLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>>? AudioInputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>>? AudioInputLevelChanged;

		/// <inheritdoc/>
		public IEnumerable<string> GetAudioPresetIds()
		{
			return new List<string>();
		}

		/// <inheritdoc/>
		public IEnumerable<string> GetAudioInputIds()
		{
			return inputIds;
		}

		/// <inheritdoc/>
		public IEnumerable<string> GetAudioOutputIds()
		{
			return outputIds;
		}

		/// <inheritdoc/>
		public void AddInputChannel(string id, string levelTag, string muteTag, int bankIndex, int levelMax, int levelMin, int routerIndex)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "AddInputChannel", "id");
			inputIds.Add(id);
		}

		/// <inheritdoc/>
		public void AddOutputChannel(string id, string levelTag, string muteTag, string routerTag, int routerIndex, int bankIndex, int levelMax, int levelMin)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "AddOutputChannel", "id");
			outputIds.Add(id);
		}

		/// <summary>
		/// Interface method not supported by this device.
		/// </summary>
		public void AddPreset(string id, int index)
		{
			Logger.Warn(
				"DmMd400AvSwitch {0} AddPreset({1},{2}) - Presets not supported by this device.",
				Id,
				id,
				index);
		}

		/// <inheritdoc/>
		public void ClearVideoRoute(uint output)
		{
			if (output > md400.NumberOfOutputs)
			{
				Logger.Error(
					$"Switch {Id} ClearVideoRoute({output}) - output argument out of bounds. Max out = {md400.NumberOfOutputs}.");

				return;
			}

			md400.Outputs[output]!.VideoOut = null;
		}

		/// <inheritdoc/>
		public override void Connect()
		{
			if (md400.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Error($"Switch {Id} Connect() - failed to register device: {md400.RegistrationFailureReason}");
			}
		}

		/// <inheritdoc/>
		public override void Disconnect()
		{
			if (md400.UnRegister() != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Error(
					$"Switch {Id} Disconnect() - failed to unregister device: {md400.UnRegistrationFailureReason}");
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public uint GetCurrentVideoSource(uint output)
		{
			return GetCurrentSource(output, "GetCurrentVideoSource");
		}

		/// <inheritdoc/>
		public void RouteVideo(uint source, uint output)
		{
			if (!IsOnline)
			{
				Logger.Warn($"AvSwitch {Id} - RouteVideo(): Device is offline.");
				return;
			}

			if (source > md400.NumberOfInputs || output > md400.NumberOfOutputs)
			{
				Logger.Error(
					$"Switch {Id} RouteVideo({source},{output}) - input or output argument out of bounds. Max in = {md400.NumberOfInputs}, max out = {md400.NumberOfOutputs}.");

				return;
			}

			md400.Outputs[output]!.VideoOut = md400.Inputs[source];
		}

		/// <inheritdoc/>
		public void SetAudioOutputLevel(string id, int level)
		{
			if (!IsOnline)
			{
				Logger.Warn($"AvSwitch {Id} - SetAudioLevel(): Device is offline.");
				return;
			}

			if (level is < 0 or > 100)
			{
				Logger.Error($"Switch {Id} SetAudioLevel({level}) - argument out of bounds. Min = 0, max = 100");
			}

			const int newRange = (VolMax - VolMin);
			var scaledLevel = ((level * newRange) / 100) + VolMin;
			audioOutput.AudioOutput.Volume.ShortValue = (short)scaledLevel;
		}

		/// <inheritdoc/>
		public void SetAudioOutputMute(string id, bool state)
		{
			if (!IsOnline)
			{
				Logger.Warn($"AvSwitch {Id} - SetAudioMute(): Device is offline.");
				return;
			}

			if (state)
			{
				audioOutput.AudioOutput.AudioMute();
			}
			else
			{
				audioOutput.AudioOutput.AudioUnmute();
			}
		}

		/// <inheritdoc/>
		public void SetAudioInputLevel(string id, int level) { }

		/// <inheritdoc/>
		public void SetAudioInputMute(string id, bool state) { }

		/// <inheritdoc/>
		public int GetAudioInputLevel(string id)
		{
			return 0;
		}

		/// <inheritdoc/>
		public bool GetAudioInputMute(string id)
		{
			return false;
		}

		/// <inheritdoc/>
		public int GetAudioOutputLevel(string id)
		{
			if (!IsOnline)
			{
				Logger.Warn($"AvSwitch {Id} - AudioLevel: Device offline, returning 0.");
				return 0;
			}

			var level = (int)audioOutput.AudioOutput.VolumeFeedback.ShortValue;
			const int oldRange = VolMax - VolMin;
			var scaledLevel = (((level - VolMin) * 100) / oldRange);
			return scaledLevel;
		}

		/// <inheritdoc/>
		public bool GetAudioOutputMute(string id)
		{
			if (IsOnline) return audioOutput.AudioOutput.AudioMuteFeedback.BoolValue;
			Logger.Warn($"AvSwitch {Id} - AudioMute: Device offline, returning false.");
			return false;
		}

		/// <summary>
		/// Interface feature not supported by this device.
		/// </summary>
		/// <param name="id">Unique ID of preset to recall.</param>
		public void RecallAudioPreset(string id) { }

		private uint GetCurrentSource(uint output, string callerMethod)
		{
			if (!IsOnline)
			{
				Logger.Warn($"switch ID {Id} - {callerMethod}(): Device offline.");

				return 0;
			}

			if (output > md400.NumberOfOutputs)
			{
				Logger.Warn($"switch ID {output} - {callerMethod}({output}): output index out of bounds.");

				return 0;
			}

			return md400.Outputs[output]!.VideoOutFeedback!.Number;
		}

		private void Dispose(bool disposing)
		{
			if (isDisposed) return;
			if (disposing)
			{
				md400.Dispose();
			}

			isDisposed = true;
		}

		private void Md400_DMOutputChange(Switch? device, DMOutputEventArgs args)
		{
			switch (args.EventId)
			{
				case DMOutputEventIds.VideoOutEventId:
					var temp = VideoRouteChanged;
					temp?.Invoke(this, new GenericDualEventArgs<string, uint>(Id, 1));
					break;
			}
		}

		private void Md400_DMSystemChange(Switch? device, DMSystemEventArgs args)
		{
			switch (args.EventId)
			{
				case DMSystemEventIds.AuxAudioMuteFeedbackEventId:
					var temp = AudioOutputMuteChanged;
					if (temp != null)
					{
						string channelId = (outputIds.Count > 0) ? outputIds[0] : Id;
						temp.Invoke(this, new GenericDualEventArgs<string, string>(Id, channelId));
					}
					break;

				case DMSystemEventIds.AuxOutputVolumeFeedbackEventId:
					var vTemp = AudioOutputLevelChanged;
					if (vTemp != null)
					{
						string channelId = (outputIds.Count > 0) ? outputIds[0] : Id;
						vTemp.Invoke(this, new GenericDualEventArgs<string, string>(Id, channelId));
					}
					break;
			}
		}

		private void Md400_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			IsOnline = args.DeviceOnLine;
			NotifyOnlineStatus();
		}

		public void Initialize(string hostName, int port, string id, string label, int numInputs, int numOutputs) { }
	}
}
