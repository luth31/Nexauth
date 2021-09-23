using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;
using System.Net.Sockets;

namespace Nexauth.Networking.Tests {
    public class ServerTest {

        [Fact]
        public void IsBound_ServerListening_ReturnsTrue() {
            // Arrange
            var server = new Server(new NullLogger<Server>(), new ServerOptions());
            // Act
            server.Start();
            bool isBound = server.IsBound;
            server.Stop();
            // Assert
            Assert.True(isBound);
        }

        [Fact]
        public void VerifyClientDisconnected_ServerFull_ReturnsTrue() {
            // Arrange
            var server = new Server(new NullLogger<Server>(), new ServerOptions(){ MaxClients = 10 });
            // Act
            server.Start();
            for (int i = 0; i < 10; ++i) {
                TcpClient client = new TcpClient();
                client.Connect("127.0.0.1", 8300);
            }
            TcpClient last_client = new TcpClient();
            last_client.Connect("127.0.0.1", 8300);
            Thread.Sleep(10);
            var disconnected = last_client.Client.Poll(1000, SelectMode.SelectRead) && (last_client.Client.Available == 0);
            server.Stop();
            // Assert
            Assert.True(disconnected);
        }
            server.Stop();
        }
    }
}
