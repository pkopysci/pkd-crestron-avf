using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;
using pkd_ui_service.Fusion.DeviceUse;
using pkd_ui_service.Fusion.ErrorManagement;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace pkd_ui_service.Fusion
{
	public class FusionInterface : IFusionInterface
	{
		private const string PcTag = "pc";
		private const string VgaTag = "vga";
		private const string HdmiTag = "hdmi";
		private const string PodiumTag = "podium";
		private const string OtherMicTag = "other";

		private readonly Dictionary<uint, Action<bool>> userBoolSigHandlers;
		private readonly List<FusionDeviceData> sources;
		private readonly List<FusionDeviceData> mics;
		private readonly FusionRoom fusion;
		private readonly FusionDeviceUse useTracker;
		private readonly FusionErrorManager errorManager;
		private bool disposed;

		public FusionInterface(uint ipId, CrestronControlSystem control, string name, string guid)
		{
			fusion = new FusionRoom(ipId, control, name, guid);
			useTracker = new FusionDeviceUse(fusion);
			errorManager = new FusionErrorManager(fusion);
			sources = [];
			mics = [];
			userBoolSigHandlers = new Dictionary<uint, Action<bool>>()
			{
				{ (uint)FusionBooleanJoins.VideoMute, VideoBlankHandler},
				{ (uint)FusionBooleanJoins.VideoFreeze, VideoFreezeHandler },
				{ (uint)FusionBooleanJoins.SelectPc, PcSelectHandler },
				{ (uint)FusionBooleanJoins.SelectVga, VgaSelectHandler },
				{ (uint)FusionBooleanJoins.SelectHdmi, HdmiSelectHandler },
				{ (uint)FusionBooleanJoins.VolumeMute, ToggleProgramMuteHandler },
				{ (uint)FusionBooleanJoins.PodiumMute, TogglePodiumMuteHandler },
				{ (uint)FusionBooleanJoins.MicMute, ToggleMicMuteHandler },
			};
		}

		~FusionInterface()
		{
			Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler<EventArgs>? OnlineStatusChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<bool>>? SystemStateChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<bool>>? DisplayPowerChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<EventArgs>? AudioMuteChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<EventArgs>? DisplayBlankChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<EventArgs>? DisplayFreezeChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<uint>>? ProgramAudioChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? MicMuteChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>>? SourceSelectRequested;

		/// <inheritdoc/>
		public bool IsOnline => fusion.IsOnline;

		/// <inheritdoc/>
		public void Initialize()
		{
			fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.VideoMute, "Video Mute", eSigIoMask.InputOutputSig);
			fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.VideoFreeze, "Video Freeze", eSigIoMask.InputOutputSig);
			fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.SelectHdmi, "Select HDMI", eSigIoMask.InputOutputSig);
			fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.PodiumMute, "Podium Mute", eSigIoMask.InputOutputSig);
			fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.VolumeMute, "Volume Mute", eSigIoMask.InputOutputSig);
			fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.MicMute, "Mic Mute", eSigIoMask.InputOutputSig);
			fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.SelectPc, "Select PC", eSigIoMask.InputOutputSig);
			fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.SelectVga, "Select VGA", eSigIoMask.InputOutputSig);
			fusion.AddSig(eSigType.UShort, (uint)FusionUshortJoins.AudioLevel, "Audio Levels", eSigIoMask.InputOutputSig);
			fusion.AddSig(eSigType.UShort, (uint)FusionUshortJoins.LampHours, "Lamp Hours", eSigIoMask.InputSigOnly);
			fusion.OnlineStatusChange += FusionConnectionHandler;
			fusion.FusionStateChange += FusionStateChangeHandler;

			FusionRVI.GenerateFileForAllFusionDevices();
			if (fusion.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Error("FusionInterface.Initialize() - Failed to register connection: {0}", fusion.RegistrationFailureReason);
			}
		}

		/// <inheritdoc/>
		public void UpdateSystemState(bool state)
		{
			fusion.SystemPowerOn.InputSig.BoolValue = state;
		}

		/// <inheritdoc/>
		public void UpdateProgramAudioMute(bool state)
		{
			fusion
				.UserDefinedBooleanSigDetails[(uint)FusionBooleanJoins.VolumeMute]!
				.InputSig.BoolValue = state;
		}

		/// <inheritdoc/>
		public void UpdateProgramAudioLevel(uint level)
		{
			fusion
				.UserDefinedUShortSigDetails[(uint)FusionUshortJoins.AudioLevel]!
				.InputSig.UShortValue = (ushort)level;
		}

		/// <inheritdoc/>
		public void UpdateDisplayPower(bool state)
		{
			fusion.DisplayPowerOn.InputSig.BoolValue = state;
		}

		/// <inheritdoc/>
		public void UpdateDisplayBlank(bool state)
		{
			fusion
				.UserDefinedBooleanSigDetails[(ushort)FusionBooleanJoins.VideoMute]!
				.InputSig.BoolValue = state;
		}

		/// <inheritdoc/>
		public void UpdateDisplayFreeze(bool state)
		{
			fusion
				.UserDefinedBooleanSigDetails[(ushort)FusionBooleanJoins.VideoFreeze]!
				.InputSig.BoolValue = state;
		}

		/// <inheritdoc/>
		public void UpdateMicMute(string id, bool state)
		{
			var found = mics.FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCulture));
			if (found == null) return;
			var join = found.Tags.Contains(PodiumTag) ? (uint)FusionBooleanJoins.PodiumMute : (uint)FusionBooleanJoins.MicMute;
			fusion.UserDefinedBooleanSigDetails[join]!.InputSig.BoolValue = state;
		}

		/// <inheritdoc/>
		public void UpdateSelectedSource(string id)
		{
			var found = sources.FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCulture));
			if (found == null) return;

			fusion.UserDefinedBooleanSigDetails[(uint)FusionBooleanJoins.SelectHdmi]!.InputSig.BoolValue = found.Tags.Contains(HdmiTag);
			fusion.UserDefinedBooleanSigDetails[(uint)FusionBooleanJoins.SelectVga]!.InputSig.BoolValue = found.Tags.Contains(VgaTag);
			fusion.UserDefinedBooleanSigDetails[(uint)FusionBooleanJoins.SelectPc]!.InputSig.BoolValue = found.Tags.Contains(PcTag);
		}

		/// <inheritdoc/>
		public void AddAvSource(string id, string label, string[] tags)
		{
			sources.Add(new FusionDeviceData()
			{
				Id = id,
				Label = label,
				Tags = tags,
			});
		}

		/// <inheritdoc/>
		public void AddMicrophone(string id, string label, string[] tags)
		{
			mics.Add(new FusionDeviceData()
			{
				Id = id,
				Label = label,
				Tags = tags,
			});
		}

		/// <inheritdoc/>
		public void AddDeviceToUseTracking(string id, string label)
		{
			useTracker.AddDeviceToUseTracking(id, label);
		}

		/// <inheritdoc/>
		public void AddDisplayToUseTracking(string id, string label)
		{
			useTracker.AddDisplayToUseTracking(id, label);
		}

		/// <inheritdoc/>
		public void StartDeviceUse(string id)
		{
			useTracker.StartDeviceUse(id);
		}

		/// <inheritdoc/>
		public void StopDeviceUse(string id)
		{
			useTracker.StopDeviceUse(id);
		}

		/// <inheritdoc/>
		public void StartDisplayUse(string id)
		{
			useTracker.StartDisplayUse(id);
		}

		/// <inheritdoc/>
		public void StopDisplayUse(string id)
		{
			useTracker.StopDisplayUse(id);
		}

		/// <inheritdoc/>
		public void AddOfflineDevice(string devId, string devName)
		{
			errorManager.AddOfflineDevice(devId, devName);
		}

		/// <inheritdoc/>
		public void ClearOfflineDevice(string devId)
		{
			Logger.Debug("FusionInterface.ClearOfflineDevice({0})", devId);

			errorManager.ClearOfflineDevice(devId);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing)
			{
				fusion.OnlineStatusChange -= FusionConnectionHandler;
				fusion.FusionStateChange -= FusionStateChangeHandler;
				fusion.RemoveAllSigs();
				userBoolSigHandlers.Clear();
				fusion.UnRegister();
			}

			disposed = true;
		}

		private void FusionConnectionHandler(GenericBase device, OnlineOfflineEventArgs args)
		{
			Notify(OnlineStatusChanged);
		}

		private void FusionStateChangeHandler(FusionBase device, FusionStateEventArgs args)
		{
			switch (args.EventId)
			{
				case FusionEventIds.SystemPowerOnReceivedEventId:
					SystemStateOnReceived(args.UserConfiguredSigDetail);
					break;

				case FusionEventIds.SystemPowerOffReceivedEventId:
					SystemStateOffReceived(args.UserConfiguredSigDetail);
					break;

				case FusionEventIds.DisplayPowerOnReceivedEventId:
					DisplayPowerOnReceived(args.UserConfiguredSigDetail);
					break;

				case FusionEventIds.DisplayPowerOffReceivedEventId:
					DisplayPowerOffReceived(args.UserConfiguredSigDetail);
					break;

				case FusionEventIds.UserConfiguredBoolSigChangeEventId:
					HandleUserBoolSig(args.UserConfiguredSigDetail);
					break;

				case FusionEventIds.UserConfiguredUShortSigChangeEventId:
					HandleUserUshortSig(args.UserConfiguredSigDetail);
					break;
			}
		}

		private void Notify(EventHandler<EventArgs>? handler)
		{
			var temp = handler;
			temp?.Invoke(this, EventArgs.Empty);
		}

		private void Notify(EventHandler<GenericSingleEventArgs<bool>>? handler, bool state)
		{
			var temp = handler;
			temp?.Invoke(this, new GenericSingleEventArgs<bool>(state));
		}

		private void Notify(EventHandler<GenericSingleEventArgs<string>>? handler, string value)
		{
			var temp = handler;
			temp?.Invoke(this, new GenericSingleEventArgs<string>(value));
		}

		private void HandleUserBoolSig(object sig)
		{
			if (!(sig is BooleanSigData tempSig))
			{
				return;
			}

			if (userBoolSigHandlers.TryGetValue(tempSig.Number, out var action))
			{
				action.Invoke(tempSig.OutputSig.BoolValue);
			}
		}

		private void HandleUserUshortSig(object sig)
		{
			if (sig is not UShortSigData tempSig) return;
			if (tempSig.Number != (ushort)FusionUshortJoins.AudioLevel) return;
			
			var temp = ProgramAudioChangeRequested;
			temp?.Invoke(this, new GenericSingleEventArgs<uint>(tempSig.OutputSig.UShortValue));
		}

		private void SystemStateOnReceived(object sig)
		{
			if (sig is not BooleanSigDataFixedName powerSig) return;

			bool buttonState = powerSig.OutputSig.BoolValue;
			if (!buttonState) return;
			
			Logger.Debug("FusionInterface.SystemStateOnReceived()");
			Notify(SystemStateChangeRequested, true);
		}

		private void SystemStateOffReceived(object sig)
		{
			if (!(sig is BooleanSigDataFixedName powerSig))
			{
				return;
			}

			bool buttonState = powerSig.OutputSig.BoolValue;
			if (buttonState)
			{
				Logger.Debug("FusionInterface.SystemStateOffReceived()");
				Notify(SystemStateChangeRequested, false);
			}
		}

		private void DisplayPowerOnReceived(object sig)
		{
			if (!(sig is BooleanSigDataFixedName powerSig))
			{
				return;
			}

			bool buttonState = powerSig.OutputSig.BoolValue;
			if (buttonState)
			{
				Logger.Debug("FusionInterface.DisplayPowerOnReceived()");
				Notify(DisplayPowerChangeRequested, true);
			}
		}

		private void DisplayPowerOffReceived(object sig)
		{
			if (sig is not BooleanSigDataFixedName powerSig) return;
			bool buttonState = powerSig.OutputSig.BoolValue;
			if (!buttonState) return;
			
			Logger.Debug("FusionInterface.DisplayPowerOffReceived()");
			Notify(DisplayPowerChangeRequested, false);
		}

		private void VideoBlankHandler(bool buttonState)
		{
			if (buttonState)
			{
				Notify(DisplayBlankChangeRequested);
			}
		}

		private void VideoFreezeHandler(bool buttonState)
		{
			if (buttonState)
			{
				Notify(DisplayFreezeChangeRequested);
			}
		}

		private void PcSelectHandler(bool buttonState)
		{
			if (!buttonState) return;
			var found = sources.FirstOrDefault(x => x.Tags.Contains(PcTag));
			if (found == null) return;
			Notify(SourceSelectRequested, found.Id);
		}

		private void VgaSelectHandler(bool buttonState)
		{
			if (!buttonState) return;
			var found = sources.FirstOrDefault(x => x.Tags.Contains(VgaTag));
			if (found != null)
			{
				Notify(SourceSelectRequested, found.Id);
			}
		}

		private void HdmiSelectHandler(bool buttonState)
		{
			if (!buttonState) return;
			var found = sources.FirstOrDefault(x => x.Tags.Contains(HdmiTag));
			if (found != null)
			{
				Notify(SourceSelectRequested, found.Id);
			}
		}

		private void ToggleProgramMuteHandler(bool buttonState)
		{
			if (!buttonState) return;
			Notify(AudioMuteChangeRequested);
		}

		private void TogglePodiumMuteHandler(bool buttonState)
		{
			if (!buttonState) return;
			var found = mics.FirstOrDefault(x => x.Tags.Contains(PodiumTag));
			if (found != null)
			{
				Notify(MicMuteChangeRequested, found.Id);
			}
		}

		private void ToggleMicMuteHandler(bool buttonState)
		{
			if (!buttonState) return;
			var found = mics.FirstOrDefault(x => x.Tags.Contains(OtherMicTag));
			if (found != null)
			{
				Notify(MicMuteChangeRequested, found.Id);
			}
		}
	}
}
