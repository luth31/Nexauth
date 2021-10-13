using Xunit;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading;
using System.Net.Sockets;

namespace Nexauth.Protocol.Tests {
        public class ServerTest {

        Server _server;
        ILoggerFactory _loggerFactory;
        
        const int ClientCount = 10;

        public ServerTest() {
            _loggerFactory = Util.GetLoggerFactory();
        }
        public void IsBound_NoConditions_ReturnsTrue() {
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
            Thread.Sleep(100);
            var disconnected = last_client.Client.Poll(1000, SelectMode.SelectRead) && (last_client.Client.Available == 0);
            server.Stop();
            // Assert
            Assert.True(disconnected);
        }

        [Fact]
        public void VerifyClientsConnected_NoConditions_ReturnsTrue() {
            // Arrange
            var handler = new ClientHandler(_loggerFactory.CreateLogger<ClientHandler>());
            var factoryMock = new Mock<Func<ClientHandler>>();
            factoryMock.Setup(f => f.Invoke()).Returns(handler);

            _server = _server = new Server(_loggerFactory.CreateLogger<Server>(),
                                            new Listener(),
                                            new SessionManager(_loggerFactory.CreateLogger<SessionManager>(), factoryMock.Object),
                                            new ServerOptions());
                                            
            TcpClient[] clients;
            _server.Start();

            // Act
            clients = Util.ConnectClients(ClientCount, "127.0.0.1", 8300);
            Thread.Sleep(100);
            var connected = clients.AreConnected();
            _server.Stop();

            // Assert
            Assert.True(connected);
        }
    }
}
