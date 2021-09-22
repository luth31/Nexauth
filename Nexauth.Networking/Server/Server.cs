using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Nexauth.Networking {
    public class Server {
        public Server(ILogger<Server> logger, ServerOptions Options = null) {
            _logger = logger;
            if (Options != null) {
                // If IP is invalid fallback to localhost
                if (!Util.IsIPv4Valid(Options.Address))
                    Options.Address = "127.0.0.1";
                _options = Options;
            }
            else
                _options = new ServerOptions();
            _cancellationTokenSource = new CancellationTokenSource();
            
        }

        public void Start() {
            // Parse should be safe
            IPAddress address = IPAddress.Parse(_options.Address);
            _tcpListener = new TcpListener(address, _options.Port);
            try {
                _tcpListener.Start();
            } catch (SocketException e) {
                _logger.LogError($"Socket Exception: {e.Message}");
            }
            _logger.LogInformation($"Started listening on {address}:{_options.Port}");
            _ = StartAsyncSocketAcceptor(_cancellationTokenSource.Token);
        }

        public async Task StartAsyncSocketAcceptor(CancellationToken Token) {
            List<Socket> socketList = new List<Socket>();
            while (true) {
                if (Token.IsCancellationRequested) {
                    _logger.LogInformation($"Termination requested.");
                    return;
                }
                var socket = await _tcpListener.AcceptSocketAsync();
                if (socketList.Count < _options.MaxClients) {
                    socketList.Add(socket);
                HandleClientAsync(socket, Token);
                _logger.LogInformation($"Client connected!");
                }
                else {
                    _logger.LogInformation($"Client attempting connection but server is full!");
                    Console.WriteLine("Disconnecting");
                    socket.Close();
                }
            }
        }

        public void Stop() {
            _cancellationTokenSource.Cancel();
            _tcpListener.Stop();
        }

        public bool IsBound {
            get {
                return _tcpListener.Server.IsBound;
            }
        }

        private async void HandleClientAsync(Socket Socket, CancellationToken Token) {
            while (true) {
                if (Token.IsCancellationRequested) {
                    Socket.Close();
                    return;
                }
                // Handle data
                await Task.Delay(50);
            }
        }

        CancellationTokenSource _cancellationTokenSource;
        private ServerOptions _options;
        private TcpListener _tcpListener;
        private readonly ILogger<Server> _logger;
    }
}
