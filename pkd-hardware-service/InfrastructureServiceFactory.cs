using Crestron.SimplSharpPro;
using pkd_common_utils.Validation;
using pkd_domain_service;

namespace pkd_hardware_service
{
	/// <summary>
	/// Helper class for creating the IInfrastructureService object that will control the actual hardware in the system.
	/// </summary>
	public static class InfrastructureServiceFactory
	{
		/// <summary>
		/// Create the Infrastructure service object that contains all device connections that were defined in the domain.
		/// This method will not establish connections to the devices.
		/// </summary>
		/// <param name="domain">The data class with all device information that was included in the configuration.</param>
		/// <param name="control">The host processor that this program is running on.</param>
		/// <returns>The hardware control service that will be used to send commands to physical devices.</returns>
		/// <exception cref="ArgumentNullException">If domain or control are null.</exception>
		public static IInfrastructureService CreateInfrastructureService(IDomainService domain, CrestronControlSystem control)
		{
			ParameterValidator.ThrowIfNull(domain, "Ctor", nameof(domain));
			ParameterValidator.ThrowIfNull(control, "Ctor", nameof(control));

			var hwService = new InfrastructureService(control);
			foreach (var display in domain.Displays)
			{
				hwService.AddDisplay(display);
			}

			foreach (var avs in domain.RoutingInfo.MatrixData)
			{
				hwService.AddAvSwitch(avs, domain.RoutingInfo);
			}

			foreach (var endpoint in domain.Endpoints)
			{
				hwService.AddEndpoint(endpoint);
			}

			foreach (var dsp in domain.Dsps)
			{
				hwService.AddDsp(dsp);
			}

			foreach (var channel in domain.AudioChannels)
			{
				hwService.AddAudioChannel(channel);
			}

			foreach (var cableBox in domain.CableBoxes)
			{
				hwService.AddCableBox(cableBox);
			}

			foreach (var lighting in domain.Lighting)
			{
				hwService.AddLightingDevice(lighting);
			}

			foreach (var videoWall in domain.VideoWalls)
			{
				hwService.AddVideoWall(videoWall);
			}

			foreach (var camera in domain.Cameras)
			{
				hwService.AddCamera(camera);
			}

			return hwService;
		}
	}
}
