namespace pkd_domain_service.Data.UserInterfaceData
{
	using Newtonsoft.Json;
	using System.Collections.Generic;

	/// <summary>
	/// Configuration data for a single UI menu control.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class MenuItem : BaseData
	{
		/// <summary>
		/// Gets or sets a value indicating whether this menu item should be visible on the UI or not.
		/// </summary>
		[JsonProperty("Visible")]
		public bool Visible { get; set; }

		/// <summary>
		/// Gets or sets the label to display on the UI for the control.
		/// </summary>
		[JsonProperty("Label")]
		public string Label { get; set; }

		/// <summary>
		/// Gets or sets the icon to display on the UI for the control.
		/// </summary>
		[JsonProperty("Icon")]
		public string Icon { get; set; }

		/// <summary>
		/// Gets or sets the control / activity that will be displayed when
		/// the menu item is selected.
		/// </summary>
		[JsonProperty("Control")]
		public string Control { get; set; }

		/// <summary>
		/// Gets or sets the ID of the source to route when the menu item is selected.
		/// Can be the empty string ("").
		/// </summary>
		[JsonProperty("SourceSelect")]
		public string SourceSelect { get; set; }

		[JsonProperty("Tags")]
		public List<string> Tags { get; set; } = new List<string>();

		public override string ToString()
		{
			return string.Format(
				"MenuItem {0}: Label = {1}, Icon = {2}, Control = {3}, SourceSelect = {4}",
				Id ?? "NO ID",
				Label ?? "NULL",
				Icon ?? "NULL",
				Control ?? "NULL",
				SourceSelect ?? "NULL");
		}
	}
}
