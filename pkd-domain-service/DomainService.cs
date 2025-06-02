using System.Collections.ObjectModel;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
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
	/// Implementation of the Domain hardware provider service.
	/// </summary>
	public sealed class DomainService : IDomainService
	{
		private readonly DataContainer config;

		/// <summary>
		/// Initializes a new instance of the <see cref="DomainService"/> class.
		/// </summary>
		public DomainService()
			: this(new DataContainer())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DomainService"/> class.
		/// </summary>
		/// <param name="configuration">The configuration object representing the system setup.</param>
		public DomainService(DataContainer configuration)
		{
			ParameterValidator.ThrowIfNull(configuration, "DomainService.Ctor()", nameof(configuration));
			config = configuration;
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<Display> Displays => config?.Displays == null ? new List<Display>().AsReadOnly() : config.Displays.AsReadOnly();

		/// <inheritdoc/>
		public ReadOnlyCollection<Dsp> Dsps => config?.Audio.Dsps == null ? new List<Dsp>().AsReadOnly() : config.Audio.Dsps.AsReadOnly();

		/// <inheritdoc/>
		public ReadOnlyCollection<Channel> AudioChannels => config?.Audio.Channels == null ? new List<Channel>().AsReadOnly() : config.Audio.Channels.AsReadOnly();

		/// <inheritdoc/>
		public ReadOnlyCollection<Camera> Cameras => config?.Cameras == null ? new List<Camera>().AsReadOnly() : config.Cameras.AsReadOnly();

		/// <inheritdoc/>
		public ReadOnlyCollection<LightingInfo> Lighting => config?.LightingControllers == null ? new List<LightingInfo>().AsReadOnly() : config.LightingControllers.AsReadOnly();

		/// <inheritdoc/>
		public ReadOnlyCollection<UserInterface> UserInterfaces => config?.UserInterfaces == null ? new List<UserInterface>().AsReadOnly() : config.UserInterfaces.AsReadOnly();

		/// <inheritdoc/>
		public ReadOnlyCollection<Endpoint> Endpoints => config?.Endpoints == null ? new List<Endpoint>().AsReadOnly() : config.Endpoints.AsReadOnly();

		/// <inheritdoc/>
		public ReadOnlyCollection<Bluray> Blurays => config?.Blurays == null ? new List<Bluray>().AsReadOnly() : config.Blurays.AsReadOnly();

		/// <inheritdoc/>
		public ReadOnlyCollection<CableBox> CableBoxes => config?.CableBoxes == null ? new List<CableBox>().AsReadOnly() : config.CableBoxes.AsReadOnly();
		
		/// <inheritdoc/>
		public ReadOnlyCollection<VideoWall> VideoWalls => config?.VideoWalls == null ? new List<VideoWall>().AsReadOnly() : config.VideoWalls.AsReadOnly();

		/// <inheritdoc/>
		public FusionInfo Fusion => config?.FusionInfo == null ? new FusionInfo() : config.FusionInfo;

		/// <inheritdoc/>
		public Routing RoutingInfo => config?.Routing == null ? new Routing() : config.Routing;

		/// <inheritdoc/>
		public RoomInfo RoomInfo => config?.RoomInfo == null ? new RoomInfo() : config.RoomInfo;

		/// <inheritdoc/>
		public ServerInfo ServerInfo => config?.ServerInfo == null ? new ServerInfo() : config.ServerInfo;

		/// <inheritdoc/>
		public Display GetDisplay(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetDisplay()", nameof(id));
			var found = config.Displays.FirstOrDefault(x => x.Id == id);
			if (found == null)
			{
				Logger.Warn($"DomainService.{nameof(GetDisplay)}() - Display with id {id} not found");
			}

			return found ?? new Display();
		}

		/// <inheritdoc/>
		public Dsp GetDsp(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetDsp()", nameof(id));
			var found = config.Audio.Dsps.FirstOrDefault(x => x.Id == id);
			if (found == null)
			{
				Logger.Warn($"DomainService.{nameof(GetDsp)}() - Dsp with id {id} not found");
			}
			
			return found ?? new Dsp();
		}

		/// <inheritdoc/>
		public Camera GetCamera(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetCamera()", nameof(id));
			var found = config.Cameras.FirstOrDefault(x => x.Id == id);
			if (found == null)
			{
				Logger.Warn($"DomainService.{nameof(GetCamera)}() - Camera with id {id} not found");
			}
			
			return found ?? new Camera();
		}

		/// <inheritdoc/>
		public LightingInfo GetLightingInfo(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetLightingInfo()", nameof(id));
			var found = config.LightingControllers.FirstOrDefault(x => x.Id == id);
			if (found == null)
			{
				Logger.Warn($"DomainService.{nameof(GetLightingInfo)}() - Lighting with id {id} not found)");
			}
			
			return found ?? new LightingInfo();
		}

		/// <inheritdoc/>
		public UserInterface GetUserInterface(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetUserInterface()", nameof(id));
			var found = config.UserInterfaces.FirstOrDefault(x => x.Id == id);
			if (found == null)
			{
				Logger.Warn($"DomainService.{nameof(GetUserInterface)}() - User interface {id} not found");
			}
			
			return found ?? new UserInterface();
		}

		/// <inheritdoc/>
		public Endpoint GetEndpoint(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetEndpoint()", nameof(id));
			var found = config.Endpoints.FirstOrDefault(x => x.Id == id);
			if (found == null)
			{
				Logger.Warn($"DomainService.{nameof(GetEndpoint)}() - Endpoint with id {id} not found");
			}
			
			return found ?? new Endpoint();
		}

		/// <inheritdoc/>
		public Bluray GetBluray(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetBluray()", nameof(id));
			var found = config.Blurays.FirstOrDefault(x => x.Id == id);
			if (found == null)
			{
				Logger.Warn($"DomainService.{nameof(GetBluray)}() - Bluray with id {id} not found");
			}
			
			return found ?? new Bluray();
		}

		/// <inheritdoc/>
		public CableBox GetCableBox(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetCableBox()", nameof(id));
			var found = config.CableBoxes.FirstOrDefault(x => x.Id == id);
			if (found == null)
			{
				Logger.Warn($"DomainService.{nameof(GetCableBox)}() - CableBox with id {id} not found)");
			}
			
			return found ?? new CableBox();
		}
		
		/// <inheritdoc/>
		public VideoWall GetVideoWall(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetVideoWall()", nameof(id));
			var found = config.VideoWalls.FirstOrDefault(x => x.Id == id);
			if (found == null)
			{
				Logger.Warn($"DomainService.{nameof(GetVideoWall)}() - VideoWall with id {id} not found.");
			}
			
			return found ?? new VideoWall();
		}
	}
}
