using System;

namespace Nexauth.Protocol.Packets {
    public class PacketMetadata {
        public int Id;
        public Type Class;
        public EncryptionType Encryption;
        public PacketType Type;
    }
}