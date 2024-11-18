namespace pkd_common_utils.NetComs
{
	using System;
	using System.Linq;
	using System.Text;
	using pkd_common_utils.GenericEventArgs;
	using pkd_common_utils.Logging;
	using Crestron.SimplSharp;
	using Crestron.SimplSharp.CrestronSockets;

	/// <summary>
	/// Simple TCP/IP client for ethernet communications.
	/// </summary>
	public class BasicTcpClient : IDisposable
	{
		private TCPClient client;
		private CTimer retryTimer;
		private string hostname;
		private int port;
		private int bufferSize;
		private bool userDisconnect;
		private bool disposed;

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicTcpClient"/> class.
		/// Uses default values: host  = localhost, port = 80, bufferSize = 5000
		/// </summary>
		public BasicTcpClient()
			: this("localhost", 80, 5000)
		{
		}

		~BasicTcpClient()
		{
			this.Dispose(false);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicTcpClient"/> class.
		/// Uses the default values: port = 80, bufferSize = 5000.
		/// This class does not check hostname formatting.
		/// </summary>
		/// <param name="hostname">The Ip address or hostname to connect with.</param>
		/// <exception cref="ArgumentNullException">If 'hostname' is null or empty.</exception>
		public BasicTcpClient(string hostname)
			: this(hostname, 80, 5000)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicTcpClient"/> class.
		/// Uses the default value bufferSize = 5000.
		/// This class does not check hostname formatting.
		/// </summary>
		/// <param name="hostname">The IP Address or hostname to connect with.</param>
		/// <param name="port">The port number used to connect. Value = 0 - 65535</param>
		/// <exception cref="ArgumentNullException">if 'hostname' is null or empty</exception>
		/// <exception cref="ArgumentException">if port is outside the range of 0-65535.</exception>
		public BasicTcpClient(string hostname, int port)
			: this(hostname, port, 5000)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicTcpClient"/> class.
		/// This class does not check hostname formatting.
		/// </summary>
		/// <param name="hostname">The IP Address or hostname to connect with.</param>
		/// <param name="port">The port number used to connect. Value = 0 - 65535</param>
		/// <exception cref="ArgumentNullException">if 'hostname' is null or empty</exception>
		/// <exception cref="ArgumentException">if port is outside the range of 0-65535 or if bufferSize is less than 0.</exception>
		public BasicTcpClient(string hostname, int port, int bufferSize)
		{
			if (String.IsNullOrEmpty(hostname))
			{
				throw new ArgumentNullException("BasicTcpClient.Ctor() - arugment 'hostname' cannot be null or empty.");
			}

			if (port < 0 || port > 65535 || bufferSize < 0)
			{
				throw new ArgumentException("BasicTcpClient.Ctor() - arguemnts 'port' and 'bufferSize' cannot be negative.");
			}

			this.userDisconnect = false;
			this.ReconnectTime = 30000;
			this.hostname = hostname;
			this.port = port;
			this.bufferSize = bufferSize;
			this.client = new TCPClient(hostname, port, bufferSize);
			this.client.SocketStatusChange += this.client_SocketStatusChange;
		}

		/// <summary>
		/// Triggered each time a connection attempt fails. Data package contains the SocketStatus enum for the failure.
		/// </summary>
		public event EventHandler<GenericSingleEventArgs<SocketStatus>> ConnectionFailed;

		/// <summary>
		/// Triggered on a successful connection with the host.
		/// </summary>
		public event EventHandler ClientConnected;

		/// <summary>
		/// Triggered whenever the connection status changes. Current status can be obtained
		/// from the ClientStatusMessage property.
		/// </summary>
		public event EventHandler StatusChanged;

		/// <summary>
		/// Triggered whenever any data is recieved from the server. Arguments packages has the stringified data.
		/// Subscribe to this event if stringified data is desired rather than the raw bytes.
		/// </summary>
		public event EventHandler<GenericSingleEventArgs<string>> RxRecieved;

		/// <summary>
		/// Triggered whenever any data is recieved from the server.
		/// Subscribe to this event the raw byte data is desired rather than stringified data.
		/// </summary>
		public event EventHandler<GenericSingleEventArgs<byte[]>> RxBytesRecieved;

		/// <summary>
		/// The hostname or IP address set at object creation.
		/// </summary>
		public string Hostname
		{
			get
			{
				return this.hostname;
			}
		}

		/// <summary>
		/// Gets the last set of data sent by the server.
		/// </summary>
		public string RxData { get; private set; }

		/// <summary>
		/// Gets the most recent response from the server as an array of bytes.
		/// </summary>
		public byte[] RxBytes { get; private set; }

		/// <summary>
		/// Gets the current connection status. <see cref="SocketStatus"/>.
		/// </summary>
		public SocketStatus ClientStatusMessage
		{
			get
			{
				return this.client.ClientStatus;
			}
		}

		/// <summary>
		/// Gets the port number being used for connection.
		/// </summary>
		public int Port
		{
			get
			{
				return this.port;
			}
		}

		/// <summary>
		/// Gets the current buffer size used when sending or receiving responses from the server.
		/// This is set at object creation.
		/// </summary>
		public int BufferSize
		{
			get
			{
				return this.bufferSize;
			}
		}

		/// <summary>
		/// Gets the current connection status. True = client reports connected, false = client reports disconnected.
		/// </summary>
		public bool Connected
		{
			get
			{
				return this.client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED;
			}
		}

		/// <summary>
		/// Gets or sets whether the client should auto-attempt a reconnect at the ReconnectTime interval.
		/// </summary>
		public bool EnableReconnect { get; set; }

		/// <summary>
		/// Gets or sets the time between reconnect attempts, in Milliseconds.
		/// </summary>
		public int ReconnectTime { get; set; }

		/// <summary>
		/// Attempt to connect to the server. This will disconnect a currently active connection.
		/// </summary>
		public void Connect()
		{
			if (this.Connected)
			{
				Disconnect();
			}

			this.client.ConnectToServerAsync(this.ConnectionCallback);
			this.userDisconnect = false;
		}

		/// <summary>
		/// Disconnect from the server if currently connected. Does nothing if there is no active connection.
		/// </summary>
		public void Disconnect()
		{
			this.userDisconnect = true;
			if (this.Connected)
			{
				this.client.DisconnectFromServer();
			}
		}

		/// <summary>
		/// Send a string of information to the server. Length is limitted by the value of BufferSize.
		/// No action is taken if 'data' is null.
		/// </summary>
		/// <param name="data">The stringified data to send to the server. Cannot be null</param>
		public void Send(string data)
		{
			if (data != null)
			{
				var byteData = Encoding.GetEncoding("ISO-8859-1").GetBytes(data);
				this.client.SendDataAsync(byteData, byteData.Length, this.SendDataCallback);
			}
		}

		public void Send(byte[] data)
		{
			if (data != null && data.Length > 0)
			{
				this.client.SendDataAsync(data, data.Length, this.SendDataCallback);
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void SendDataCallback(TCPClient client, int numBytesSent) { }

		private void client_SocketStatusChange(TCPClient myTCPClient, SocketStatus clientSocketStatus)
		{
			switch (clientSocketStatus)
			{
				case SocketStatus.SOCKET_STATUS_CONNECTED:
					this.client.ReceiveDataAsync(this.RxHandler);
					var temp = this.ClientConnected;
					Logger.Debug("TcpClient {0} - Socket connected.", this.client.AddressClientConnectedTo);
					temp?.Invoke(this, new EventArgs());
					break;

				case SocketStatus.SOCKET_STATUS_CONNECT_FAILED:
				case SocketStatus.SOCKET_STATUS_DNS_FAILED:
				case SocketStatus.SOCKET_STATUS_NO_CONNECT:
					var failed = this.ConnectionFailed;
					failed?.Invoke(this, new GenericSingleEventArgs<SocketStatus>(clientSocketStatus));

					Logger.Debug("TcpClient {0} - EnableReconnect = {1}", this.client.AddressClientConnectedTo, this.EnableReconnect);
					if (this.EnableReconnect)
					{
						Logger.Debug("TcpClient {0} - No Connect. Attempting Reconnect.", this.client.AddressClientConnectedTo);
						this.AttemptReconnect();
					}
					break;

				default:
					var generic = this.StatusChanged;
					generic?.Invoke(this, new EventArgs());
					break;
			}
		}

		private void ConnectionCallback(TCPClient con)
		{
			if (con.ClientStatus != SocketStatus.SOCKET_STATUS_CONNECTED && this.EnableReconnect)
			{
				this.StatusChanged?.Invoke(this, new EventArgs());

				this.AttemptReconnect();
			}
		}

		private void AttemptReconnect()
		{
			Logger.Debug("TcpClient {0} - AttemptReconnect()", this.client.AddressClientConnectedTo);
			if (!this.userDisconnect)
			{
				if (this.Connected)
				{
					this.client.DisconnectFromServer();
				}

				this.retryTimer = new CTimer(x =>
				{
					if (this.client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
					{
						return;
					}

					this.client.ConnectToServerAsync(ConnectionCallback);
				}, this.ReconnectTime);
			}
		}

		private void RxHandler(TCPClient client, int dataLength)
		{
			if (dataLength > 0)
			{
				var bytes = this.client.IncomingDataBuffer.Take(dataLength).ToArray();
				var stringData = Encoding.GetEncoding("ISO-8859-1").GetString(bytes, 0, bytes.Length);
				this.RxBytes = bytes;
				this.RxData = stringData;
				this.RxRecieved?.Invoke(this, new GenericSingleEventArgs<string>(stringData));

				var byteTemp = this.RxBytesRecieved;
				byteTemp?.Invoke(this, new GenericSingleEventArgs<byte[]>(RxBytes));

				client.ReceiveDataAsync(this.RxHandler);
			}
		}

		private void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					this.client.DisconnectFromServer();
					this.client.Dispose();
					this.retryTimer?.Dispose();
				}

				this.disposed = true;
			}
		}
	}
}
