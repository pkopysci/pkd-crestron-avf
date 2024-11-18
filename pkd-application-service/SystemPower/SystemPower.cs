namespace pkd_application_service.SystemPower
{
	using Crestron.SimplSharp;
	using pkd_common_utils.Validation;
	using pkd_domain_service;
	using pkd_domain_service.Data.DisplayData;
	using pkd_hardware_service;
	using pkd_hardware_service.EndpointDevices;
	using System;
	using System.Linq;

	/// <summary>
	/// System state/power management app.
	/// </summary>
	internal class SystemPowerApp : ISystemPowerApp, IDisposable
	{
		private readonly IInfrastructureService infrastructure;
		private readonly IDomainService domain;
		private readonly CTimer autoOffTimer;
		private DateTime autoOffTime = DateTime.Today;
		private bool disposed;
		private static readonly long TIMER_INTERVAL = 59000;

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

			this.AutoShutdownEnabled = true;
			this.autoOffTimer = new CTimer(this.AutoOffTimer_Elapsed, TIMER_INTERVAL);
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="SystemPowerApp"/> class.
		/// </summary>
		~SystemPowerApp()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			this.Dispose(false);
		}

		/// <inheritdoc/>
		public event EventHandler SystemStateChanged;

		/// <inheritdoc/>
		public bool CurrentSystemState { get; private set; }

		/// <inheritdoc/>
		public bool AutoShutdownEnabled { get; private set; }

		/// <inheritdoc/>
		public void AutoShutdownDisable()
		{
			this.AutoShutdownEnabled = false;
		}

		/// <inheritdoc/>
		public void AutoShutdownEnable()
		{
			this.AutoShutdownEnabled = true;
		}

		/// <inheritdoc/>
		public void SetActive()
		{
			if (!this.CurrentSystemState)
			{
				this.CurrentSystemState = true;
				this.SetDisplaysActive();
				this.NotifyChange();
			}
		}

		/// <inheritdoc/>
		public void SetStandby()
		{
			if (this.CurrentSystemState)
			{
				this.CurrentSystemState = false;
				this.SetDisplaysStandby();
				this.NotifyChange();
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

			this.autoOffTime = DateTime.Today.AddHours(hour).AddMinutes(minute);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Standard disposing pattern.
		/// </summary>
		/// <param name="disposing">Disposing flag.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					this.autoOffTimer?.Dispose();
				}

				this.disposed = true;
			}
		}

		private void AutoOffTimer_Elapsed(object sender)
		{
			if (this.AutoShutdownEnabled)
			{
				var now = DateTime.Now;
				if (now.Hour == this.autoOffTime.Hour && now.Minute == this.autoOffTime.Minute)
				{
					this.SetStandby();
				}

				this.autoOffTimer.Reset(TIMER_INTERVAL);
			}
		}

		private void SetDisplaysActive()
		{
			foreach (var display in this.infrastructure.Displays.GetAllDevices())
			{
				display.PowerOn();
				var data = this.domain.Displays.FirstOrDefault(x => x.Id == display.Id);
				if (data != default(Display) && data.HasScreen && !string.IsNullOrEmpty(data.RelayController))
				{
					var endpoint = this.infrastructure.Endpoints.GetDevice(data.RelayController);
					if (endpoint is IRelayDevice relayDev)
					{
						relayDev.PulseRelay(data.ScreenDownRelay, 1800);
					}
				}
			}
		}

		private void SetDisplaysStandby()
		{
			foreach (var display in this.infrastructure.Displays.GetAllDevices())
			{
				display.PowerOff();
				var data = this.domain.Displays.FirstOrDefault(x => x.Id == display.Id);
				if (data != default(Display) && data.HasScreen && !string.IsNullOrEmpty(data.RelayController))
				{
					var endpoint = this.infrastructure.Endpoints.GetDevice(data.RelayController);
					if (endpoint is IRelayDevice relayDev)
					{
						relayDev.PulseRelay(data.ScreenUpRelay, 1800);
					}
				}
			}
		}

		private void NotifyChange()
		{
			var temp = this.SystemStateChanged;
			temp?.Invoke(this, EventArgs.Empty);
		}
	}
}
