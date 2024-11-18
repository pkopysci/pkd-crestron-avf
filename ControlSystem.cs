namespace pkd_crestron_avf
{
	using Crestron.SimplSharp;
	using Crestron.SimplSharpPro;
	using Crestron.SimplSharpPro.CrestronThread;
	using pkd_application_service;
	using pkd_common_utils.Logging;
	using pkd_config_service;
	using pkd_domain_service;
	using pkd_hardware_service;
	using pkd_ui_service;
	using System;

	public class ControlSystem : CrestronControlSystem
	{
		private ConfigurationService configService;
		private IDomainService domainService;
		private IInfrastructureService infrastructureService;
		private IApplicationService applicationService;
		private IPresentationService presentationService;
		private Thread startupThread;

		/// <summary>
		/// ControlSystem Constructor. Starting point for the SIMPL#Pro program.
		/// Use the constructor to:
		/// * Initialize the maximum number of threads (max = 400)
		/// * Register devices
		/// * Register event handlers
		/// * Add Console Commands
		/// 
		/// Please be aware that the constructor needs to exit quickly; if it doesn't
		/// exit in time, the SIMPL#Pro program will exit.
		/// 
		/// You cannot send / receive data in the constructor
		/// </summary>
		public ControlSystem()
			: base()
		{
			try
			{
				Thread.MaxNumberOfUserThreads = 20;

				//Subscribe to the controller events (System, Program, and Ethernet)
				CrestronEnvironment.SystemEventHandler += new SystemEventHandler(ControllerSystemEventHandler);
				CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControllerProgramEventHandler);
				CrestronEnvironment.EthernetEventHandler += new EthernetEventHandler(ControllerEthernetEventHandler);

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

		/// <summary>
		/// InitializeSystem - this method gets called after the constructor 
		/// has finished. 
		/// 
		/// Use InitializeSystem to:
		/// * Start threads
		/// * Configure ports, such as serial and verisports
		/// * Start and initialize socket connections
		/// Send initial device configurations
		/// 
		/// Please be aware that InitializeSystem needs to exit quickly also; 
		/// if it doesn't exit in time, the SIMPL#Pro program will exit.
		/// </summary>
		public override void InitializeSystem()
		{
			try
			{
				Logger.SetProgramId($"PGM {this.ProgramNumber}");
				Logger.Info("GCU Main - Initializing System...");
				this.startupThread = new Thread(Startup, new object());
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
			this.configService = new ConfigurationService(this.ProgramNumber, this);
			this.configService.ConfigLoadComplete += this.ConfigLoadCompleteHandler;
			this.configService.ConfigLoadFailed += this.ConfigLoadFailedHandler;

			try
			{
				this.configService.LoadConfig();
			}
			catch (Exception e)
			{
				Logger.Error("GCU Main - Bootup failed: {0}", e.Message);
			}

			return userObj;
		}

		private void ConfigLoadCompleteHandler(object sender, EventArgs args)
		{
			bool bootSuccess = false;
			this.domainService = this.configService.Domain;
			this.infrastructureService = InfrastructureServiceFactory.CreateInfrastructureService(this.domainService, this);

			this.applicationService = ApplicationControlFactory.CreateAppService(this.infrastructureService, this.domainService);
			this.applicationService.SetAutoShutdownTime(23, 0);

			if (this.applicationService != null)
			{
				this.presentationService = PresentationServiceFactory.CreatePresentationService(this.applicationService, this);
				this.presentationService.Initialize();
				bootSuccess = true;
			}

			this.infrastructureService.ConnectAllDevices();

			Logger.Info("GCU Main - startup {0}", (bootSuccess) ? "Complete." : "Failed.");
		}

		private void ConfigLoadFailedHandler(object sender, EventArgs args)
		{
			Logger.Error("GCU Main - startup Failed.");
		}

		/// <summary>
		/// Event Handler for Ethernet events: Link Up and Link Down. 
		/// Use these events to close / re-open sockets, etc. 
		/// </summary>
		/// <param name="ethernetEventArgs">This parameter holds the values 
		/// such as whether it's a Link Up or Link Down event. It will also indicate 
		/// wich Ethernet adapter this event belongs to.
		/// </param>
		protected void ControllerEthernetEventHandler(EthernetEventArgs ethernetEventArgs)
		{
			switch (ethernetEventArgs.EthernetEventType)
			{//Determine the event type Link Up or Link Down
				case (eEthernetEventType.LinkDown):
					//Next need to determine which adapter the event is for. 
					//LAN is the adapter is the port connected to external networks.
					if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
					{
						//
					}
					break;
				case (eEthernetEventType.LinkUp):
					if (ethernetEventArgs.EthernetAdapter == EthernetAdapterType.EthernetLANAdapter)
					{

					}
					break;
			}
		}

		/// <summary>
		/// Event Handler for Programmatic events: Stop, Pause, Resume.
		/// Use this event to clean up when a program is stopping, pausing, and resuming.
		/// This event only applies to this SIMPL#Pro program, it doesn't receive events
		/// for other programs stopping
		/// </summary>
		/// <param name="programStatusEventType"></param>
		protected void ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
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
					//The program has been stopped.
					//Close all threads. 
					//Shutdown all Client/Servers in the system.
					//General cleanup.
					//Unsubscribe to all System Monitor events
					this.configService?.Dispose();
					this.configService = null;
					// TODO: Dispose all other services
					this.startupThread?.Abort();

					break;
			}

		}

		/// <summary>
		/// Event Handler for system events, Disk Inserted/Ejected, and Reboot
		/// Use this event to clean up when someone types in reboot, or when your SD /USB
		/// removable media is ejected / re-inserted.
		/// </summary>
		/// <param name="systemEventType"></param>
		protected void ControllerSystemEventHandler(eSystemEventType systemEventType)
		{
			switch (systemEventType)
			{
				case (eSystemEventType.DiskInserted):
					//Removable media was detected on the system
					break;
				case (eSystemEventType.DiskRemoved):
					//Removable media was detached from the system
					break;
				case (eSystemEventType.Rebooting):
					//The system is rebooting. 
					//Very limited time to preform clean up and save any settings to disk.
					break;
			}

		}
	}
}