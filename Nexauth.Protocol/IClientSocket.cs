namespace Nexauth.Protocol {
    public interface IClientSocket : ISocket {
        void Connect(string Host, int Port);
    }
}