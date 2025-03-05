using pkd_domain_service.Data.CameraData;
using pkd_domain_service.Data.DisplayData;
using pkd_domain_service.Data.DspData;
using pkd_domain_service.Data.EndpointData;
using pkd_domain_service.Data.LightingData;
using pkd_domain_service.Data.RoutingData;
using pkd_domain_service.Data.TransportDeviceData;
using pkd_domain_service.Data.VideoWallData;
using pkd_hardware_service.AudioDevices;
using pkd_hardware_service.AvSwitchDevices;
using pkd_hardware_service.BaseDevice;
using pkd_hardware_service.CameraDevices;
using pkd_hardware_service.DisplayDevices;
using pkd_hardware_service.EndpointDevices;
using pkd_hardware_service.LightingDevices;
using pkd_hardware_service.TransportDevices;
using pkd_hardware_service.VideoWallDevices;

namespace pkd_hardware_service
{
	/// <summary>
	/// Properties and methods for the Infrastructure service that provides hardware control.
	/// </summary>
	public interface IInfrastructureService : IDisposable
	{
		/// <summary>
		/// Gets a collection of DSP devices that are configured in the system.
		/// </summary>
		DeviceContainer<IAudioControl> Dsps { get; }

		/// <summary>
		/// Gets a collection of AV Switching devices that are configured in the system.
		/// </summary>
		DeviceContainer<IAvSwitcher> AvSwitchers { get; }

		/// <summary>
		/// Gets a collection of display devices that are configured in the system.
		/// </summary>
		DeviceContainer<IDisplayDevice> Displays { get; }

		/// <summary>
		/// Gets a collection of endpoint (RMC-100, CEN-IO, etc.) that are configured in the system.
		/// </summary>
		DeviceContainer<IEndpointDevice> Endpoints { get; }

		/// <summary>
		/// Gets a collection of IR Cable box devices that are in the system configuration.
		/// </summary>
		DeviceContainer<ITransportDevice> CableBoxes { get; }

		/// <summary>
		/// Gets a collection of lighting controllers that are in the system configuration.
		/// </summary>
		DeviceContainer<ILightingDevice> LightingDevices { get; }

		/// <summary>
		/// Gets a collection of video wall controllers that are in the system configuration.
		/// </summary>
		DeviceContainer<IVideoWallDevice> VideoWallDevices { get; }
		
		/// <summary>
		/// Gets a collection of controllable cameras in the system configuration.
		/// </summary>
		DeviceContainer<ICameraDevice> CameraDevices { get; }
		
		/// <summary>
		/// Add a DSP control object to the current collection. Any DSP in the collection with
		/// a matching ID will be replaced.
		/// </summary>
		/// <param name="dsp">The DSP device to add or replace.</param>
		void AddDsp(Dsp dsp);

		/// <summary>
		/// Add an audio input or output channel to a DSP in the current collection. This will look for a DSP
		/// with a matching ID and then configure that device with the channel.
		/// </summary>
		/// <param name="channel">The channel data object used to configure the DSP control.</param>
		void AddAudioChannel(Channel channel);

		/// <summary>
		/// Add a display control object to the current collection. Any display in the collection with
		/// a matching ID will be replaced.
		/// </summary>
		/// <param name="display">The display device to add or replace.</param>
		void AddDisplay(Display display);

		/// <summary>
		/// Add an AV switcher control object to the current collection. Any AV switcher in the collection with
		/// a matching ID will be replaced.
		/// </summary>
		/// <param name="avSwitch">The AV switcher to add or replace.</param>
		/// <param name="routingData">The config data containing all inputs and outputs in the system.</param>
		void AddAvSwitch(MatrixData avSwitch, Routing routingData);

		/// <summary>
		/// Add an endpoint control object to the current collection. Any endpoint in the collection with
		/// a matching ID shall be replaced.
		/// </summary>
		/// <param name="endpointData">the endpoint data to add or replace.</param>
		void AddEndpoint(Endpoint endpointData);

		/// <summary>
		/// Add a cable box or satellite TV transport control object to the current collection. Any connection in the collection
		/// with a matching ID shall be replaced.
		/// </summary>
		/// <param name="cableBox">The cable box or Sat TV transport control config to add.</param>
		void AddCableBox(CableBox cableBox);

		/// <summary>
		/// Add a lighting controller to the current collection. Any connection in the collection
		/// with a matching ID shall be replaced.
		/// </summary>
		/// <param name="lightingDevice">The lighting controller to add.</param>
		void AddLightingDevice(LightingInfo lightingDevice);
		
		/// <summary>
		/// Add a video wall controller to the current collection.
		/// </summary>
		/// <param name="videoWall">The config data for the video wall device to add.</param>
		void AddVideoWall(VideoWall videoWall);

		/// <summary>
		/// Add a camera controller to the current collection.
		/// </summary>
		/// <param name="cameraData">The config data for the camera device to add.</param>
		void AddCamera(Camera cameraData);

		/// <summary>
		/// Initialize all hardware connections and connect to the devices for control.
		/// </summary>
		void ConnectAllDevices();
	}
}
