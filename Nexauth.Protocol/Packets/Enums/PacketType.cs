using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Nexauth.Protocol.Packets {
    public enum PacketType {
        CLIENT_PACKET,
        SERVER_PACKET,
    }
}