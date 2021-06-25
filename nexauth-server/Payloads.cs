using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Sockets;

namespace nexauth {
    public enum Opcodes : Int32 {
        NULL = 0,
        CLIENT_HELLO = 1,
        SERVER_HELLO = 2,
        CLIENT_SECURE_REQ = 3,
        SERVER_SECURE_RES = 4,
        CLIENT_SEND_PUBKEY = 5,
    }
    public static class Payload {
        public static T ReadAs<T>(NetworkStream stream) {
            byte[] size_bin = new byte[4];
            byte[] opcode_bin = new byte[4];
            stream.Read(size_bin, 0, 4);
            stream.Read(opcode_bin, 0, 4);
            Int32 size = BitConverter.ToInt32(size_bin);
            Opcodes opcode = (Opcodes)BitConverter.ToInt32(opcode_bin);
            byte[] payload_bin = new byte[size];
            stream.Read(payload_bin, 0, size);
            string payload_str = Encoding.ASCII.GetString(payload_bin);
            T payload = JsonSerializer.Deserialize<T>(payload_str);

            return payload;
        }
    }
    public class AbstractPayload {
        public Opcodes Opcode { get; set; } = Opcodes.NULL;

        public void Send(NetworkStream stream) {
            byte[] payload = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(this, this.GetType()));
            byte[] opcode = BitConverter.GetBytes((Int32)this.Opcode);
            byte[] size = BitConverter.GetBytes(payload.Length);
            stream.Write(size, 0, size.Length);
            stream.Write(opcode, 0, opcode.Length);
            stream.Write(payload, 0, payload.Length);
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

    public class CSecureReqPayload : AbstractPayload {
        public CSecureReqPayload() {
            Opcode = Opcodes.CLIENT_SECURE_REQ;
        }
    }

    public class SSecureResPayload : AbstractPayload {
        public SSecureResPayload(string pubKey) {
            Opcode = Opcodes.SERVER_SECURE_RES;
            this.publicKey = pubKey;
        }

        public SSecureResPayload() { }

        public string publicKey { get; set; }
    }

    public class CSendPubKeyPayload : AbstractPayload {
        public CSendPubKeyPayload(string pubKey) {
            Opcode = Opcodes.CLIENT_SEND_PUBKEY;
            this.publicKey = pubKey;
        }

        public CSendPubKeyPayload() { }

        public string publicKey { get; set; }
    }
}
