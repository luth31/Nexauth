using System;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace nexauth {
    public enum Opcodes : Int32 {
        NULL = 0,
        CLIENT_HELLO = 1,
        SERVER_HELLO = 2,
        CLIENT_BEGIN_SECURE = 3,
        SERVER_SEND_PUBKEY = 4,
        CLIENT_SEND_PUBKEY = 5,
        SERVER_SEND_AESKEY = 6,
        CLIENT_BEGIN_AUTH = 7,
        SERVER_AUTH_STANDBY = 8,
        SERVER_AUTH_STATE_CHANGED = 9
    }
    public class PayloadHelper {
        public static void SendPayload(byte[] payload, TcpClient client) {
            byte[] size = BitConverter.GetBytes(payload.Length);
            client.GetStream().Write(size, 0, size.Length);
            client.GetStream().Write(payload, 0, payload.Length);
        }

        public static async void SendPayloadAsync(byte[] payload, TcpClient client) {
            byte[] size = BitConverter.GetBytes(payload.Length);
            await client.GetStream().WriteAsync(size, 0, size.Length);
            await client.GetStream().WriteAsync(payload, 0, payload.Length);
        }

        public static byte[] ReadPayload(TcpClient client) {
            // Read payload size asynchronously
            byte[] size_buffer = new byte[4];
            client.GetStream().Read(size_buffer, 0, 4);
            // Convert buffer to Int32
            Int32 size = BitConverter.ToInt32(size_buffer);
            // Read payload asynchronously
            byte[] payload_buffer = new byte[size];
            client.GetStream().Read(payload_buffer, 0, size);
            // Return byte array
            return payload_buffer;
        }

        public static async Task<byte[]> ReadPayloadAsync(TcpClient client) {
            // Read payload size
            byte[] size_buffer = new byte[4];
            await client.GetStream().ReadAsync(size_buffer, 0, 4);
            // Convert buffer to Int32
            Int32 size = BitConverter.ToInt32(size_buffer);
            // Read payload asynchronously
            byte[] payload_buffer = new byte[size];
            await client.GetStream().ReadAsync(payload_buffer, 0, size);
            // Return byte array
            return payload_buffer;
        }

        public static void PrintPayload(Opcodes opcode) {
            if (Print)
                Console.WriteLine($"Sent payload {opcode}");
        }

        public static void PrintPayload<T>(T payload) {
            if (Print)
                Console.WriteLine($"Received payload {((AbstractPayload)(object)payload).Opcode}");
        }

        static bool Print = true;
    }

    public static class Payload {
        public static T ReadAs<T>(TcpClient client) {
            byte[] payload_buffer = PayloadHelper.ReadPayload(client);
            T payload = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload_buffer));
            PayloadHelper.PrintPayload(payload);
            return payload;
        }
        public async static Task<T> ReadAsyncAs<T>(TcpClient client) {
            byte[] payload_buffer = await PayloadHelper.ReadPayloadAsync(client);
            T payload = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload_buffer));
            PayloadHelper.PrintPayload(payload);
            return payload;
        }
    }
    public static class EncryptedPayload {
        public static T ReadAs<T>(TcpClient client, RSACryptoServiceProvider provider) {
            byte[] payload_buffer = PayloadHelper.ReadPayload(client);
            // Decrypt data using given provider
            byte[] payload_decrypted = provider.Decrypt(payload_buffer, false);
            // Convert and deserialize payload to T type
            T payload = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload_decrypted));
            PayloadHelper.PrintPayload(payload);
            return payload;
        }
        public async static Task<T> ReadAsyncAs<T>(TcpClient client, RSACryptoServiceProvider provider) {
            byte[] payload_buffer = await PayloadHelper.ReadPayloadAsync(client);
            // Decrypt data using given provider
            byte[] payload_decrypted = provider.Decrypt(payload_buffer, false);
            // Decode and deserialize payload to T type
            T payload = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload_decrypted));
            PayloadHelper.PrintPayload(payload);
            return payload;
        }

        public static T ReadAs<T>(TcpClient client, AESProvider provider) {
            byte[] payload_buffer = PayloadHelper.ReadPayload(client);
            // Decrypt data using given provider
            byte[] payload_decrypted = provider.Decrypt(payload_buffer);
            // Convert and deserialize payload to T type
            T payload = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload_decrypted));
            PayloadHelper.PrintPayload(payload);
            return payload;
        }
        public async static Task<T> ReadAsyncAs<T>(TcpClient client, AESProvider provider) {
            byte[] payload_buffer = await PayloadHelper.ReadPayloadAsync(client);
            // Decrypt data using given provider
            byte[] payload_decrypted = provider.Decrypt(payload_buffer);
            // Decode and deserialize payload to T type
            T payload = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload_decrypted));
            PayloadHelper.PrintPayload(payload);
            return payload;
        }
    }
    public class AbstractPayload {
        public Opcodes Opcode { get; set; } = Opcodes.NULL;

        public void Send(TcpClient client) {
            byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this, this.GetType()));
            PayloadHelper.PrintPayload(Opcode);
            PayloadHelper.SendPayload(payload, client);
        }

        public void SendAsync(TcpClient client) {
            byte[] payload = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(this, this.GetType()));
            PayloadHelper.PrintPayload(Opcode);
            PayloadHelper.SendPayloadAsync(payload, client);
        }

        public void SendEncrypted(TcpClient client, RSACryptoServiceProvider provider) {
            byte[] payload = provider.Encrypt(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this, this.GetType())), false);
            PayloadHelper.PrintPayload(Opcode);
            PayloadHelper.SendPayload(payload, client);
        }

        public void SendEncryptedAsync(TcpClient client, RSACryptoServiceProvider provider) {
            byte[] payload = provider.Encrypt(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this, this.GetType())), false);
            PayloadHelper.PrintPayload(Opcode);
            PayloadHelper.SendPayloadAsync(payload, client);
        }

        public void SendEncrypted(TcpClient client, AESProvider provider) {
            byte[] payload = provider.Encrypt(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this, this.GetType())));
            PayloadHelper.PrintPayload(Opcode);
            PayloadHelper.SendPayload(payload, client);
        }

        public void SendEncryptedAsync(TcpClient client, AESProvider provider) {
            byte[] payload = provider.Encrypt(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this, this.GetType())));
            PayloadHelper.PrintPayload(Opcode);
            PayloadHelper.SendPayloadAsync(payload, client);
        }

    };

    public class CHelloPayload : AbstractPayload {
        public CHelloPayload() {
            Opcode = Opcodes.CLIENT_HELLO;
        }
    }

    public class SHelloPayload : AbstractPayload {
        public SHelloPayload() {
            Opcode = Opcodes.SERVER_HELLO;
        }
    }

    public class CBeginSecurePayload : AbstractPayload {
        public CBeginSecurePayload() {
            Opcode = Opcodes.CLIENT_BEGIN_SECURE;
        }
    }

    public class SSendPubkeyPayload : AbstractPayload {
        public SSendPubkeyPayload(string pubKey = "") : this() {
            this.publicKey = pubKey;
        }

        public SSendPubkeyPayload() {
            Opcode = Opcodes.SERVER_SEND_PUBKEY;
        }

        public string publicKey { get; set; }
    }

    public class CSendPubkeyPayload : AbstractPayload {
        public CSendPubkeyPayload(string pubKey = "") : this() {
            this.publicKey = pubKey;
        }

        public CSendPubkeyPayload() {
            Opcode = Opcodes.CLIENT_SEND_PUBKEY;
        }

        public string publicKey { get; set; }
    }

    public class SSendAesKeyPayload : AbstractPayload {
        public SSendAesKeyPayload(byte[] key, byte[] nonce) : this() {
            this.key = key;
            this.nonce = nonce;
        }

        public SSendAesKeyPayload() {
            Opcode = Opcodes.SERVER_SEND_AESKEY;
        }

        public byte[] key { get; set; }
        public byte[] nonce { get; set; }
    }

    public class CBeginAuthPayload : AbstractPayload {
        public CBeginAuthPayload(string username = "") : this() {
            this.username = username;
        }

        public CBeginAuthPayload() {
            Opcode = Opcodes.CLIENT_BEGIN_AUTH;
        }

        public string username { get; set; }
    }

    public class SAuthStandbyPayload : AbstractPayload {
        public SAuthStandbyPayload() {
            Opcode = Opcodes.SERVER_AUTH_STANDBY;
        }
    }

    public class SAuthNewStatePayload : AbstractPayload {
        public SAuthNewStatePayload() {
            Opcode = Opcodes.SERVER_AUTH_STATE_CHANGED;
        }

        public bool success { get; set; }
        public string message { get; set; }
    }
}
