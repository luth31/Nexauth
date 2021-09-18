using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;
using System.Net.Sockets;

namespace Nexauth.Networking.Tests {
    public class ServerTest {

        [Fact]
        public void IsBound_ServerStarted_ReturnsTrue() {
            // Arrange
            var server = new Server(new NullLogger<Server>(), new ServerOptions());
            // Act
            server.Start();
            // Assert
            Assert.True(server.IsBound);
            // Cleanup
            server.Stop();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        public void GetConnectionCount_SomeClients_ReturnsInput(int clientsCount) {
            // Arrange
            var server = new Server(new NullLogger<Server>(), new ServerOptions());
            // Act
            server.Start();
            for (int i = 0; i < clientsCount; ++i) {
                TcpClient client = new TcpClient();
                client.Connect("127.0.0.1", 8300);
            }
            Thread.Sleep(10);
            // Assert
            Assert.Equal(clientsCount, server.GetConnectionCount);
            // Cleanup
            server.Stop();
        }
    }
}
