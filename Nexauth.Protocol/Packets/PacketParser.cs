using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Nexauth.Protocol.Packets {
    public class PacketParser {
        public PacketParser(ILogger<PacketParser> Logger) {
            _logger = Logger;
            _packetContainer = new Dictionary<ushort, PacketData>();
            RegisterDefaultPackets();
        }

        void RegisterPacket<T>(ushort Id) where T : Packet {
            if (!Enum.IsDefined(typeof(PacketId), Id)) {
                _logger.LogError($"Couldn't register packet ID {Id}, custom packets are not supported yet.");
            }
            RegisterPacket<T>((PacketId)Id);
        }

        void RegisterPacket<T>(PacketId Id) where T : Packet{
            ushort id = (ushort)Id;
            if (_packetContainer.ContainsKey(id)) {
                _logger.LogError($"Packet ID {id} already registered.");
            }
            PacketData pktData = new PacketData() { Id = Id, Class = typeof(T) };
            _packetContainer.Add(id, pktData);
        }

        void RegisterDefaultPackets() {
            RegisterPacket<ClientHello>(PacketId.CLIENT_HELLO);
        }

        private readonly ILogger<PacketParser> _logger;
        Dictionary<ushort, PacketData> _packetContainer;
    }
}