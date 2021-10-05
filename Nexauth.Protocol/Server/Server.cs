using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Nexauth.Protocol {
    public class Server {
        public Server(ILogger<Server> Logger, SessionMgr Session, ServerOptions Options) {
            _logger = Logger;
            // If IP is invalid fallback to localhost
            if (!Util.IsIPv4Valid(Options.Address))
                    Options.Address = "127.0.0.1";
            _options = Options;
            _sessionMgr = Session;
            IsListening = false;
        }

        public void Start() {
            // Parse should be safe
            IPAddress address = IPAddress.Parse(_options.Address);
            _tcpListener = new TcpListener(address, _options.Port);
            _cancellationTokenSource = new CancellationTokenSource();
            _sessionMgr.Init(_cancellationTokenSource.Token);
            try {
                _tcpListener.Start();
            } catch (SocketException e) {
                _logger.LogError($"SocketException while starting: {e.Message}");
                return;
            }
            _logger.LogInformation($"Started listening on {address}:{_options.Port}");
            IsListening = true;
            AcceptorLoop();
        }

        private async void AcceptorLoop() {
            while (true) {
                if (_cancellationTokenSource.IsCancellationRequested) {
                    _logger.LogInformation("Termination requested.");
                    return;
                }
                TcpClient client;
                try {
                    client = await _tcpListener.AcceptTcpClientAsync();
                } catch (InvalidOperationException) {
                    _logger.LogError($"Attempting to accept sockets while TcpListener is not listening!");
                    return;
                } catch (SocketException e) {
                    _logger.LogError($"Acceptor SocketError: {e.Message}");
                    return;
                }
                if (_sessionMgr.SessionCount >= _options.MaxClients) {
                    client.Close();
                    _logger.LogInformation("Server is full. Disconnecting...");
                }
                else {
                    _logger.LogInformation("Accepted connection.");
                    try {
                        _sessionMgr.AddClient(client);
                    } catch (InvalidOperationException e) {
                        _logger.LogError($"Couldn't add client to SessionMgr: {e.Message}");
                    }
                }
            }
        }

        public void Stop() {
            _cancellationTokenSource.Cancel();
            try {
                _tcpListener.Stop();
            } catch (SocketException e) {
                _logger.LogError($"Socket Exception while stopping: {e.Message}");
            } finally {
                IsListening = false;
            }
        }
        
        public bool IsBound {
            get {
                return _tcpListener.Server.IsBound;
            }
        }

        public bool IsListening { get; private set; }
        private CancellationTokenSource _cancellationTokenSource;
        private ServerOptions _options;
        private TcpListener _tcpListener;
        private readonly ILogger<Server> _logger;
        private readonly SessionMgr _sessionMgr;
    }
}
