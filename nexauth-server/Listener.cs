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
using nexauth_server.Models;
using Microsoft.EntityFrameworkCore;

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
            var optionsBuilder = new DbContextOptionsBuilder<AuthContext>();
            optionsBuilder.UseInMemoryDatabase("AuthContext");
            authContext = new AuthContext(optionsBuilder.Options);
            cprng = new RNGCryptoServiceProvider();
            Console.WriteLine("Keypair generated!");
        }

        public async void StartAsync() {
            listener.Start();
            Console.WriteLine($"Listening on {listener.LocalEndpoint}");
            Console.WriteLine("Awaiting connection...");
            while (true) {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected!");
                AuthClient(client);
            }
        }

        private async void AuthClient(TcpClient client) {
            try {
                await HandleHello(client);
                await HandleSecureChannel(client);
                await HandleDoAuth(client);
            }
            catch (Exception e){
                Console.WriteLine($"Failed client authentication. Reason: {e.Message}");
                client.Close();
            }
        }

        private async Task HandleHello(TcpClient client) {
            CHelloPayload chello_payload = await Payload.ReadAsyncAs<CHelloPayload>(client);
            SHelloPayload shello_payload = new SHelloPayload();
            shello_payload.SendAsync(client);
        }

        private async Task HandleSecureChannel(TcpClient client) {
            CBeginSecurePayload csecure_payload = await Payload.ReadAsyncAs<CBeginSecurePayload>(client);
            SSendPubkeyPayload spubkey_payload = new SSendPubkeyPayload(sProvider.ToXmlString(false));
            spubkey_payload.SendAsync(client);
            CSendPubkeyPayload cpubkey1_payload = await EncryptedPayload.ReadAsyncAs<CSendPubkeyPayload>(client, sProvider);
            CSendPubkeyPayload cpubkey2_payload = await EncryptedPayload.ReadAsyncAs<CSendPubkeyPayload>(client, sProvider);
            string cpubkey = String.Concat(cpubkey1_payload.publicKey, cpubkey2_payload.publicKey);
            cProvider = new RSACryptoServiceProvider();
            cProvider.FromXmlString(cpubkey);
            byte[] key = new byte[16];
            byte[] nonce = new byte[8];
            cprng.GetBytes(key);
            cprng.GetBytes(nonce);
            aesProvider = new AESProvider(AESProvider.AES_KEY_SIZE.AES_KEY_128, key, nonce);
            SSendAesKeyPayload aes_payload = new SSendAesKeyPayload(key, nonce);
            aes_payload.SendEncryptedAsync(client, cProvider);
        }

        private async Task HandleDoAuth(TcpClient client) {
            CBeginAuthPayload cauth_payload = await EncryptedPayload.ReadAsyncAs<CBeginAuthPayload>(client, aesProvider);
            SAuthStandbyPayload sauth_payload = new SAuthStandbyPayload();
            sauth_payload.SendEncryptedAsync(client, aesProvider);
            SAuthNewStatePayload slogged_payload = new SAuthNewStatePayload();
            var user = GetUser(cauth_payload.username);
            if (user == null) {
                slogged_payload.success = false;
                slogged_payload.message = "User doesn't exist!";
            }
            /*else if (!await AwaitAuthentication()) {
                slogged_payload.success = false;
                slogged_payload.message = "Authentication timed out!";
            }*/
            else {
                CreateAuthRequest(user);
                slogged_payload.success = false;
                slogged_payload.message = "WIP";
            }
            slogged_payload.SendEncryptedAsync(client, aesProvider);
            client.Close();
        }

        private void CreateAuthRequest(User user) {
            var challenge = CreateChallenge();
            var outdated = authContext.AuthRequests.Where(r => r.UserId == user.Id).ToList();
            foreach (var entry in outdated) {
                authContext.Remove(entry);
            }
            var request = new AuthRequest { UserId = user.Id, Challenge = challenge, Completed = false };
            authContext.AuthRequests.Add(request);
            authContext.SaveChanges();
            Console.WriteLine($"Created authentication request for user {user.Username}. Challenge: {challenge}");
        }

        private string CreateChallenge() {
            byte[] buffer = new byte[128];
            cprng.GetBytes(buffer);
            return Convert.ToBase64String(buffer); ;
        }

        private User GetUser(string username) {
            return authContext.User.Where(u => u.Username == username).FirstOrDefault();
        }

        private AuthContext authContext;
        private AESProvider aesProvider;
        RNGCryptoServiceProvider cprng;
        private RSACryptoServiceProvider cProvider;
        private readonly RSACryptoServiceProvider sProvider;
        private readonly TcpListener listener;
    }
}
