namespace pkd_domain_service
{
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
	using System.Collections.Generic;
	using System.Collections.ObjectModel;


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
			ParameterValidator.ThrowIfNull(configuration, "DomainService.Ctor()", "configuration");
			this.config = configuration;
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<Display> Displays
		{
			get
			{
				if (this.config == null || this.config.Displays == null)
				{
					return new List<Display>().AsReadOnly();
				}

				return this.config.Displays.AsReadOnly();
			}
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<Dsp> Dsps
		{
			get
			{
				if (this.config == null || this.config.Audio.Dsps == null)
				{
					return new List<Dsp>().AsReadOnly();
				}

				return this.config.Audio.Dsps.AsReadOnly();
			}
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<Channel> AudioChannels
		{
			get
			{
				if (this.config == null || this.config.Audio.Channels == null)
				{
					return new List<Channel>().AsReadOnly();
				}

				return this.config.Audio.Channels.AsReadOnly();
			}
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<Camera> Cameras
		{
			get
			{
				if (this.config == null || this.config.Cameras == null)
				{
					return new List<Camera>().AsReadOnly();
				}

				return this.config.Cameras.AsReadOnly();
			}
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<LightingInfo> Lighting
		{
			get
			{
				if (this.config == null || this.config.LightingControllers == null)
				{
					return new List<LightingInfo>().AsReadOnly();
				}

				return this.config.LightingControllers.AsReadOnly();
			}
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<UserInterface> UserInterfaces
		{
			get
			{
				if (this.config == null || this.config.UserInterfaces == null)
				{
					return new List<UserInterface>().AsReadOnly();
				}

				return this.config.UserInterfaces.AsReadOnly();
			}
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<Endpoint> Endpoints
		{
			get
			{
				if (this.config == null || this.config.Endpoints == null)
				{
					return new List<Endpoint>().AsReadOnly();
				}

				return this.config.Endpoints.AsReadOnly();
			}
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<Bluray> Blurays
		{
			get
			{
				if (this.config == null || this.config.Blurays == null)
				{
					return new List<Bluray>().AsReadOnly();
				}

				return this.config.Blurays.AsReadOnly();
			}
		}

		/// <inheritdoc/>
		public ReadOnlyCollection<CableBox> CableBoxes
		{
			get
			{
				if (this.config == null || this.config.CableBoxes == null)
				{
					return new List<CableBox>().AsReadOnly();
				}

				return this.config.CableBoxes.AsReadOnly();
			}
		}

		/// <inheritdoc/>
		public FusionInfo Fusion
		{
			get
			{
				if (this.config == null || this.config.FusionInfo == null)
				{
					return new FusionInfo();
				}

				return this.config.FusionInfo;
			}
		}

		/// <inheritdoc/>
		public Routing RoutingInfo
		{
			get
			{
				if (this.config == null || this.config.Routing == null)
				{
					return new Routing();
				}

				return this.config.Routing;
			}
		}

		/// <inheritdoc/>
		public RoomInfo RoomInfo
		{
			get
			{
				if (this.config == null || this.config.RoomInfo == null)
				{
					return new RoomInfo();
				}

				return this.config.RoomInfo;
			}
		}

		/// <inheritdoc/>
		public ServerInfo ServerInfo
		{
			get
			{
				if (this.config == null || this.config.ServerInfo == null)
				{
					return new ServerInfo();
				}

				return this.config.ServerInfo;
			}
		}

		/// <inheritdoc/>
		public Display GetDisplay(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetDisplay()", "id");
			if (this.config == null)
			{
				Logger.Error(string.Format("DomainService.GetDisplay() - No configuration data present when requesting ID {0}.", id));
				return new Display();
			}

			Display found = this.config.Displays.Find(x => x.Id == id);
			if (found == null)
			{
				Logger.Warn(string.Format("DomainService.GetDisplay() - Unable to find display with ID {0}.", id));
				found = new Display();
			}

			return found;
		}

		/// <inheritdoc/>
		public Dsp GetDsp(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetDsp()", "id");
			if (this.config == null)
			{
				Logger.Error(string.Format("DomainService.GetDsp() - No configuration data present when requesting DSP {0}", id));
				return new Dsp();
			}

			Dsp found = this.config.Audio.Dsps.Find(x => x.Id.Equals(id));
			if (found == null)
			{
				Logger.Warn(string.Format("DomainService.GetDsp() - Unable to find DSP with ID {0}", id));
				found = new Dsp();
			}

			return found;
		}

		/// <inheritdoc/>
		public Camera GetCamera(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetCamera()", "id");

			if (this.config == null)
			{
				Logger.Warn(string.Format("DomainService.GetCamera()() - Unable to find camera with ID {0}", id));
				return new Camera();
			}

			Camera found = this.config.Cameras.Find(x => x.Id.Equals(id));
			if (found == null)
			{
				Logger.Warn(string.Format("DomainService.GetCamera() - Unable to find DSP with ID {0}", id));
				found = new Camera();
			}

			return found;
		}

		/// <inheritdoc/>
		public LightingInfo GetLightingInfo(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetLightingInfo()", "id");
			if (this.config == null)
			{
				Logger.Warn(string.Format("DomainService.GetLightingInfo() - Unable to find lighting data with ID {0}", id));
				return new LightingInfo();
			}

			LightingInfo found = this.config.LightingControllers.Find(x => x.Id.Equals(id));
			if (found == null)
			{
				Logger.Warn(string.Format("DomainService.GetLightingInfo() - Unable to find Lighting info with ID {0}", id));
				found = new LightingInfo();
			}

			return found;
		}

		/// <inheritdoc/>
		public UserInterface GetUserInterface(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetUserInterface()", "id");
			if (this.config == null)
			{
				Logger.Warn(string.Format("DomainService.GetUserInterface() - Unable to find user interface data with ID {0}", id));
				return new UserInterface();
			}

			UserInterface found = this.config.UserInterfaces.Find(x => x.Id.Equals(id));
			if (found == null)
			{
				Logger.Warn(string.Format("DomainService.GetUserInterface() - Unable to find user interface with ID {0}", id));
				found = new UserInterface();
			}

			return found;
		}

		/// <inheritdoc/>
		public Endpoint GetEndpoint(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetEndpoint()", "id");

			if (this.config == null)
			{
				Logger.Warn(string.Format("DomainService.GetEndpoint() - Unable to find endpoint with ID {0}", id));
				return new Endpoint();
			}

			Endpoint found = this.config.Endpoints.Find(x => x.Id.Equals(id));
			if (found == null)
			{
				Logger.Warn(string.Format("DomainService.GetEndpoint() - Unable to find Endpoint with ID {0}", id));
				found = new Endpoint();
			}

			return found;
		}

		/// <inheritdoc/>
		public Bluray GetBluray(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetBluray()", "id");
			if (this.config == null)
			{
				Logger.Warn(string.Format("DomainService.GetBluray() - Unable to find Blu-ray with ID {0}", id));
				return new Bluray();
			}

			Bluray found = this.config.Blurays.Find(x => x.Id.Equals(id));
			if (found == null)
			{
				Logger.Warn(string.Format("DomainService.GetBluray() - Unable to find Bluray info with ID {0}", id));
				found = new Bluray();
			}

			return found;
		}

		/// <inheritdoc/>
		public CableBox GetCableBox(string id)
		{
			ParameterValidator.ThrowIfNullOrEmpty(id, "GetCableBox()", "id");
			if (this.config == null)
			{
				Logger.Warn(string.Format("DomainService.GetCableBox() - Unable to find cable box info with ID {0}", id));
				return new CableBox();
			}

			CableBox found = this.config.CableBoxes.Find(x => x.Id == id);
			if (found == null)
			{
				Logger.Warn(string.Format("DomainService.GetCableBox() - Unable to find Bluray info with ID {0}", id));
				found = new CableBox();
			}

			return found;
		}
	}
}
