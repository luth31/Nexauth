using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Sockets;
using System.Threading;
using System;

namespace Nexauth.Protocol {
    public class SessionMgr {
        public SessionMgr(ILogger<SessionMgr> Logger, Func<ClientHandler> HandlerFactory){
            _logger = Logger;
            _handlerFactory = HandlerFactory;
            Initialized = false;
        }

        public void Init(CancellationToken Token) {
            _ct = Token;
            _sessions = new ConcurrentDictionary<int, ClientHandler>();
            _sessionCounter = 0;
            Initialized = true;
        }

        public void AddClient(TcpClient Client) {
            ClientHandler handler = new ClientHandler(_sessionCounter, Client, _ct);
            if (_sessions.TryAdd(_sessionCounter, handler)) {
                _logger.LogInformation($"Registered handler with Id {_sessionCounter}");
                _sessionCounter++;
            }
            handler.Handle();
        }

        public void ClientDisconnected(int Id) {
            if (_sessions.TryRemove(Id, out _))
                _logger.LogInformation($"Removed handler with Id {Id}");
        }

        public int SessionCount {
            get {
                return _sessions.Count;
            }
        }

        public bool Initialized { get; private set; }
        private CancellationToken _ct;
        private readonly ILogger<SessionMgr> _logger;
        private readonly Func<ClientHandler> _handlerFactory;
        int _sessionCounter;
        private ConcurrentDictionary<int, ClientHandler> _sessions;
    }
}