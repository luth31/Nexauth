namespace Nexauth.Protocol {
    public interface ISocket {
        void Close();
        int Receive(byte[] Buffer);
        int Send(byte[] Buffer);
    }
}