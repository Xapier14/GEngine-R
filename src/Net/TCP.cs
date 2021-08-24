using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GEngine.Net
{
    public class TCPServerEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public TCPClient Client { get; set; }
        public TCPServerEventArgs()
        {
            Client = null;
            Message = "";
            Title = "";
        }
    }
    public class TCPServer : IDisposable
    {
        private const int THREAD_WAIT = 200; //ms
        private const int CLIENT_TIMEOUT = 15; //s
        private const string CONNECTION_STRING = "GEngineConnect";

        private TcpListener _listener;
        private ushort _port;
        private bool _listening;
        private Thread _listenThread;

        public delegate void TCPServerEventHandler(TCPServerEventArgs eventArgs);
        public TCPServerEventHandler OnClientConnect;
        public TCPServerEventHandler OnClientFail;

        public bool Listening => _listening;

        public ushort Port => _port;

        public TCPServer(ushort port)
        {
            if (port == 0)
                throw new EngineException("Port number cannot be 0.");
            _port = port;
            _listener = new TcpListener(new IPEndPoint(IPAddress.Any, _port));
            _listening = false;
            _listenThread = new Thread(new ThreadStart(ListenThread));
        }

        public void Start()
        {
            // ignore if we are already listening
            if (_listening)
                return;
            _listener.Start();
            _listening = true;
            _listenThread?.Start();
        }

        public void Stop()
        {
            // ignore if we are not listening
            if (!_listening)
                return;
            _listener.Stop();
            _listening = false;
            _listenThread = new Thread(new ThreadStart(ListenThread));
        }

        private void ListenThread()
        {
            while (_listening)
            {
                if (_listener.Pending())
                {
                    TcpClient tcp = _listener.AcceptTcpClient();
                    new Thread(new ParameterizedThreadStart(ProcessClient)).Start(tcp);
                }
                Thread.Sleep(THREAD_WAIT);
            }
        }

        private void ProcessClient(object tcpClient)
        {
            Stopwatch timer = new();
            TcpClient client = tcpClient as TcpClient;

            timer.Start();
            // wait for connection string
            while (client.Available < Encoding.UTF8.GetByteCount(CONNECTION_STRING))
            {
                if (timer.ElapsedMilliseconds > CLIENT_TIMEOUT * 1000)
                {
                    // connection timed out
                    try
                    {
                        client.Client.Close();
                        client.Dispose();
                    }
                    catch { }
                    client = null;
                    timer.Stop();
                    timer = null;
                    OnClientFail?.Invoke(new TCPServerEventArgs()
                    {
                        Title = "Connection Timed Out",
                        Message = "Client failed to reply on time."
                    });
                    return;
                }
            }
            timer.Stop();

            // receive data
            byte[] conBuffer = new byte[Encoding.UTF8.GetByteCount(CONNECTION_STRING)];
            client.GetStream().Read(conBuffer, 0, conBuffer.Length);

            // check if it matches the connection string
            if (Encoding.UTF8.GetString(conBuffer) == CONNECTION_STRING)
            {
                // success

                // send connection string as well to inform client
                client.GetStream().Write(conBuffer, 0, conBuffer.Length);

                // raise event
                TCPClient tcp = new(client);
                OnClientConnect?.Invoke(new TCPServerEventArgs()
                {
                    Title = "Client Connected",
                    Message = "Client connected successfully.",
                    Client = tcp
                });
            }
            else
            {
                // fail
                try
                {
                    client.Client.Close();
                    client.Dispose();
                }
                catch { }
                client = null;
                timer = null;
                OnClientFail?.Invoke(new TCPServerEventArgs()
                {
                    Title = "Incorrect Message Data",
                    Message = "Client sent invalid initial connection data."
                });
                return;
            }
        }

        #region TCPServer.Dispose

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _listener.Stop();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _listener = null;
                _listenThread = null;
                _listening = false;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TCPServer()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion TCPServer.Dispose
    }
    public class TCPClient : INetClient
    {
        private const string CONNECTION_STRING = "GEngineConnect";
        private const int CLIENT_TIMEOUT = 10; //s

        private TcpClient _client;

        public int Available => _client.Available;
        public bool Connected => _client.Connected;

        internal TCPClient(TcpClient client, bool initiate = false)
        {
            _client = client;
            if (initiate)
            {
                Stopwatch timer = new();
                // send intial connection string
                byte[] conBuffer = Encoding.UTF8.GetBytes(CONNECTION_STRING);
                _client.GetStream().Write(conBuffer, 0, conBuffer.Length);

                // wait for reply connection string
                timer.Start();
                while (_client.Available < Encoding.UTF8.GetByteCount(CONNECTION_STRING))
                {
                    if (timer.ElapsedMilliseconds > CLIENT_TIMEOUT * 1000)
                    {
                        // timeout
                        try
                        {
                            _client.Client.Close();
                            _client.Dispose();
                        }
                        catch { }
                        _client = null;
                        timer.Stop();
                        timer = null;
                        throw new EngineException("Connection timed out while connecting to host.");
                    }
                }
                timer.Stop();
                byte[] receiveBuffer = new byte[Encoding.UTF8.GetByteCount(CONNECTION_STRING)];
                _client.GetStream().Read(receiveBuffer, 0, receiveBuffer.Length);

                if (Encoding.UTF8.GetString(receiveBuffer) != CONNECTION_STRING)
                {
                    // fail
                    try
                    {
                        _client.Client.Close();
                        _client.Dispose();
                    }
                    catch { }
                    _client = null;
                    throw new EngineException("Host sent invalid connection reply.");
                }
            }
        }

        public static TCPClient Connect(string hostname, ushort port)
        {
            TcpClient client = new();
            try
            {
                client.Connect(hostname, port);
            }
            catch (SocketException sEx)
            {
                Debug.Log("Could not connect to host.");
                throw new EngineException(sEx.Message);
            }
            catch (Exception)
            {
                Debug.Log("An error occurred in connecting to host.");
                throw new EngineException("Generic error.");
            }

            return new TCPClient(client, true);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (_client != null)
            {
                if (_client.Connected)
                {
                    _client.GetStream().Write(buffer, offset, count);
                }
            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (_client != null)
            {
                if (_client.Connected)
                {
                    return _client.GetStream().Read(buffer, offset, count);
                }
            }
            return 0;
        }

        #region TCPClient.Dispose

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _client.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _client.Dispose();
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TCPClient()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion TCPClient.Dispose
    }
}