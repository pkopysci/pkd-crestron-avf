namespace pkd_domain_service.Data.UserInterfaceData
{
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Text;

	/// <summary>
	/// Configuration data for a single user interface.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class UserInterface : BaseData
	{
		public UserInterface()
		{
			this.Menu = new List<MenuItem>();
			this.Tags = new List<string>();
		}

		/// <summary>
		/// Gets or sets the IP-ID used to connect to the user interface. This is an integer representation
		/// of a hex value.
		/// </summary>
		[JsonProperty("IpId")]
		public int IpId { get; set; }

		/// <summary>
		/// Gets or sets the model of the touchsceen (I.E. tsw760, tsw770, etc.).
		/// </summary>
		[DefaultValue("tsw760")]
		[JsonProperty("Model", DefaultValueHandling = DefaultValueHandling.Populate)]
		public string Model { get; set; }

		/// <summary>
		/// The smart graphics data library needed if the UI is a VTPro-e based project.
		/// </summary>
		[JsonProperty("Sgd")]
		public string Sgd { get; set; }

		/// <summary>
		/// Gets or sets the default activity to present when the system enters the active state.
		/// </summary>
		[JsonProperty("DefaultActivity")]
		public string DefaultActivity { get; set; }

		/// <summary>
		/// Gets or sets the collection of main menu items to display on the UI.
		/// </summary>
		[JsonProperty("Menu")]
		public List<MenuItem> Menu { get; set; }

		/// <summary>
		/// Collection of tags that can define special behavior for the UI.
		/// </summary>
		[JsonProperty("Tags")]
		public List<string> Tags { get; set; }

		[DefaultValue("")]
		[JsonProperty("ClassName",DefaultValueHandling = DefaultValueHandling.Populate)]
		public string ClassName { get; set; }

		[DefaultValue("")]
		[JsonProperty("Library", DefaultValueHandling = DefaultValueHandling.Populate)]
		public string Library { get; set; }

		public override string ToString()
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("User Interface ").Append(Id?? "NO ID").Append(":\n\r");
			bldr.Append("IpId: ").Append(IpId);
			bldr.Append(", Model: ").Append(Model?? "NULL");
			bldr.Append(", Sgd: ").Append(Sgd?? "NULL");
			bldr.Append(", DefaultActivity: ").Append(DefaultActivity?? "NULL");
			bldr.Append("\n\rMenu:\n\r");
			foreach (var item in Menu)
			{
				bldr.Append(item).Append("\n\r");
			}

			bldr.Append("Tags: [");
			foreach (var tag in Tags)
			{
				bldr.Append(tag).Append(", ");
			}

			bldr.Append("]");
			return bldr.ToString();
		}
	}
}
