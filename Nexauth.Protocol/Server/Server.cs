using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Nexauth.Protocol {
    public class Server {
        public Server(ILogger<Server> Logger, IListenerSocket Listener,  ISessionManager SessionManager, ServerOptions Options) {
            _logger = Logger;
            _listener = Listener;
            // If IP is invalid fallback to localhost
            if (!Util.IsIPv4Valid(Options.Address))
                    Options.Address = "127.0.0.1";
            _options = Options;
            _sessionMgr = SessionManager;
            IsListening = false;
        }

        public void Start() {
            // Parse should be safe
            _cancellationTokenSource = new CancellationTokenSource();
            _sessionMgr.Init(_cancellationTokenSource.Token);
            try {
                _listener.Listen(_options.Address, _options.Port);
            } catch (SocketException e) {
                _logger.LogError($"SocketException while starting: {e.Message}");
                return;
            }
            _logger.LogInformation($"Started listening on {_options.Address}:{_options.Port}");
            IsListening = true;
            AcceptorLoop();
        }

        private async void AcceptorLoop() {
            while (true) {
                if (_cancellationTokenSource.IsCancellationRequested) {
                    _logger.LogInformation("Termination requested.");
                    return;
                }
                Socket client = null;
                try {
                    client = await _listener.AcceptSocketAsync();
                } catch (ObjectDisposedException) {
                    _logger.LogInformation("Object disposed.");
                    break;
                } catch (InvalidOperationException) {
                    _logger.LogWarning($"Attempting to accept sockets while Listener is not listening!");
                    return;
                } catch (SocketException e) {
                    _logger.LogError($"Listener SocketError: {e.Message}");
                    return;
                }
                _logger.LogInformation($"Max clients check. Connected: {_sessionMgr.SessionCount} Limit: {_options.MaxClients}");
                if (_sessionMgr.SessionCount >= _options.MaxClients) {
                    client.Close();
                    _logger.LogInformation("Server is full. Disconnecting...");
                }
                else {
                    _logger.LogInformation("Accepted connection.");
                    try {
                        _sessionMgr.RegisterClient(client);
                    } catch (InvalidOperationException e) {
                        _logger.LogError($"Couldn't add client to SessionMgr: {e.Message}");
                    }
                }
            }
        }

        public void Stop() {
            _cancellationTokenSource.Cancel();
            try {
                _listener.Close();
            } catch (SocketException e) {
                _logger.LogError($"Socket Exception while stopping: {e.Message}");
            } finally {
                IsListening = false;
            }
        }
        
        public bool IsBound {
            get {
                return _listener.IsBound;
            }
        }

        public bool IsListening { get; private set; }
        private CancellationTokenSource _cancellationTokenSource;
        private ServerOptions _options;
        private IListenerSocket _listener;
        private readonly ILogger<Server> _logger;
        private readonly ISessionManager _sessionMgr;
    }
}
