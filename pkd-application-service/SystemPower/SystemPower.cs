using Crestron.SimplSharp;
using pkd_common_utils.Validation;
using pkd_domain_service;
using pkd_hardware_service;
using pkd_hardware_service.EndpointDevices;

namespace pkd_application_service.SystemPower
{
	/// <summary>
	/// System state/power management app.
	/// </summary>
	internal class SystemPowerApp : ISystemPowerApp, IDisposable
	{
		private const long TimerInterval = 59000;
		private readonly IInfrastructureService infrastructure;
		private readonly IDomainService domain;
		private readonly CTimer autoOffTimer;
		private DateTime autoOffTime = DateTime.Today;
		private bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="SystemPowerApp"/> class.
		/// </summary>
		/// <param name="infrastructure">The hardware control service section of the program.</param>
		/// <param name="domain">The configuration service of the program.</param>
		public SystemPowerApp(IInfrastructureService infrastructure, IDomainService domain)
		{
			ParameterValidator.ThrowIfNull(infrastructure, "Ctor", "Infrastructure");
			ParameterValidator.ThrowIfNull(domain, "Ctor", "domain");
			this.infrastructure = infrastructure;
			this.domain = domain;

			AutoShutdownEnabled = true;
			autoOffTimer = new CTimer(AutoOffTimer_Elapsed, TimerInterval);
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="SystemPowerApp"/> class.
		/// </summary>
		~SystemPowerApp()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler? SystemStateChanged;

		/// <inheritdoc/>
		public bool CurrentSystemState { get; private set; }

		/// <inheritdoc/>
		public bool AutoShutdownEnabled { get; private set; }

		/// <inheritdoc/>
		public void AutoShutdownDisable()
		{
			AutoShutdownEnabled = false;
		}

		/// <inheritdoc/>
		public void AutoShutdownEnable()
		{
			AutoShutdownEnabled = true;
		}

		/// <inheritdoc/>
		public void SetActive()
		{
			if (!CurrentSystemState)
			{
				CurrentSystemState = true;
				SetDisplaysActive();
				NotifyChange();
			}
		}

		/// <inheritdoc/>
		public void SetStandby()
		{
			if (CurrentSystemState)
			{
				CurrentSystemState = false;
				SetDisplaysStandby();
				NotifyChange();
			}
		}

		/// <inheritdoc/>
		public void SetAutoShutdownTime(int hour, int minute)
		{
			if (hour > 23 || hour < 0)
			{
				throw new ArgumentException("SetAutoShutdownTime() - hour must be in the range 0-23.");
			}

			if (minute > 59 || minute < 0)
			{
				throw new ArgumentException("SetAutoShutdownTime() - minute must be in the range 0-59");
			}

			autoOffTime = DateTime.Today.AddHours(hour).AddMinutes(minute);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Standard disposing pattern.
		/// </summary>
		/// <param name="disposing">Disposing flag.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing)
			{
				autoOffTimer.Dispose();
			}

			disposed = true;
		}

		private void AutoOffTimer_Elapsed(object? sender)
		{
			if (!AutoShutdownEnabled) return;
			var now = DateTime.Now;
			if (now.Hour == autoOffTime.Hour && now.Minute == autoOffTime.Minute)
			{
				SetStandby();
			}

			autoOffTimer.Reset(TimerInterval);
		}

		private void SetDisplaysActive()
		{
			foreach (var display in infrastructure.Displays.GetAllDevices())
			{
				display.PowerOn();
				var data = domain.Displays.FirstOrDefault(x => x.Id == display.Id);
				if (data is not { HasScreen: true } || string.IsNullOrEmpty(data.RelayController)) continue;
				
				var endpoint = infrastructure.Endpoints.GetDevice(data.RelayController);
				if (endpoint is IRelayDevice relayDev)
				{
					relayDev.PulseRelay(data.ScreenDownRelay, 1800);
				}
			}
		}

		private void SetDisplaysStandby()
		{
			foreach (var display in infrastructure.Displays.GetAllDevices())
			{
				display.PowerOff();
				var data = domain.Displays.FirstOrDefault(x => x.Id == display.Id);
				if (data is not { HasScreen: true } || string.IsNullOrEmpty(data.RelayController)) continue;
				
				var endpoint = infrastructure.Endpoints.GetDevice(data.RelayController);
				if (endpoint is IRelayDevice relayDev)
				{
					relayDev.PulseRelay(data.ScreenUpRelay, 1800);
				}
			}
		}

		private void NotifyChange()
		{
			var temp = SystemStateChanged;
			temp?.Invoke(this, EventArgs.Empty);
		}
	}
}
