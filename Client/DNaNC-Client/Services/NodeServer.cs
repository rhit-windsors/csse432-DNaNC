using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DNaNC_Client.Objects;

namespace DNaNC_Client.Services;

public static class NodeServer
{
    public static bool KillServer = false;
    public static void Listen()
    {
        if (NodeManager.LocalNode == null)
        {
            //Sleep and call listen again
            Thread.Sleep(1000);
            Listen();
        }
        //Create TCP listener
        TcpListener listener = new TcpListener(IPAddress.Any, NodeManager.LocalNode.Port);
        listener.Start();
        while (!KillServer)
        {
            //Accept the client
            TcpClient client = listener.AcceptTcpClient();
            //Handle the client
            Task.Run(() => HandleClient(client));
        }
        listener.Stop();
    }

    private static void HandleClient(TcpClient client)
    {
        //Receive the message
        byte[] buffer = new byte[1024];
        List<byte> byteStore = new List<byte>();
        while (client.Client.Receive(buffer) != 0)
        {
            byteStore.AddRange(buffer);
            if(Encoding.UTF8.GetString(buffer).Contains("EOF"))
            {
                break;
            }
        }
        
        //Decode the message
        var message = Encoding.UTF8.GetString(byteStore.ToArray());
        //Cut null characters
        message = message.Replace("\0", "");
        //Cut EOF
        message = message.Replace("_EOF", "");
        
        //Handle the message
        if (message.Contains("GET_SUCCESSOR"))
        {
            //Send the successor
            client.Client.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(NodeManager.Successor) + "_EOF"));
        }
        
        if(message.Contains("GET_PREDECESSOR"))
        {
            //Send the predecessor
            client.Client.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(NodeManager.Predecessor) + "_EOF"));
        }
        
        if(message.Contains("ALERT_NODE_JOIN"))
        {
            //Send ACK
            client.Client.Send(Encoding.UTF8.GetBytes("ACK_EOF"));
            //Receive the new node
            byteStore = new List<byte>();
            buffer = new byte[1024];
            while (client.Client.Receive(buffer) != 0)
            {
                byteStore.AddRange(buffer);
                if(Encoding.UTF8.GetString(buffer).Contains("EOF"))
                {
                    break;
                }
            }
            
            //Decode the message
            var newNode = Encoding.UTF8.GetString(byteStore.ToArray());
            //Cut null characters
            newNode = newNode.Replace("\0", "");
            //Cut EOF
            newNode = newNode.Replace("_EOF", "");
            
            var node = JsonSerializer.Deserialize<Node>(newNode);
            if (node != null) NodeManager.NodeJoined(node);
        }
        
        if(message.Contains("ALERT_SUCCESSOR_LEAVE"))
        {
            //Send ACK
            client.Client.Send(Encoding.UTF8.GetBytes("ACK_EOF"));
            
            NodeCleanup.TempPause = true;
            //Receive the new node
            byteStore = new List<byte>();
            buffer = new byte[1024];
            while (client.Client.Receive(buffer) != 0)
            {
                byteStore.AddRange(buffer);
                if(Encoding.UTF8.GetString(buffer).Contains("EOF"))
                {
                    break;
                }
            }
            
            //Decode the message
            var newNode = Encoding.UTF8.GetString(byteStore.ToArray());
            //Cut null characters
            newNode = newNode.Replace("\0", "");
            //Cut EOF
            newNode = newNode.Replace("_EOF", "");
            
            var node = JsonSerializer.Deserialize<Node>(newNode);
            if (node != null) NodeManager.SuccessorLeaving(node);
        }
        
        if(message.Contains("ALERT_PREDECESSOR_LEAVE"))
        {
            //Send ACK
            client.Client.Send(Encoding.UTF8.GetBytes("ACK_EOF"));
            
            NodeCleanup.TempPause = true;
            //Receive the new node
            byteStore = new List<byte>();
            buffer = new byte[1024];
            while (client.Client.Receive(buffer) != 0)
            {
                byteStore.AddRange(buffer);
                if(Encoding.UTF8.GetString(buffer).Contains("EOF"))
                {
                    break;
                }
            }
            
            //Decode the message
            var newNode = Encoding.UTF8.GetString(byteStore.ToArray());
            //Cut null characters
            newNode = newNode.Replace("\0", "");
            //Cut EOF
            newNode = newNode.Replace("_EOF", "");
            
            var node = JsonSerializer.Deserialize<Node>(newNode);
            if (node != null) NodeManager.PredecessorLeaving(node);
        }

        if (message.Contains("QUERY_FILE"))
        {
            //Send ACK
            client.Client.Send(Encoding.UTF8.GetBytes("ACK_EOF"));

            //Recieve the file name
            byteStore = new List<byte>();
            buffer = new byte[1024];
            while (client.Client.Receive(buffer) != 0)
            {
                byteStore.AddRange(buffer);
                if(Encoding.UTF8.GetString(buffer).Contains("EOF"))
                {
                    break;
                }
            }
            
            //Decode the message
            var fileName = Encoding.UTF8.GetString(byteStore.ToArray());
            //Cut null characters
            fileName = fileName.Replace("\0", "");
            //Cut EOF
            fileName = fileName.Replace("_EOF", "");
            
            //Send ACK
            client.Client.Send(Encoding.UTF8.GetBytes("ACK_EOF"));
            
            //recieve the node JSON
            byteStore = new List<byte>();
            buffer = new byte[1024];
            while (client.Client.Receive(buffer) != 0)
            {
                byteStore.AddRange(buffer);
                if(Encoding.UTF8.GetString(buffer).Contains("EOF"))
                {
                    break;
                }
            }
            
            //Decode the message
            var newNode = Encoding.UTF8.GetString(byteStore.ToArray());
            //Cut null characters
            newNode = newNode.Replace("\0", "");
            //Cut EOF
            newNode = newNode.Replace("_EOF", "");
            
            var node = JsonSerializer.Deserialize<Node>(newNode);
            
            NodeManager.Query(fileName, node);
        }

        if (message.Contains("ALERT_NODE_TO_FILE"))
        {
            //Send ACK
            client.Client.Send(Encoding.UTF8.GetBytes("ACK_EOF"));
            //Recieve the file name
            byteStore = new List<byte>();
            buffer = new byte[1024];
            while (client.Client.Receive(buffer) != 0)
            {
                byteStore.AddRange(buffer);
                if(Encoding.UTF8.GetString(buffer).Contains("EOF"))
                {
                    break;
                }
            }
            
            //Decode the message
            var fileName = Encoding.UTF8.GetString(byteStore.ToArray());
            //Cut null characters
            fileName = fileName.Replace("\0", "");
            //Cut EOF
            fileName = fileName.Replace("_EOF", "");
            
            //Send ACK
            client.Client.Send(Encoding.UTF8.GetBytes("ACK_EOF"));
            //recieve the node JSON
            byteStore = new List<byte>();
            buffer = new byte[1024];
            while (client.Client.Receive(buffer) != 0)
            {
                byteStore.AddRange(buffer);
                if(Encoding.UTF8.GetString(buffer).Contains("EOF"))
                {
                    break;
                }
            }
            
            //Decode the message
            var newNode = Encoding.UTF8.GetString(byteStore.ToArray());
            //Cut null characters
            newNode = newNode.Replace("\0", "");
            //Cut EOF
            newNode = newNode.Replace("_EOF", "");
            
            var node = JsonSerializer.Deserialize<Node>(newNode);
            
            FileShare.LocatedFiles.Add(new LocatedFile(node, fileName));
        }

        if (message.Contains("REQUEST_FILE"))
        {
            //Send ACK
            client.Client.Send(Encoding.UTF8.GetBytes("ACK_EOF"));
            //Recieve the file name
            byteStore = new List<byte>();
            buffer = new byte[1024];
            while (client.Client.Receive(buffer) != 0)
            {
                byteStore.AddRange(buffer);
                if(Encoding.UTF8.GetString(buffer).Contains("EOF"))
                {
                    break;
                }
            }
            
            //Decode the message
            var fileName = Encoding.UTF8.GetString(byteStore.ToArray());
            //Cut null characters
            fileName = fileName.Replace("\0", "");
            //Cut EOF
            fileName = fileName.Replace("_EOF", "");
            
            //Send ACK
            client.Client.Send(Encoding.UTF8.GetBytes("ACK_EOF"));
            
            //Wait for ready signal
            buffer = new byte[1024];
            client.Client.Receive(buffer);
            
            //Get the file
            var file = FileShare.SharedFiles.FirstOrDefault(f => f.Name.Contains(fileName));
            
            var messageToSend = Encoding.UTF8.GetBytes(Convert.ToBase64String(File.ReadAllBytes(file.FullName)));
            
            //Send the file
            int totalLength = messageToSend.Length;
            int bytesSent = 0;
            while (bytesSent <= totalLength)
            {
                if(totalLength - bytesSent < 1024)
                {
                    client.Client.Send(messageToSend, bytesSent, totalLength - bytesSent, SocketFlags.None);
                    break;
                }
                bytesSent += client.Client.Send(messageToSend, bytesSent, 1024, SocketFlags.None);
            }
        }
        
        client.Close();
    }
    
}