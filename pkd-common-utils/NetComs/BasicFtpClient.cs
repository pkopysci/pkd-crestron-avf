using pkd_common_utils.Logging;
using pkd_common_utils.Validation;
using Renci.SshNet;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace pkd_common_utils.NetComs
{
	/// <summary>
	/// Utility class for checking remote SFTP servers and downloading files. Uses either Username/password or SSH keys
	/// for connection.
	/// </summary>
	public class BasicFtpClient : IDisposable
	{
		private readonly SftpClient _client;
		private bool _disposed;

		/// <summary>
		/// Creates an instance of <see cref="BasicFtpClient"/> using a username and password for SFTP connection.
		/// </summary>
		/// <param name="host">The IP address or hostname of the SFTP server.</param>
		/// <param name="username">The username used to log into the SFTP server.</param>
		/// <param name="password">The password used to log into the SFTP server.</param>
		/// <exception cref="ArgumentException">If any argument is null or empty.</exception>
		public BasicFtpClient(string host, string username, string password)
		{
			ParameterValidator.ThrowIfNullOrEmpty(host, "Ctor", nameof(host));
			ParameterValidator.ThrowIfNullOrEmpty(username, "Ctor", nameof(username));
			ParameterValidator.ThrowIfNullOrEmpty(password, "Ctor", nameof(password));

			_client = new SftpClient(host, username, password);
			LastErrorMessage = string.Empty;
		}

		/// <summary>
		/// Creates an instance of <see cref="BasicFtpClient"/> using a username and SSH key for SFTP connection.
		/// </summary>
		/// <param name="host">The IP address or hostname of the SFTP server.</param>
		/// <param name="username">The username used to log into the SFTP server.</param>
		/// <param name="sshKey">The private key used to authenticate with the server.</param>
		/// <exception cref="ArgumentException">If any argument is null or empty.</exception>
		public BasicFtpClient(string host, string username, PrivateKeyFile sshKey)
		{
			ParameterValidator.ThrowIfNullOrEmpty(host, "Ctor", nameof(host));
			ParameterValidator.ThrowIfNullOrEmpty(username, "Ctor", nameof(username));
			ParameterValidator.ThrowIfNull(sshKey, "Ctor", nameof(sshKey));

			_client = new SftpClient(host, username, sshKey);
			LastErrorMessage = string.Empty;
		}

		/// <summary>Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.</summary>
		~BasicFtpClient()
		{
			Dispose(false);
		}

		/// <summary>
		/// Triggered when a response is received from the server after calling QueryFileNames() successfully.
		/// Response data will be stored in the FileNamesReceived property.
		/// </summary>
		public event EventHandler<EventArgs>? FileQueryComplete;

		/// <summary>
		/// Triggered whenever there is an error querying, downloading, or connected to the SFTP server.
		/// Error information will be stored in the LastErrorMessage property.
		/// </summary>
		public event EventHandler<EventArgs>? ErrorOccurred;

		/// <summary>
		/// Triggered when a file download from the remote server has completed successfully.
		/// </summary>
		public event EventHandler<EventArgs>? DownloadComplete;

		/// <summary>
		/// Error information on the last error event.
		/// </summary>
		public string LastErrorMessage { get; private set; }

		/// <summary>
		/// A collection of file names (including extension) that were in the directory provided in the most recent
		/// call to QueryFileNames(). This will be empty if there are no files or QueryFileName() has not been called.
		/// </summary>
		public List<string> FilesNamesReceived { get; private set; } = [];

		/// <summary>
		/// Gets a value indicating whether there is an active connection with the remote server.
		/// </summary>
		public bool IsConnected => _client.IsConnected;

		/// <summary>
		/// Attempts to connect to the remote SFTP server. Does nothing if the client is already connected.
		/// </summary>
		public void Connect()
		{
			if (_client.IsConnected) return;
			Logger.Debug("BasicFtpClient.Connect()");
			try
			{
				_client.Connect();
			}
			catch (Exception e)
			{
				Logger.Error(e,  "BasicFtpClient.Connect() failed");
				Notify(ErrorOccurred);
			}
		}

		/// <summary>
		/// Attempts to disconnect from the remote server. Does nothing if there is no active client connection.
		/// </summary>
		public void Disconnect()
		{
			if (!_client.IsConnected) return;
			Logger.Debug("BasicFtpClient.Disconnect()");
			_client.Disconnect();
		}

		/// <summary>
		/// Queries the remote server for the names and extensions of all files in the target directory. This will
		/// not store any responses that are subdirectories. Only file names are saved.
		/// Does nothing if there is no active connection.
		/// </summary>
		/// <param name="remoteDirectory">The full directory path on the remote server.</param>
		/// <exception cref="ArgumentException">If any argument is null or the empty string.</exception>
		public void QueryFileNames(string remoteDirectory)
		{
			ParameterValidator.ThrowIfNullOrEmpty(remoteDirectory, "QueryFileNames", "remoteDirectory");

			if (!_client.IsConnected)
			{
				Logger.Warn("BasicFtpClient.QueryFileNames() - SFTP Client is not connected.");
				return;
			}

			var allFiles = new List<string>();
			try
			{
				var fileObjs = _client.ListDirectory(remoteDirectory, (numFiles) => { Logger.Debug("num files = {0}", numFiles); });
				allFiles.AddRange(from file in fileObjs where !file.IsDirectory select file.Name);
				FilesNamesReceived = allFiles;
				Notify(FileQueryComplete);
			}
			catch (Exception ex)
			{
				LastErrorMessage = ex.Message;
				Notify(ErrorOccurred);
			}
		}

		/// <summary>
		/// Downloads the given file from the remote server to the provided local directory and file name.
		/// Does nothing if there is not an active connection with the remote server.
		/// </summary>
		/// <param name="remoteFilePath">The full path to the file that will be downloaded, including file extension.</param>
		/// <param name="localFilePath">The full path to the local file that will be saved, including file extension.</param>
		/// <exception cref="ArgumentException">If any argument is null or the empty string.</exception>
		public void DownloadFile(string remoteFilePath, string localFilePath)
		{
			ParameterValidator.ThrowIfNullOrEmpty(remoteFilePath, "DownloadFile", "remoteFilePath");
			ParameterValidator.ThrowIfNullOrEmpty(localFilePath, "DownloadFile", "localFilePath");

			if (!_client.IsConnected)
			{
				Logger.Error("BasicFtpClient.DownloadFile() - SFTP Client is not connected.");
				return;
			}

			Logger.Info("Downloading dependency {0}...", remoteFilePath);

			try
			{
				if (!_client.Exists(remoteFilePath))
				{
					var msg = $"Cannot download file {remoteFilePath} - it does not exist on remote server.";
					Logger.Error(msg);
					LastErrorMessage = msg;
					Notify(ErrorOccurred);
					return;
				}

				var stream = new FileStream(localFilePath, FileMode.Create);
				_client.BeginDownloadFile(remoteFilePath, stream, DownloadCallback);
			}
			catch (Exception ex)
			{
				LastErrorMessage = ex.Message;
				Notify(ErrorOccurred);
			}
		}

		///<inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				if (_client.IsConnected)
				{
					_client.Disconnect();
				}

				_client.Dispose();
			}
		
			_disposed = true;
		}

		private void Notify(EventHandler<EventArgs>? handler)
		{
			handler?.Invoke(this, EventArgs.Empty);
		}

		private void DownloadCallback(IAsyncResult result)
		{
			Logger.Debug("BasicFtpClient.DownloadCallback() - result = {0}", result.IsCompleted);
			_client.EndDownloadFile(result);
			if (result.IsCompleted)
			{
				Notify(DownloadComplete);
			}
		}
	}
}
