namespace pkd_ui_service.Fusion
{
	internal class FusionDeviceData
	{
		public string Id { get; init; } = string.Empty;
		public string Label { get; init; } = string.Empty;
		public string TypeTag { get; set; } = string.Empty;
		public DateTime StartTime { get; set; } = DateTime.MinValue;
		public string[] Tags { get; init; } = [];
	}
}
