namespace pkd_ui_service.Fusion.ErrorManagement
{
	using Crestron.SimplSharpPro.Fusion;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Queue-based implmentation of the IFusionErrorManager interface.
	/// </summary>
	internal class FusionErrorManager : IFusionErrorManager
	{
		private Queue<FusionDeviceData> offlineQueue;
		private FusionDeviceData currentNotice;
		private readonly FusionRoom fusion;

		/// <summary>
		/// Instantiate a new instance of <see cref="FusionErrorManager"/>.
		/// </summary>
		/// <param name="fusion">The root Fusion communication object.</param>
		public FusionErrorManager(FusionRoom fusion)
		{
			ParameterValidator.ThrowIfNull(fusion, "FusionErrorManager.Ctor", "fusion");
			this.fusion = fusion;
			this.offlineQueue = new Queue<FusionDeviceData>();
		}

		///<inheritdoc/>
		public void AddOfflineDevice(string devId, string devName)
		{
			ParameterValidator.ThrowIfNullOrEmpty(devId, "FusionErrorManager.AddOfflineDevice", "devId");
			ParameterValidator.ThrowIfNullOrEmpty(devName, "FusionErrorManager.AddOfflineDevice", "devName");

			if (this.offlineQueue.Any(x => x.Id.Equals(devId, StringComparison.InvariantCulture)))
			{
				Logger.Debug("FusionErrorManager.AddOfflineDevice() - duplicate ID found: {0}", devId);
				return;
			}

			this.offlineQueue.Enqueue(new FusionDeviceData() { Id = devId, Label = devName });
			if (this.currentNotice == null && this.offlineQueue.Count == 1)
			{
				this.SendNextError();
			}
		}

		///<inheritdoc/>
		public void ClearOfflineDevice(string devId)
		{
			ParameterValidator.ThrowIfNullOrEmpty(devId, "FusionErrorManager.ClearOfflineDevice", "devId");
			Logger.Error("Device Error EID: {0} - cleared", devId);
			this.offlineQueue = new Queue<FusionDeviceData>(this.offlineQueue.Where(x => !x.Id.Equals(devId, StringComparison.InvariantCulture)));
			if (this.currentNotice != null && this.currentNotice.Id.Equals(devId, StringComparison.InvariantCulture))
			{
				this.SendNextError();
			}
		}

		private void SendNextError()
		{
			if (this.offlineQueue.Count > 0)
			{
				this.currentNotice = this.offlineQueue.Dequeue();
				string errorMessage = string.Format("3:{0} offline", this.currentNotice.Label);
				this.fusion.ErrorMessage.InputSig.StringValue = errorMessage;
				Logger.Error("Device Error EID: {0} - {1}", this.currentNotice.Id, errorMessage);
			}
			else
			{
				Logger.Debug("FusionErrorManager.SendNextError() - Clearing errors.");
				this.currentNotice = null;
				this.fusion.ErrorMessage.InputSig.StringValue = "0:";
			}
		}
	}
}
