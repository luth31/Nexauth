using Xunit;
using System.Threading;
using System.Net.Sockets;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Nexauth.Protocol.Tests {
    public class ServerTest {

        Mock<ISessionManager> _sessionManager;
        Mock<IListenerSocket> _listener;
        Server _server;
        ILoggerFactory _loggerFactory;
        
        const int ClientCount = 10;

        public ServerTest() {
            _loggerFactory = Util.GetLoggerFactory();
        }

        [Fact]
        public void IsBound_NoConditions_ReturnsTrue() {
            // Arrange
            _sessionManager = new Mock<ISessionManager>();
            _server = new Server(_loggerFactory.CreateLogger<Server>(),
                                new Listener(),
                                _sessionManager.Object,
                                new ServerOptions());
            _server.Start();

            // Act
            bool isBound = _server.IsBound;
            
            _server.Stop();

            // Assert
            Assert.True(isBound);
        }

        /*[Fact]
        public void VerifyClientDisconnected_ServerFull_ReturnsTrue() {
            // Arrange
            var handler = new Mock<ClientHandler>();
            var factoryMock = new Mock<Func<ClientHandler>>();
            factoryMock.Setup(f => f.Invoke()).Returns(handler.Object);
            _server = new Server(new NullLogger<Server>(), 
                                new Listener(),
                                new SessionManager(new NullLogger<ISessionManager>(), factoryMock.Object),
                                new ServerOptions(){ MaxClients = ClientCount });

            TcpClient[] clients = new TcpClient[ClientCount];
            for (int i = 0; i < ClientCount; ++i)
                clients[i] = new TcpClient();

            int connectedUsers = 0;
            /*_sessionManager.Setup(sMgr => sMgr.RegisterClient(It.IsAny<Socket>()))
                .Callback(() => {
                    Interlocked.Increment(ref connectedUsers);
                });
            _server.Start();

            // Act
            foreach(var client in clients) {
                client.Connect("127.0.0.1", 8300);
            }

            TcpClient last_client = new TcpClient();
            last_client.Connect("127.0.0.1", 8300);
            Console.WriteLine(connectedUsers);

            Thread.Sleep(100);
            Console.WriteLine(connectedUsers);

            var disconnected = last_client.Client.Poll(1000, SelectMode.SelectRead) && (last_client.Client.Available == 0);

            _server.Stop();

            // Assert
            Assert.True(disconnected);
        }*/

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
