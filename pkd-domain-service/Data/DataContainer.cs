namespace pkd_domain_service.Data
{
	using pkd_domain_service.Data.CameraData;
	using pkd_domain_service.Data.DisplayData;
	using pkd_domain_service.Data.DspData;
	using pkd_domain_service.Data.FusionData;
	using pkd_domain_service.Data.LightingData;
	using pkd_domain_service.Data.RoomInfoData;
	using pkd_domain_service.Data.RoutingData;
	using pkd_domain_service.Data.TransportDeviceData;
	using pkd_domain_service.Data.UserInterfaceData;
	using pkd_domain_service.Data.EndpointData;
	using System.Collections.Generic;
	using System.Text;
	using Newtonsoft.Json;

	/// <summary>
	/// Object representation of the JSON configuration file.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class DataContainer
	{
		[JsonProperty("ServerInfo")]
		public ServerInfo ServerInfo { get; set; }

		[JsonProperty("RoomInfo")]
		public RoomInfo RoomInfo { get; set; }

		[JsonProperty("UserInterfaces")]
		public List<UserInterface> UserInterfaces { get; set; }

		[JsonProperty("Displays")]
		public List<Display> Displays { get; set; }

		[JsonProperty("Routing")]
		public Routing Routing { get; set; }

		[JsonProperty("Audio")]
		public Audio Audio { get; set; }

		[JsonProperty("Cameras")]
		public List<Camera> Cameras { get; set; }

		[JsonProperty("Blurays")]
		public List<Bluray> Blurays { get; set; }

		[JsonProperty("CableBoxes")]
		public List<CableBox> CableBoxes { get; set; }

		[JsonProperty("LightingControllers")]
		public List<LightingInfo> LightingControllers { get; set; }

		[JsonProperty("Endpoints")]
		public List<Endpoint> Endpoints { get; set; }

		[JsonProperty("FusionInfo")]
		public FusionInfo FusionInfo { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("\n\r** CONFIG DATA OBJECT START ***\n\r");
			bldr.AppendLine(ServerInfo.ToString());
			bldr.AppendLine(RoomInfo.ToString());
			bldr.AppendLine("USER INTERFACES:");
			if (UserInterfaces != null)
			{
				foreach (var ui in UserInterfaces)
				{
					bldr.AppendLine(ui.ToString());
				}
			}

			bldr.AppendLine("DISPLAYS:");
			if (Displays != null)
			{
				foreach (var display in Displays)
				{
					bldr.AppendLine(display.ToString());
				}
			}

			bldr.AppendLine("ROUTING:");
			if (Routing != null)
			{
				bldr.AppendLine(Routing.ToString());
			}

			bldr.AppendLine("AUDIO:");
			if (Audio != null)
			{
				bldr.AppendLine(Audio.ToString());
			}

			bldr.AppendLine("CAMERAS:");
			if (Cameras != null)
			{
				foreach (var cam in Cameras)
				{
					bldr.AppendLine(cam.ToString());
				}
			}

			bldr.AppendLine("BLURAYS:");
			if (Blurays != null)
			{
				foreach (var br in Blurays)
				{
					bldr.AppendLine(br.ToString());
				}
			}

			bldr.AppendLine("CABLE BOXES:");
			if (CableBoxes != null)
			{
				foreach (var cb in CableBoxes)
				{
					bldr.AppendLine(cb.ToString());
				}
			}

			bldr.AppendLine("LIGHTING CONTROLLERS:");
			if (LightingControllers != null)
			{
				foreach (var lr in LightingControllers)
				{
					bldr.AppendLine(lr.ToString());
				}
			}

			bldr.AppendLine("ENDPOINTS:");
			if (Endpoints != null)
			{
				foreach (var ep in Endpoints)
				{
					bldr.AppendLine(ep.ToString());
				}
			}

			bldr.AppendLine("FUSION:");
			if (FusionInfo != null)
			{
				bldr.AppendLine(FusionInfo.ToString());
			}

			bldr.Append("\n\r** CONFIG DATA OBJECT END ***\n\r");

			return bldr.ToString();
		}
	}

}
