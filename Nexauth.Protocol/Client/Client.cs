using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Nexauth.Protocol {
    public class Client {
        public Client(ILogger<Client> Logger, ClientOptions Options = null) {
            if (Options != null) {
                _options = new ClientOptions();
            }
            else {
                _options = Options;
            }
            _cancellationTokenSource = new CancellationTokenSource();
            _client = new TcpClient();
        }

        public void Connect() {
            
        }

        public async Task HandleConnectionAsync(CancellationToken Token) {
            while (true) {
                await Task.Delay(100);
            }
        }

        CancellationTokenSource _cancellationTokenSource;
        TcpClient _client;
        ClientOptions _options;
    }
}