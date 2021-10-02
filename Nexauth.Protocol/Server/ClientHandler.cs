using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nexauth.Protocol {
    public class ClientHandler {
        public ClientHandler(int Id, TcpClient Client, CancellationToken Token) {
            _logger = new NullLogger<ClientHandler>();
            _id = Id;
            _client = Client;
            _ct = Token;
        }
        
        public async void Handle() {
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

        public readonly ILogger<ClientHandler> _logger;
        int _id;
        CancellationToken _ct;
        TcpClient _client;
    }
}