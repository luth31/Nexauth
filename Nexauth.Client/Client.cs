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
            if (client != null)
                client.Close();
        }

        public async void BeginAuth() {
            try {
                await Hello();
                await SecureChannel();
                await DoAuth();
            } catch (Exception e) {
                Debug.WriteLine($"Authentication failed! Reason: {e.Message}");
                if (client != null) {
                    client.Close();
                    client = null;
                }
            }
        }

        private async Task Hello() {
            CHelloPayload chello_payload = new CHelloPayload();
            chello_payload.SendAsync(client);
            SHelloPayload shello_payload = await Payload.ReadAsyncAs<SHelloPayload>(client);
        }

        private async Task SecureChannel() {
            CBeginSecurePayload csecure_payload = new CBeginSecurePayload();
            csecure_payload.SendAsync(client);
            parent.UpdateStatusText(parent.STATUS_LABEL, "Exchanging keys...", Brushes.BlueViolet);
            SSendPubkeyPayload spubkey_payload = await Payload.ReadAsyncAs<SSendPubkeyPayload>(client);
            cRSAProvider = new RSACryptoServiceProvider(4096);
            sRSAProvider = new RSACryptoServiceProvider();
            sRSAProvider.FromXmlString(spubkey_payload.publicKey);
            string pubkey = cRSAProvider.ToXmlString(false);
            CSendPubkeyPayload cpubkey1_payload = new CSendPubkeyPayload(pubkey[..(pubkey.Length / 2)]);
            CSendPubkeyPayload cpubkey2_payload = new CSendPubkeyPayload(pubkey[(pubkey.Length / 2)..]);
            cpubkey1_payload.SendEncryptedAsync(client, sRSAProvider);
            cpubkey2_payload.SendEncryptedAsync(client, sRSAProvider);
            SSendAesKeyPayload aes_payload = await EncryptedPayload.ReadAsyncAs<SSendAesKeyPayload>(client, cRSAProvider);
            aesProvider = new AESProvider(AESProvider.AES_KEY_SIZE.AES_KEY_128, aes_payload.key, aes_payload.nonce);
        }

        private async Task DoAuth() {
            CBeginAuthPayload cauth_payload = new CBeginAuthPayload(username);
            cauth_payload.SendEncryptedAsync(client, aesProvider);
            SAuthStandbyPayload sauth_payload = await EncryptedPayload.ReadAsyncAs<SAuthStandbyPayload>(client, aesProvider);
            parent.UpdateStatusText(parent.STATUS_LABEL, "Awaiting authentication...", Brushes.Orange);
            SAuthNewStatePayload slogged_payload = await EncryptedPayload.ReadAsyncAs<SAuthNewStatePayload>(client, aesProvider);
            if (slogged_payload.success) {
                parent.UpdateStatusText(parent.STATUS_LABEL, "Authenticated!", Brushes.Green);
                MessageBox.Show($"Authentication successful! ({slogged_payload.message})");
            }
            else {
                parent.UpdateStatusText(parent.STATUS_LABEL, "Unauthenticated", Brushes.Red);
                MessageBox.Show($"Authentication failed! Reason: {slogged_payload.message}");
            }
            client.Close();
            client = null;
        }

        private AESProvider aesProvider;
        private RSACryptoServiceProvider cRSAProvider;
        private RSACryptoServiceProvider sRSAProvider;
        private TcpClient client;
        private Int32 port;
        private MainWindow parent;
        private IPAddress address;
        private string username;
    }
}
