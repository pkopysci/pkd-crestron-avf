using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using pkd_application_service;
using pkd_common_utils.Logging;
using pkd_config_service;
using pkd_hardware_service;
using pkd_ui_service;

namespace pkd_crestron_avf
{
    public class ControlSystem : CrestronControlSystem
    {
        private ConfigurationService? _configService;
        private readonly CrestronTextWriter _consoleWriter = new();

        private IInfrastructureService? _infrastructureService;
        private IApplicationService? _applicationService;
        private IPresentationService? _presentationService;

        public ControlSystem()
        {
            try
            {
                Crestron.SimplSharpPro.CrestronThread.Thread.MaxNumberOfUserThreads = 100;
                CrestronEnvironment.ProgramStatusEventHandler += ControllerProgramEventHandler;
                Console.SetOut(_consoleWriter);
                
                CrestronConsole.AddNewConsoleCommand(
                    SetDebugMode,
                    "setdebug",
                    "setdebug [on | off]",
                    ConsoleAccessLevelEnum.AccessOperator);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        public override void InitializeSystem()
        {
            try
            {
                _ = Task.Run(async () =>
                {
                    // wait for Crestron firmware to finish startup
                    await (Task.Delay(1000)).ConfigureAwait(false);

                    Logger.SetProgramId($"PGM {ProgramNumber}");
                    Logger.Info("GCU Main - Initializing System...");
                    Startup();
                });
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in InitializeSystem: {0}", e.Message);
            }
        }

        private void SetDebugMode(string args)
        {
            if (args.ToUpper().Equals("ON", StringComparison.InvariantCulture))
            {
                Logger.SetDebugOn();
            }
            else
            {
                Logger.SetDebugOff();
            }
        }

        private void Startup()
        {
            Logger.Info("GCU Main - Startup()");
            _configService = new ConfigurationService(ProgramNumber);
            _configService.ConfigLoadComplete += ConfigLoadCompleteHandler;
            _configService.ConfigLoadFailed += ConfigLoadFailedHandler;
            try
            {
                _configService.LoadConfig();
            }
            catch (Exception e)
            {
                Logger.Error("GCU Main - Boot up failed:\n{0}", e.Message);
            }
        }

        private void ConfigLoadCompleteHandler(object? sender, EventArgs args)
        {
            var domainService = _configService?.Domain;
            if (domainService == null)
            {
                Logger.Error("ControlSystem.ConfigLoadCompleteHandler: failed to get domain service from config.");
                return;
            }

            _infrastructureService = InfrastructureServiceFactory.CreateInfrastructureService(domainService, this);
            _applicationService = ApplicationControlFactory.CreateAppService(_infrastructureService, domainService);
            if (_applicationService == null)
            {
                Logger.Error("ControlSystem.ConfigLoadCompleteHandler: failed to create application service.");
                return;
            }

            _applicationService.SetAutoShutdownTime(23, 0);
            _presentationService = PresentationServiceFactory.CreatePresentationService(_applicationService, this);
            _presentationService.Initialize();
            _infrastructureService.ConnectAllDevices();

            Logger.Info("startup Complete.");
        }

        private void ConfigLoadFailedHandler(object? sender, EventArgs args)
        {
            Logger.Error("startup Failed.");
        }

        private void ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            if (programStatusEventType != eProgramStatusEventType.Stopping) return;
            (_presentationService as IDisposable)?.Dispose();
            (_applicationService as IDisposable)?.Dispose();
            _configService?.Dispose();
        }
    }
}