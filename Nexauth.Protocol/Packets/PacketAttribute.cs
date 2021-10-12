namespace Nexauth.Protocol.Packets {
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class PacketAttribute: System.Attribute {
        public PacketAttribute(int Id, PacketType Type, EncryptionType Encryption = EncryptionType.ENCRYPTION_NONE) {
            this.Id = Id;
            this.Type = Type;
            this.Encryption = Encryption;
        }
        public int Id { get; }
        public PacketType Type { get; }
        public EncryptionType Encryption { get; }

    }
}