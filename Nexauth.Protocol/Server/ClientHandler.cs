using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nexauth.Protocol {
    public class ClientHandler {
        public ClientHandler(ILogger<ClientHandler> Logger) {
            _logger = Logger;
            Initialized = false;
        }

        public void Init(int Id, TcpClient Client, CancellationToken Token) {
            _id = Id;
            _client = Client;
            _ct = Token;
            Initialized = true;
        }
        
        public async void Handle() {
            if (!Initialized) {
                throw new InvalidOperationException("ClientHandler not initialized.");
            }
            while(true) {
               if (_ct.IsCancellationRequested) {
                    _client.Close();
                    _logger.LogInformation($"Cancellation requested. Disconnecting client {_id}");
                    return;
                }
                if (_client.Client.Poll(1000, SelectMode.SelectRead) && _client.Client.Available == 0) {
                    _logger.LogInformation($"Client {_id} disconnected.");
                    return;
                }
                if (_client.Available > 0) {
                    Console.WriteLine($"ToDo: Handle data. Available data: {_client.Available}");
                    byte[] data = new byte[_client.Available];
                    _client.GetStream().Read(data, 0, _client.Available);
                    string str = System.Text.Encoding.ASCII.GetString(data);
                    Console.WriteLine($"Received data from client {_id}: {str}");
                }
                await Task.Delay(100);
            }
        }

        private bool Initialized;
        private int _id;
        private readonly ILogger<ClientHandler> _logger;
        private CancellationToken _ct;
        private TcpClient _client;
    }
}