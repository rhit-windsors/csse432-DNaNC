using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DNaNC_Client.Services
{
    public class ClientPulseService
    {
        public ClientPulseService()
        {
            // Call the pulse here
        }

        // Method to listen for the server to pulse us
        public void ListenForPulse()
        {
            // Listen for the server to pulse us on our port
            var random = new Random();
            var port = random.Next(3320, 65533);
            
            //Check if the port is available
            while (!IsPortAvailable(port))
            {
                port = random.Next(3320, 65533);
            }
            
            // Start the listener
            Task.Run(() =>
            {
                TcpListener listener = new TcpListener(System.Net.IPAddress.Any, port);
                listener.Start();
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    byte[] data = new byte[client.ReceiveBufferSize];
                    int bytesRead = stream.Read(data, 0, data.Length);
                    string message = Encoding.ASCII.GetString(data, 0, bytesRead);
                    if (message == "PULSE")
                    {
                        // Send a response back to the server
                        byte[] response = Encoding.ASCII.GetBytes("PULSE-ACK");
                        stream.Write(response, 0, response.Length);
                    }
                    client.Close();
                }
            });
        }

        private bool IsPortAvailable(int port)
        {
            // Check if the port is available
            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    tcpClient.Connect("127.0.0.1", 9081);
                    tcpClient.Close();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
