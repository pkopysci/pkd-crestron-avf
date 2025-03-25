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
		private Thread? startupThread;
		private ConfigurationService? configService;

		private IInfrastructureService? infrastructureService;
		private IApplicationService? applicationService;
		private IPresentationService? presentationService;
		
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
				startupThread = new Thread(Startup, new object());
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
			configService = new ConfigurationService(ProgramNumber);
			configService.ConfigLoadComplete += ConfigLoadCompleteHandler;
			configService.ConfigLoadFailed += ConfigLoadFailedHandler;
			try
			{
				configService.LoadConfig();
			}
			catch (Exception e)
			{
				Logger.Error("GCU Main - Boot up failed:\n{0}", e.Message);
			}

			return userObj;
		}

		private void ConfigLoadCompleteHandler(object? sender, EventArgs args)
		{
			var domainService = configService?.Domain;
			if (domainService == null)
			{
				Logger.Error("ControlSystem.ConfigLoadCompleteHandler: failed to get domain service from config.");
				return;
			}
			
			infrastructureService = InfrastructureServiceFactory.CreateInfrastructureService(domainService, this);
			applicationService = ApplicationControlFactory.CreateAppService(infrastructureService, domainService);
			if (applicationService == null)
			{
				Logger.Error("ControlSystem.ConfigLoadCompleteHandler: failed to create application service.");
				return;
			}
			
			applicationService.SetAutoShutdownTime(23, 0);
			presentationService = PresentationServiceFactory.CreatePresentationService(applicationService, this);
			presentationService.Initialize();
			infrastructureService.ConnectAllDevices();

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
					(presentationService as IDisposable)?.Dispose();
					(applicationService as IDisposable)?.Dispose();
					configService?.Dispose();
					break;
			}
		}
	}
}