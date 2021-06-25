using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using nexauth;

namespace nexauth_server {
    class Listener {
        public Listener(string address, Int32 port) {
            IPAddress addr;
            IPHostEntry entry = Dns.GetHostEntry(address);
            if (entry.AddressList.Length > 0)
                addr = entry.AddressList[0];
            else {
                if (!IPAddress.TryParse(address, out addr)) {
                    Console.WriteLine($"'{address}' cannot be parsed as domain or IP!");
                    Environment.Exit(-1);
                }
            }
            listener = new TcpListener(addr, port);
        }

        public void Start() {
            listener.Start();
            Console.WriteLine($"Listening on {listener.LocalEndpoint}");
            Console.WriteLine("Awaiting connection...");
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected!");
        }

        private readonly TcpListener listener;
    }
}
