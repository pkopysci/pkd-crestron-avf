using Newtonsoft.Json;
using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using pkd_domain_service.Data;

namespace pkd_domain_service
{
	/// <summary>
	/// Help class for building an IDomain hardware provider service.
	/// </summary>
	public static class DomainFactory
	{

		/// <summary>
		/// Attempt to create an IDomain object from the given configuration data.
		/// </summary>
		/// <param name="data">The serialized JSON data to parse.</param>
		/// <returns>A new IDomainService based on the provided JSON data.</returns>
		public static IDomainService CreateDomainFromJson(string data)
		{
			try
			{
				ParameterValidator.ThrowIfNullOrEmpty(data, "DomainFactory.CreateDomainFromJson", nameof(data));

				Logger.Info("Deserializing JSON configuration...");
				var service = JsonConvert.DeserializeObject<DataContainer>(data) ?? new DataContainer();

				Logger.Info("Creating Domain service from configuration...");
				return new DomainService(service);
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "DomainFactory.CreateDomainFromJson() - failed to create data objects.");
				return new DomainService();
			}
		}
	}
}
