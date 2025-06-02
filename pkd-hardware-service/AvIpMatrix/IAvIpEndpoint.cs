namespace pkd_hardware_service.AvIpMatrix
{
	/// <summary>
	/// required methods and properties for implementing a transmitter or receiver AV-IP endpoint.
	/// </summary>
	public interface IAvIpEndpoint
	{
		/// <summary>
		/// Gets a value indicating whether the Av over IP endpoint is online or offline (true or false).
		/// </summary>
		bool IsOnline { get; }

		/// <summary>
		/// Gets what type of endpoint the device is (encoder or decoder).
		/// </summary>
		AvIpEndpointTypes EndpointType { get; }
	}
}
