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
    /// <remarks>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </remarks>
    /// <param name="programSlot">The program slot number to search for when loading configuration.</param>
    public sealed class ConfigurationService(uint programSlot, CrestronControlSystem parent) : IDisposable
	{
		private static readonly string ROOT = "/user/4s-plugins/";
		private readonly uint programSlot = programSlot;
		private readonly Queue<DependencyData> missingDependencies = new Queue<DependencyData>();
		private readonly CrestronControlSystem parent = parent;
		private bool disposed;
		private BasicFtpClient? client;

        ~ConfigurationService()
		{
			Dispose(false);
		}

		/// <summary>
		/// Triggered when all dependencies have been downloaded and loaded into the program.
		/// </summary>
		public event EventHandler<EventArgs>? ConfigLoadComplete;

		/// <summary>
		/// Triggered if there was a failure downloading dependencies or creating the Domain service.
		/// </summary>
		public event EventHandler<EventArgs>? ConfigLoadFailed;

		/// <summary>
		/// Gets the Domain hardware management service that was created from the configuration file.
		/// </summary>
		public IDomainService? Domain { get; private set; }

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
					.Where(file => file.EndsWith(".json")).FirstOrDefault() ?? string.Empty;

				if (configPath.Equals(string.Empty))
				{
					Logger.Error("ConfigurationService.LoadConfig() - no config file found.");
					Notify(ConfigLoadFailed);
					return;
				}
				
				Logger.Info($"Looking for configuration file {configPath}...");

				if (!TryLoadConfig(configPath))
				{
					Notify(ConfigLoadFailed);
					return;
				}

				// Check local user folder for all libaries and dependencies.
				if (!TryFindMissingDependencies())
				{
					Notify(ConfigLoadFailed);
					return;
				}

				// Download any missing dependencies from the server.
				Logger.Info("{0} dependencies missing.", missingDependencies.Count);
				if (missingDependencies.Count == 0)
				{
					Notify(ConfigLoadComplete);
					return;
				}

				TryDownloadDependencies();
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "ConfigurationService.Ctor() failed to load configuration.");
				Notify(ConfigLoadFailed);
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void CleanupClient()
		{
			if (client != null)
			{
				client.Disconnect();
				client.DownloadComplete -= ClientDownloadCompleteHandler;
				client.ErrorOccurred -= ClientErrorHandler;
				client.Dispose();
				client = null;
			}
		}

		private void CreateClient()
		{
			if (Domain == null)
			{
				Logger.Error("ConfigurationService.CreateClient() - Domain object not populated.");
				return;
			}

			string keyPath = DirectoryHelper.NormalizePath(DirectoryHelper.GetUserFolder() + "/" + Domain.ServerInfo.Key);
			PrivateKeyFile key = new PrivateKeyFile(keyPath);
			client = new BasicFtpClient(
				Domain.ServerInfo.Host,
				Domain.ServerInfo.User,
				key);

			client.DownloadComplete += ClientDownloadCompleteHandler;
			client.ErrorOccurred += ClientErrorHandler;
			client.Connect();
		}

		private void ClientDownloadCompleteHandler(object? sender, EventArgs e)
		{
			if (missingDependencies.Count == 0)
			{
				CleanupClient();

				Logger.Info("All dependencies downloaded. Restarting program...");
#pragma warning disable IDE0059 // Unnecessary assignment of a value
				var bldr = new StringBuilder();
				CrestronConsole.SendControlSystemCommand("progreset -p:" + programSlot, out bldr);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
			}
			else
			{
				var nextItem = missingDependencies.Dequeue();
				client?.DownloadFile(nextItem.Remote, nextItem.Local);
			}
		}

		private void ClientErrorHandler(object? sender, EventArgs e)
		{
			Logger.Error($"Failed to download a dependency: {client?.LastErrorMessage}");

            if (missingDependencies.Count == 0)
			{
				CleanupClient();
				Notify(ConfigLoadFailed);
				return;
			}

			var item = missingDependencies.Dequeue();
			client?.DownloadFile(item.Remote, item.Local);
		}

		private void CheckDependency(string fileName, string remoteSubDir, string[] allFiles)
		{
			DependencyData data = new DependencyData()
			{
				Local = DirectoryHelper.NormalizePath(DirectoryHelper.GetUserFolder() + "/" + fileName),
				Remote = ROOT + remoteSubDir + "/" + fileName
			};

			if (allFiles.Contains(data.Local))
			{
				return;
			}

			// Prevent duplicates
			foreach (var file in missingDependencies)
			{
				if (file.Equals(data))
				{
					return;
				}
			}

			Logger.Debug($"Missing dependency {data.Local}");
			missingDependencies.Enqueue(data);
		}

		private void CheckLogicLibrary(string[] allFiles)
		{
			string appService = Domain?.RoomInfo.Logic.AppServiceLibrary ?? string.Empty;
			if (string.IsNullOrEmpty(appService))
			{
				return;
			}

			Logger.Debug("Application pluggin set in config. Getting dependency...");
			CheckDependency(appService, "services", allFiles);
		}

		private void CheckUiLibrary(string[] allFiles)
		{
			if (Domain == null)
			{
				Logger.Error("ConfigurationService.CheckUiLibrary() - Domain data not populated.");
				return;
			}

			foreach (var ui in Domain.UserInterfaces)
			{
				if (!string.IsNullOrEmpty(ui.Sgd))
				{
					CheckDependency(ui.Sgd, "ui", allFiles);
				}

				if (!string.IsNullOrEmpty(ui.Library))
				{
					CheckDependency(ui.Library, "ui", allFiles);
				}
			}
		}

		private void CheckProjectorLibrary(string[] allFiles)
		{
            if (Domain == null)
            {
                Logger.Error("ConfigurationService.CheckProjectorLibrary() - Domain data not populated.");
                return;
            }

            foreach (var display in Domain.Displays)
			{
				if (string.IsNullOrEmpty(display.Connection.Driver))
				{
					continue;
				}

				CheckDependency(display.Connection.Driver, "displays", allFiles);
			}
		}

		private void CheckAvRouterLibrary(string[] allFiles)
		{
            if (Domain == null)
            {
                Logger.Error("ConfigurationService.CheckAvRouterLibrary() - Domain data not populated.");
                return;
            }

            foreach (var router in Domain.RoutingInfo.MatrixData)
			{
				if (string.IsNullOrEmpty(router.Connection.Driver))
				{
					continue;
				}

				CheckDependency(router.Connection.Driver, "avr", allFiles);
			}
		}

		private void CheckDspLibrary(string[] allFiles)
		{
            if (Domain == null)
            {
                Logger.Error("ConfigurationService.CheckDspLibrary() - Domain data not populated.");
                return;
            }

            foreach (var dsp in Domain.Dsps)
			{
				if (string.IsNullOrEmpty(dsp.Connection.Driver))
				{
					continue;
				}

				foreach (var dependency in dsp.Dependencies)
				{
					CheckDependency(dependency, "dsps", allFiles);
				}

				CheckDependency(dsp.Connection.Driver, "dsps", allFiles);
			}
		}

		private void CheckCableboxLibrary(string[] allFiles)
		{
            if (Domain == null)
            {
                Logger.Error("ConfigurationService.CheckCableboxLibrary() - Domain data not populated.");
                return;
            }

            foreach (var cbox in Domain.CableBoxes)
			{
				if (string.IsNullOrEmpty(cbox.Connection.Driver))
				{
					continue;
				}

				CheckDependency(cbox.Connection.Driver, "cableboxes", allFiles);
			}
		}

		private void CheckLightingLibrary(string[] allFiles)
		{
            if (Domain == null)
            {
                Logger.Error("ConfigurationService.CheckLightingLibrary() - Domain data not populated.");
                return;
            }

            foreach (var controller in Domain.Lighting)
			{
				if (string.IsNullOrEmpty(controller.Connection.Driver))
				{
					continue;
				}

				CheckDependency(controller.Connection.Driver, "lighting", allFiles);
			}
		}

		private bool TryFindMissingDependencies()
		{
            if (Domain == null)
            {
                Logger.Error("ConfigurationService.TryFindMissingDependencies() - Domain data not populated.");
                return false;
            }

            Logger.Info("Checking for missing dependencies...");
			try
			{
				// Format: /user/[filename].[extension]
				var allFiles = Directory.GetFiles(DirectoryHelper.GetUserFolder())
					.Where(file => !file.EndsWith("_config.json") && !file.Equals(Domain.ServerInfo.Key))
					.ToArray();

				CheckLogicLibrary(allFiles);
				CheckUiLibrary(allFiles);
				CheckProjectorLibrary(allFiles);
				CheckAvRouterLibrary(allFiles);
				CheckDspLibrary(allFiles);
				CheckCableboxLibrary(allFiles);
				CheckLightingLibrary(allFiles);
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
					using (StreamReader reader = new(configPath))
					{
						string? line;
						while ((line = reader.ReadLine()) != null)
						{
							bldr.Append(line);
						}
					}

					Domain = DomainFactory.CreateDomainFromJson(bldr.ToString());
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
			if (missingDependencies.Count == 0)
			{
				Notify(ConfigLoadComplete);
				return;
			}

			if (Domain?.ServerInfo == null
				|| string.IsNullOrEmpty(Domain.ServerInfo.Host)
				|| string.IsNullOrEmpty(Domain.ServerInfo.Key))
			{
				Logger.Error("No manifester server information in config file. Cannot download dependencies.");
				Notify(ConfigLoadFailed);
				return;
			}

			Logger.Info("Downloading missing dependencies...");
			CreateClient();
			var dependency = missingDependencies.Dequeue();
			client?.DownloadFile(dependency.Remote, dependency.Local);
		}

		private void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				client?.Dispose();
			}

			disposed = true;
		}

		private void Notify(EventHandler<EventArgs>? handler)
		{
			handler?.Invoke(this, EventArgs.Empty);
		}
	}

}
