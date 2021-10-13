using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Nexauth.Protocol.Tests {
    public static class Util {
        public static TcpClient ConnectClient(string Host, int Port) {
            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Parse(Host), Port);
            return client;
        }

        public static TcpClient[] ConnectClients(int Count, string Host, int Port) {
            TcpClient[] clients = new TcpClient[Count]; 
            for (int i = 0; i < Count; ++i) {
                clients[i] = new TcpClient();
                clients[i].Connect(IPAddress.Parse(Host), Port);
            }
            return clients;
        }

        public static void Disconnect(this TcpClient[] clients) {
            foreach (var client in clients)
                client.Close();
        }

        public static void Disconnect(this TcpClient client) {
            client.Close();
        }

        public static bool IsConnected(this TcpClient client) {
            return !(client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available > 0));
        }

        public static bool AreConnected(this TcpClient[] clients) {
            bool connected = true;
            for (int i = 0; i < clients.Length; ++i)
                connected = connected && !(clients[i].Client.Poll(1000, SelectMode.SelectRead) && (clients[i].Client.Available > 0));
            return connected;
        }

        public static ILoggerFactory GetLoggerFactory() {
            if (_factory == null) {
                var config = new NLog.Config.LoggingConfiguration();
                var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
                config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logconsole);
                NLog.LogManager.Configuration = config;
                _factory = new NLog.Extensions.Logging.NLogLoggerFactory();
            }
            return _factory;
        }

        static ILoggerFactory _factory;
    }
}