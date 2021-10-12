using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net.Sockets;
using System.Threading;
using System;

namespace Nexauth.Protocol {
    public class SessionManager : ISessionManager {
        public SessionManager(ILogger<ISessionManager> Logger, Func<ClientHandler> HandlerFactory){
            _logger = Logger;
            _handlerFactory = HandlerFactory;
            _sessions = new ConcurrentDictionary<int, ISessionHandler>();
            _sessionCounter = 0;
            Initialized = false;
        }

        public void Init(CancellationToken Token) {
            _ct = Token;
            Initialized = true;
        }

        public void RegisterClient(Socket Client) {
            ThrowIfUninitialized();
            ISessionHandler handler = _handlerFactory.Invoke();
            handler.Init(_sessionCounter, Client, _ct);
            if (_sessions.TryAdd(_sessionCounter, handler)) {
                _logger.LogInformation($"Registered handler with Id {_sessionCounter}");
                _sessionCounter++;
            }
            else {
                Console.WriteLine("");
            }
            handler.Execute();
        }

        public void Disconnect(int Id) {
            ThrowIfUninitialized();
            ISessionHandler handler;
            if (_sessions.TryRemove(Id, out handler)) {
                //handler.Disconnect();
                _logger.LogInformation($"Disconnected client with handler Id {Id}");
            }
            else {
                _logger.LogWarning($"Client with handler Id {Id} does not exist!");
            }
        }

        private void ThrowIfUninitialized() {
            if (!Initialized)
                throw new InvalidOperationException("SessionManager not initialized");
        }

        public int SessionCount {
            get {
                return Initialized ? _sessions.Count : 0;
            }
        }

        public bool Initialized { get; private set; }
        private CancellationToken _ct;
        private readonly ILogger<ISessionManager> _logger;
        private readonly Func<ISessionHandler> _handlerFactory;
        private int _sessionCounter;
        private ConcurrentDictionary<int, ISessionHandler> _sessions;
    }
}