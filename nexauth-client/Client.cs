using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Text.Json;
using System.Security.Cryptography;
using System.Windows;
using nexauth;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace nexauth_client {
    class Client {
        public Client(MainWindow parent) {
            this.parent = parent;
        }

        public async Task<bool> SetHost(string Address, Int32 Port) {
            IPAddress addr;
            parent.UpdateStatusText(parent.STATUS_LABEL, "Looking up host...", Brushes.Gray);
            parent.ClearStatusText(parent.HOST_STATUS_LABEL);
            try {
                IPHostEntry entry = await Dns.GetHostEntryAsync(Address);
                addr = entry.AddressList[0];
            }
            catch (System.Net.Sockets.SocketException e) {
                parent.UpdateStatusText(parent.HOST_STATUS_LABEL, "Invalid host!", Brushes.Red);
                return false;
            }
            this.address = addr;
            this.port = Port;
            return true;
        }

        public bool SetUsername(string username) {
            parent.ClearStatusText(parent.USERNAME_STATUS_LABEL);
            Regex reg = new Regex("^[a-zA-Z0-9]*$");
            if (reg.IsMatch(username))
                this.username = username;
            else {
                parent.UpdateStatusText(parent.USERNAME_STATUS_LABEL, "Invalid username!", Brushes.Red);
                return false;
            }
            return true;
        }

        public void Connect() {
            if (client != null)
                return;
            client = new TcpClient();
            client.Connect(address, port);

        }

        public void Disconnect() {
            client.Close();
        }

        public async void BeginSecureConnection() {
            CHelloPayload chello_payload = new CHelloPayload();
            chello_payload.SendAsync(client);
            Debug.WriteLine("Sent CHelloPayload");
            SHelloPayload shello_payload = await Payload.ReadAsyncAs<SHelloPayload>(client);
            Debug.WriteLine("Received SHelloPayload");
            CBeginSecurePayload csecure_payload = new CBeginSecurePayload();
            csecure_payload.SendAsync(client);
            Debug.WriteLine("Sent CBeginSecurePayload");
            SSendPubkeyPayload spubkey_payload = await Payload.ReadAsyncAs<SSendPubkeyPayload>(client);
            Debug.WriteLine($"Received SSendPubkeyPayload pubkey {spubkey_payload.publicKey}");
            cRSAProvider = new RSACryptoServiceProvider(4096);
            sRSAProvider = new RSACryptoServiceProvider();
            sRSAProvider.FromXmlString(spubkey_payload.publicKey);
            string pubkey = cRSAProvider.ToXmlString(false);
            Debug.WriteLine($"RSA key: {pubkey}\nRSA key size: {pubkey.Length}");
            CSendPubkeyPayload cpubkey1_payload = new CSendPubkeyPayload(pubkey[..(pubkey.Length/2)]);
            CSendPubkeyPayload cpubkey2_payload = new CSendPubkeyPayload(pubkey[(pubkey.Length/2)..]);
            cpubkey1_payload.SendEncryptedAsync(client, sRSAProvider);
            cpubkey2_payload.SendEncryptedAsync(client, sRSAProvider);
            Debug.WriteLine("Sent CSendPubkeyPayload");
            // TODO: Await for challenge and complete it
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

        private RSACryptoServiceProvider cRSAProvider;
        private RSACryptoServiceProvider sRSAProvider;
        private TcpClient client;
        private Int32 port;
        private MainWindow parent;
        private IPAddress address;
        private string username;
    }
}
