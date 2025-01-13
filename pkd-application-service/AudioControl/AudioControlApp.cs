namespace pkd_application_service.AudioControl
{
    using pkd_application_service.Base;
    using pkd_common_utils.GenericEventArgs;
    using pkd_common_utils.Logging;
    using pkd_common_utils.Validation;
    using pkd_hardware_service.AudioDevices;
    using pkd_hardware_service.BaseDevice;
    using pkd_hardware_service.Routable;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Class for handling audio adjustments and updates.
    /// </summary>
    internal class AudioControlApp : IAudioControlApp, IAudioPresetApp, IDisposable
	{
		private readonly DeviceContainer<IAudioControl> dspDevices;
		private readonly List<AudioChannelInfoContainer> inputChannels;
		private readonly List<AudioChannelInfoContainer> outputChannels;
		private readonly Dictionary<string, List<InfoContainer>> allPresets;
		private bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="AudioControlApp"/> class.
		/// </summary>
		/// <param name="dspDevices">Collection of all devices in the system with audio controls.</param>
		/// <param name="inputChannels">Collection of all input auido channels in the system with level control.</param>
		/// <param name="outputChannels">Collection of all audio output channels in the system with level control.</param>
		public AudioControlApp(
			DeviceContainer<IAudioControl> dspDevices,
			List<AudioChannelInfoContainer> inputChannels,
			List<AudioChannelInfoContainer> outputChannels,
			Dictionary<string, List<InfoContainer>> allPresets)
		{
			ParameterValidator.ThrowIfNull(dspDevices, "Ctor", "dspDevices");
			ParameterValidator.ThrowIfNull(inputChannels, "Ctor", "inputChannels");
			ParameterValidator.ThrowIfNull(outputChannels, "Ctor", "outputChannels");
			ParameterValidator.ThrowIfNull(allPresets, "Ctor", "allPresets");

			this.dspDevices = dspDevices;
			this.inputChannels = inputChannels;
			this.outputChannels = outputChannels;
			this.allPresets = allPresets;
			this.SubscribeDevices();
		}

		~AudioControlApp()
		{
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioOutputLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioOutputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioInputLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioInputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioDspConnectionStatusChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> AudioOutputRouteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>> AudioZoneEnableChanged;

		/// <inheritdoc/>
		public ReadOnlyCollection<AudioChannelInfoContainer> GetAudioInputChannels()
		{
			return new ReadOnlyCollection<AudioChannelInfoContainer>(this.inputChannels);
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<AudioChannelInfoContainer> GetAudioOutputChannels()
		{
			return new ReadOnlyCollection<AudioChannelInfoContainer>(this.outputChannels);
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<InfoContainer> GetAllAudioDspDevices()
		{
			List<InfoContainer> deviceInfo = new List<InfoContainer>();
			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				deviceInfo.Add(new InfoContainer(dsp.Id, dsp.Label, string.Empty, new List<string>(), dsp.IsOnline));
			}

			return new ReadOnlyCollection<InfoContainer>(deviceInfo);
		}

		/// <inheritdoc/>
		public bool QueryAudioDspConnectionStatus(string id)
		{
			var device = this.dspDevices.GetDevice(id);
			if (device == null)
			{
				Logger.Error("AudioControlApp.QueryAudioDspConnectionStatus({0}) - no device with that id.", id);
				return false;
			}

			Logger.Debug("ApplicationService.AudioControlApp.QueryAudioDspConnectionStatus({0}) => {1}", id, device.IsOnline);
			return device.IsOnline;
		}

		/// <inheritdoc/>
		public int QueryAudioInputLevel(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				Logger.Error("AudioControlApp.QueryAudioInputLevel() - 'id' cannot be null or empty.");
				return 0;
			}

			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (dsp.GetAudioInputIds().Contains(id))
				{
					return dsp.GetAudioInputLevel(id);
				}
			}

			Logger.Warn("AudioControlApp.QueryAudioInputLevel() - Could not find channel with ID {0}", id);
			return 0;
		}

		/// <inheritdoc/>
		public int QueryAudioOutputLevel(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				Logger.Error("AudioControlApp.QueryAudioOutputLevel() - 'id' cannot be null or empty.");
				return 0;
			}

			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (dsp.GetAudioOutputIds().Contains(id))
				{
					return dsp.GetAudioOutputLevel(id);
				}
			}

			Logger.Warn("AudioControlApp.QueryAudioOutputLevel() - Could not find channel with ID {0}", id);
			return 0;
		}

		/// <inheritdoc/>
		public bool QueryAudioOutputMute(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				Logger.Error("AudioControlApp.QueryAudioOutputMute() - 'id' cannot be null or empty.");
				return false;
			}

			Logger.Debug("AudioControlApp.QueryAudioOutputMute({0})", id);
			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (dsp.GetAudioOutputIds().Contains(id))
				{
					return dsp.GetAudioOutputMute(id);
				}
			}

			Logger.Warn("AudioControlApp.QueryAudioOutputMute() - Could not find channel with ID {0}", id);
			return false;
		}

		/// <inheritdoc/>
		public string QueryAudioOutputRoute(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				Logger.Error("AudioControlApp.QueryAudioOutputRoute() - 'id' cannot be null or empty.");
				return string.Empty;
			}

			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (dsp.GetAudioOutputIds().Contains(id) && (dsp is IAudioRoutable))
				{
					return (dsp as IAudioRoutable).GetCurrentAudioSource(id);
				}
			}

			Logger.Warn("AudioControlApp.QueryAudioOutputRoute() - Could not find channel with ID {0}", id);
			return string.Empty;
		}

		/// <inheritdoc/>
		public bool QueryAudioInputMute(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				Logger.Error("AudioControlApp.QueryAudioInputMute() - 'id' cannot be null or empty.");
				return false;
			}

			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (dsp.GetAudioInputIds().Contains(id))
				{
					return dsp.GetAudioInputMute(id);
				}
			}

			Logger.Warn("AudioControlApp.QueryAudioInputMute() - Could not find channel with ID {0}", id);
			return false;
		}

		/// <inheritdoc/>
		public bool QueryAudioZoneState(string channelId, string zoneId)
		{
			if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(zoneId))
			{
				Logger.Error(
					"ApplicationService.AudioControlApp.QueryAudioZoneState({0}, {1}) - no argument can be null or empty.",
					channelId,
					zoneId);

				return false;
			}

			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (!(dsp is IAudioZoneEnabler))
				{
					continue;
				}

				if (dsp.GetAudioInputIds().Contains(channelId) || dsp.GetAudioOutputIds().Contains(channelId))
				{
					return (dsp as IAudioZoneEnabler).QueryAudioZoneEnable(channelId, zoneId);
				}
			}

			Logger.Error("ApplicationService.AudioControlApp.QueryAudioZoneState() - could not find a DSP with channel {0}", channelId);
			return false;
		}

		/// <inheritdoc/>
		public void SetAudioInputLevel(string id, int level)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "SetAudioInputLevel", "id");
			if (level < 0 || level > 100)
			{
				Logger.Error("SetAudioInputLevel() - level {0} is out of range 0-100.", level);
				return;
			}

			Logger.Debug("ApplicationService.AudioControlApp.SetAudioInputLevel({0},{1}", id, level);

			bool found = false;
			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (dsp.GetAudioInputIds().Contains(id))
				{
					dsp.SetAudioInputLevel(id, level);
					found = true;
					break;
				}
			}

			if (!found)
			{
				Logger.Error("SetAudioInputLevel() - Unable to find input with ID {0}", id);
			}
		}

		/// <inheritdoc/>
		public void SetAudioInputMute(string id, bool mute)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "SetAudioInputMute", "id");

			bool found = false;
			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (dsp.GetAudioInputIds().Contains(id))
				{
					found = true;
					dsp.SetAudioInputMute(id, mute);
					break;
				}
			}

			if (!found)
			{
				Logger.Error("SetAudioInputMute() - Unable to find input with ID {0}", id);
			}
		}

		/// <inheritdoc/>
		public void SetAudioOutputLevel(string id, int level)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "SetAudioOutputLevel", "id");
			if (level < 0 || level > 100)
			{
				Logger.Error("SetAudioOutputLevel() - level {0} is out of range 0-100.", level);
				return;
			}

			bool found = false;
			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (dsp.GetAudioOutputIds().Contains(id))
				{
					dsp.SetAudioOutputLevel(id, level);
					found = true;
					break;
				}
			}

			if (!found)
			{
				Logger.Error("SetAudioOutputLevel() - Unable to find output with ID {0}", id);
			}
		}

		/// <inheritdoc/>
		public void SetAudioOutputMute(string id, bool mute)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "SetAudioOutputMute", "id");

			bool found = false;
			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (dsp.GetAudioOutputIds().Contains(id))
				{
					found = true;
					dsp.SetAudioOutputMute(id, mute);
					break;
				}
			}

			if (!found)
			{
				Logger.Error("SetAudioOutputMute() - Unable to find output with ID {0}", id);
			}
		}

		/// <inheritdoc/>
		public void SetAudioOutputRoute(string srcId, string destId)
		{
			if (string.IsNullOrEmpty(srcId) || string.IsNullOrEmpty(destId))
			{
				Logger.Error("AudioControlApp.SetAudioOutputRoute({0}, {1}) - srcId and destId cannot be null or empty.", srcId, destId);
				return;
			}
			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (dsp.GetAudioOutputIds().Contains(destId) && (dsp is IAudioRoutable))
				{
					(dsp as IAudioRoutable).RouteAudio(srcId, destId);
					break;
				}
			}
		}

		/// <inheritdoc/>
		public void ToggleAudioZoneState(string channelId, string zoneId)
		{
			Logger.Debug("AudioControlApp.ToggleAudioZoneState({0}, {1})", channelId, zoneId);

			if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(zoneId))
			{
				Logger.Error("AudioControlApp.ToggleAudioZoneState({0}, {1}) - channelId and zoneId cannot be null or empty.", channelId, zoneId);
				return;
			}

			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				if (!(dsp is IAudioZoneEnabler))
				{
					continue;
				}

				if (dsp.GetAudioInputIds().Contains(channelId) || dsp.GetAudioOutputIds().Contains(channelId))
				{
					(dsp as IAudioZoneEnabler).ToggleAudioZoneEnable(channelId, zoneId);
				}
			}
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<InfoContainer> QueryDspAudioPresets(string dspId)
		{
			if (this.allPresets.TryGetValue(dspId, out List<InfoContainer> presets))
			{
				return new ReadOnlyCollection<InfoContainer>(presets);
			}
			else
			{
				return new ReadOnlyCollection<InfoContainer>(new List<InfoContainer>());
			}
		}

		/// <inheritdoc/>
		public void RecallAudioPreset(string dspId, string presetId)
		{
			var dsp = this.dspDevices.GetDevice(dspId);
			if (dsp == null)
			{
				Logger.Error("AudioControlApp.RecallAudioPreset({0}, {1}) - DSP not found.", dspId, presetId);
			}

			dsp.RecallAudioPreset(presetId);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void SubscribeDevices()
		{
			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				dsp.ConnectionChanged += this.DspConnectionChangeHandler;
				dsp.AudioInputLevelChanged += this.AudioInputLevelChangeHandler;
				dsp.AudioInputMuteChanged += this.AudioInputMuteChangeHandler;
				dsp.AudioOutputLevelChanged += this.AudioOutputLevelChangeHandler;
				dsp.AudioOutputMuteChanged += this.AudioOutputMuteChangeHandler;

				if (dsp is IAudioRoutable routable)
				{
					routable.AudioRouteChanged += this.AudioRouteChangeHandler;
				}

				if (dsp is IAudioZoneEnabler dspZoneControlable)
				{
					dspZoneControlable.AudioZoneEnableChanged += this.ZoneEnableChangeHandler;
				}
			}
		}

		private void UnsusbcribeDevices()
		{
			foreach (var dsp in this.dspDevices.GetAllDevices())
			{
				dsp.ConnectionChanged -= this.DspConnectionChangeHandler;
				dsp.AudioInputLevelChanged -= this.AudioInputLevelChangeHandler;
				dsp.AudioInputMuteChanged -= this.AudioInputMuteChangeHandler;
				dsp.AudioOutputLevelChanged -= this.AudioOutputLevelChangeHandler;
				dsp.AudioOutputMuteChanged -= this.AudioOutputMuteChangeHandler;
			}
		}

		private void DspConnectionChangeHandler(object sender, GenericSingleEventArgs<string> e)
		{
			this.Notify(this.AudioDspConnectionStatusChanged, e.Arg);
		}

		private void ZoneEnableChangeHandler(object sender, GenericDualEventArgs<string, string> e)
		{
			var temp = this.AudioZoneEnableChanged;
			temp?.Invoke(this, e);
		}

		private void AudioOutputMuteChangeHandler(object sender, GenericDualEventArgs<string, string> e)
		{
			Logger.Debug("AudioControlApp.AudioOutputMuteChangeHandler()- {0}, {1}", e.Arg1, e.Arg2);
			this.Notify(this.AudioOutputMuteChanged, e.Arg2);
		}

		private void AudioOutputLevelChangeHandler(object sender, GenericDualEventArgs<string, string> e)
		{
			this.Notify(this.AudioOutputLevelChanged, e.Arg2);
		}

		private void AudioInputMuteChangeHandler(object sender, GenericDualEventArgs<string, string> e)
		{
			this.Notify(this.AudioInputMuteChanged, e.Arg2);
		}

		private void AudioInputLevelChangeHandler(object sender, GenericDualEventArgs<string, string> e)
		{
			this.Notify(this.AudioInputLevelChanged, e.Arg2);
		}

		private void AudioRouteChangeHandler(object sender, GenericDualEventArgs<string, string> e)
		{
			this.Notify(this.AudioOutputRouteChanged, e.Arg2);
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					this.UnsusbcribeDevices();
				}

				this.disposed = true;
			}
		}

		private void Notify(EventHandler<GenericSingleEventArgs<string>> handler, string arg)
		{
			var temp = handler;
			temp?.Invoke(this, new GenericSingleEventArgs<string>(arg));
		}
	}

}
