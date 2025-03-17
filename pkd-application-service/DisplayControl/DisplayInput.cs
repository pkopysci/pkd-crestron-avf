namespace pkd_application_service.DisplayControl
{
	/// <summary>
	/// data object representing a single input on a display or projector device.
	/// </summary>
	public class DisplayInput
	{
		/// <summary>
		/// The id of the input, used for internal referencing. Defaults to "DI-DEFAULT".
		/// </summary>
		public string Id { get; set; } = "DI-DEFAULT";

		/// <summary>
		/// Human-friendly name of the input. Defaults to "Display Input".
		/// </summary>
		public string Label { get; set; } = "Display Input";

		/// <summary>
		/// The input index on the display device for this input.
		/// </summary>
		public int InputNumber { get; set; }

		/// <summary>
		/// Collection of functional tags that are associated with this input. These tags can be used by the application service
		/// or user interface implementation.
		/// </summary>
		public List<string> Tags { get; set; } = [];
	}
}
