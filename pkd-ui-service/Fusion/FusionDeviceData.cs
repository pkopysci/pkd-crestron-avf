namespace pkd_ui_service.Fusion
{
	using System;

	internal class FusionDeviceData
	{
		public string Id { get; set; } = string.Empty;
		public string Label { get; set; } = string.Empty;
		public string TypeTag { get; set; } = string.Empty;
		public DateTime StartTime { get; set; } = DateTime.MinValue;
		public string[] Tags { get; set; } = [];
	}
}
