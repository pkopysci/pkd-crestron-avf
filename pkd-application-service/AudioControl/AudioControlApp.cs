// ReSharper disable SuspiciousTypeConversion.Global

using System.Collections.ObjectModel;
using pkd_application_service.Base;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_hardware_service.AudioDevices;
using pkd_hardware_service.BaseDevice;
using pkd_hardware_service.Routable;

namespace pkd_application_service.AudioControl
{
	/// <summary>
    /// Class for handling audio adjustments and updates.
    /// </summary>
    internal class AudioControlApp : IAudioControlApp, IAudioPresetApp, IDisposable
	{
		private readonly DeviceContainer<IAudioControl> _dspDevices;
		private readonly List<AudioChannelInfoContainer> _inputChannels;
		private readonly List<AudioChannelInfoContainer> _outputChannels;
		private readonly Dictionary<string, List<InfoContainer>> _allPresets;
		private bool _disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="AudioControlApp"/> class.
		/// </summary>
		/// <param name="dspDevices">Collection of all devices in the system with audio controls.</param>
		/// <param name="inputChannels">Collection of all input audio channels in the system with level control.</param>
		/// <param name="outputChannels">Collection of all audio output channels in the system with level control.</param>
		/// <param name="allPresets">Collection of audio preset data that was included in the config.</param>
		public AudioControlApp(
			DeviceContainer<IAudioControl> dspDevices,
			List<AudioChannelInfoContainer> inputChannels,
			List<AudioChannelInfoContainer> outputChannels,
			Dictionary<string, List<InfoContainer>> allPresets)
		{
			ParameterValidator.ThrowIfNull(dspDevices, "Ctor", nameof(dspDevices));
			ParameterValidator.ThrowIfNull(inputChannels, "Ctor", nameof(inputChannels));
			ParameterValidator.ThrowIfNull(outputChannels, "Ctor", nameof(outputChannels));
			ParameterValidator.ThrowIfNull(allPresets, "Ctor", nameof(allPresets));

			_dspDevices = dspDevices;
			_inputChannels = inputChannels;
			_outputChannels = outputChannels;
			_allPresets = allPresets;
			SubscribeDevices();
		}

		~AudioControlApp()
		{
			Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioOutputLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioOutputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioInputLevelChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioInputMuteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioDspConnectionStatusChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? AudioOutputRouteChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericDualEventArgs<string, string>>? AudioZoneEnableChanged;

		/// <inheritdoc/>
		public ReadOnlyCollection<AudioChannelInfoContainer> GetAudioInputChannels()
		{
			return new ReadOnlyCollection<AudioChannelInfoContainer>(_inputChannels);
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<AudioChannelInfoContainer> GetAudioOutputChannels()
		{
			return new ReadOnlyCollection<AudioChannelInfoContainer>(_outputChannels);
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<InfoContainer> GetAllAudioDspDevices()
		{
			var deviceInfo = new List<InfoContainer>();
			foreach (var dsp in _dspDevices.GetAllDevices())
			{
				deviceInfo.Add(new InfoContainer(dsp.Id, dsp.Label, string.Empty, [], dsp.IsOnline)
				{
					Manufacturer = dsp.Manufacturer,
					Model = dsp.Model,
				});
			}

			return new ReadOnlyCollection<InfoContainer>(deviceInfo);
		}

		/// <inheritdoc/>
		public bool QueryAudioDspConnectionStatus(string id)
		{
			var device = _dspDevices.GetDevice(id);
			if (device == null)
			{
				Logger.Error("AudioControlApp.QueryAudioDspConnectionStatus({0}) - no device with that id.", id);
				return false;
			}
			
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

			foreach (var dsp in _dspDevices.GetAllDevices())
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

			foreach (var dsp in _dspDevices.GetAllDevices())
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
			foreach (var dsp in _dspDevices.GetAllDevices())
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

			foreach (var dsp in _dspDevices.GetAllDevices())
			{
				if (dsp.GetAudioOutputIds().Contains(id) && dsp is IAudioRoutable routable)
				{
					return routable.GetCurrentAudioSource(id);
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

			foreach (var dsp in _dspDevices.GetAllDevices())
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

			foreach (var dsp in _dspDevices.GetAllDevices())
			{
				if (dsp is not IAudioZoneEnabler) continue;
				if ((dsp.GetAudioInputIds().Contains(channelId) || dsp.GetAudioOutputIds().Contains(channelId))
				    && dsp is IAudioZoneEnabler zoneEnabler)
				{
					return zoneEnabler.QueryAudioZoneEnable(channelId, zoneId);
				}
			}

			Logger.Error("ApplicationService.AudioControlApp.QueryAudioZoneState() - could not find a DSP with channel {0}", channelId);
			return false;
		}

		/// <inheritdoc/>
		public void SetAudioInputLevel(string id, int level)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "SetAudioInputLevel", nameof(id));
			if (level is < 0 or > 100)
			{
				Logger.Error("SetAudioInputLevel() - level {0} is out of range 0-100.", level);
				return;
			}

			Logger.Debug("ApplicationService.AudioControlApp.SetAudioInputLevel({0},{1}", id, level);

			var found = false;
			foreach (var dsp in _dspDevices.GetAllDevices())
			{
				if (!dsp.GetAudioInputIds().Contains(id)) continue;
				dsp.SetAudioInputLevel(id, level);
				found = true;
				break;
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
			foreach (var dsp in _dspDevices.GetAllDevices())
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

			var found = false;
			foreach (var dsp in _dspDevices.GetAllDevices())
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
			ParameterValidator.ThrowIfNullOrEmpty(id, "SetAudioOutputMute", nameof(id));

			Logger.Debug($"AudioControlApp.SetAudioOutputMute({id}, {mute}");
			
			var found = false;
			foreach (var dsp in _dspDevices.GetAllDevices())
			{
				if (!dsp.GetAudioOutputIds().Contains(id)) continue;
				found = true;
				dsp.SetAudioOutputMute(id, mute);
				break;
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
			foreach (var dsp in _dspDevices.GetAllDevices())
			{
				if (!dsp.GetAudioOutputIds().Contains(destId) || dsp is not IAudioRoutable routable) continue;
				routable.RouteAudio(srcId, destId);
				break;
			}
		}

		/// <inheritdoc/>
		public void ToggleAudioZoneState(string channelId, string zoneId)
		{
			if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(zoneId))
			{
				Logger.Error("AudioControlApp.ToggleAudioZoneState({0}, {1}) - channelId and zoneId cannot be null or empty.", channelId, zoneId);
				return;
			}

			foreach (var dsp in _dspDevices.GetAllDevices())
			{
				if (dsp is not IAudioZoneEnabler enabler) continue;
				if (dsp.GetAudioInputIds().Contains(channelId) || dsp.GetAudioOutputIds().Contains(channelId))
				{
					enabler.ToggleAudioZoneEnable(channelId, zoneId);
				}
			}
		}

		/// <inheritdoc/>
		public void SetAudioZoneState(string channelId, string zoneId, bool state)
		{
			if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(zoneId))
			{
				Logger.Error($"AudioControlApp.SetAudioZoneState({channelId}, {zoneId}, {state}) - channelId and zoneId cannot be null or empty.");
				return;
			}

			foreach (var dsp in _dspDevices.GetAllDevices())
			{
				if (dsp is not IAudioZoneEnabler enabler) continue;
				if (dsp.GetAudioInputIds().Contains(channelId) || dsp.GetAudioOutputIds().Contains(channelId))
				{
					enabler.SetAudioZoneEnable(channelId, zoneId, state);
				}
			}
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<InfoContainer> QueryDspAudioPresets(string dspId)
		{
			return _allPresets.TryGetValue(dspId, out var presets) ? 
				new ReadOnlyCollection<InfoContainer>(presets) :
				new ReadOnlyCollection<InfoContainer>(new List<InfoContainer>());
		}

		/// <inheritdoc/>
		public void RecallAudioPreset(string dspId, string presetId)
		{
			var dsp = _dspDevices.GetDevice(dspId);
			if (dsp == null)
			{
				Logger.Error("AudioControlApp.RecallAudioPreset({0}, {1}) - DSP not found.", dspId, presetId);
				return;
			}

			dsp.RecallAudioPreset(presetId);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void SubscribeDevices()
		{
			foreach (var dsp in _dspDevices.GetAllDevices())
			{
				dsp.ConnectionChanged += DspConnectionChangeHandler;
				dsp.AudioInputLevelChanged += AudioInputLevelChangeHandler;
				dsp.AudioInputMuteChanged += AudioInputMuteChangeHandler;
				dsp.AudioOutputLevelChanged += AudioOutputLevelChangeHandler;
				dsp.AudioOutputMuteChanged += AudioOutputMuteChangeHandler;

				if (dsp is IAudioRoutable routable)
				{
					routable.AudioRouteChanged += AudioRouteChangeHandler;
				}

				if (dsp is IAudioZoneEnabler dspZoneControllable)
				{
					dspZoneControllable.AudioZoneEnableChanged += ZoneEnableChangeHandler;
				}
			}
		}

		private void UnsubscribeDevices()
		{
			foreach (var dsp in _dspDevices.GetAllDevices())
			{
				dsp.ConnectionChanged -= DspConnectionChangeHandler;
				dsp.AudioInputLevelChanged -= AudioInputLevelChangeHandler;
				dsp.AudioInputMuteChanged -= AudioInputMuteChangeHandler;
				dsp.AudioOutputLevelChanged -= AudioOutputLevelChangeHandler;
				dsp.AudioOutputMuteChanged -= AudioOutputMuteChangeHandler;
			}
		}

		private void DspConnectionChangeHandler(object? sender, GenericSingleEventArgs<string> e)
		{
			Notify(AudioDspConnectionStatusChanged, e.Arg);
		}

		private void ZoneEnableChangeHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			var temp = AudioZoneEnableChanged;
			temp?.Invoke(this, e);
		}

		private void AudioOutputMuteChangeHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			Notify(AudioOutputMuteChanged, e.Arg2);
		}

		private void AudioOutputLevelChangeHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			Notify(AudioOutputLevelChanged, e.Arg2);
		}

		private void AudioInputMuteChangeHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			Notify(AudioInputMuteChanged, e.Arg2);
		}

		private void AudioInputLevelChangeHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			Notify(AudioInputLevelChanged, e.Arg2);
		}

		private void AudioRouteChangeHandler(object? sender, GenericDualEventArgs<string, string> e)
		{
			Notify(AudioOutputRouteChanged, e.Arg2);
		}

		private void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				UnsubscribeDevices();
			}

			_disposed = true;
		}

		private void Notify(EventHandler<GenericSingleEventArgs<string>>? handler, string arg)
		{
			handler?.Invoke(this, new GenericSingleEventArgs<string>(arg));
		}
	}
}
