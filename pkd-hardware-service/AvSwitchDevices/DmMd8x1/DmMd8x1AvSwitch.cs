using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_domain_service.Data.RoutingData;
using pkd_hardware_service.AudioDevices;
using SimplSharpProDM;

namespace pkd_hardware_service.AvSwitchDevices.DmMd8x1
{
    /// <summary>
    /// Wrapper class for controlling a Crestron DM-MD-8x1 AV Switch.
    /// </summary>
    public class DmMd8X1AvSwitch : BaseDevice.BaseDevice, IAvSwitcher, IDisposable, IAudioControl
    {
        private const short VolMin = -800;
        private const short VolMax = 100;
        private const uint MicIndex = 1;
        private readonly DmMd4kAudioOutputStream? _analogAudio;
        private readonly List<string> _inputIds;
        private readonly List<string> _outputIds;
        private readonly DmMd8x14kC _dmSwitch;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DmMd8X1AvSwitch"/> class.
        /// </summary>
        /// <param name="config">The configuration data used to control the device.</param>
        /// <param name="parent">The root control system that is running the program.</param>
        public DmMd8X1AvSwitch(MatrixData config, CrestronControlSystem parent)
        {
            ParameterValidator.ThrowIfNull(config, "Ctor", nameof(config));
            ParameterValidator.ThrowIfNull(parent, "Ctor", nameof(parent));

            _inputIds = [];
            _outputIds = [];
            Id = config.Id;
            Label = config.Label;
            _dmSwitch = new DmMd8x14kC((uint)config.Connection.Port, parent)
            {
                Description = Id,
            };

            _dmSwitch.DMSystemChange += DmSwitch_DMSystemChange;
            _dmSwitch.OnlineStatusChange += DmSwitch_OnlineStatusChange;
            _dmSwitch.DMOutputChange += DmSwitch_DMOutputChange;
            _analogAudio = _dmSwitch.Outputs[1]?.AudioOutputStream;

            Manufacturer = "Crestron";
            Model = "HD-MD-8x1";
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
            return _inputIds;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetAudioOutputIds()
        {
            return _outputIds;
        }

        /// <inheritdoc/>
        public void AddInputChannel(string id, string levelTag, string muteTag, int bankIndex, int levelMax,
            int levelMin, int routerIndex, List<string> _)
        {
            ParameterValidator.ThrowIfNullOrEmpty(id, "AddInputChannel", "id");
            _inputIds.Add(id);
        }

        /// <inheritdoc/>
        public void AddOutputChannel(string id, string levelTag, string muteTag, string routerTag, int routerIndex,
            int bankIndex, int levelMax, int levelMin, List<string> _)
        {
            ParameterValidator.ThrowIfNullOrEmpty(id, "AddOutputChannel", "id");
            _outputIds.Add(id);
        }

        /// <summary>
        /// Interface method not supported by this device.
        /// </summary>
        public void AddPreset(string id, int index)
        {
            Logger.Warn(
                "Dm8x1AvSwitch {0} AddPreset({1},{2}) - Presets not supported by this device.",
                Id,
                id,
                index);
        }

        /// <inheritdoc/>
        public int GetAudioInputLevel(string id)
        {
            if (!CheckOnline("AudioInputLevel") || _analogAudio == null)
            {
                return 0;
            }

            return ConvertToPercent(_analogAudio.OutputMixer.MicLevelFeedback[MicIndex]!.ShortValue);
        }

        /// <inheritdoc/>
        public void SetAudioInputLevel(string id, int level)
        {
            if (!CheckOnline("SetAudioInputLevel") || _analogAudio == null)
            {
                return;
            }

            var scaled = ConvertToDb(level);
            if (scaled is < VolMin or > VolMax)
            {
                Logger.Error($"AvSwitch {Id} SetAudioInputLevel(): argument must be between {VolMin} and {VolMax}.");
                return;
            }

            _analogAudio.OutputMixer.MicLevel[MicIndex]!.ShortValue = scaled;
        }

        /// <inheritdoc/>
        public int GetAudioOutputLevel(string id)
        {
            if (!CheckOnline("AudioOutputLevel") || _analogAudio == null)
            {
                return 0;
            }

            return ConvertToPercent(_analogAudio.SourceLevelFeedBack.ShortValue);
        }

        /// <inheritdoc/>
        public void SetAudioOutputLevel(string id, int level)
        {
            if (!CheckOnline("SetAudioOutputLevel") || _analogAudio == null)
            {
                return;
            }

            var scaled = ConvertToDb(level);
            if (scaled is < VolMin or > VolMax)
            {
                Logger.Error($"AvSwitch {Id} SetAudioOutputLevel(): argument must be between {VolMin} and {VolMax}.");
                return;
            }

            _analogAudio.SourceLevel.ShortValue = scaled;
        }

        /// <inheritdoc/>
        public void SetAudioInputMute(string id, bool mute)
        {
            if (!CheckOnline("SetAudioInputMute") || _analogAudio == null)
            {
                return;
            }

            if (mute)
            {
                _analogAudio.OutputMixer.MicMuteOn(MicIndex);
            }
            else
            {
                _analogAudio.OutputMixer.MicMuteOff(MicIndex);
            }
        }

        /// <inheritdoc/>
        public bool GetAudioInputMute(string id)
        {
            if (!CheckOnline("AudioInputMute") || _analogAudio == null)
            {
                return false;
            }

            return _analogAudio.OutputMixer.MicMuteOnFeedback[MicIndex]!.BoolValue;
        }

        /// <inheritdoc/>
        public void SetAudioOutputMute(string id, bool state)
        {
            if (!CheckOnline("SetAudioOutputMute") || _analogAudio == null)
            {
                return;
            }

            if (state)
            {
                _analogAudio.SourceMuteOn();
            }
            else
            {
                _analogAudio.SourceMuteOff();
            }
        }

        /// <inheritdoc/>
        public bool GetAudioOutputMute(string id)
        {
            if (!CheckOnline("AudioOutputMute") || _analogAudio == null)
            {
                return false;
            }

            return _analogAudio.SourceMuteOnFeedBack.BoolValue;
        }

        /// <inheritdoc/>
        public void ClearVideoRoute(uint output)
        {
            if (!CheckOnline("ClearVideoRoute"))
            {
                return;
            }

            if (output > _dmSwitch.NumberOfOutputs)
            {
                Logger.Error(
                    $"AvSwitch {Id} ClearVideoRoute({output}) - input or output argument out of bounds. Max out = {_dmSwitch.NumberOfOutputs}");

                return;
            }

            _dmSwitch.Outputs[output]!.VideoOut = null;
        }

        /// <summary>
        /// Interface feature not supported by this device.
        /// </summary>
        /// <param name="id">The id of the preset to recall.</param>
        public void RecallAudioPreset(string id)
        {
        }

        /// <inheritdoc/>
        public override void Connect()
        {
            if (_dmSwitch.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                Logger.Error($"DM-8x1 switch {Id} - Failed to register device: {_dmSwitch.RegistrationFailureReason}");
            }
        }

        /// <inheritdoc/>
        public override void Disconnect()
        {
            if (_dmSwitch.UnRegister() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                Logger.Error(string.Format(
                    "DM-8x1 switch {0} - Failed to unregister device: {1}",
                    Id,
                    _dmSwitch.UnRegistrationFailureReason));
            }
        }

        /// <inheritdoc/>
        public uint GetCurrentVideoSource(uint output)
        {
            return GetCurrentSource(output, "GetCurrentVideoSource");
        }

        /// <inheritdoc/>
        public void RouteVideo(uint source, uint output)
        {
            if (!CheckOnline("RouteVideo"))
            {
                return;
            }

            if (source > _dmSwitch.NumberOfInputs ||
                output > _dmSwitch.NumberOfOutputs ||
                source == 0 ||
                output == 0)
            {
                Logger.Error(
                    $"AvSwitch {Id} RouteVideo({source},{output}) - input or output argument out of bounds. Input range = 1-{_dmSwitch.NumberOfInputs}, output range = 1-{_dmSwitch.NumberOfOutputs}");

                return;
            }

            _dmSwitch.Outputs[output]!.VideoOut = _dmSwitch.Inputs[source];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void Initialize(string hostName, int port, string id, string label, int numInputs, int numOutputs)
        {
            _dmSwitch.Register();
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                Disconnect();
                _dmSwitch.Dispose();
            }

            _isDisposed = true;
        }

        private void DmSwitch_DMOutputChange(Switch? device, DMOutputEventArgs args)
        {
            switch (args.EventId)
            {
                case DMOutputEventIds.SourceMuteOnFeedBackEventId:
                    NotifyOutputMuteChange();
                    break;
                case DMOutputEventIds.SourceLevelFeedBackEventId:
                    NotifyOutputLevelChange();
                    break;
                case DMOutputEventIds.Mic1LevelFeedBackEventId:
                    NotifyInputLevelChange();
                    break;
                case DMOutputEventIds.Mic1MuteOnFeedBackEventId:
                    NotifyInputMuteChange();
                    break;
                case DMOutputEventIds.VideoOutEventId:
                    NotifySourceChange();
                    break;
            }
        }

        private void DmSwitch_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            IsOnline = args.DeviceOnLine;
            NotifyOnlineStatus();
        }

        private void DmSwitch_DMSystemChange(Switch? device, DMSystemEventArgs args)
        {
            Logger.Info(string.Format(
                "AvSwitch {0} DmSwitch_DMSystemChange: ID: {1}, index: {2}",
                Id,
                args.EventId,
                args.Index));
        }

        private bool CheckOnline(string methodName)
        {
            if (!IsOnline)
            {
                Logger.Warn(string.Format(
                    "AvSwitch {0} - {1}: Device is offline",
                    Id,
                    methodName));
            }

            return IsOnline;
        }

        private uint GetCurrentSource(uint output, string callerName)
        {
            if (output > _dmSwitch.NumberOfOutputs)
            {
                Logger.Error(
                    $"AvSwitch {Id} {callerName}() ({output}) - input or output argument out of bounds. Max out = {_dmSwitch.NumberOfOutputs}");

                return 0;
            }

            var routed = _dmSwitch.Outputs[output]!.VideoOutFeedback;
            return routed?.Number ?? 0;
        }

        private void NotifyOutputMuteChange()
        {
            var temp = AudioOutputMuteChanged;
            if (temp == null) return;
            var channelId = (_outputIds.Count > 0) ? _outputIds[0] : Id;
            temp.Invoke(this, new GenericDualEventArgs<string, string>(Id, channelId));
        }

        private void NotifyOutputLevelChange()
        {
            var temp = AudioOutputLevelChanged;
            if (temp == null) return;
            var channelId = (_outputIds.Count > 0) ? _outputIds[0] : Id;
            temp.Invoke(this, new GenericDualEventArgs<string, string>(Id, channelId));
        }

        private void NotifySourceChange()
        {
            VideoRouteChanged?.Invoke(this, new GenericDualEventArgs<string, uint>(Id, 1));
        }

        private void NotifyInputLevelChange()
        {
            var temp = AudioInputLevelChanged;
            if (temp == null) return;
            var channelId = (_inputIds.Count > 0) ? _outputIds[0] : Id;
            temp.Invoke(this, new GenericDualEventArgs<string, string>(Id, channelId));
        }

        private void NotifyInputMuteChange()
        {
            var temp = AudioInputMuteChanged;
            if (temp == null) return;
            var channelId = (_inputIds.Count > 0) ? _outputIds[0] : Id;
            temp.Invoke(this, new GenericDualEventArgs<string, string>(Id, channelId));
        }

        private static short ConvertToDb(int percent)
        {
            const int percentMax = 100;
            const int dbMin = -800;
            const int dbRange = 900;

            return (short)(((percent * dbRange) / percentMax) + dbMin);
        }

        private static int ConvertToPercent(int dbVal)
        {
            const int dbMin = -800;
            const int percentMin = 0;
            const int dbRange = 900;
            const int percentRange = 100;

            return (((dbVal - dbMin) * percentRange) / dbRange) + percentMin;
        }
    }
}