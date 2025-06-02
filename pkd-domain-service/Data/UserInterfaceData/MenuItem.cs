namespace pkd_domain_service.Data.UserInterfaceData
{
	/// <summary>
	/// Configuration data for a single UI menu control.
	/// </summary>
	public class MenuItem : BaseData
	{
		/// <summary>
		/// Gets or sets a value indicating whether this menu item should be visible on the UI or not.
		/// </summary>
		public bool Visible { get; set; }

		/// <summary>
		/// Gets or sets the label to display on the UI for the control.
		/// </summary>
		public string Label { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the icon to display on the UI for the control.
		/// </summary>
		public string Icon { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the control / activity that will be displayed when
		/// the menu item is selected.
		/// </summary>
		public string Control { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the ID of the source to route when the menu item is selected.
		/// Can be the empty string ("").
		/// </summary>
		public string SourceSelect { get; set; } = string.Empty;

		public List<string> Tags { get; set; } = new List<string>();
	}
}
