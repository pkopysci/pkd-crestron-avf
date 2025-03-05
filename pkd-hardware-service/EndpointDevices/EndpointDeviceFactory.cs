using Crestron.SimplSharpPro;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_domain_service.Data.EndpointData;

namespace pkd_hardware_service.EndpointDevices
{
	/// <summary>
	/// Factory class used to build a hardware control interface for all endpoint types (relay, ir, rs-232, ect.)
	/// </summary>
	internal static class EndpointDeviceFactory
	{

		/// <summary>
		/// Create a new Endpoint control interface from the given configuration data.
		/// </summary>
		/// <param name="ep">The Endpoint configuration data used to create the object.</param>
		/// <param name="processor">The host control system for interfacing with the endpoint.</param>
		/// <returns>A new Endpoint control object registered with the processor. Returns null on a failure.</returns>
		/// <exception cref="ArgumentNullException"> if any argument is null.</exception>
		public static IEndpointDevice? CreateEndpointDevice(Endpoint ep, CrestronControlSystem processor)
		{
			ParameterValidator.ThrowIfNull(ep, "CreateEndpointDevice", "ep");
			ParameterValidator.ThrowIfNull(processor, "CreateEndpointDevice", "processor");

			try
			{
				switch (ep.Class.ToUpper())
				{
					case "PROCESSOR":
						Logger.Info("Creating a processor-based endpoint.");
						return new ProcessorEndpoint(ep, processor);
					case "C2NIO":
						Logger.Info("Creating a C2N-IO endpoint with ID {0}", ep.Id);
						return new C2NIoRelayDevice(ep, processor);
					case "CENIORY104":
						Logger.Info("Creating a CEN-IO-RY104 endpoint with ID {0}", ep.Id);
						return new CenIoRy401RelayDevice(ep, processor);
					default:
						Logger.Error(
							"CreateEndpointDevices() - Endpoint {0} has unsupported class: {1}",
							ep.Id,
							ep.Class);

						return null;
				}
			}
			catch (Exception e)
			{
				Logger.Error(e, "Failed to create endpoint device with ID {0}", ep.Id);
			}

			return null;
		}
	}
}
