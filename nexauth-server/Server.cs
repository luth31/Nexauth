using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace nexauth_server {
    class Server {
        public Server() {
            listener = new Listener("127.0.0.1", 8300);
            provider = new RSACryptoServiceProvider(4096);
        }

        RSACryptoServiceProvider provider;
        private Listener listener;
    }
}
