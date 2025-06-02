namespace pkd_common_utils.NetComs;

using System.Net.Sockets;

/// <summary>
/// An asynchronous wrapper that manages an underlying .NET TcpClient object.
/// </summary>
public class TcpClientWrapper : IDisposable
{
    private TcpClient? _client = new();
    private bool _disposed;

    /// <summary>
    /// default destructor for <see cref="TcpClientWrapper"/>
    /// </summary>
    ~TcpClientWrapper()
    {
        Dispose(false);
    }
    
    /// <summary>
    /// Gets or sets the amount of time to wait for a connection before reporting a failure
    /// to connect. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The IP Address used to connect to the remote host. Defaults to 127.0.0.1.
    /// </summary>
    public string IpAddress { get; init; } = "127.0.0.1";

    /// <summary>
    /// The port number used to connect to the remote host. Defaults to 80.
    /// </summary>
    public int Port { get; init; } = 80;
    
    /// <summary>
    /// True = the client is currently connected to a remote host, false = disconnected.
    /// </summary>
    public bool IsConnected => _client is { Client.Connected: true };
    
    /// <summary>
    /// Callback method triggered when the client successfully connects to a remote host.
    /// </summary>
    public Func<TcpClientWrapper, Task>? OnConnectedCallback { get; init; }
    
    /// <summary>
    /// Callback method triggered when the client disconnects from the remote host for any reason.
    /// The bool parameter indicates if the disconnect was caused by the remote host.
    /// </summary>
    public Func<TcpClientWrapper, bool, Task>? OnDisconnectedCallback { get; init; }
    
    /// <summary>
    /// Callback method invoked when any data is received from the remote host. The int parameter
    /// indicates how many bytes were received.
    /// </summary>
    /// <remarks>This will block incoming data until this callback completes.</remarks>
    public Func<TcpClientWrapper, byte[], Task>? OnDataReceivedCallback { get; init; }

    /// <summary>
    /// Callback method triggered each time the client fails to connect in the amount of time
    /// set by <see cref="ConnectionTimeout"/>
    /// </summary>
    public Action<TcpClientWrapper, string>? OnConnectionFailedCallback { get; init; }
    
    /// <summary>
    /// Attempts an asynchronous connection to the server defined by <see cref="IpAddress"/>:<see cref="Port"/>.
    /// If a connection is not established in the amount of time defined by <see cref="ConnectionTimeout"/> then the underlying
    /// client is closed and <see cref="OnConnectionFailedCallback"/> is invoked.
    /// </summary>
    public async Task ConnectAsync()
    {
        try
        {
            _client ??= new TcpClient();
            var tryConnectTask = _client.ConnectAsync(IpAddress, Port);
            var timeoutTask = Task.Delay(ConnectionTimeout);
            if (await Task.WhenAny(tryConnectTask, timeoutTask) == timeoutTask)
            {
                Disconnect();
                OnConnectionFailedCallback?.Invoke(this, $"Connection timed out for {IpAddress}:{Port}");
                return;
            }
        }
        catch (Exception e)
        {
            OnConnectionFailedCallback?.Invoke(this, $"Error connecting to {IpAddress}:{Port} - {e.Message}");
            return;
        }
        
        // wait for user callback on connection events
        await ConnectedAsync();
        
        // start reading incoming data
        await BeginStreamRead();
    }
    
    /// <summary>
    /// Closes the connection with a remote server and releases the internal TcpClient resources.
    /// </summary>
    public void Disconnect()
    {
        _client?.Close();
        _client = null;
    }

    /// <summary>
    /// Attempts to send data to the remote server. This method does nothing if there is no active connection.
    /// </summary>
    /// <param name="data">The byte data to send to the server. This cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that will be forwarded to the socket stream.</param>
    /// <exception cref="ArgumentNullException">If data is null.</exception>
    public async Task SendAsync(byte[] data, CancellationToken cancellationToken = default)
    {
        if (_client is not { Connected: true }) return;
        ArgumentNullException.ThrowIfNull(data);
       await _client.GetStream().WriteAsync(data, cancellationToken);
    }
    
    /// <summary>
    /// Closes the existing connection and releases TcpClient resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private Task ConnectedAsync()
    {
        return OnConnectedCallback == null ? 
            Task.CompletedTask :
            OnConnectedCallback(this);
    }

    private async Task BeginStreamRead()
    {
        if (_client == null) return;
        var readBuffer = new byte[1024];
        while (true)
        {
            var readLength = -1;
            try
            {
                readLength = await _client.GetStream().ReadAsync(
                    readBuffer, 
                    0,
                    readBuffer.Length);
            }
            catch (Exception e)
            {
                HandleReadException(e);
            }

            if (readLength <= 0)
            {
                // socket has disconnected or errored out
                await HandleDisconnect(wasRemote: true);
                return;
            }
            
            await HandleAsyncReceive(readBuffer[..readLength]);
        }
    }

    private void HandleReadException(Exception exception)
    {
        if (exception.InnerException is not SocketException socketErr)
        {
            Console.WriteLine($"Unknown Exception type: {exception.InnerException?.GetType()}");
            return;
        }
                
        switch (socketErr.ErrorCode)
        {
            case (int)SocketError.OperationAborted:
                Console.WriteLine("Operation aborted.");
                return;
            case (int)SocketError.ConnectionAborted:
                Console.WriteLine("Connection aborted");
                return;
            case (int)SocketError.ConnectionReset:
                Console.WriteLine($"Connection closed remotely on {IpAddress}:{Port}");
                return;
            default:
                Console.WriteLine(exception.Message);
                break;
        }
    }
    
    private Task HandleAsyncReceive(byte[] bytesRead)
    {
        return OnDataReceivedCallback == null ? Task.CompletedTask : OnDataReceivedCallback(this, bytesRead);
    }
    
    private Task HandleDisconnect(bool wasRemote)
    {
        _client?.Close();
        _client = null;
        return OnDisconnectedCallback == null ? Task.CompletedTask : OnDisconnectedCallback(this, wasRemote);
    }
    
    private void Dispose(bool disposing)
    {
        if (_disposed || !disposing) return;
        Disconnect();
        _disposed = true;
    }
}