using Xunit;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Nexauth.Protocol.Tests {
    public class ListenerTest {
        const string Host = "127.0.0.1";
        const int Port = 8000;
        public ListenerTest() {
            _listener = new Listener();
        }

        [Fact]
        public void IsBound_Listening_ReturnsTrue() {
            // Arrange
            _listener.Listen(Host, Port);

            // Act
            bool isBound = _listener.IsBound;
            _listener.Close();

            // Assert
            Assert.True(isBound);
        }

        [Fact]
        public void IsBound_AfterClose_ReturnsFalse() {
            // Arrange
            _listener.Listen(Host, Port);
            TcpClient client = null;
            // Act
            _listener.Close();
            var exception = Record.Exception(() => client.ConnectClient(Host,Port));
            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<SocketException>(exception);
        }

        public async void ClientConnected_NoConditions_ReturnsTrue() {
            // Arrange
            _listener.Listen(Host, Port);
            TcpClient client = null;

            // Act
            client.ConnectClient(Host, Port);
            _ = await _listener.AcceptSocketAsync();

            // Assert
            Assert.True(client.IsConnected());
        }

        private IListenerSocket _listener;
    }
}