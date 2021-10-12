using System.Threading;
using System.Net.Sockets;

namespace Nexauth.Protocol {
    public interface ISessionManager {
        void Init(CancellationToken Token);
        void RegisterClient(Socket Client);
        void Disconnect(int Id);
        int SessionCount { get; }
        bool Initialized { get; }
    }
}