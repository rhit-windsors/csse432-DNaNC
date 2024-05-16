using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DNaNC_Server.Objects;

namespace DNaNC_Server.Services;

public static class NodeManager
{
    public static Node LocalNode { get; set; } = null!;
    public static Node? Successor { get; set; }
    public static Node? SuccessorSuccessor { get; set; }
    public static Node? Predecessor { get; set; }
    public static Node? PredecessorPredecessor { get; set; }
    public static List<String> QueryHistory { get; set; } = new List<string>();
    
    public static void InitNetwork(int port)
    {
        SetLocalNode(GetPublicIP(), port);
        //Set the successor
        Successor = LocalNode;
        //Set the Predecessor
        Predecessor = LocalNode;
        
        //Start cleanup
        _ = Task.Run(NodeCleanup.Cleanup);
    }

    public static void Query(string fileName, Node node)
    {
        if (node != LocalNode)
        {
            //Smush the filename and the node together
            var query = $"{fileName}:{node.Id}";
            if (QueryHistory.Contains(query))
            {
                return;
            }
            
            QueryHistory.Add(query);
            //Check if we have the file
            if (FileShare.FileExists(fileName).Count > 0)
            {
                //Say we have it
                AlertNodeToFile(node, FileShare.FileExists(fileName).First().Name);
            }
        }

        if (Successor.Id == Predecessor.Id)
        {
            //Only send the query to the successor
            SendQuery(fileName, Successor, node);
            return;
        }
        
        //Pass the query to the successor
        if (Successor != LocalNode)
        {
            SendQuery(fileName, Successor, node);
        }
        
        //Pass the query to the predecessor
        if (Predecessor != LocalNode)
        {
            SendQuery(fileName, Predecessor, node);
        }
    }

    private static void SendQuery(string fileName, Node node, Node requestingNode)
    {
        TcpClient client = new TcpClient(node.Host, node.Port);
        client.Client.Send("QUERY_FILE_EOF"u8.ToArray());
        //Receive the server's acknowledgement
        var buffer = new byte[1024];
        client.Client.Receive(buffer);
        //Send the file name
        client.Client.Send(Encoding.UTF8.GetBytes(fileName + "_EOF"));
        //Receive the server's acknowledgement
        buffer = new byte[1024];
        client.Client.Receive(buffer);
        //Send the node
        var jsonString = JsonSerializer.Serialize(requestingNode);
        client.Client.Send(Encoding.UTF8.GetBytes(jsonString + "_EOF"));
        client.Close();
    }

    public static void RequestFile(int foundIndex)
    {
        var file = FileShare.LocatedFiles[foundIndex];
        //Connect to the node
        var node = file.FileLocation;
        TcpClient client = new TcpClient(node.Host, node.Port);
        client.Client.Send("REQUEST_FILE_EOF"u8.ToArray());
        //Receive the server's acknowledgement
        var buffer = new byte[1024];
        client.Client.Receive(buffer);
        //Send the file name
        client.Client.Send(Encoding.UTF8.GetBytes(file.FileName + "_EOF"));
        
        //Recieve the server's acknowledgement
        buffer = new byte[1024];
        client.Client.Receive(buffer);
        
        //Send the ready signal
        client.Client.Send("READY_EOF"u8.ToArray());
        
        //Recieve the file
        buffer = new byte[1024];
        List<byte> byteStore = new List<byte>();
        while (client.Client.Receive(buffer) != 0)
        {
            byteStore.AddRange(buffer);
            //Check if the EOF is reached
            if(Encoding.UTF8.GetString(buffer).Contains("_EOF"))
            {
                break;
            }
            buffer = new byte[1024];
        }
        
        client.Close();
        
        //Decode the base64
        var fileData = Encoding.UTF8.GetString(byteStore.ToArray());
        fileData = fileData.Replace("\0", "").Replace("_EOF", "");
        var fileBytes = Convert.FromBase64String(fileData);
        
        //Save in downloads folder
        var downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Path.DirectorySeparatorChar + "Downloads" + Path.DirectorySeparatorChar;
        
        //Write the file
        File.WriteAllBytes(downloadsPath + file.FileName, fileBytes);
    }
    
    public static void Join(string host, int port, int localPort)
    {
        if (host == "127.0.0.1" || host == "localhost")
        {
            host = GetPublicIP();
        }
        
        var node = new Node(host, port);
        
        //Set the local node
        SetLocalNode(host, localPort);
        
        //Set the successor
        Successor = node;
        
        //Ask the node for its predecessor
        Predecessor = GetPredecessor(node);
        
        //Alert the successor that this node is its predecessor
        AlertNodeJoin(node);
        
        //Ask the node for its successor
        SuccessorSuccessor = GetSuccessor(Successor);

        PredecessorPredecessor = GetPredecessor(node);
        
        Console.WriteLine("Node joined the network");
        
        //Start the cleanup service
        _ = Task.Run(NodeCleanup.Cleanup);
    }

    public static void Leave()
    {
        //Turn off the cleanup service
        NodeCleanup.KillCleanup = true;
        NodeServer.KillServer = true;
        
        //If you are the pred and succ, you are the only node in the network
        if (Predecessor.Id == LocalNode.Id && Successor.Id == LocalNode.Id)
        {
            return;
        }
        
        //Alert the nodes around that you are leaving
        AlertNodeLeave(Predecessor, true);
        AlertNodeLeave(Successor, false);
    }
    
    public static void NodeJoined(Node node)
    {
        //Pause the cleanup service
        NodeCleanup.Paused = true;
        
        //Set the predecessor
        Predecessor = node;
        
        //Set the predecessor's predecessor
        PredecessorPredecessor = GetPredecessor(node);
        
        //Cleanup the Successor
        if (Successor == LocalNode)
        {
            Successor = node;
        }
        
        //Resume the cleanup service
        NodeCleanup.Paused = false;
    }

    public static void SuccessorLeaving(Node node)
    {
        //Set the new successor
        Successor = node;
        
        //Set the new successor's successor
        SuccessorSuccessor = GetSuccessor(node);
    }
    
    public static void PredecessorLeaving(Node node)
    {
        //Set the new predecessor
        Predecessor = node;
        
        //Set the new predecessor's predecessor
        PredecessorPredecessor = GetPredecessor(node);
    }
    
    private static void SetLocalNode(string host, int port)
    {
        LocalNode = new Node(GetPublicIP(), port);
    }
    
    public static Node? GetPredecessor(Node node)
    {
        //Connect to the node
        TcpClient client = new TcpClient(node.Host, node.Port);
        client.Client.Send("GET_PREDECESSOR_EOF"u8.ToArray());
        //Recieve the predecessor
        byte[] buffer = new byte[1024];
        List<byte> byteStore = new List<byte>();
        while (client.Client.Receive(buffer) != 0)
        {
            byteStore.AddRange(buffer);
            //Check if the EOF is reached
            if(Encoding.UTF8.GetString(buffer).Contains("EOF"))
            {
                break;
            }
        }
        
        client.Close();
        
        //Convert byteStore to string
        var jsonString = Encoding.UTF8.GetString(byteStore.ToArray());
        
        //Cut off the extra data and the EOF
        jsonString = jsonString.Replace("\0", "").Replace("_EOF", "");
        
        //Decode the JSON received
        Node? predecessor = JsonSerializer.Deserialize<Node>(jsonString);
        
        return predecessor;
    }
    
    public static Node? GetSuccessor(Node node)
    {
        //Connect to the node
        TcpClient client = new TcpClient(node.Host, node.Port);
        client.Client.Send("GET_SUCCESSOR_EOF"u8.ToArray());
        //Recieve the successor
        byte[] buffer = new byte[1024];
        List<byte> byteStore = new List<byte>();
        while (client.Client.Receive(buffer) != 0)
        {
            byteStore.AddRange(buffer);
            //Check if the EOF is reached
            if(Encoding.UTF8.GetString(buffer).Contains("EOF"))
            {
                break;
            }
        }
        
        client.Close();
        
        //Convert byteStore to string
        var jsonString = Encoding.UTF8.GetString(byteStore.ToArray());
        
        //Cut off the extra data and the EOF
        jsonString = jsonString.Replace("\0", "").Replace("_EOF", "");
        
        //Decode the JSON received
        Node? successor = JsonSerializer.Deserialize<Node>(jsonString);
        
        return successor;
    }

    public static void AlertNodeJoin(Node node)
    {
        TcpClient client = new TcpClient(node.Host, node.Port);
        client.Client.Send("ALERT_NODE_JOIN_EOF"u8.ToArray());
        //Send the local node
        var jsonString = JsonSerializer.Serialize(LocalNode);
        client.Client.Send(Encoding.UTF8.GetBytes(jsonString + "_EOF"));
        client.Close();
    }
    
    public static void AlertNodeLeave(Node node, bool isSuccessor)
    {
        if (isSuccessor)
        {
            TcpClient client = new TcpClient(node.Host, node.Port);
            client.Client.Send("ALERT_SUCCESSOR_LEAVE_EOF"u8.ToArray());
            //Receive the server's acknowledgement
            var buffer = new byte[1024];
            client.Client.Receive(buffer);
            //Send the local node
            var jsonString = JsonSerializer.Serialize(Successor);
            client.Client.Send(Encoding.UTF8.GetBytes(jsonString + "_EOF"));
            client.Close();
        }
        else
        {
            TcpClient client = new TcpClient(node.Host, node.Port);
            client.Client.Send("ALERT_PREDECESSOR_LEAVE_EOF"u8.ToArray());
            //Receive the server's acknowledgement
            var buffer = new byte[1024];
            client.Client.Receive(buffer);
            //Send the local node
            var jsonString = JsonSerializer.Serialize(Predecessor);
            client.Client.Send(Encoding.UTF8.GetBytes(jsonString + "_EOF"));
            client.Close();
        }
        
    }

    private static void AlertNodeToFile(Node node, string fileName)
    {
        TcpClient client = new TcpClient(node.Host, node.Port);
        client.Client.Send("ALERT_NODE_TO_FILE_EOF"u8.ToArray());
        //Wait for the server to acknowledge
        var buffer = new byte[1024];
        client.Client.Receive(buffer);
        //Send the file name
        client.Client.Send(Encoding.UTF8.GetBytes(fileName + "_EOF"));
        //Wait for the server to acknowledge
        buffer = new byte[1024];
        client.Client.Receive(buffer);
        //Send the local node
        var jsonString = JsonSerializer.Serialize(LocalNode);
        client.Client.Send(Encoding.UTF8.GetBytes(jsonString + "_EOF"));
        client.Close();
    }
    
    //Try two methods
    private static string GetPublicIP()
    {
        try
        {
            string url = "http://checkip.dyndns.org";
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] a = response.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string a4 = a3[0];
            return a4;
        }
        catch(Exception ex)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");

            request.UserAgent = "curl"; // this will tell the server to return the information as if the request was made by the linux "curl" command

            string publicIPAddress;

            request.Method = "GET";
            using (WebResponse response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    publicIPAddress = reader.ReadToEnd();
                }
            }

            return publicIPAddress.Replace("\n", "");
        }
    }
    
}