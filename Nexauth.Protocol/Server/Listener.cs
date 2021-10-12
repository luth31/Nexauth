using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace Nexauth.Protocol {
    public class Listener : IListenerSocket {
        public Listener() {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public Task<Socket> AcceptSocketAsync() {
            return Task<Socket>.Factory.FromAsync(_socket.BeginAccept, _socket.EndAccept, null);
        }

        public void Listen(string Host, int Port) {

            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(Host), Port);
            _socket.Bind(endpoint);
            _socket.Listen();
        }

        public void Close() {
            _socket.Dispose();
        }

        public int Receive(byte[] Buffer) {
            return _socket.Receive(Buffer);
        }

        public int Send(byte[] Buffer) {
            return _socket.Send(Buffer);
        }

        public bool IsBound {
            get {
                return _socket.IsBound;
            }
        }

        private Socket _socket;
    }
}