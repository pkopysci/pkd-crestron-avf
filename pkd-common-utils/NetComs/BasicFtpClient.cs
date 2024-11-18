namespace pkd_common_utils.NetComs
{
	using Crestron.SimplSharp.CrestronIO;
	using Crestron.SimplSharp.Ssh;
	using pkd_common_utils.Logging;
	using pkd_common_utils.Validation;
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Utility class for checking remote SFTP servers and downloading files. Uses either Username/password or SSH keys
	/// for connection.
	/// </summary>
	public class BasicFtpClient : IDisposable
	{
		private readonly SftpClient client;
		private bool disposed;

		/// <summary>
		/// Creates an instance of <see cref="BasicFtpClient"/> using a username and password for SFTP connection.
		/// </summary>
		/// <param name="host">The IP address or hostname of the SFTP server.</param>
		/// <param name="username">The username used to log into the SFTP server.</param>
		/// <param name="password">The password used to log into the SFTP server.</param>
		/// <exception cref="ArgumentException">If any argument is null or empty.</exception>
		public BasicFtpClient(string host, string username, string password)
		{
			ParameterValidator.ThrowIfNullOrEmpty(host, "Ctor", "host");
			ParameterValidator.ThrowIfNullOrEmpty(username, "Ctor", "username");
			ParameterValidator.ThrowIfNullOrEmpty(password, "Ctor", "password");

			this.client = new SftpClient(host, username, password);
			this.LastErrorMessage = string.Empty;
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
			ParameterValidator.ThrowIfNullOrEmpty(host, "Ctor", "host");
			ParameterValidator.ThrowIfNullOrEmpty(username, "Ctor", "username");
			ParameterValidator.ThrowIfNull(sshKey, "Ctor", "sshKey");

			this.client = new SftpClient(host, username, sshKey);
			this.LastErrorMessage = string.Empty;
		}

		// NO DEFAULT FOR YOU!
		private BasicFtpClient() { }

		~BasicFtpClient()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// Triggered when a response is received from the server after calling QueryFileNames() successfully.
		/// Response data will stored in the FileNamesReceived property.
		/// </summary>
		public event EventHandler<EventArgs> FileQueryComplete;

		/// <summary>
		/// Triggered whenever there is an error querying, downloading, or connected to the the SFTP server.
		/// Error information will be stored in the LastErrorMessage property.
		/// </summary>
		public event EventHandler<EventArgs> ErrorOccurred;

		/// <summary>
		/// Triggered when a file download from the remote server has completed successfully.
		/// </summary>
		public event EventHandler<EventArgs> DownloadComplete;

		/// <summary>
		/// Error information on the last error event.
		/// </summary>
		public string LastErrorMessage { get; private set; }

		/// <summary>
		/// A collection of file names (including extension) that were in the directory provided in the most recent
		/// call to QueryFileNames(). This will be empty if there are no files or QueryFileName() has not been called.
		/// </summary>
		public List<string> FilesNamesReceived { get; private set; }

		/// <summary>
		/// Gets a value indicating whethere or not there is an active connection with the remote server.
		/// </summary>
		public bool IsConnected { get { return this.client.IsConnected; } }

		/// <summary>
		/// Attempts to connect to the remote SFTP server. Does nothing if the client is already connected.
		/// </summary>
		public void Connect()
		{
			if (!this.client.IsConnected)
			{
				Logger.Debug("BasicFtpClient.Connect()");
				this.client.Connect();
			}
		}

		/// <summary>
		/// Attempts to disconnect from the remote server. Does nothing if there is no active client connection.
		/// </summary>
		public void Disconnect()
		{
			if (this.client.IsConnected)
			{
				Logger.Debug("BasicFtpClient.Disconnect()");
				this.client.Disconnect();
			}
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

			if (!this.client.IsConnected)
			{
				Logger.Warn("BasicFtpClient.QueryFileNames() - SFTP Client is not connected.");
				return;
			}

			List<string> allFiles = new List<string>();
			try
			{
				var fileObjs = this.client.ListDirectory(remoteDirectory, (numFiles) => { Logger.Debug("num files = {0}", numFiles); });
				foreach (var file in fileObjs)
				{
					if (!file.IsDirectory)
					{
						allFiles.Add(file.Name);
					}
				}

				this.FilesNamesReceived = allFiles;
				this.Notify(this.FileQueryComplete);
			}
			catch (Exception ex)
			{
				this.LastErrorMessage = ex.Message;
				this.Notify(this.ErrorOccurred);
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

			if (!this.client.IsConnected)
			{
				Logger.Error("BasicFtpClient.DownloadFile() - SFTP Client is not connected.");
				return;
			}

			Logger.Info("Downloading dependency {0}...", remoteFilePath);

			try
			{
				if (!client.Exists(remoteFilePath))
				{
					string msg = string.Format("Cannot download file {0} - it does not exist on remote server.", remoteFilePath);
					Logger.Error(msg);
					this.LastErrorMessage = msg;
					this.Notify(this.ErrorOccurred);
					return;
				}

				FileStream stream = new FileStream(localFilePath, FileMode.Create);
				client.BeginDownloadFile(remoteFilePath, stream, this.DownloadCallback);
			}
			catch (Exception ex)
			{
				this.LastErrorMessage = ex.Message;
				this.Notify(this.ErrorOccurred);
			}
		}

		///<inheritdoc />
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}

			if (disposing)
			{
				if (this.client.IsConnected)
				{
					this.client.Disconnect();
				}

				this.client.Dispose();
			}

			this.disposed = true;
		}

		private void Notify(EventHandler<EventArgs> handler)
		{
			handler?.Invoke(this, EventArgs.Empty);
		}

		private void DownloadCallback(Crestron.SimplSharp.CrestronIO.IAsyncResult result)
		{
			Logger.Debug("BasicFtpClient.DownloadCallback() - result = {0}", result.IsCompleted);
			this.client.EndDownloadFile(result);
			if (result.IsCompleted)
			{
				this.Notify(this.DownloadComplete);
			}
		}

		private void SyncronousDownloadCallback(ulong bytesRead)
		{
			Logger.Debug("BasicFtpClient.DownloadCallback() - result = {0}", bytesRead);
			this.Notify(this.DownloadComplete);
		}
	}
}
