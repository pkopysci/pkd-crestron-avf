namespace pkd_ui_service.Fusion
{
	using Crestron.SimplSharpPro;
	using Crestron.SimplSharpPro.Fusion;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using pkd_ui_service.Fusion.DeviceUse;
	using pkd_ui_service.Fusion.ErrorManagement;
	using System;
	using System.Collections.Generic;
	using System.Linq;


	public class FusionInterface : IFusionInterface
	{
		private static readonly string PC_TAG = "pc";
		private static readonly string VGA_TAG = "vga";
		private static readonly string HDMI_TAG = "hdmi";
		private static readonly string PODIUIM_TAG = "podium";
		private static readonly string OTHER_MIC_TAG = "other";

		private readonly Dictionary<uint, Action<bool>> userBoolSigHandlers;
		private readonly List<FusionDeviceData> sources;
		private readonly List<FusionDeviceData> mics;
		private readonly FusionRoom fusion;
		private readonly IFusionDeviceUse UseTracker;
		private readonly IFusionErrorManager errorManager;
		private bool disposed;

		public FusionInterface(uint ipId, CrestronControlSystem control, string name, string guid)
		{
			this.fusion = new FusionRoom(ipId, control, name, guid);
			this.UseTracker = new FusionDeviceUse(this.fusion);
			this.errorManager = new FusionErrorManager(this.fusion);
			this.sources = new List<FusionDeviceData>();
			this.mics = new List<FusionDeviceData>();
			this.userBoolSigHandlers = new Dictionary<uint, Action<bool>>()
			{
				{ (uint)FusionBooleanJoins.VideoMute, this.VideoBlankHandler},
				{ (uint)FusionBooleanJoins.VideoFreeze, this.VideoFreezeHandler },
				{ (uint)FusionBooleanJoins.SelectPc, this.PcSelectHandler },
				{ (uint)FusionBooleanJoins.SelectVga, this.VgaSelectHandler },
				{ (uint)FusionBooleanJoins.SelectHdmi, this.HdmiSelectHandler },
				{ (uint)FusionBooleanJoins.VolumeMute, this.ToggleProgramMuteHandler },
				{ (uint)FusionBooleanJoins.PodiumMute, this.TogglePodiumMuteHandler },
				{ (uint)FusionBooleanJoins.MicMute, this.ToggleMicMuteHandler },
			};
		}

		~FusionInterface()
		{
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler<EventArgs> OnlineStatusChanged;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<bool>> SystemStateChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<bool>> DisplayPowerChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<EventArgs> AudioMuteChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<EventArgs> DisplayBlankChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<EventArgs> DisplayFreezeChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<uint>> ProgramAudioChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> MicMuteChangeRequested;

		/// <inheritdoc/>
		public event EventHandler<GenericSingleEventArgs<string>> SourceSelectRequested;

		/// <inheritdoc/>
		public bool IsOnline { get { return this.fusion.IsOnline; } }

		/// <inheritdoc/>
		public void Initialize()
		{
			this.fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.VideoMute, "Video Mute", eSigIoMask.InputOutputSig);
			this.fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.VideoFreeze, "Video Freeze", eSigIoMask.InputOutputSig);
			this.fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.SelectHdmi, "Select HDMI", eSigIoMask.InputOutputSig);
			this.fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.PodiumMute, "Podium Mute", eSigIoMask.InputOutputSig);
			this.fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.VolumeMute, "Volume Mute", eSigIoMask.InputOutputSig);
			this.fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.MicMute, "Mic Mute", eSigIoMask.InputOutputSig);
			this.fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.SelectPc, "Select PC", eSigIoMask.InputOutputSig);
			this.fusion.AddSig(eSigType.Bool, (uint)FusionBooleanJoins.SelectVga, "Select VGA", eSigIoMask.InputOutputSig);
			this.fusion.AddSig(eSigType.UShort, (uint)FusionUshortJoins.AudioLevel, "Audio Levels", eSigIoMask.InputOutputSig);
			this.fusion.AddSig(eSigType.UShort, (uint)FusionUshortJoins.LampHours, "Lamp Hours", eSigIoMask.InputSigOnly);
			this.fusion.OnlineStatusChange += this.FusionConnectionHandler;
			this.fusion.FusionStateChange += this.FusionStateChangeHandler;

			FusionRVI.GenerateFileForAllFusionDevices();
			if (this.fusion.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Error("FusionInterface.Initialize() - Failed to register connection: {0}", this.fusion.RegistrationFailureReason);
			}
		}

		/// <inheritdoc/>
		public void UpdateSystemState(bool state)
		{
			this.fusion.SystemPowerOn.InputSig.BoolValue = state;
		}

		/// <inheritdoc/>
		public void UpdateProgramAudioMute(bool state)
		{
			this.fusion
				.UserDefinedBooleanSigDetails[(uint)FusionBooleanJoins.VolumeMute]
				.InputSig.BoolValue = state;
		}

		/// <inheritdoc/>
		public void UpdateProgramAudioLevel(uint level)
		{
			this.fusion
				.UserDefinedUShortSigDetails[(uint)FusionUshortJoins.AudioLevel]
				.InputSig.UShortValue = (ushort)level;
		}

		/// <inheritdoc/>
		public void UpdateDisplayPower(bool state)
		{
			this.fusion.DisplayPowerOn.InputSig.BoolValue = state;
		}

		/// <inheritdoc/>
		public void UpdateDisplayBlank(bool state)
		{
			this.fusion
				.UserDefinedBooleanSigDetails[(ushort)FusionBooleanJoins.VideoMute]
				.InputSig.BoolValue = state;
		}

		/// <inheritdoc/>
		public void UpdateDisplayFreeze(bool state)
		{
			this.fusion
				.UserDefinedBooleanSigDetails[(ushort)FusionBooleanJoins.VideoFreeze]
				.InputSig.BoolValue = state;
		}

		/// <inheritdoc/>
		public void UpdateMicMute(string id, bool state)
		{
			var found = this.mics.FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCulture));
			if (found != default(FusionDeviceData))
			{
				uint join = found.Tags.Contains(PODIUIM_TAG) ? (uint)FusionBooleanJoins.PodiumMute : (uint)FusionBooleanJoins.MicMute;
				this.fusion.UserDefinedBooleanSigDetails[join].InputSig.BoolValue = state;
			}
		}

		/// <inheritdoc/>
		public void UpdateSelectedSource(string id)
		{
			var found = this.sources.FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCulture));
			if (found == default(FusionDeviceData))
			{
				return;
			}

			this.fusion.UserDefinedBooleanSigDetails[(uint)FusionBooleanJoins.SelectHdmi].InputSig.BoolValue = found.Tags.Contains(HDMI_TAG);
			this.fusion.UserDefinedBooleanSigDetails[(uint)FusionBooleanJoins.SelectVga].InputSig.BoolValue = found.Tags.Contains(VGA_TAG);
			this.fusion.UserDefinedBooleanSigDetails[(uint)FusionBooleanJoins.SelectPc].InputSig.BoolValue = found.Tags.Contains(PC_TAG);
		}

		/// <inheritdoc/>
		public void AddAvSource(string id, string label, string[] tags)
		{
			this.sources.Add(new FusionDeviceData()
			{
				Id = id,
				Label = label,
				Tags = tags,
			});
		}

		/// <inheritdoc/>
		public void AddMicrophone(string id, string label, string[] tags)
		{
			this.mics.Add(new FusionDeviceData()
			{
				Id = id,
				Label = label,
				Tags = tags,
			});
		}

		/// <inheritdoc/>
		public void AddDeviceToUseTracking(string id, string label)
		{
			this.UseTracker.AddDeviceToUseTracking(id, label);
		}

		/// <inheritdoc/>
		public void AddDisplayToUseTracking(string id, string label)
		{
			this.UseTracker.AddDisplayToUseTracking(id, label);
		}

		/// <inheritdoc/>
		public void StartDeviceUse(string id)
		{
			this.UseTracker.StartDeviceUse(id);
		}

		/// <inheritdoc/>
		public void StopDeviceUse(string id)
		{
			this.UseTracker.StopDeviceUse(id);
		}

		/// <inheritdoc/>
		public void StartDisplayUse(string id)
		{
			this.UseTracker.StartDisplayUse(id);
		}

		/// <inheritdoc/>
		public void StopDisplayUse(string id)
		{
			this.UseTracker.StopDisplayUse(id);
		}

		/// <inheritdoc/>
		public void AddOfflineDevice(string devId, string devName)
		{
			this.errorManager.AddOfflineDevice(devId, devName);
		}

		/// <inheritdoc/>
		public void ClearOfflineDevice(string devId)
		{
			Logger.Debug("FusionInterface.ClearOfflineDevice({0})", devId);

			this.errorManager.ClearOfflineDevice(devId);
		}

		/// <inheritdoc/>
		public virtual void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					this.fusion.UnRegister();
					this.fusion.Dispose();
				}

				this.disposed = true;
			}
		}

		private void FusionConnectionHandler(GenericBase device, OnlineOfflineEventArgs args)
		{
			this.Notify(this.OnlineStatusChanged);
		}

		private void FusionStateChangeHandler(FusionBase device, FusionStateEventArgs args)
		{
			switch (args.EventId)
			{
				case FusionEventIds.SystemPowerOnReceivedEventId:
					this.SystemStateOnReceived(args.UserConfiguredSigDetail);
					break;

				case FusionEventIds.SystemPowerOffReceivedEventId:
					this.SystemStateOffReceived(args.UserConfiguredSigDetail);
					break;

				case FusionEventIds.DisplayPowerOnReceivedEventId:
					this.DisplayPowerOnReceived(args.UserConfiguredSigDetail);
					break;

				case FusionEventIds.DisplayPowerOffReceivedEventId:
					this.DisplayPowerOffReceived(args.UserConfiguredSigDetail);
					break;

				case FusionEventIds.UserConfiguredBoolSigChangeEventId:
					this.HandleUserBoolSig(args.UserConfiguredSigDetail);
					break;

				case FusionEventIds.UserConfiguredUShortSigChangeEventId:
					this.HandleUserUshortSig(args.UserConfiguredSigDetail);
					break;
			}
		}

		private void Notify(EventHandler<EventArgs> handler)
		{
			var temp = handler;
			temp?.Invoke(this, EventArgs.Empty);
		}

		private void Notify(EventHandler<GenericSingleEventArgs<bool>> handler, bool state)
		{
			var temp = handler;
			temp?.Invoke(this, new GenericSingleEventArgs<bool>(state));
		}

		private void Notify(EventHandler<GenericSingleEventArgs<string>> handler, string value)
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

			if (this.userBoolSigHandlers.TryGetValue(tempSig.Number, out Action<bool> action))
			{
				action.Invoke(tempSig.OutputSig.BoolValue);
			}
		}

		private void HandleUserUshortSig(object sig)
		{
			if (!(sig is UShortSigData tempSig))
			{
				return;
			}

			if (tempSig.Number == (ushort)FusionUshortJoins.AudioLevel)
			{
				var temp = this.ProgramAudioChangeRequested;
				temp?.Invoke(this, new GenericSingleEventArgs<uint>(tempSig.OutputSig.UShortValue));
			}
		}

		private void SystemStateOnReceived(object sig)
		{
			if (!(sig is BooleanSigDataFixedName powerSig))
			{
				return;
			}

			bool buttonState = powerSig.OutputSig.BoolValue;
			if (buttonState)
			{
				Logger.Debug("FusionInterface.SystemStateOnReceived()");
				this.Notify(this.SystemStateChangeRequested, true);
			}
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
				this.Notify(this.SystemStateChangeRequested, false);
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
				this.Notify(this.DisplayPowerChangeRequested, true);
			}
		}

		private void DisplayPowerOffReceived(object sig)
		{
			if (!(sig is BooleanSigDataFixedName powerSig))
			{
				return;
			}

			bool buttonState = powerSig.OutputSig.BoolValue;
			if (buttonState)
			{
				Logger.Debug("FusionINterface.DisplayPowerOffReceived()");
				this.Notify(this.DisplayPowerChangeRequested, false);
			}
		}

		private void VideoBlankHandler(bool buttonState)
		{
			if (buttonState)
			{
				this.Notify(this.DisplayBlankChangeRequested);
			}
		}

		private void VideoFreezeHandler(bool buttonState)
		{
			if (buttonState)
			{
				this.Notify(this.DisplayFreezeChangeRequested);
			}
		}

		private void PcSelectHandler(bool buttonState)
		{
			if (buttonState)
			{
				var found = this.sources.FirstOrDefault(x => x.Tags.Contains(PC_TAG));
				if (found != default(FusionDeviceData))
				{
					var temp = this.SourceSelectRequested;
					this.Notify(this.SourceSelectRequested, found.Id);
				}
			}
		}

		private void VgaSelectHandler(bool buttonState)
		{
			if (buttonState)
			{
				var found = this.sources.FirstOrDefault(x => x.Tags.Contains(VGA_TAG));
				if (found != default(FusionDeviceData))
				{
					this.Notify(this.SourceSelectRequested, found.Id);
				}
			}
		}

		private void HdmiSelectHandler(bool buttonState)
		{
			if (buttonState)
			{
				var found = this.sources.FirstOrDefault(x => x.Tags.Contains(HDMI_TAG));
				if (found != default(FusionDeviceData))
				{
					this.Notify(this.SourceSelectRequested, found.Id);
				}
			}
		}

		private void ToggleProgramMuteHandler(bool buttonState)
		{
			if (buttonState)
			{
				this.Notify(this.AudioMuteChangeRequested);
			}
		}

		private void TogglePodiumMuteHandler(bool buttonState)
		{
			if (buttonState)
			{
				var found = this.mics.FirstOrDefault(x => x.Tags.Contains(PODIUIM_TAG));
				if (found != default(FusionDeviceData))
				{
					this.Notify(this.MicMuteChangeRequested, found.Id);
				}
			}
		}

		private void ToggleMicMuteHandler(bool buttonState)
		{
			if (buttonState)
			{
				var found = this.mics.FirstOrDefault(x => x.Tags.Contains(OTHER_MIC_TAG));
				if (found != default(FusionDeviceData))
				{
					this.Notify(this.MicMuteChangeRequested, found.Id);
				}
			}
		}
	}
}
