using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nexauth.Protocol {
    public class ClientHandler : ISessionHandler {
        public ClientHandler(ILogger<ClientHandler> Logger) {
            _logger = Logger;
            Initialized = false;
        }

        public void Init(int Id, Socket Client, CancellationToken Token) {
            _id = Id;
            _client = Client;
            _ct = Token;
            Initialized = true;
        }
        
        public async void Execute() {
            if (!Initialized) {
                throw new InvalidOperationException("ClientHandler not initialized.");
            }
            while(true) {
               if (_ct.IsCancellationRequested) {
                    _client.Close();
                    _logger.LogTrace($"Cancellation requested. Client {_id} disconnected.");
                    return;
                }
                if (IsDisconnected()) {
                    _logger.LogInformation($"Client {_id} disconnected.");
                    // Announce to SessionManager
                    return;
                }
                if (IsDataAvailable()) {
                    // Packet packet = _parser.Parse();
                    // _handler.Handle(packet);
                }
                await Task.Delay(100);
            }
        }

        public bool IsDisconnected() {
            bool disconnected = true;
            try {
                disconnected = _client.Poll(1000, SelectMode.SelectRead) && _client.Available == 0;
            } catch (SocketException e) {
                _logger.LogError($"SocketError while checking connection status: {e.Message}");
            } catch (ObjectDisposedException) {
                _logger.LogWarning($"Attempting to check connection status of disposed socket!");
            }
            return disconnected;
        }

        public bool IsDataAvailable() {
            bool available = false;
            try {
                available = _client.Available > 0;
            } catch (SocketException e) {
                _logger.LogError($"SocketError while checking available data: {e.Message}");
            } catch (ObjectDisposedException) {
                _logger.LogWarning($"Attempting to check available data of disposed socket!");
            }
            return available;
        }

        private const int MaxPacketSize = 1000;
        private bool Initialized;
        private int _id;
        private readonly ILogger<ClientHandler> _logger;
        private CancellationToken _ct;
        private Socket _client;
    }
}