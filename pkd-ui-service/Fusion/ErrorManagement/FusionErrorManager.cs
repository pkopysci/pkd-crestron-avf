using Crestron.SimplSharpPro.Fusion;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;

namespace pkd_ui_service.Fusion.ErrorManagement
{
	/// <summary>
	/// Queue-based implementation of the IFusionErrorManager interface.
	/// </summary>
	internal class FusionErrorManager : IFusionErrorManager
	{
		private Queue<FusionDeviceData> offlineQueue;
		private FusionDeviceData? currentNotice;
		private readonly FusionRoom fusion;

		/// <summary>
		/// Instantiate a new instance of <see cref="FusionErrorManager"/>.
		/// </summary>
		/// <param name="fusion">The root Fusion communication object.</param>
		public FusionErrorManager(FusionRoom fusion)
		{
			ParameterValidator.ThrowIfNull(fusion, "FusionErrorManager.Ctor", "fusion");
			this.fusion = fusion;
			offlineQueue = new Queue<FusionDeviceData>();
		}

		///<inheritdoc/>
		public void AddOfflineDevice(string devId, string devName)
		{
			ParameterValidator.ThrowIfNullOrEmpty(devId, "FusionErrorManager.AddOfflineDevice", "devId");
			ParameterValidator.ThrowIfNullOrEmpty(devName, "FusionErrorManager.AddOfflineDevice", "devName");

			if (offlineQueue.Any(x => x.Id.Equals(devId, StringComparison.InvariantCulture)))
			{
				Logger.Debug("FusionErrorManager.AddOfflineDevice() - duplicate ID found: {0}", devId);
				return;
			}

			offlineQueue.Enqueue(new FusionDeviceData() { Id = devId, Label = devName });
			if (currentNotice == null && offlineQueue.Count == 1)
			{
				SendNextError();
			}
		}

		///<inheritdoc/>
		public void ClearOfflineDevice(string devId)
		{
			ParameterValidator.ThrowIfNullOrEmpty(devId, "FusionErrorManager.ClearOfflineDevice", "devId");
			Logger.Debug("Device Error EID: {0} - cleared", devId);
			offlineQueue = new Queue<FusionDeviceData>(offlineQueue.Where(x => !x.Id.Equals(devId, StringComparison.InvariantCulture)));
			if (currentNotice != null && currentNotice.Id.Equals(devId, StringComparison.InvariantCulture))
			{
				SendNextError();
			}
		}

		private void SendNextError()
		{
			if (offlineQueue.Count > 0)
			{
				currentNotice = offlineQueue.Dequeue();
				var errorMessage = $"3:{currentNotice.Label} offline";
				fusion.ErrorMessage.InputSig.StringValue = errorMessage;
				Logger.Error("Device Error EID: {0} - {1}", currentNotice.Id, errorMessage);
			}
			else
			{
				Logger.Debug("FusionErrorManager.SendNextError() - Clearing errors.");
				currentNotice = null;
				fusion.ErrorMessage.InputSig.StringValue = "0:";
			}
		}
	}
}
