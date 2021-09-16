using System;

namespace Nexauth.Networking.Server {
    public class ServerOptions {
        public string Address { get; set; } = "127.0.0.1";

        public ushort Port { get; set; } = 8300;
    }
}
