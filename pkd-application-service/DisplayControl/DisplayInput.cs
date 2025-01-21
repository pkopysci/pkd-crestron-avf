namespace pkd_application_service.DisplayControl
{
	using System.Collections.Generic;

	public class DisplayInput
	{
		public string Id { get; set; } = "DI-DEFAULT";

		public string Label { get; set; } = "Display Input";

		public int InputNumber { get; set; }

		public List<string> Tags { get; set; } = [];
	}
}
