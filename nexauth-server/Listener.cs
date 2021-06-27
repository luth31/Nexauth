using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using nexauth;
using System.IO;
using System.IO.Compression;

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
            Console.WriteLine("Generating RSA 4096-bit key pair...");
            sProvider = new RSACryptoServiceProvider(4096);
            cProvider = new RSACryptoServiceProvider();
            Console.WriteLine("Keypair generated!");
        }

        public async void StartAsync() {
            listener.Start();
            Console.WriteLine($"Listening on {listener.LocalEndpoint}");
            Console.WriteLine("Awaiting connection...");
            while (true) {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected!");
                HandleClientAuth(client);
            }
        }

        private async void HandleClientAuth(TcpClient client) {
            try {
                Console.WriteLine("Awaiting CHelloPayload");
                CHelloPayload chello_payload = await Payload.ReadAsyncAs<CHelloPayload>(client);
                Console.WriteLine("Received CHelloPayload");
                SHelloPayload shello_payload = new SHelloPayload();
                shello_payload.SendAsync(client);
                Console.WriteLine("Sent SHelloPayload");
                CBeginSecurePayload csecure_payload = await Payload.ReadAsyncAs<CBeginSecurePayload>(client);
                SSendPubkeyPayload spubkey_payload = new SSendPubkeyPayload(sProvider.ToXmlString(false));
                spubkey_payload.SendAsync(client);
                CSendPubkeyPayload cpubkey1_payload = await EncryptedPayload.ReadAsyncAs<CSendPubkeyPayload>(client, sProvider);
                CSendPubkeyPayload cpubkey2_payload = await EncryptedPayload.ReadAsyncAs<CSendPubkeyPayload>(client, sProvider);
                string cpubkey = String.Concat(cpubkey1_payload.publicKey, cpubkey2_payload.publicKey);
                Console.WriteLine($"Received encrypted public key from client");
                cProvider = new RSACryptoServiceProvider();
                cProvider.FromXmlString(cpubkey);
                RNGCryptoServiceProvider cprng = new RNGCryptoServiceProvider();
                byte[] key = new byte[16];
                byte[] nonce = new byte[8];
                cprng.GetBytes(key);
                cprng.GetBytes(nonce);
                aesProvider = new AESProvider(AESProvider.AES_KEY_SIZE.AES_KEY_128, key, nonce);
                SSendAesKeyPayload aes_payload = new SSendAesKeyPayload(key, nonce);
                aes_payload.SendEncryptedAsync(client, cProvider);
                Console.WriteLine($"Sent SSendAesKeyPayload");
                CBeginAuthPayload cauth_payload = await EncryptedPayload.ReadAsyncAs<CBeginAuthPayload>(client, aesProvider);
                Console.WriteLine($"Received CBeginAuthPayload");
                SAuthStandbyPayload sauth_payload = new SAuthStandbyPayload();
                sauth_payload.SendEncryptedAsync(client, aesProvider);
                Console.WriteLine($"Sent SAuthStandbyPayload");
                Console.WriteLine($"Simulating authentication: Sleep 5000");
                await Task.Delay(5000);
                SAuthSuccessPayload slogged_payload = new SAuthSuccessPayload();
                slogged_payload.SendEncryptedAsync(client, aesProvider);
                Console.WriteLine($"User authenticated! Closing connection.");
                client.Close();
            }
            catch (Exception e){
                Console.WriteLine($"Failed client authentication. Reason: {e.Message}");
            }
        }

        public static byte[] Compress(byte[] data) {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal)) {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data) {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress)) {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

        private AESProvider aesProvider;
        private RSACryptoServiceProvider cProvider;
        private readonly RSACryptoServiceProvider sProvider;
        private readonly TcpListener listener;
    }
}
