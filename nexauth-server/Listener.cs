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
            while (true) {
                Console.WriteLine("Awaiting CHelloPayload (Opcode 1)");
                CHelloPayload cpayload = Payload.ReadAs<CHelloPayload>(client.GetStream());
                Console.WriteLine($"Received payload! Opcode: {cpayload.Opcode}");
                Console.WriteLine("Sending SHelloPayload... (Opcode 2)");
                SHelloPayload payload = new SHelloPayload();
                payload.Send(client.GetStream());
                Console.WriteLine("Sent SHelloPayload! (Opcode 2)");
                CSecureReqPayload secure_req = Payload.ReadAs<CSecureReqPayload>(client.GetStream());
                string pubkey =
@"-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCbXmvJvECrf641tjoqwD214LAM
nLNj1tmE6L3MuTIeysRF/P8o1LuhQe9G3hhukwAhuL3opSRxIFmw4CHn++epwk7C
uW1ix0IRZMreOAVwMNP2a4hPNyRajblShNFTqL78mefqNoz3j1HHUxuk8qZLhPCo
vvfadeiEn/W99W5EqwIDAQAB
-----END PUBLIC KEY-----";
                Console.WriteLine($"Received payload CSecureReqPayload! {secure_req.ToString()}");
                SSecureResPayload secure_res = new SSecureResPayload(pubkey);
                Console.WriteLine($"Sending payload SSecureResPayload with key {secure_res.publicKey}");
                secure_res.Send(client.GetStream());
            }
        }

        private readonly TcpListener listener;
    }
}
