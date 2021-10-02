using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Sockets;
using System.Threading;

namespace Nexauth.Protocol {
    public class SessionMgr {
        public SessionMgr(CancellationToken Token, ILogger<SessionMgr> Logger = null) {
            _ct = Token;
            _logger = new NullLogger<SessionMgr>();
            _sessions = new ConcurrentDictionary<int, ClientHandler>();
            _sessionCounter = 0;
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
        private CancellationToken _ct;
        private readonly ILogger<SessionMgr> _logger;
        int _sessionCounter;
        private ConcurrentDictionary<int, ClientHandler> _sessions;
    }
}