using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Text.Json;
using nexauth;

namespace nexauth_client {
    class Client {
        public Client(MainWindow parent) {
            this.parent = parent;
        }

        public async Task<bool> SetHost(string Address, Int32 Port) {
            IPAddress addr;
            parent.UpdateStatusText(parent.STATUS_LABEL, "Looking up host...", Brushes.Gray);
            parent.ClearStatusText(parent.HOST_STATUS_LABEL);
            try {
                IPHostEntry entry = await Dns.GetHostEntryAsync(Address);
                addr = entry.AddressList[0];
            }
            catch (System.Net.Sockets.SocketException e) {
                parent.UpdateStatusText(parent.HOST_STATUS_LABEL, "Invalid host!", Brushes.Red);
                return false;
            }
            this.address = addr;
            this.port = Port;
            return true;
        }

        public bool SetUsername(string username) {
            parent.ClearStatusText(parent.USERNAME_STATUS_LABEL);
            Regex reg = new Regex("^[a-zA-Z0-9]*$");
            if (reg.IsMatch(username))
                this.username = username;
            else {
                parent.UpdateStatusText(parent.USERNAME_STATUS_LABEL, "Invalid username!", Brushes.Red);
                return false;
            }
            return true;
        }

        public void Connect() {
            if (client != null)
                return;
            client = new TcpClient();
            client.Connect(address, port);

        }

        public void Disconnect() {
            client.Close();
        }

        public void SendUsername() {
            byte[] name = Encoding.ASCII.GetBytes(username);
            CHelloPayload payload = new CHelloPayload();
            //Payload.Send<CHelloPayload>(client.GetStream(), payload);
            payload.Send(client.GetStream());
            Console.WriteLine("Sent CHelloPayload (opcode 1)");
            Console.WriteLine("Awaiting SHelloPayload (opcode 1)...");
            SHelloPayload spayload = Payload.ReadAs<SHelloPayload>(client.GetStream());
            Console.WriteLine($"Received payload! Opcode: {spayload.Opcode}");
            Console.WriteLine($"Sending CSecureReqPayload...");
            CSecureReqPayload secure_req = new CSecureReqPayload();;
            secure_req.Send(client.GetStream());
            Console.WriteLine($"Awaiting SSecureResPayload...");
            SSecureResPayload secure_res = Payload.ReadAs<SSecureResPayload>(client.GetStream());
            Console.WriteLine($"Received SSecureResPayload! Public key: ${secure_res.publicKey}");
        }

        bool connected;
        private TcpClient client;
        private Int32 port;
        private MainWindow parent;
        private IPAddress address;
        private string username;
    }
}
