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
using dotAPNS;
using System.Net.Http;

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
            cprng = new RNGCryptoServiceProvider();
            APNSProvider = ApnsClient.CreateUsingCert("aps.p12").UseSandbox();
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
                Console.WriteLine("Closing connection!");
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
                slogged_payload.SendEncryptedAsync(client, aesProvider);
                client.Close();
                Console.WriteLine("Closing connection!");
                return;
            }
            CreateAuthRequest(user);
            bool success = await CheckAuthConfirmation(user);
            if (!success) {
                slogged_payload.success = false;
                slogged_payload.message = "Authentication timed out!";
            } else {
                slogged_payload.success = true;
                slogged_payload.message = "You are authenticated!";
                Console.WriteLine("Request has been signed!");
            }
            slogged_payload.SendEncryptedAsync(client, aesProvider);
            client.Close();
            Console.WriteLine("Closing connection!");
        }

        private async Task<bool> CheckAuthConfirmation(User user) {
            int retries = 0;
            while (retries < 30) {
                using (var authContext = CreateDbContext()) {
                    var authReq = authContext.AuthRequests.Where(r => r.UserId == user.Id).FirstOrDefault();
                    if (authReq == null)
                        return false;
                    if (authReq.Completed)
                        return true;
                }
                Console.WriteLine($"Request for '{user.Username}' not signed yet! Rechecking in 2 seconds... ({retries+1})");
                ++retries;
                await Task.Delay(2000);
            }
            Console.WriteLine("Request was not signed in 60 seconds. Timing out authentication...");
            return false;
        }

        private void CreateAuthRequest(User user) {
            var challenge = CreateChallenge();
            using (var authContext = CreateDbContext()) {
                var outdated = authContext.AuthRequests.Where(r => r.UserId == user.Id).ToList();
                foreach (var entry in outdated) {
                    authContext.Remove(entry);
                }
                var request = new AuthRequest { UserId = user.Id, Challenge = challenge, Completed = false };
                authContext.AuthRequests.Add(request);
                authContext.SaveChanges();
            }
            Console.WriteLine($"Created authentication request for user {user.Username}. Challenge: {challenge}");
            SendNotification(user);
        }

        private async void SendNotification(User user) {
            var push = new ApplePush(ApplePushType.Alert)
                .AddAlert("Authentication", "Authentication required!")
                .AddToken(user.Token);
            try {
                var response = await APNSProvider.SendAsync(push);
                if (response.IsSuccessful)
                    Console.WriteLine("[APNS] Notification sent to authenticator!");
                else {
                    switch (response.Reason) {
                        case ApnsResponseReason.BadCertificateEnvironment:
                            Console.WriteLine("[APNS] Certificate is for wrong environment!");
                            break;
                        default:
                            Console.WriteLine($"[APNS] Failed: {response.ReasonString}");
                            break;
                    }
                }
            } catch (TaskCanceledException) {
                Console.WriteLine("[APNS] HTTP request timed out!");
            } catch (HttpRequestException ex) {
                Console.WriteLine($"[APNS] HTTP request failed {ex.Message}!");
            } catch (ApnsCertificateExpiredException) {
                Console.WriteLine("[APNS] Certificate expired!");
            }
        }
        private string CreateChallenge() {
            byte[] buffer = new byte[256];
            cprng.GetBytes(buffer);
            return Convert.ToBase64String(buffer); ;
        }

        private User GetUser(string username) {
            using (var authContext = CreateDbContext()) {
                return authContext.User.Where(u => u.Username == username).FirstOrDefault();
            }
        }

        private AuthContext CreateDbContext() {
            var optionsBuilder = new DbContextOptionsBuilder<AuthContext>();
            optionsBuilder.UseInMemoryDatabase("AuthContext");
            return new AuthContext(optionsBuilder.Options);
        }

        private ApnsClient APNSProvider;
        private AESProvider aesProvider;
        RNGCryptoServiceProvider cprng;
        private RSACryptoServiceProvider cProvider;
        private readonly RSACryptoServiceProvider sProvider;
        private readonly TcpListener listener;
    }
}
