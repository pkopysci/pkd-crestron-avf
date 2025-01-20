namespace pkd_crestron_avf
{
	using Crestron.SimplSharp;
	using Crestron.SimplSharpPro;
	using Crestron.SimplSharpPro.CrestronThread;
	using System;

	public class ControlSystem : CrestronControlSystem
	{
		private Thread? startupThread;
		
		public ControlSystem()
			: base()
		{
			try
			{
				Thread.MaxNumberOfUserThreads = 20;
				CrestronEnvironment.ProgramStatusEventHandler += new ProgramStatusEventHandler(ControllerProgramEventHandler);

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
				// TODO: Add logger to entry point.
			}
			else
			{
				// TODO: disable logger
			}
		}

		private object Startup(object userObj)
		{

			try
			{
				// TODO: Startup system.
			}
			catch (Exception e)
			{
				// TODO: Catch startup errors
			}

			return userObj;
		}

		private void ConfigLoadCompleteHandler(object sender, EventArgs args)
		{
			// TODO: ControlSystem.ConfigLoadCompleteHandler()
		}

		private void ConfigLoadFailedHandler(object sender, EventArgs args)
		{
			// TODO: Log failure to logging system.
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
					// TODO: Dispose of all services
					break;
			}

		}
	}
}