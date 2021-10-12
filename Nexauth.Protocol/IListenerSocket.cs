using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;

namespace Nexauth.Protocol {
    public interface IListenerSocket : ISocket {
        Task<Socket> AcceptSocketAsync();
        void Listen(string Host, int Port);
        bool IsBound { get; }
    }
}