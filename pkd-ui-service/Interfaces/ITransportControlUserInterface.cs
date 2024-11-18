namespace pkd_ui_service.Interfaces
{
	using pkd_application_service.Base;
	using pkd_common_utils.GenericEventArgs;
	using pkd_ui_service.Utility;
	using System;
	using System.Collections.ObjectModel;

	public interface ITransportControlUserInterface
	{
		event EventHandler<GenericDualEventArgs<string, TransportTypes>> TransportControlRequest;
		event EventHandler<GenericDualEventArgs<string, string>> TransportDialRequest;
		event EventHandler<GenericDualEventArgs<string, string>> TransportDialFavoriteRequest;

		void SetCableBoxData(ReadOnlyCollection<TransportInfoContainer> data);
	}
}
