using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using pkd_application_service;
using pkd_common_utils.Logging;
using pkd_config_service;
using pkd_hardware_service;
using pkd_ui_service;
using Thread = Crestron.SimplSharpPro.CrestronThread.Thread;

namespace pkd_crestron_avf
{
	public class ControlSystem : CrestronControlSystem
	{
		private Thread? _startupThread;
		private ConfigurationService? _configService;

		private IInfrastructureService? _infrastructureService;
		private IApplicationService? _applicationService;
		private IPresentationService? _presentationService;
		
		public ControlSystem()
		{
			try
			{
				Thread.MaxNumberOfUserThreads = 20;
				CrestronEnvironment.ProgramStatusEventHandler += ControllerProgramEventHandler;

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
				Logger.SetProgramId($"PGM {ProgramNumber}");
				Logger.Info("GCU Main - Initializing System...");
				_startupThread = new Thread(Startup, new object());
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

		private object Startup(object userObj)
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

			return userObj;
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
			switch (programStatusEventType)
			{
				case (eProgramStatusEventType.Paused):
					//The program has been paused.  Pause all user threads/timers as needed.
					break;
				case (eProgramStatusEventType.Resumed):
					//The program has been resumed. Resume all the user threads/timers as needed.
					break;
				case (eProgramStatusEventType.Stopping):
					(_presentationService as IDisposable)?.Dispose();
					(_applicationService as IDisposable)?.Dispose();
					_configService?.Dispose();
					break;
			}
		}
	}
}