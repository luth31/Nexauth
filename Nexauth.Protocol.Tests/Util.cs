using System.Net;
using System.Net.Sockets;

namespace Nexauth.Protocol.Tests {
    public static class Util {
        public static void ConnectClient(this TcpClient client, string Host, int Port) {
            client = new TcpClient();
            client.Connect(IPAddress.Parse(Host), Port);
        }

        public static void ConnectClients(this TcpClient[] clients, int Count, string Host, int Port) {
            clients = new TcpClient[Count]; 
            for (int i = 0; i < Count; ++i) {
                clients[i] = new TcpClient();
                clients[i].Connect(IPAddress.Parse(Host), Port);
            }
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
    }
}