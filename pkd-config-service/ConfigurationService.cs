namespace pkd_config_service
{
	using Crestron.SimplSharp;
	using Crestron.SimplSharp.Ssh;
	using Crestron.SimplSharpPro;
	using pkd_common_utils.FileOps;
	using pkd_common_utils.Logging;
	using pkd_common_utils.NetComs;
	using pkd_domain_service;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Class for loading configuration data from a JSON file.
	/// </summary>
	public sealed class ConfigurationService : IDisposable
	{
		private static readonly string ROOT = "/user/4s-plugins/";
		private readonly uint programSlot;
		private readonly Queue<DependencyData> missingDependencies;
		private readonly CrestronControlSystem parent;
		private bool disposed;
		private bool downloadFailed;
		private BasicFtpClient client;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConfigurationService"/> class.
		/// </summary>
		/// <param name="programSlot">The program slot number to search for when loading configuration.</param>
		public ConfigurationService(uint programSlot, CrestronControlSystem parent)
		{
			this.programSlot = programSlot;
			this.parent = parent;
			this.missingDependencies = new Queue<DependencyData>();
		}

		private ConfigurationService() { }

		~ConfigurationService()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// Triggered when all dependencies have been downloaded and loaded into the program.
		/// </summary>
		public event EventHandler<EventArgs> ConfigLoadComplete;

		/// <summary>
		/// Triggered if there was a failure downloading dependencies or creating the Domain service.
		/// </summary>
		public event EventHandler<EventArgs> ConfigLoadFailed;

		/// <summary>
		/// Gets the Domain hardware management service that was created from the configuration file.
		/// </summary>
		public IDomainService Domain { get; private set; }

		/// <summary>
		/// Load all dependency information and create the Domain service. This will also attempt to download any
		/// missing dependencies from the ServerInfo data of the configuration file.
		/// </summary>
		public void LoadConfig()
		{
			Logger.Info("Reading config file...");

			string configFormat = string.Format("*slot{0:D2}_config*", programSlot);

			try
			{
				string configPath = Directory.GetFiles(DirectoryHelper.GetUserFolder(), configFormat)
					.Where(file => file.EndsWith(".json")).FirstOrDefault();

				Logger.Info("Looking for configuration file {0}...", configPath);

				if (!this.TryLoadConfig(configPath))
				{
					this.Notify(this.ConfigLoadFailed);
					return;
				}

				// Check local user folder for all libaries and dependencies.
				if (!this.TryFindMissingDependencies())
				{
					this.Notify(this.ConfigLoadFailed);
					return;
				}

				// Download any missing dependencies from the server.
				Logger.Info("{0} dependencies missing.", this.missingDependencies.Count);
				if (this.missingDependencies.Count == 0)
				{
					this.Notify(this.ConfigLoadComplete);
					return;
				}

				this.TryDownloadDependencies();
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "ConfigurationService.Ctor() failed to load configuration.");
				this.Notify(this.ConfigLoadFailed);
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void CleanupClient()
		{
			if (this.client != null)
			{
				this.client.Disconnect();
				this.client.DownloadComplete -= this.ClientDownloadCompleteHandler;
				this.client.ErrorOccurred -= this.ClientErrorHandler;
				this.client.Dispose();
				this.client = null;
			}
		}

		private void CreateClient()
		{
			string keyPath = DirectoryHelper.NormalizePath(DirectoryHelper.GetUserFolder() + "/" + this.Domain.ServerInfo.Key);
			PrivateKeyFile key = new PrivateKeyFile(keyPath);
			this.client = new BasicFtpClient(
				this.Domain.ServerInfo.Host,
				this.Domain.ServerInfo.User,
				key);

			this.client.DownloadComplete += this.ClientDownloadCompleteHandler;
			this.client.ErrorOccurred += this.ClientErrorHandler;
			this.client.Connect();
		}

		private void ClientDownloadCompleteHandler(object sender, EventArgs e)
		{
			if (this.missingDependencies.Count == 0)
			{
				this.CleanupClient();

				Logger.Info("All dependencies downloaded. Restarting program...");
#pragma warning disable IDE0059 // Unnecessary assignment of a value
				var bldr = new StringBuilder();
				CrestronConsole.SendControlSystemCommand("progreset -p:" + this.programSlot, out bldr);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
			}
			else
			{
				var nextItem = this.missingDependencies.Dequeue();
				this.client.DownloadFile(nextItem.Remote, nextItem.Local);
			}
		}

		private void ClientErrorHandler(object sender, EventArgs e)
		{
			Logger.Error("Failed to download a dependency: {0}", this.client.LastErrorMessage);
			this.downloadFailed = true;
			if (this.missingDependencies.Count == 0)
			{
				this.CleanupClient();
				this.Notify(this.ConfigLoadFailed);
				return;
			}

			var item = this.missingDependencies.Dequeue();
			this.client.DownloadFile(item.Remote, item.Local);
		}

		private void CheckDependency(string fileName, string remoteSubDir, string[] allFiles)
		{
			DependencyData data = new DependencyData()
			{
				Local = DirectoryHelper.NormalizePath(DirectoryHelper.GetUserFolder() + "/" + fileName),
				Remote = ROOT + remoteSubDir + "/" + fileName
			};

			Logger.Debug("Checking dependency {0}", data.Local);

			if (allFiles.Contains(data.Local))
			{
				return;
			}

			// Prevent duplicates
			foreach (var file in this.missingDependencies)
			{
				if (file.Equals(data))
				{
					return;
				}
			}

			this.missingDependencies.Enqueue(data);
		}

		private void CheckLogicLibrary(string[] allFiles)
		{
			string appService = this.Domain.RoomInfo.Logic.AppServiceLibrary;
			if (string.IsNullOrEmpty(appService))
			{
				return;
			}

			Logger.Debug("Application pluggin set in config. Getting dependency...");
			this.CheckDependency(appService, "services", allFiles);
		}

		private void CheckUiLibrary(string[] allFiles)
		{
			foreach (var ui in this.Domain.UserInterfaces)
			{
				if (!string.IsNullOrEmpty(ui.Sgd))
				{
					this.CheckDependency(ui.Sgd, "ui", allFiles);
				}

				if (!string.IsNullOrEmpty(ui.Library))
				{
					CheckDependency(ui.Library, "ui", allFiles);
				}
			}
		}

		private void CheckProjectorLibrary(string[] allFiles)
		{
			foreach (var display in this.Domain.Displays)
			{
				if (string.IsNullOrEmpty(display.Connection.Driver))
				{
					continue;
				}

				this.CheckDependency(display.Connection.Driver, "displays", allFiles);
			}
		}

		private void CheckAvRouterLibrary(string[] allFiles)
		{
			foreach (var router in this.Domain.RoutingInfo.MatrixData)
			{
				if (string.IsNullOrEmpty(router.Connection.Driver))
				{
					continue;
				}

				this.CheckDependency(router.Connection.Driver, "avr", allFiles);
			}
		}

		private void CheckDspLibrary(string[] allFiles)
		{
			foreach (var dsp in this.Domain.Dsps)
			{
				if (string.IsNullOrEmpty(dsp.Connection.Driver))
				{
					continue;
				}

				foreach (var dependency in dsp.Dependencies)
				{
					this.CheckDependency(dependency, "dsps", allFiles);
				}

				this.CheckDependency(dsp.Connection.Driver, "dsps", allFiles);
			}
		}

		private void CheckCableboxLibrary(string[] allFiles)
		{
			foreach (var cbox in this.Domain.CableBoxes)
			{
				if (string.IsNullOrEmpty(cbox.Connection.Driver))
				{
					continue;
				}

				this.CheckDependency(cbox.Connection.Driver, "cableboxes", allFiles);
			}
		}

		private void CheckLightingLibrary(string[] allFiles)
		{
			foreach (var controller in this.Domain.Lighting)
			{
				if (string.IsNullOrEmpty(controller.Connection.Driver))
				{
					continue;
				}

				this.CheckDependency(controller.Connection.Driver, "lighting", allFiles);
			}
		}

		private bool TryFindMissingDependencies()
		{
			Logger.Info("Checking for missing dependencies...");
			try
			{
				// Format: /user/[filename].[extension]
				var allFiles = Directory.GetFiles(DirectoryHelper.GetUserFolder())
					.Where(file => !file.EndsWith("_config.json") && !file.Equals(this.Domain.ServerInfo.Key))
					.ToArray();

				this.CheckLogicLibrary(allFiles);
				this.CheckUiLibrary(allFiles);
				this.CheckProjectorLibrary(allFiles);
				this.CheckAvRouterLibrary(allFiles);
				this.CheckDspLibrary(allFiles);
				this.CheckCableboxLibrary(allFiles);
				this.CheckLightingLibrary(allFiles);
				return true;
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "ConfigurationService.FindMissingDependencies()");
				return false;
			}
		}

		private bool TryLoadConfig(string configPath)
		{
			if (!string.IsNullOrEmpty(configPath))
			{
				try
				{
					StringBuilder bldr = new StringBuilder();
					using (StreamReader reader = new StreamReader(configPath))
					{
						string line;
						while ((line = reader.ReadLine()) != null)
						{
							bldr.Append(line);
						}
					}

					this.Domain = DomainFactory.CreateDomainFromJson(bldr.ToString());
					return true;
				}
				catch (Exception ex)
				{
					Logger.Error("ConfigurationService failed to load config file: {0}", ex.Message);
					return false;
				}
			}
			else
			{
				Logger.Error("ConfigurationService - Unable to locate configuration file.");
				return false;
			}
		}

		private void TryDownloadDependencies()
		{
			this.downloadFailed = false;
			if (this.missingDependencies.Count == 0)
			{
				this.Notify(this.ConfigLoadComplete);
				return;
			}

			if (this.Domain.ServerInfo == null
				|| string.IsNullOrEmpty(this.Domain.ServerInfo.Host)
				|| string.IsNullOrEmpty(this.Domain.ServerInfo.Key))
			{
				Logger.Error("No manifester server information in config file. Cannot download dependencies.");
				this.downloadFailed = true;
				this.Notify(this.ConfigLoadFailed);
				return;
			}

			Logger.Info("Downloading missing dependencies...");
			this.CreateClient();
			var dependency = this.missingDependencies.Dequeue();
			this.client.DownloadFile(dependency.Remote, dependency.Local);
		}

		private void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}

			if (disposing)
			{
				this.client?.Dispose();
			}

			this.disposed = true;
		}

		private void Notify(EventHandler<EventArgs> handler)
		{
			handler?.Invoke(this, EventArgs.Empty);
		}
	}

}
