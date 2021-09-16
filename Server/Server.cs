using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Nexauth.Networking.Server {
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
        }

        public async void Start() {
            // Parse should be safe
            IPAddress address = IPAddress.Parse(_options.Address);
            _tcpListener = new TcpListener(address, _options.Port);
            try {
                _tcpListener.Start();
            } catch (SocketException e) {
                _logger.LogError($"Socket Exception: {e.Message}");
            }
            _logger.LogInformation($"Started listening on {address}:{_options.Port}");
            while (true) {
                var client = await _tcpListener.AcceptSocketAsync();
                _logger.LogInformation($"Client connected!");
            }
        }

        public void Stop() {
            _tcpListener.Stop();
        }
        private ServerOptions _options;
        private TcpListener _tcpListener;
        private readonly ILogger<Server> _logger;
    }
}
