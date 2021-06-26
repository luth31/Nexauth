using System;
using System.Collections.Generic;
using System.Linq;
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
        SERVER_CHALLENGE = 6,
        CLIENT_RESPONSE = 7
    }
    public static class Payload {
        public static T ReadAs<T>(TcpClient client) {
            // Read payload size
            byte[] size_buffer = new byte[4];
            client.GetStream().Read(size_buffer, 0, 4);
            // Convert buffer to Int32
            Int32 size = BitConverter.ToInt32(size_buffer);
            // Read payload synchronously
            byte[] payload_buffer = new byte[size];
            client.GetStream().Read(payload_buffer, 0, size);
            // Convert and deserialize payload to T type
            T payload = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload_buffer));
            return payload;
        }
        public async static Task<T> ReadAsyncAs<T>(TcpClient client) {
            // Read payload size
            byte[] size_buffer = new byte[4];
            await client.GetStream().ReadAsync(size_buffer, 0, 4);
            // Convert buffer to Int32
            Int32 size = BitConverter.ToInt32(size_buffer);
            // Read payload asynchronously
            byte[] payload_buffer = new byte[size];
            await client.GetStream().ReadAsync(payload_buffer, 0, size);
            // Decode and deserialize payload to T type
            T payload = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload_buffer));
            return payload;
        }
    }
    public static class EncryptedPayload {
        public static T ReadAs<T>(TcpClient client, RSACryptoServiceProvider provider) {
            // Read payload size
            byte[] size_buffer = new byte[4];
            client.GetStream().Read(size_buffer, 0, 4);
            // Convert buffer to Int32
            Int32 size = BitConverter.ToInt32(size_buffer);
            // Read payload synchronously
            byte[] payload_buffer = new byte[size];
            client.GetStream().Read(payload_buffer, 0, size);
            // Decrypt data using given provider
            byte[] payload_decrypted = provider.Decrypt(payload_buffer, false);
            // Convert and deserialize payload to T type
            T payload = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload_decrypted));
            return payload;
        }
        public async static Task<T> ReadAsyncAs<T>(TcpClient client, RSACryptoServiceProvider provider) {
            // Read payload size
            byte[] size_buffer = new byte[4];
            await client.GetStream().ReadAsync(size_buffer, 0, 4);
            // Convert buffer to Int32
            Int32 size = BitConverter.ToInt32(size_buffer);
            // Read payload asynchronously
            byte[] payload_buffer = new byte[size];
            await client.GetStream().ReadAsync(payload_buffer, 0, size);
            // Decrypt data using given provider
            byte[] payload_decrypted = provider.Decrypt(payload_buffer, false);
            // Decode and deserialize payload to T type
            T payload = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(payload_decrypted));
            return payload;
        }
    }
    public class AbstractPayload {
        public Opcodes Opcode { get; set; } = Opcodes.NULL;

        public void Send(TcpClient client) {
            byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this, this.GetType()));
            byte[] size = BitConverter.GetBytes(payload.Length);
            client.GetStream().WriteAsync(size, 0, size.Length);
            client.GetStream().Write(payload, 0, payload.Length);
        }

        public async void SendAsync(TcpClient client) {
            byte[] payload = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(this, this.GetType()));
            byte[] size = BitConverter.GetBytes(payload.Length);
            await client.GetStream().WriteAsync(size, 0, size.Length);
            await client.GetStream().WriteAsync(payload, 0, payload.Length);
        }

        public void SendEncrypted(TcpClient client, RSACryptoServiceProvider provider) {
            byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this, this.GetType()));
            byte[] payload_encrypted = provider.Encrypt(payload, false);
            byte[] size = BitConverter.GetBytes(payload_encrypted.Length);
            client.GetStream().Write(size, 0, size.Length);
            client.GetStream().Write(payload_encrypted, 0, payload_encrypted.Length);
        }

        public async void SendEncryptedAsync(TcpClient client, RSACryptoServiceProvider provider) {
            byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this, this.GetType()));
            byte[] payload_encrypted = provider.Encrypt(payload, false);
            byte[] size = BitConverter.GetBytes(payload_encrypted.Length);
            await client.GetStream().WriteAsync(size, 0, size.Length);
            await client.GetStream().WriteAsync(payload_encrypted, 0, payload_encrypted.Length);
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
        public SSendPubkeyPayload(string pubKey = "") {
            Opcode = Opcodes.SERVER_SEND_PUBKEY;
            this.publicKey = pubKey;
        }

        public SSendPubkeyPayload() { }

        public string publicKey { get; set; }
    }

    public class CSendPubkeyPayload : AbstractPayload {
        public CSendPubkeyPayload(string pubKey = "") {
            Opcode = Opcodes.CLIENT_SEND_PUBKEY;
            this.publicKey = pubKey;
        }

        public CSendPubkeyPayload() { }

        public string publicKey { get; set; }
    }
}
