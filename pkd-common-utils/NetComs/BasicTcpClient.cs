using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using pkd_common_utils.GenericEventArgs;
using pkd_common_utils.Logging;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace pkd_common_utils.NetComs
{
	/// <summary>
	/// Simple TCP/IP client for ethernet communications.
	/// </summary>
	public class BasicTcpClient : IDisposable
	{
		private readonly TCPClient _client;
		private CTimer? _retryTimer;
		private bool _userDisconnect;
		private bool _disposed;

		/// <summary>Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.</summary>
		~BasicTcpClient()
		{
			Dispose(false);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BasicTcpClient"/> class.
		/// This class does not check hostname formatting.
		/// </summary>
		/// <param name="hostname">The IP Address or hostname to connect with. Defaults to 'localhost'</param>
		/// <param name="port">The port number used to connect. Value = 0 - 65535, defaults to 80.</param>
		/// <param name="bufferSize">The size of the read/write stream buffer. Defaults to 5000.</param>
		/// <exception cref="ArgumentNullException">if 'hostname' is null or empty</exception>
		/// <exception cref="ArgumentException">if port is outside the range of 0-65535 or if bufferSize is less than 0.</exception>
		public BasicTcpClient(string hostname = "localhost", int port = 80, int bufferSize = 5000)
		{
			if (string.IsNullOrEmpty(hostname))
			{
				throw new ArgumentNullException(hostname, "BasicTcpClient.Ctor() - argument 'hostname' cannot be null or empty.");
			}

			if (port < 0 || port > 65535 || bufferSize < 0)
			{
				throw new ArgumentException("BasicTcpClient.Ctor() - arguments 'port' and 'bufferSize' cannot be negative.");
			}

			Port = port;
			BufferSize = bufferSize;
			Hostname = hostname;
			_userDisconnect = false;
			ReconnectTime = 30000;
			_client = new TCPClient(hostname, port, bufferSize);
			_client.SocketStatusChange += client_SocketStatusChange;
		}

		/// <summary>
		/// Triggered each time a connection attempt fails. Data package contains the SocketStatus enum for the failure.
		/// </summary>
		public event EventHandler<GenericSingleEventArgs<SocketStatus>>? ConnectionFailed;

		/// <summary>
		/// Triggered on a successful connection with the host.
		/// </summary>
		public event EventHandler? ClientConnected;

		/// <summary>
		/// Triggered whenever the connection status changes. Current status can be obtained
		/// from the ClientStatusMessage property.
		/// </summary>
		public event EventHandler? StatusChanged;

		/// <summary>
		/// Triggered whenever any data is received from the server. Arguments packages has the string data.
		/// Subscribe to this event if string data is desired rather than the raw bytes.
		/// </summary>
		public event EventHandler<GenericSingleEventArgs<string>>? RxReceived;

		/// <summary>
		/// Triggered whenever any data is received from the server.
		/// Subscribe to this event the raw byte data is desired rather than string data.
		/// </summary>
		public event EventHandler<GenericSingleEventArgs<byte[]>>? RxBytesReceived;

		/// <summary>
		/// The hostname or IP address set at object creation.
		/// </summary>
		public string Hostname { get; private set; }

		/// <summary>
		/// Gets the last set of data sent by the server.
		/// </summary>
		public string RxData { get; private set; } = string.Empty;

		/// <summary>
		/// Gets the most recent response from the server as an array of bytes.
		/// </summary>
		public byte[] RxBytes { get; private set; } = [];

		/// <summary>
		/// Gets the current connection status. <see cref="SocketStatus"/>.
		/// </summary>
		public SocketStatus ClientStatusMessage => _client.ClientStatus;

		/// <summary>
		/// Gets the port number being used for connection.
		/// </summary>
		public int Port { get; }

		/// <summary>
		/// Gets the current buffer size used when sending or receiving responses from the server.
		/// This is set at object creation.
		/// </summary>
		public int BufferSize { get; }

		/// <summary>
		/// Gets the current connection status. True = client reports connected, false = client reports disconnected.
		/// </summary>
		public bool Connected => _client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED;

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
			if (Connected)
			{
				Disconnect();
			}

			_client.ConnectToServerAsync(ConnectionCallback);
			_userDisconnect = false;
		}

		/// <summary>
		/// Disconnect from the server if currently connected. Does nothing if there is no active connection.
		/// </summary>
		public void Disconnect()
		{
			_userDisconnect = true;
			if (Connected)
			{
				_client.DisconnectFromServer();
			}
		}

		/// <summary>
		/// Send a string of information to the server. Length is limited by the value of BufferSize.
		/// No action is taken if 'data' is null.
		/// </summary>
		/// <param name="data">The string data to send to the server. Cannot be null</param>
		public void Send(string data)
		{
			var byteData = Encoding.GetEncoding("ISO-8859-1").GetBytes(data);
			_client.SendDataAsync(byteData, byteData.Length, SendDataCallback);
		}

		/// <summary>
		/// Send a command to the server as a byte array.
		/// </summary>
		/// <param name="data">The byte data to send to the server.</param>
		public void Send(byte[] data)
		{
			if (data.Length > 0)
			{
				_client.SendDataAsync(data, data.Length, SendDataCallback);
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void SendDataCallback(TCPClient clientObj, int numBytesSent) { }

		private void client_SocketStatusChange(TCPClient myTcpClient, SocketStatus clientSocketStatus)
		{
			switch (clientSocketStatus)
			{
				case SocketStatus.SOCKET_STATUS_CONNECTED:
					_client.ReceiveDataAsync(RxHandler);
					var temp = ClientConnected;
					Logger.Debug("TcpClient {0} - Socket connected.", _client.AddressClientConnectedTo);
					temp?.Invoke(this, EventArgs.Empty);
					break;

				case SocketStatus.SOCKET_STATUS_CONNECT_FAILED:
				case SocketStatus.SOCKET_STATUS_DNS_FAILED:
				case SocketStatus.SOCKET_STATUS_NO_CONNECT:
					var failed = ConnectionFailed;
					failed?.Invoke(this, new GenericSingleEventArgs<SocketStatus>(clientSocketStatus));

					Logger.Debug("TcpClient {0} - EnableReconnect = {1}", _client.AddressClientConnectedTo, EnableReconnect);
					if (EnableReconnect)
					{
						Logger.Debug("TcpClient {0} - No Connect. Attempting Reconnect.", _client.AddressClientConnectedTo);
						AttemptReconnect();
					}
					break;

				default:
					var generic = StatusChanged;
					generic?.Invoke(this, EventArgs.Empty);
					break;
			}
		}

		private void ConnectionCallback(TCPClient clientObj)
		{
			if (clientObj.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED || !EnableReconnect) return;
			StatusChanged?.Invoke(this, EventArgs.Empty);
			AttemptReconnect();
		}

		private void AttemptReconnect()
		{
			Logger.Debug("TcpClient {0} - AttemptReconnect()", _client.AddressClientConnectedTo);
			if (_userDisconnect) return;
			if (Connected)
			{
				_client.DisconnectFromServer();
			}

			_retryTimer = new CTimer(_ =>
			{
				if (_client.ClientStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
				{
					return;
				}

				_client.ConnectToServerAsync(ConnectionCallback);
			}, ReconnectTime);
		}

		private void RxHandler(TCPClient clientObj, int dataLength)
		{
			if (dataLength <= 0) return;
			var bytes = clientObj.IncomingDataBuffer.Take(dataLength).ToArray();
			var stringData = Encoding.GetEncoding("ISO-8859-1").GetString(bytes, 0, bytes.Length);
			RxBytes = bytes;
			RxData = stringData;
			RxReceived?.Invoke(this, new GenericSingleEventArgs<string>(stringData));

			var byteTemp = RxBytesReceived;
			byteTemp?.Invoke(this, new GenericSingleEventArgs<byte[]>(RxBytes));
			clientObj.ReceiveDataAsync(RxHandler);
		}

		private void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				_client.DisconnectFromServer();
				_client.Dispose();
				_retryTimer?.Dispose();
			}

			_disposed = true;
		}
	}
}
