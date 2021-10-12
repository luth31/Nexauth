namespace Nexauth.Protocol.Packets {
    [Packet(1, PacketType.CLIENT_PACKET)]
    class ClientHello {
        public string Version { get; set; }
    }
}