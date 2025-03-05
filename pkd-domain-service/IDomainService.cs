using System.Collections.ObjectModel;
using pkd_domain_service.Data;
using pkd_domain_service.Data.CameraData;
using pkd_domain_service.Data.DisplayData;
using pkd_domain_service.Data.DspData;
using pkd_domain_service.Data.EndpointData;
using pkd_domain_service.Data.FusionData;
using pkd_domain_service.Data.LightingData;
using pkd_domain_service.Data.RoomInfoData;
using pkd_domain_service.Data.RoutingData;
using pkd_domain_service.Data.TransportDeviceData;
using pkd_domain_service.Data.UserInterfaceData;
using pkd_domain_service.Data.VideoWallData;

namespace pkd_domain_service
{
	/// <summary>
	/// Common properties and methods for the Domain hardware provider service.
	/// </summary>
	public interface IDomainService
	{
		/// <summary>
		/// Gets a collection all display devices defined in the configuration.
		/// </summary>
		ReadOnlyCollection<Display> Displays { get; }

		/// <summary>
		/// Gets a collection of all DSP devices defined in the configuration.
		/// </summary>
		ReadOnlyCollection<Dsp> Dsps { get; }

		/// <summary>
		/// Gets a collection of all camera devices defined in the configuration.
		/// </summary>
		ReadOnlyCollection<Camera> Cameras { get; }

		/// <summary>
		/// Gets a collection of all Lighting data defined in the configuration.
		/// </summary>
		ReadOnlyCollection<LightingInfo> Lighting { get; }

		/// <summary>
		/// Gets a collection of all UI data models defined in the configuration.
		/// </summary>
		ReadOnlyCollection<UserInterface> UserInterfaces { get; }

		/// <summary>
		/// Gets a collection of all audio/video endpoints defined in the configuration.
		/// </summary>
		ReadOnlyCollection<Endpoint> Endpoints { get; }

		/// <summary>
		/// Gets a collection of all Blu-ray devices defined in the configuration.
		/// </summary>
		ReadOnlyCollection<Bluray> Blurays { get; }

		/// <summary>
		/// Gets a collection of all cable box devices defined in the configuration.
		/// </summary>
		ReadOnlyCollection<CableBox> CableBoxes { get; }

		/// <summary>
		/// Gets a collection of all audio channels defined in the configuration.
		/// </summary>
		ReadOnlyCollection<Channel> AudioChannels { get; }
		
		/// <summary>
		/// Gets a collection of all video wall controllers defined in the configuration.
		/// </summary>
		ReadOnlyCollection<VideoWall> VideoWalls { get; }
		
		/// <summary>
		/// Gets the Fusion configuration data defined in the config file.
		/// </summary>
		FusionInfo Fusion { get; }

		/// <summary>
		/// Gets the routing map defined in the configuration file.
		/// </summary>
		Routing RoutingInfo { get; }

		/// <summary>
		/// Gets the basic room information as defined in the configuration file.
		/// </summary>
		RoomInfo RoomInfo { get; }

		/// <summary>
		/// Get the remote dependency server information as defined in the configuration file.
		/// </summary>
		ServerInfo ServerInfo { get; }

		/// <summary>
		/// Search through all displays in the configuration for one with an ID that matches
		/// 'id'.
		/// If a display cannot be found a warning is written to the logging system.
		/// </summary>
		/// <param name="id">The ID of the display to search for.</param>
		/// <returns>The first instance that matches id, or an empty display object.</returns>
		Display GetDisplay(string id);

		/// <summary>
		/// Search through all DSPs in the configuration for one with an ID that matches
		/// 'id'.
		/// If a DSP cannot be found a warning is written to the logging system.
		/// </summary>
		/// <param name="id">The ID of the DSP to search for.</param>
		/// <returns>The first instance that matches id, or an empty DSP object.</returns>
		Dsp GetDsp(string id);

		/// <summary>
		/// Search through all cameras in the configuration for one with an ID that matches
		/// 'id'.
		/// If a camera cannot be found a warning is written to the logging system.
		/// </summary>
		/// <param name="id">The ID of the camera to search for.</param>
		/// <returns>The first instance that matches id, or an empty camera object.</returns>
		Camera GetCamera(string id);

		/// <summary>
		/// Search through all lights in the configuration for one with an ID that matches
		/// 'id'.
		/// If a light cannot be found a warning is written to the logging system.
		/// </summary>
		/// <param name="id">The ID of the light to search for.</param>
		/// <returns>The first instance that matches id, or an empty camera object.</returns>
		LightingInfo GetLightingInfo(string id);

		/// <summary>
		/// Search through all user interfaces in the configuration for one with an ID that matches
		/// 'id'.
		/// If a user interface cannot be found a warning is written to the logging system.
		/// </summary>
		/// <param name="id">The ID of the user interface to search for.</param>
		/// <returns>The first instance that matches id, or an empty user interface object.</returns>
		UserInterface GetUserInterface(string id);

		/// <summary>
		/// Search through all AV endpoints in the configuration for one with an ID that matches
		/// 'id'.
		/// If an AV endpoint cannot be found a warning is written to the logging system.
		/// </summary>
		/// <param name="id">The ID of the AV endpoint to search for.</param>
		/// <returns>The first instance that matches id, or an empty AV endpoint object.</returns>
		Endpoint GetEndpoint(string id);

		/// <summary>
		/// Search through all Blu-rays in the configuration for one with an ID that matches
		/// 'id'.
		/// If a Blu-ray cannot be found a warning is written to the logging system.
		/// </summary>
		/// <param name="id">The ID of the Blu-ray to search for.</param>
		/// <returns>The first instance that matches id, or an empty Blu-ray object.</returns>
		Bluray GetBluray(string id);

		/// <summary>
		/// Search through all cable boxes in the configuration for one with and ID
		/// that matches 'id'.
		/// </summary>
		/// <param name="id">The ID of the cable box to search for.</param>
		/// <returns>The first instance that matches 'id', or an empty cable box object.</returns>
		CableBox GetCableBox(string id);
		
		/// <summary>
		/// Search through all video walls in the configuration for one with a matching id.
		/// </summary>
		/// <param name="id">the id of the video wall to search for.</param>
		/// <returns>The first instance that matches 'id' or an empty video wall object.</returns>
		VideoWall GetVideoWall(string id);
	}

}
