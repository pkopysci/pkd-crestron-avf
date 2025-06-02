using pkd_application_service;

namespace pkd_ui_service.Utility
{
	internal static class TransportUtilities
	{
		private static readonly Dictionary<TransportTypes, Action<IApplicationService, string>> Actions = new Dictionary<TransportTypes, Action<IApplicationService, string>>()
		{
			{ TransportTypes.PowerOn, (app, devId) => { app.TransportPowerOn(devId); } },
			{ TransportTypes.PowerOff, (app, devId) => { app.TransportPowerOff(devId); } },
			{ TransportTypes.PowerToggle, (app, devId) => { app.TransportPowerToggle(devId); } },
			{ TransportTypes.Dash, (app, devId) => { app.TransportDash(devId); } },
			{ TransportTypes.ChannelUp, (app, devId) => { app.TransportChannelUp(devId); } },
			{ TransportTypes.ChannelDown, (app, devId) => { app.TransportChannelDown(devId); } },
			{ TransportTypes.PageUp, (app, devId) => { app.TransportPageUp(devId); } },
			{ TransportTypes.PageDown, (app, devId) => { app.TransportPageDown(devId); } },
			{ TransportTypes.Guide, (app, devId) => { app.TransportGuide(devId); } },
			{ TransportTypes.Menu, (app, devId) => { app.TransportMenu(devId); } },
			{ TransportTypes.Info, (app, devId) => { app.TransportInfo(devId); } },
			{ TransportTypes.Exit, (app, devId) => { app.TransportExit(devId); } },
			{ TransportTypes.Back, (app, devId) => { app.TransportBack(devId); } },
			{ TransportTypes.Play, (app, devId) => { app.TransportPlay(devId); } },
			{ TransportTypes.Pause, (app, devId) => { app.TransportPause(devId); } },
			{ TransportTypes.Stop, (app, devId) => { app.TransportStop(devId); } },
			{ TransportTypes.Record, (app, devId) => { app.TransportRecord(devId); } },
			{ TransportTypes.ScanForward, (app, devId) => { app.TransportScanForward(devId); } },
			{ TransportTypes.ScanReverse, (app, devId) => { app.TransportScanReverse(devId); } },
			{ TransportTypes.SkipForward, (app, devId) => { app.TransportSkipForward(devId); } },
			{ TransportTypes.SkipReverse, (app, devId) => { app.TransportSkipReverse(devId); } },
			{ TransportTypes.NavUp, (app, devId) => { app.TransportNavUp(devId); } },
			{ TransportTypes.NavDown, (app, devId) => { app.TransportNavDown(devId); } },
			{ TransportTypes.NavLeft, (app, devId) => { app.TransportNavLeft(devId); } },
			{ TransportTypes.NavRight, (app, devId) => { app.TransportNavRight(devId); } },
			{ TransportTypes.Red, (app, devId) => { app.TransportRed(devId); } },
			{ TransportTypes.Green, (app, devId) => { app.TransportGreen(devId); } },
			{ TransportTypes.Yellow, (app, devId) => { app.TransportYellow(devId); } },
			{ TransportTypes.Blue, (app, devId) => { app.TransportBlue(devId); } },
			{ TransportTypes.Select, (app, devId) => {app.TransportSelect(devId); } },
		};

		public static void SendCommand(IApplicationService appService, string id, TransportTypes transportType)
		{
			if (Actions.TryGetValue(transportType, out var act))
			{
				act.Invoke(appService, id);
			}
		}
	}
}
