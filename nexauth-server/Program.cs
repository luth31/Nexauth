using System;

namespace nexauth_server {
    class Program {
        static void Main(string[] args) {
            Listener listener = new Listener("127.0.0.1", 8300);
            listener.Start();
        }
    }
}
