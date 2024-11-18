namespace pkd_hardware_service.AvSwitchDevices.DmMd8x1
{
	using Crestron.SimplSharpPro;
	using Crestron.SimplSharpPro.DM;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using pkd_domain_service.Data.RoutingData;
	using pkd_hardware_service.BaseDevice;
	using pkd_hardware_service.AudioDevices;
	using SimplSharpProDM;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Wrapper class for controlling a Crestron DM-MD-8x1 AV Switch.
	/// </summary>
	public class DmMd8x1AvSwitch : BaseDevice, IAvSwitcher, IDisposable, IAudioControl
	{
		private static readonly short VolMin = -800;
		private static readonly short VolMax = 100;
		private static readonly uint MicIndex = 1;
		private readonly DmMd4kAudioOutputStream analogAudio;
		private readonly List<string> inputIds;
		private readonly List<string> outputIds;
		private DmMd8x14kC dmSwitch;
		private bool isDisposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="DmMd8x1AvSwitch"/> class.
		/// </summary>
		/// <param name="config">The configuration data used to control the device.</param>
		/// <param name="parent">The root control system that is running the program.</param>
		public DmMd8x1AvSwitch(MatrixData config, CrestronControlSystem parent)
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
			this.dmSwitch = new DmMd8x14kC((uint)config.Connection.Port, parent)
			{
				Description = this.Id,
			};

			this.dmSwitch.DMSystemChange += this.DmSwitch_DMSystemChange;
			this.dmSwitch.OnlineStatusChange += this.DmSwitch_OnlineStatusChange;
			this.dmSwitch.DMOutputChange += this.DmSwitch_DMOutputChange;

			this.analogAudio = this.dmSwitch.Outputs[1].AudioOutputStream;
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
				"Dm8x1AvSwitch {0} AddPreset({1},{2}) - Presets not supported by this device.",
				this.Id,
				id,
				index);
		}

		/// <inheritdoc/>
		public int GetAudioInputLevel(string id)
		{
			if (!this.CheckOnline("AudioInputLevel"))
			{
				return 0;
			}

			return this.ConvertToPercecnt(this.analogAudio.OutputMixer.MicLevelFeedback[MicIndex].ShortValue);
		}

		/// <inheritdoc/>
		public void SetAudioInputLevel(string id, int level)
		{
			if (!this.CheckOnline("SetAudioInputLevel"))
			{
				return;
			}

			short scaled = this.ConvertToDb(level);
			if (!(scaled >= VolMin && scaled <= VolMax))
			{
				Logger.Error(string.Format(
					"AvSwitch {0} SetAudioInputLevel(): argument must be between {1} and {2}.",
					this.Id,
					VolMin,
					VolMax));

				return;
			}

			this.analogAudio.OutputMixer.MicLevel[MicIndex].ShortValue = scaled;
		}

		/// <inheritdoc/>
		public int GetAudioOutputLevel(string id)
		{
			if (!this.CheckOnline("AudioOutputLevel"))
			{
				return 0;
			}

			return this.ConvertToPercecnt(this.analogAudio.SourceLevelFeedBack.ShortValue);
		}

		/// <inheritdoc/>
		public void SetAudioOutputLevel(string id, int level)
		{
			if (!this.CheckOnline("SetAudioOutputLevel"))
			{
				return;
			}

			short scaled = this.ConvertToDb(level);
			if (!(scaled >= VolMin && scaled <= VolMax))
			{
				Logger.Error(string.Format(
					"AvSwitch {0} SetAudioOutputLevel(): argument must be between {1} and {2}.",
					this.Id,
					VolMin,
					VolMax));

				return;
			}

			this.analogAudio.SourceLevel.ShortValue = scaled;
		}

		/// <inheritdoc/>
		public void SetAudioInputMute(string id, bool mute)
		{
			if (!this.CheckOnline("SetAudioInputMute"))
			{
				return;
			}

			if (mute)
			{
				this.analogAudio.OutputMixer.MicMuteOn(MicIndex);
			}
			else
			{
				this.analogAudio.OutputMixer.MicMuteOff(MicIndex);
			}
		}

		/// <inheritdoc/>
		public bool GetAudioInputMute(string id)
		{
			if (!this.CheckOnline("AudioInputMute"))
			{
				return false; ;
			}

			return this.analogAudio.OutputMixer.MicMuteOnFeedback[MicIndex].BoolValue;
		}

		/// <inheritdoc/>
		public void SetAudioOutputMute(string id, bool state)
		{
			if (!this.CheckOnline("SetAudioOutputMute"))
			{
				return;
			}

			if (state)
			{
				this.analogAudio.SourceMuteOn();
			}
			else
			{
				this.analogAudio.SourceMuteOff();
			}
		}

		/// <inheritdoc/>
		public bool GetAudioOutputMute(string id)
		{
			if (!this.CheckOnline("AudioOutputMute"))
			{
				return false;
			}

			return this.analogAudio.SourceMuteOnFeedBack.BoolValue;
		}

		/// <inheritdoc/>
		public void ClearVideoRoute(uint output)
		{
			if (!this.CheckOnline("ClearVideoRoute"))
			{
				return;
			}

			if (output > this.dmSwitch.NumberOfOutputs)
			{
				Logger.Error(string.Format(
					"AvSwitch {0} ClearVideoRoute({1}) - input or output argument out of bounds. Max out = {2}",
					this.Id,
					output,
					this.dmSwitch.NumberOfOutputs));

				return;
			}

			this.dmSwitch.Outputs[output].VideoOut = null;
		}

		/// <summary>
		/// Interface feature not supported by this device.
		/// </summary>
		/// <param name="id">Id of preset to recall.</param>
		public void RecallAudioPreset(string id) { }

		/// <inheritdoc/>
		public override void Connect()
		{
			if (this.dmSwitch.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Error(string.Format(
					"DM-8x1 switch {0} - Failed to register device: {1}",
					this.Id,
					this.dmSwitch.RegistrationFailureReason));
			}
		}

		/// <inheritdoc/>
		public override void Disconnect()
		{
			if (this.dmSwitch.UnRegister() != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Error(string.Format(
					"DM-8x1 switch {0} - Failed to unregister device: {1}",
					this.Id,
					this.dmSwitch.UnRegistrationFailureReason));
			}
		}

		/// <inheritdoc/>
		public uint GetCurrentVideoSource(uint output)
		{
			return this.GetCurrentSource(output, "GetCurrentVideoSource");
		}

		/// <inheritdoc/>
		public void RouteVideo(uint source, uint output)
		{
			if (!this.CheckOnline("RouteVideo"))
			{
				return;
			}

			if (source > this.dmSwitch.NumberOfInputs ||
				output > this.dmSwitch.NumberOfOutputs ||
				source == 0 ||
				output == 0)
			{
				Logger.Error(string.Format(
					"AvSwitch {0} RouteVideo({1},{2}) - input or output argument out of bounds. Input range = 1-{3}, output range = 1-{4}",
					this.Id,
					source,
					output,
					this.dmSwitch.NumberOfInputs,
					this.dmSwitch.NumberOfOutputs));

				return;
			}

			this.dmSwitch.Outputs[output].VideoOut = this.dmSwitch.Inputs[source];
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <inheritdoc/>
		public void Initialize(string hostName, int port, string id, string label, int numInputs, int numOutputs)
		{
			this.dmSwitch.Register();
		}

		private void Dispose(bool disposing)
		{
			if (!this.isDisposed)
			{
				if (disposing)
				{
					this.Disconnect();
					if (this.dmSwitch != null)
					{
						this.dmSwitch.Dispose();
						this.dmSwitch = null;
					}
				}

				this.isDisposed = true;
			}
		}

		private void DmSwitch_DMOutputChange(Switch device, DMOutputEventArgs args)
		{
			switch (args.EventId)
			{
				case DMOutputEventIds.SourceMuteOnFeedBackEventId:
					this.NotifyOutputMuteChange();
					break;
				case DMOutputEventIds.SourceLevelFeedBackEventId:
					this.NotifyOutputLevelChange();
					break;
				case DMOutputEventIds.Mic1LevelFeedBackEventId:
					this.NotifyInputLevelChange();
					break;
				case DMOutputEventIds.Mic1MuteOnFeedBackEventId:
					this.NotifyInputMuteChange();
					break;
				case DMOutputEventIds.VideoOutEventId:
					this.NotifySourceChange();
					break;
			}
		}

		private void DmSwitch_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			this.IsOnline = args.DeviceOnLine;
			this.NotifyOnlineStatus();
		}

		private void DmSwitch_DMSystemChange(Switch device, DMSystemEventArgs args)
		{
			Logger.Info(string.Format(
				"AvSwitch {0} DmSwitch_DMSystemChange: ID: {1}, index: {2}",
				this.Id,
				args.EventId,
				args.Index));
		}

		private bool CheckOnline(string methodName)
		{
			if (!this.IsOnline)
			{
				Logger.Warn(string.Format(
					"AvSwitch {0} - {1}: Device is offline",
					this.Id,
					methodName));
			}

			return this.IsOnline;
		}

		private uint GetCurrentSource(uint output, string callerName)
		{
			if (output > this.dmSwitch.NumberOfOutputs)
			{
				Logger.Error(string.Format(
					"AvSwitch {0} {1}() ({2}) - input or output argument out of bounds. Max out = {3}",
					this.Id,
					callerName,
					output,
					this.dmSwitch.NumberOfOutputs));

				return 0;
			}

			var routed = this.dmSwitch.Outputs[output].VideoOutFeedback;
			if (routed != null)
			{
				return routed.Number;
			}
			else
			{
				return 0;
			}
		}

		private void NotifyOutputMuteChange()
		{
			var temp = this.AudioOutputMuteChanged;
			if (temp != null)
			{
				string channelId = (this.outputIds.Count > 0) ? this.outputIds[0] : this.Id;
				temp.Invoke(this, new GenericDualEventArgs<string, string>(this.Id, channelId));
			}
		}

		private void NotifyOutputLevelChange()
		{
			var temp = this.AudioOutputLevelChanged;
			if (temp != null)
			{
				string channelId = (this.outputIds.Count > 0) ? this.outputIds[0] : this.Id;
				temp.Invoke(this, new GenericDualEventArgs<string, string>(this.Id, channelId));
			}
		}

		private void NotifySourceChange()
		{
			this.VideoRouteChanged?.Invoke(this, new GenericDualEventArgs<string, uint>(this.Id, 1));
		}

		private void NotifyInputLevelChange()
		{
			var temp = this.AudioInputLevelChanged;
			if (temp != null)
			{
				string channelId = (this.inputIds.Count > 0) ? this.outputIds[0] : this.Id;
				temp.Invoke(this, new GenericDualEventArgs<string, string>(this.Id, channelId));
			}
		}

		private void NotifyInputMuteChange()
		{
			var temp = this.AudioInputMuteChanged;
			if (temp != null)
			{
				string channelId = (this.inputIds.Count > 0) ? this.outputIds[0] : this.Id;
				temp.Invoke(this, new GenericDualEventArgs<string, string>(this.Id, channelId));
			}
		}

		private short ConvertToDb(int percent)
		{
			int percentMax = 100;
			int dbMin = -800;
			int dbRange = 900;

			return (short)(((percent * dbRange) / percentMax) + dbMin);
		}

		private int ConvertToPercecnt(int dbVal)
		{
			int dbMin = -800;
			int percentMin = 0;
			int dbRange = 900;
			int percentRange = 100;

			return (((dbVal - dbMin) * percentRange) / dbRange) + percentMin;
		}
	}
}
