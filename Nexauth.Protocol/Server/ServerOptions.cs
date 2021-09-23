using System;

namespace Nexauth.Protocol {
    public class ServerOptions {
        public string Address { get; set; } = "127.0.0.1";

        public ushort Port { get; set; } = 8300;
        public int MaxClients { get; set; } = 100;
    }
}
