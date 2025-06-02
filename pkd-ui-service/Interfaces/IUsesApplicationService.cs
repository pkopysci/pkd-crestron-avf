using pkd_application_service;

namespace pkd_ui_service.Interfaces;

/// <summary>
/// Interface for any UI implementation that requires a direct connection with the application service,
/// such as a REST api server.
/// </summary>
public interface IUsesApplicationService
{
    /// <summary>
    /// Will set internal references for sending state commands and queries to the system application service.
    /// </summary>
    /// <param name="applicationService">The running application service for the system.</param>
    /// <exception cref="ArgumentNullException">If 'applicationService' is null.</exception>
    void SetApplicationService(IApplicationService applicationService);
}