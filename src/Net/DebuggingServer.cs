using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;

using GEngine.Engine;
using System.Net;
using System.Diagnostics;

namespace GEngine.Net
{
    /*
     * Initial Connection Message
     *      [byte] message type
     *      [ushort] major
     *      [byte] minor
     *      [byte] title length
     *      [string?] str_title
     *      [byte] company length
     *      [string?] str_company
     *      [int] engineMaj
     *      [int] engineMin
     *      
     * Message Types
     *      200 = OK
     *      300 = Already attached
     *      120 = Busy
     *      130 = Engine already loaded
     *      100 = Force disconnect/client disconnect
     *      
     */
    public class DebuggingServer
    {
        private ushort _serverPort;
        private TcpListener _tcp;
        private Dictionary<string, (TcpClient, Thread)> _clients;
        private Thread _listenThread;
        private bool _listening;
        private GameEngine _game;

        // server info
        private ushort _major = 1;
        private byte _revision = 0;

        // constants
        private uint INITIAL_TIMEOUT = 10000; // ms

        public DebuggingServer(GameEngine game, ushort port = 7892)
        {
            if (game == null)
                throw new EngineException("Cannot create a debugging session for a null engine instance.");
            _game = game;
            _serverPort = port;
            _clients = new();
            _listening = false;
        }
        public void Initialize()
        {
            if (_tcp == null)
            {
                if (_serverPort != 0)
                {
                    _tcp = new(IPAddress.Any, _serverPort);
                }
                else
                    throw new EngineException("Port 0 cannot be used.", "DebuggingServer.Initialize()");
            }
            else
                throw new EngineException("Debugging server is already initialized.", "DebuggingServer.Initialize()");
        }
        public void Start()
        {
            if (_tcp == null)
                throw new EngineException("Debugging server not initialized.", "DebuggingServer.Start()");
            if (_listening)
                return;
            _listenThread = new(new ThreadStart(_listenerThreadWork));
            _listenThread.Name = "DebuggingServer Listener";
            _listenThread.Start();
        }
        public void Stop()
        {
            if (_listening)
            {
                _listening = false;
                _listenThread = null;
            }
        }
        private void _insert(ref byte[] src, ref byte[] data, int index)
        {
            if (index < 0 || index + data.Length >= src.Length)
                throw new ArgumentOutOfRangeException("Index is out-of-range.");
            for (int i = index; i < data.Length + index; ++i)
                src[i] = data[index - i];
        }
        private byte[] _packInfo(ushort vM, byte vN, string title, string comp)
        {
            if (title.Length > 255) title = title.Substring(0, 255);
            if (comp.Length > 255) comp = comp.Substring(0, 255);
            byte[] data = new byte[14 + title.Length + comp.Length];
            // message type = OK
            data[0] = 200;
            // version info
            Buffer.BlockCopy(data, 1, BitConverter.GetBytes(vM), 0, 2);
            Buffer.BlockCopy(data, 3, BitConverter.GetBytes(vN), 0, 1);
            // title
            byte[] titleEncoded = Encoding.UTF8.GetBytes(title);
            Buffer.BlockCopy(data, 4, BitConverter.GetBytes(title.Length), 0, 1);
            Buffer.BlockCopy(data, 5, titleEncoded, 0, titleEncoded.Length);
            // company
            byte[] compEncoded = Encoding.UTF8.GetBytes(comp);
            Buffer.BlockCopy(data, 5 + titleEncoded.Length, BitConverter.GetBytes(comp.Length), 0, 1);
            Buffer.BlockCopy(data, 6 + titleEncoded.Length, compEncoded, 0, compEncoded.Length);
            // engine version
            Buffer.BlockCopy(data, 6 + titleEncoded.Length + compEncoded.Length, BitConverter.GetBytes(Info.MajorVersion), 0, 4);
            Buffer.BlockCopy(data, 7 + titleEncoded.Length + compEncoded.Length, BitConverter.GetBytes(Info.MinorVersion), 0, 4);
            return data;
        }
        private void _listenerThreadWork()
        {
            Debug.Log("Debugger",$"Debugging server started on tcp port {_serverPort}.");
            _listening = true;
            _tcp.Start();
            while (_listening)
            {
                // listen for incoming connections
                if (_tcp.Pending())
                {
                    // accept client
                    TcpClient client = _tcp.AcceptTcpClient();

                    // retrieve the clients connecting ip
                    string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    Debug.Log("Debugger", $"Accepted connection from {ip}.");

                    // check if it exists in our client table
                    if (_clients.ContainsKey(ip))
                    {
                        // drop this client
                        Debug.Log("Debugger", $"Dropped {ip}, session from ip already exists.");
                        try
                        {
                            client.Dispose();
                        }
                        catch (ObjectDisposedException) { }
                        // listen for more
                        continue;
                    }

                    // since it does not exist, we need to accept this client
                    // so we need to send our info, then wait for a response
                    // we compare if the version is compatible.
                    // if so, we complete the connection. if not, we drop
                    byte[] data = _packInfo(_major, _revision, _game.Properties.Title, "n/a");
                    Stopwatch timeout = new();

                    // send data
                    client.GetStream().Write(data, 0, data.Length);
                    Debug.Log("Debugger", $"Sent server info to {ip}, waiting for response.");

                    // start timeout counter and wait for client response
                    timeout.Start();
                    while (!client.GetStream().DataAvailable)
                    {
                        if (timeout.ElapsedMilliseconds > INITIAL_TIMEOUT)
                        {
                            // client timed out
                            Debug.Log("Debugger", $"No response from {ip}, client timed out.");
                            try
                            {
                                client.Dispose();
                            }
                            catch (ObjectDisposedException) { }
                            continue;
                        }
                    }

                    // send success, we need to check for client response
                    ClientResponse response;
                    try
                    {
                        byte[] buffer = new byte[260];
                        client.GetStream().Read(buffer, 0, buffer.Length);
                        response = new ClientResponse(buffer);
                        Debug.Log("Debugger", $"Client response parsed.");
                    } catch (EngineException)
                    {
                        Debug.Log("Debugger", $"Insufficient reply from {ip}, client timed out.");
                        try
                        {
                            client.Dispose();
                        }
                        catch (ObjectDisposedException) { }
                        continue;
                    }
                    catch
                    {
                        Debug.Log("Debugger", $"General error on response parse. Client dropped.");
                        try
                        {
                            client.Dispose();
                        }
                        catch (ObjectDisposedException) { }
                        continue;
                    }
                    timeout.Reset();

                }
                else
                    Thread.Sleep(500);
            }
            _tcp.Stop();
            _tcp = null;
        }
    }
    public struct ClientResponse
    {
        readonly byte MessageType; //1
        readonly ushort VersionId; //2
        readonly string User; // 1 + 256
        // buffer allocation: 260 max

        public ClientResponse(byte[] data)
        {
            if (data.Length < 5)
                throw new EngineException("Insufficient packet data.", "ClientRepsonse()");
            MessageType = data[0];
            VersionId = BitConverter.ToUInt16(data, 1);
            User = Encoding.UTF8.GetString(data, 4, (int)data[3]);
        }
    }
}
