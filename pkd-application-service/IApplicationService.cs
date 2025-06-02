using System.Collections.ObjectModel;
using pkd_application_service.AudioControl;
using pkd_application_service.AvRouting;
using pkd_application_service.Base;
using pkd_application_service.DisplayControl;
using pkd_application_service.EndpointControl;
using pkd_application_service.LightingControl;
using pkd_application_service.SystemPower;
using pkd_application_service.TransportControl;
using pkd_application_service.UserInterface;
using pkd_domain_service;
using pkd_hardware_service;

namespace pkd_application_service
{
	/// <summary>
	/// Application service common properties and methods.
	/// </summary>
	public interface IApplicationService :
		ISystemPowerApp,
		IDisplayControlApp,
		IEndpointControlApp,
		IAudioControlApp,
		IAvRoutingApp,
		ITransportControlApp,
		ILightingControlApp
	{
		/// <summary>
		/// Triggered when the global freeze has been changed.
		/// </summary>
		event EventHandler GlobalVideoFreezeChanged;

		/// <summary>
		/// Triggered when the global video blank status has changed.
		/// </summary>
		event EventHandler GlobalVideoBlankChanged;

		/// <summary>
		/// Query the service for information on all user interfaces that are including in the
		/// system configuration.
		/// </summary>
		/// <returns>A data collection representing all user interfaces that will connect with this system.</returns>
		ReadOnlyCollection<UserInterfaceDataContainer> GetAllUserInterfaces();

		/// <summary>
		/// Query the service for the Fusion connection information. Data container properties that contain
		/// information are:
		/// Id = GUID used for Fusion discovery
		/// Label = the room name to display in Fusion
		/// IpId = the IP-ID used to establish a connection with the Fusion server
		/// </summary>
		/// <returns>The fusion information as defined in the system configuration.</returns>
		UserInterfaceDataContainer GetFusionInterface();

		/// <summary>
		/// Query the application service for the room information that was set in the configuration file.
		/// </summary>
		/// <returns>A data object containing general room information.</returns>
		RoomInfoContainer GetRoomInfo();

		/// <summary>
		/// Blank the output on all video endpoints. This can either be set at the displays
		/// or on the AV routing hardware.
		/// </summary>
		/// <param name="state">true = set blank active (no video), false = set blank off (show video).</param>
		void SetGlobalVideoBlank(bool state);

		/// <summary>
		/// Freeze the video output on all display endpoints. This can bet set either at the displays or
		/// on the AV routing hardware.
		/// </summary>
		/// <param name="state">true = freeze on (no motion), false = freeze off (normal video).</param>
		void SetGlobalVideoFreeze(bool state);

		/// <summary>
		/// Get the current state of the global video blank
		/// </summary>
		/// <returns>true if video output is blanked, false if video output is showing.</returns>
		bool QueryGlobalVideoBlank();

		/// <summary>
		/// Get the current state of the global video freeze.
		/// </summary>
		/// <returns>True = global video is frozen, false = normal video output.</returns>
		bool QueryGlobalVideoFreeze();

		/// <summary>
		/// Creates internal hooks and instantiates application logic objects.
		/// </summary>
		/// <param name="hwService">The hardware control service used to send commands to devices.</param>
		/// <param name="domain">The configuration domain for this system.</param>
		void Initialize(IInfrastructureService hwService, IDomainService domain);
	}
}
