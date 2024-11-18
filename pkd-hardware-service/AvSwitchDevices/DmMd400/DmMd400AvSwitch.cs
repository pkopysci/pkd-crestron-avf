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
	using pkd_hardware_service.AudioDevices;
	using pkd_hardware_service.BaseDevice;

	/// <summary>
	/// Wrapper class for controlling a Crestron DM-MD-400 tx/rx pair.
	/// </summary>
	public class DmMd400AvSwitch : BaseDevice, IAvSwitcher, IDisposable, IAudioControl
	{
		private static readonly int VolMax = 200;
		private static readonly int VolMin = -800;
		private readonly HdMdNxMHdmiOutput audioOutput;
		private HdMd400CE md400;
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
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}

			if (parent == null)
			{
				throw new ArgumentNullException("parent");
			}

			this.inputIds = new List<string>();
			this.outputIds = new List<string>();
			this.Id = config.Id;
			this.Label = config.Label;

			this.md400 = new HdMd400CE((uint)config.Connection.Port, config.Connection.Host, parent);
			this.audioOutput = (HdMdNxMHdmiOutput)this.md400.Outputs[1];
			this.md400.OnlineStatusChange += this.Md400_OnlineStatusChange;
			this.md400.DMOutputChange += this.Md400_DMOutputChange;
			this.md400.DMSystemChange += this.Md400_DMSystemChange;
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="DmMd400AvSwitch"/> class.
		/// </summary>
		~DmMd400AvSwitch()
		{
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, uint>> VideoRouteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>> AudioOutputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>> AudioOutputLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>> AudioInputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>> AudioInputLevelChanged;

		/// <inheritdoc/>
		public IEnumerable<string> GetAudioPresetIds()
		{
			return new List<string>();
		}

		/// <inheritdoc/>
		public IEnumerable<string> GetAudioInputIds()
		{
			return this.inputIds;
		}

		/// <inheritdoc/>
		public IEnumerable<string> GetAudioOutputIds()
		{
			return this.outputIds;
		}

		/// <inheritdoc/>
		public void AddInputChannel(string id, string levelTag, string muteTag, int bankIndex, int levelMax, int levelMin, int routerIndex)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "AddInputChannel", "id");
			this.inputIds.Add(id);
		}

		/// <inheritdoc/>
		public void AddOutputChannel(string id, string levelTag, string muteTag, string routerTag, int routerIndex, int bankIndex, int levelMax, int levelMin)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "AddOutputChannel", "id");
			this.outputIds.Add(id);
		}

		/// <summary>
		/// Interface method not supported by this device.
		/// </summary>
		public void AddPreset(string id, int index)
		{
			Logger.Warn(
				"DmMd400AvSwitch {0} AddPreset({1},{2}) - Presets not supported by this device.",
				this.Id,
				id,
				index);
		}

		/// <inheritdoc/>
		public void ClearVideoRoute(uint output)
		{
			if (output > this.md400.NumberOfOutputs)
			{
				Logger.Error(string.Format(
					"Switch {0} ClearVideoRoute({1}) - output argument out of bounds. Max out = {2}.",
					this.Id,
					output,
					this.md400.NumberOfOutputs));

				return;
			}

			this.md400.Outputs[output].VideoOut = null;
		}

		/// <inheritdoc/>
		public override void Connect()
		{
			if (this.md400.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Error(string.Format(
					"Switch {0} Connect() - failed to register device: {1}",
					this.Id,
					this.md400.RegistrationFailureReason));
			}
		}

		/// <inheritdoc/>
		public override void Disconnect()
		{
			if (this.md400.UnRegister() != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Error(string.Format(
					"Switch {0} Disconnect() - failed to unregister device: {1}",
					this.Id,
					this.md400.UnRegistrationFailureReason));
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public uint GetCurrentVideoSource(uint output)
		{
			return this.GetCurrentSource(output, "GetCurrentVideoSource");
		}

		/// <inheritdoc/>
		public void RouteVideo(uint source, uint output)
		{
			if (!this.IsOnline)
			{
				Logger.Warn(string.Format("AvSwitch {0} - RouteVideo(): Device is offline.", this.Id));
				return;
			}

			if (source > this.md400.NumberOfInputs || output > this.md400.NumberOfOutputs)
			{
				Logger.Error(string.Format(
					"Switch {0} RouteVideo({1},{2}) - input or output argument out of bounds. Max in = {3}, max out = {4}.",
					this.Id,
					source,
					output,
					this.md400.NumberOfInputs,
					this.md400.NumberOfOutputs));

				return;
			}

			this.md400.Outputs[output].VideoOut = this.md400.Inputs[source];
		}

		/// <inheritdoc/>
		public void SetAudioOutputLevel(string id, int level)
		{
			if (!this.IsOnline)
			{
				Logger.Warn(string.Format("AvSwitch {0} - SetAudioLevel(): Device is offline.", this.Id));
				return;
			}

			if (level < 0 || level > 100)
			{
				Logger.Error(string.Format(
					"Siwtch {0} SetAudioLevel({1}) - argument out of bounds. Min = 0, max = 100",
					this.Id,
					level));
			}

			int newRange = (VolMax - VolMin);
			int scaledLevel = ((level * newRange) / 100) + VolMin;
			this.audioOutput.AudioOutput.Volume.ShortValue = (short)scaledLevel;
		}

		/// <inheritdoc/>
		public void SetAudioOutputMute(string id, bool state)
		{
			if (!this.IsOnline)
			{
				Logger.Warn(string.Format("AvSwitch {0} - SetAudioMute(): Device is offline.", this.Id));
				return;
			}

			if (state)
			{
				this.audioOutput.AudioOutput.AudioMute();
			}
			else
			{
				this.audioOutput.AudioOutput.AudioUnmute();
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
			if (!this.IsOnline)
			{
				Logger.Warn(string.Format("AvSwitch {0} - AudioLevel: Device offline, returning 0.", this.Id));
				return 0;
			}

			int level = (int)this.audioOutput.AudioOutput.VolumeFeedback.ShortValue;
			var oldRange = VolMax - VolMin;
			var scaledLevel = (((level - VolMin) * 100) / oldRange);
			return scaledLevel;
		}

		/// <inheritdoc/>
		public bool GetAudioOutputMute(string id)
		{
			if (!this.IsOnline)
			{
				Logger.Warn(string.Format("AvSwitch {0} - AudioMute: Device offline, returning false.", this.Id));
				return false;
			}

			return this.audioOutput.AudioOutput.AudioMuteFeedback.BoolValue;
		}

		/// <summary>
		/// Interface feature not supported by this device.
		/// </summary>
		/// <param name="id">Unique ID of preset to recall.</param>
		public void RecallAudioPreset(string id) { }

		private uint GetCurrentSource(uint output, string callerMethod)
		{
			if (!this.IsOnline)
			{
				Logger.Warn(string.Format(
					"switch ID {0} - {1}(): Device offline.",
					this.Id,
					callerMethod));

				return 0;
			}

			if (output > this.md400.NumberOfOutputs)
			{
				Logger.Warn(string.Format(
					"switch ID {0} - {1}({0}): output index out of bounds.",
					output,
					callerMethod));

				return 0;
			}

			return this.md400.Outputs[output].VideoOutFeedback.Number;
		}

		private void Dispose(bool disposing)
		{
			if (!this.isDisposed)
			{
				if (disposing)
				{
					if (this.md400 != null)
					{
						this.md400.Dispose();
						this.md400 = null;
					}
				}

				this.isDisposed = true;
			}
		}

		private void Md400_DMOutputChange(Switch device, DMOutputEventArgs args)
		{
			switch (args.EventId)
			{
				case DMOutputEventIds.VideoOutEventId:
					var temp = this.VideoRouteChanged;
					temp?.Invoke(this, new GenericDualEventArgs<string, uint>(this.Id, 1));
					break;
			}
		}

		private void Md400_DMSystemChange(Switch device, DMSystemEventArgs args)
		{
			switch (args.EventId)
			{
				case DMSystemEventIds.AuxAudioMuteFeedbackEventId:
					var temp = this.AudioOutputMuteChanged;
					if (temp != null)
					{
						string channelId = (this.outputIds.Count > 0) ? this.outputIds[0] : this.Id;
						temp.Invoke(this, new GenericDualEventArgs<string, string>(this.Id, channelId));
					}
					break;

				case DMSystemEventIds.AuxOutputVolumeFeedbackEventId:
					var vTemp = this.AudioOutputLevelChanged;
					if (vTemp != null)
					{
						string channelId = (this.outputIds.Count > 0) ? this.outputIds[0] : this.Id;
						vTemp.Invoke(this, new GenericDualEventArgs<string, string>(this.Id, channelId));
					}
					break;
			}
		}

		private void Md400_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			this.IsOnline = args.DeviceOnLine;
			this.NotifyOnlineStatus();
		}

		public void Initialize(string hostName, int port, string id, string label, int numInputs, int numOutputs) { }
	}
}
