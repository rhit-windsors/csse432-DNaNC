using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using DNaNC_Client.Objects;

namespace DNaNC_Client.Services;

public static class DHTService
{
    public static Node Local { get; set; }
    
    public static DHTManager? GetDHTManager(Node? node)
    {
        if (node == null)
        {
            return null;
        }

        try
        {
           //Try connecting to the node
           TcpClient client = new TcpClient(node.Host, node.Port);
           
           //Ask for the DHTManager
           client.Client.Send("GET_DHT_MANAGER"u8.ToArray());
           
           //Receive the DHTManager
           byte[] buffer = new byte[1024];
           List<byte> byteStore = new List<byte>();
           
           //Read until the end
           while (client.Client.Receive(buffer) != 0)
           {
                byteStore.AddRange(buffer);
           }
           client.Close();
           
           //Convert byteStore to string
           var jsonString = Encoding.UTF8.GetString(byteStore.ToArray());
           
           //Decode the JSON received
           DHTManager? dhtManager = JsonSerializer.Deserialize<DHTManager>(jsonString);

           return dhtManager;
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    public static void Log(string message)
    {
        //Check if log file exists
        if (!File.Exists("dht.log"))
        {
            File.Create("dht.log").Close();
        }
        
        //Write to the log file
        File.AppendAllText("dht.log", $"{DateTime.Now}: {message}\n");
    }

    public static bool CheckManagerValid(DHTManager manager)
    {
        try
        {
            return manager is { Port: > 0, Successor: not null };
        }
        catch (Exception e)
        {
            return false;
        }
    }

    //Check if ID is in a valid range
    public static bool IdValid(UInt64 id, UInt64 start, UInt64 end)
    {
        if (start >= end)
        {
            if (id > start || id <= end)
            {
                return true;
            }
        }
        else
        {
            if (id > start && id <= end)
            {
                return true;
            }
        }

        return false;
    }
    
    //Check if the Finger is in a valid range
    public static bool FingerValid(UInt64 id, UInt64 start, UInt64 end)
    {
        if (start == end)
        {
            return true;
        }
        if (start > end)
        {
            if (id > start || id < end)
            {
                return true;
            }
        }
        else
        {
            if (id > start && id < end)
            {
                return true;
            }
        }

        return false;
    }

    public static Node? RemoteFindSuccessor(Node remoteNode, UInt64 id)
    {
        return RemoteFindSuccessor(remoteNode, id, 3);
    }
    
    private static Node? RemoteFindSuccessor(Node remoteNode, UInt64 id, int retries)
    {
        if (retries == 0)
        {
            return null;
        }

        var dhtManager = GetDHTManager(remoteNode);
        
        if (dhtManager == null)
        {
            return null;
        }

        try
        {
            return dhtManager.FindSuccessor(id);
        }
        catch(Exception e)
        {
            return RemoteFindSuccessor(remoteNode, id, retries - 1);
        }
    }
    
    public static void AlertLeaving(Node node, Node? newSuccessor, Node? newPredecessor)
    {
        //Connect to the node
        TcpClient client = new TcpClient(node.Host, node.Port);
        client.Client.Send("LEAVING"u8.ToArray());
        if (newSuccessor != null)
        {
            client.Client.Send("newSuccessor"u8.ToArray());
            //Send the newSuccessor as JSON
            var jsonString = JsonSerializer.Serialize(newSuccessor);
            client.Client.Send(Encoding.UTF8.GetBytes(jsonString));
        }
        
        if (newPredecessor != null)
        {
            client.Client.Send("newPredecessor"u8.ToArray());
            //Send the newPredecessor as JSON
            var jsonString = JsonSerializer.Serialize(newPredecessor);
            client.Client.Send(Encoding.UTF8.GetBytes(jsonString));
        }
        
        client.Close();
    }
    
    public static string GetPublicIpAddress()
    {
        var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");

        request.UserAgent = "curl"; // this will tell the server to return the information as if the request was made by the linux "curl" command

        string publicIpAddress;

        request.Method = "GET";
        using (WebResponse response = request.GetResponse())
        {
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                publicIpAddress = reader.ReadToEnd();
            }
        }

        return publicIpAddress.Replace("\n", "");
    }
    
}