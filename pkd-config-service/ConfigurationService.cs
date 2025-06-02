using System.Text;
using Crestron.SimplSharp;
using pkd_common_utils.FileOps;
using pkd_common_utils.Logging;
using pkd_common_utils.NetComs;
using pkd_domain_service;
using Renci.SshNet;

namespace pkd_config_service
{
	/// <summary>
    /// Class for loading configuration data from a JSON file.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </remarks>
    /// <param name="programSlot">The program slot number to search for when loading configuration.</param>
    public sealed class ConfigurationService(uint programSlot) : IDisposable
	{
		private const string Root = "/user/net8-plugins/";
		private readonly Queue<DependencyData> _missingDependencies = [];
		private bool _disposed;
		private bool _errorDownloading;
		private BasicFtpClient? _client;

		/// <inheritdoc />
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

			var configFormat = $"*slot{programSlot:D2}_config*";

			try
			{
				var configPath = Directory
					.GetFiles(DirectoryHelper.GetUserFolder(), configFormat)
					.FirstOrDefault(file => file.EndsWith(".json")) ?? string.Empty;

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

				// Check local user folder for all libraries and dependencies.
				if (!TryFindMissingDependencies())
				{
					Notify(ConfigLoadFailed);
					return;
				}

				// Download any missing dependencies from the server.
				Logger.Info("{0} dependencies missing.", _missingDependencies.Count);
				if (_missingDependencies.Count == 0)
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
			if (_client == null) return;
			_client.Disconnect();
			_client.DownloadComplete -= ClientDownloadCompleteHandler;
			_client.ErrorOccurred -= ClientErrorHandler;
			_client.Dispose();
			_client = null;
		}

		private bool CreateClient()
		{
			if (Domain == null)
			{
				Logger.Error("ConfigurationService.CreateClient() - Domain object not populated.");
				return false;
			}

			try
			{
				var keyPath = DirectoryHelper.NormalizePath(DirectoryHelper.GetUserFolder() + "/" + Domain.ServerInfo.Key);
				var key = new PrivateKeyFile(keyPath);
				
				Logger.Debug($"CreateClient() : Creating key file {keyPath} for {Domain.ServerInfo.Host}");
				
				_client = new BasicFtpClient(
					Domain.ServerInfo.Host,
					Domain.ServerInfo.User,
					key);

				_client.DownloadComplete += ClientDownloadCompleteHandler;
				_client.ErrorOccurred += ClientErrorHandler;
				_client.Connect();
				return true;
			}
			catch (Exception e)
			{
				Logger.Error(e, "ConfigurationService.CreateClient() failed to connect to server.");
				Notify(ConfigLoadFailed);
				return false;
			}
		}

		private void ClientDownloadCompleteHandler(object? sender, EventArgs e)
		{
			if (_missingDependencies.Count == 0)
			{
				CleanupClient();
				Logger.Info("All dependencies downloaded. Restarting program...");
				if (_errorDownloading)
				{
					Logger.Error($"ConfigurationService - Failed to download all dependencies. See error logs for details.");
					return;
				}
				
				CrestronConsole.SendControlSystemCommand("progreset -p:" + programSlot, out _);
			}
			else
			{
				var nextItem = _missingDependencies.Dequeue();
				_client?.DownloadFile(nextItem.Remote, nextItem.Local);
			}
		}

		private void ClientErrorHandler(object? sender, EventArgs e)
		{
			Logger.Error($"Failed to download a dependency: {_client?.LastErrorMessage}");
			_errorDownloading = true;
            if (_missingDependencies.Count == 0)
			{
				CleanupClient();
				Notify(ConfigLoadFailed);
				return;
			}

			var item = _missingDependencies.Dequeue();
			_client?.DownloadFile(item.Remote, item.Local);
		}

		private void CheckDependency(string fileName, string remoteSubDir, string[] allFiles)
		{
			var data = new DependencyData()
			{
				Local = DirectoryHelper.NormalizePath(Root + fileName),
				Remote = Root + remoteSubDir + "/" + fileName
			};

			if (allFiles.Contains(data.Local))
			{
				return;
			}

			// Prevent duplicates
			foreach (var file in _missingDependencies)
			{
				if (file.Equals(data))
				{
					return;
				}
			}

			Logger.Debug($"Missing dependency {data.Local}");
			_missingDependencies.Enqueue(data);
		}

		private void CheckLogicLibrary(string[] allFiles)
		{
			var appService = Domain?.RoomInfo.Logic.AppServiceLibrary ?? string.Empty;
			if (string.IsNullOrEmpty(appService))
			{
				return;
			}

			Logger.Debug("Application plugin set in config. Getting dependency...");
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

		private void CheckCableBoxLibrary(string[] allFiles)
		{
            if (Domain == null)
            {
                Logger.Error("ConfigurationService.CheckCableBoxLibrary() - Domain data not populated.");
                return;
            }

            foreach (var cableBox in Domain.CableBoxes)
			{
				if (string.IsNullOrEmpty(cableBox.Connection.Driver))
				{
					continue;
				}

				CheckDependency(cableBox.Connection.Driver, "cableboxes", allFiles);
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

		private void CheckVideoWalls(string[] allFiles)
		{
			if (Domain == null)
			{
				Logger.Error("ConfigurationService.CheckVideoWalls() - Domain data not populated.");
				return;
			}

			foreach (var vidwall in Domain.VideoWalls)
			{
				if (string.IsNullOrEmpty(vidwall.Connection.Driver))
				{
					continue;
				}
				
				CheckDependency(vidwall.Connection.Driver, "videowalls", allFiles);
			}
		}

		private void CheckCameras(string[] allFiles)
		{
			if (Domain == null)
			{
				Logger.Error("ConfigurationService.CheckCameras() - Domain data not populated.");
				return;
			}

			foreach (var camera in Domain.Cameras)
			{
				if (string.IsNullOrEmpty(camera.Connection.Driver))
				{
					continue;
				}
				
				CheckDependency(camera.Connection.Driver, "cameras", allFiles);
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
				// if root directory does not exist, create that directory.
				var normalizedRoot = DirectoryHelper.NormalizePath(Root);
				if (!Directory.Exists(normalizedRoot))
				{
					Directory.CreateDirectory(normalizedRoot);
				}
				
				// Format: /user/4s-plugins/[filename].[extension]
				var allFiles = Directory.GetFiles(normalizedRoot)
					.Where(file => !file.EndsWith("_config.json") && !file.Equals(Domain.ServerInfo.Key))
					.ToArray();

				CheckLogicLibrary(allFiles);
				CheckUiLibrary(allFiles);
				CheckProjectorLibrary(allFiles);
				CheckAvRouterLibrary(allFiles);
				CheckDspLibrary(allFiles);
				CheckCableBoxLibrary(allFiles);
				CheckLightingLibrary(allFiles);
				CheckVideoWalls(allFiles);
				CheckCameras(allFiles);
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
					var builder = new StringBuilder();
					using (StreamReader reader = new(configPath))
					{
						while (reader.ReadLine() is { } line)
						{
							builder.Append(line);
						}
					}

					Domain = DomainFactory.CreateDomainFromJson(builder.ToString());
					return true;
				}
				catch (Exception ex)
				{
					Logger.Error("ConfigurationService failed to load config file: {0}", ex.Message);
					return false;
				}
			}
			
			Logger.Error("ConfigurationService - Unable to locate configuration file.");
			return false;
		}

		private void TryDownloadDependencies()
		{
			if (_missingDependencies.Count == 0)
			{
				Notify(ConfigLoadComplete);
				return;
			}

			if (Domain?.ServerInfo == null
				|| string.IsNullOrEmpty(Domain.ServerInfo.Host)
				|| string.IsNullOrEmpty(Domain.ServerInfo.Key))
			{
				Logger.Error("No manifest server information in config file. Cannot download dependencies.");
				Notify(ConfigLoadFailed);
				return;
			}

			Logger.Info("Downloading missing dependencies...");
			if (!CreateClient()) return;
			var dependency = _missingDependencies.Dequeue();
			_client?.DownloadFile(dependency.Remote, dependency.Local);
		}

		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				_client?.Dispose();
			}

			_disposed = true;
		}

		private void Notify(EventHandler<EventArgs>? handler)
		{
			handler?.Invoke(this, EventArgs.Empty);
		}
	}
}
