using System.Threading;
using System.Net.Sockets;

namespace Nexauth.Protocol {
    public interface ISessionHandler {
        void Init(int Id, Socket client, CancellationToken Token);
        void Execute();
        bool IsDisconnected();
    }
}