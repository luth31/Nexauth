using System;

namespace Nexauth.Networking.Packets {
    public enum Opcode : ushort {
        NULL = 0,
        CLIENT_HELLO = 1,
        SERVER_HELLO = 2,
        CLIENT_BEGIN_SECURE = 3,
        SERVER_SEND_PUBKEY = 4,
        CLIENT_SEND_PUBKEY = 5,
        SERVER_SEND_AESKEY = 6,
        CLIENT_BEGIN_AUTH = 7,
        SERVER_AUTH_STANDBY = 8,
        SERVER_AUTH_STATE_CHANGED = 9,
    }
}
