namespace pkd_ui_service.Utility
{
	using pkd_application_service;
	using System;
	using System.Collections.Generic;

	internal static class TransportUtilities
	{
		private static readonly Dictionary<TransportTypes, Action<IApplicationService, string>> actions = new Dictionary<TransportTypes, Action<IApplicationService, string>>()
		{
			{ TransportTypes.PowerOn, new Action<IApplicationService, string>((app, devId) => { app.TransportPowerOn(devId); }) },
			{ TransportTypes.PowerOff, new Action<IApplicationService, string>((app, devId) => { app.TransportPowerOff(devId); }) },
			{ TransportTypes.PowerToggle, new Action<IApplicationService, string>((app, devId) => { app.TransportPowerToggle(devId); }) },
			{ TransportTypes.Dash, new Action<IApplicationService, string>((app, devId) => { app.TransportDash(devId); }) },
			{ TransportTypes.ChannelUp, new Action<IApplicationService, string>((app, devId) => { app.TransportChannelUp(devId); }) },
			{ TransportTypes.ChannelDown, new Action<IApplicationService, string>((app, devId) => { app.TransportChannelDown(devId); }) },
			{ TransportTypes.PageUp, new Action<IApplicationService, string>((app, devId) => { app.TransportPageUp(devId); }) },
			{ TransportTypes.PageDown, new Action<IApplicationService, string>((app, devId) => { app.TransportPageDown(devId); }) },
			{ TransportTypes.Guide, new Action<IApplicationService, string>((app, devId) => { app.TransportGuide(devId); }) },
			{ TransportTypes.Menu, new Action<IApplicationService, string>((app, devId) => { app.TransportMenu(devId); }) },
			{ TransportTypes.Info, new Action<IApplicationService, string>((app, devId) => { app.TransportInfo(devId); }) },
			{ TransportTypes.Exit, new Action<IApplicationService, string>((app, devId) => { app.TransportExit(devId); }) },
			{ TransportTypes.Back, new Action<IApplicationService, string>((app, devId) => { app.TransportBack(devId); }) },
			{ TransportTypes.Play, new Action<IApplicationService, string>((app, devId) => { app.TransportPlay(devId); }) },
			{ TransportTypes.Pause, new Action<IApplicationService, string>((app, devId) => { app.TransportPause(devId); }) },
			{ TransportTypes.Stop, new Action<IApplicationService, string>((app, devId) => { app.TransportStop(devId); }) },
			{ TransportTypes.Record, new Action<IApplicationService, string>((app, devId) => { app.TransportRecord(devId); }) },
			{ TransportTypes.ScanForward, new Action<IApplicationService, string>((app, devId) => { app.TransportScanForward(devId); }) },
			{ TransportTypes.ScanReverse, new Action<IApplicationService, string>((app, devId) => { app.TransportScanReverse(devId); }) },
			{ TransportTypes.SkipForward, new Action<IApplicationService, string>((app, devId) => { app.TransportSkipForward(devId); }) },
			{ TransportTypes.SkipReverse, new Action<IApplicationService, string>((app, devId) => { app.TransportSkipReverse(devId); }) },
			{ TransportTypes.NavUp, new Action<IApplicationService, string>((app, devId) => { app.TransportNavUp(devId); }) },
			{ TransportTypes.NavDown, new Action<IApplicationService, string>((app, devId) => { app.TransportNavDown(devId); }) },
			{ TransportTypes.NavLeft, new Action<IApplicationService, string>((app, devId) => { app.TransportNavLeft(devId); }) },
			{ TransportTypes.NavRight, new Action<IApplicationService, string>((app, devId) => { app.TransportNavRight(devId); }) },
			{ TransportTypes.Red, new Action<IApplicationService, string>((app, devId) => { app.TransportRed(devId); }) },
			{ TransportTypes.Green, new Action<IApplicationService, string>((app, devId) => { app.TransportGreen(devId); }) },
			{ TransportTypes.Yellow, new Action<IApplicationService, string>((app, devId) => { app.TransportYellow(devId); }) },
			{ TransportTypes.Blue, new Action<IApplicationService, string>((app, devId) => { app.TransportBlue(devId); }) },
			{ TransportTypes.Select, new Action<IApplicationService, string>((app, devId) => {app.TransportSelect(devId); }) },
		};

		public static void SendCommand(IApplicationService appService, string id, TransportTypes ttype)
		{
			if (actions.TryGetValue(ttype, out Action<IApplicationService, string> act))
			{
				act.Invoke(appService, id);
			}
		}
	}
}
