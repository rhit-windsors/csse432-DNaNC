using System.Net.Sockets;
using DNaNC_Server.Models;

namespace DNaNC_Server.Services;

public class NodeManagerService
{
    private readonly List<Node> _nodes = new List<Node>();

    public NodeManagerService()
    {
        //Going to call the node pulse method every 30 seconds
        Task.Run(NodePulse);
    }
    
    public List<Node> GetNodes()
    {
        return _nodes;
    }

    public bool RegisterNode(Node node)
    {
        if (_nodes.Any(n => n.Host == node.Host && n.Port == node.Port))
        {
            return false;
        }

        _nodes.Add(node);
        return true;
    }
    
    public bool UnregisterNode(Node node)
    {
        var nodeToRemove = _nodes.FirstOrDefault(n => n.Host == node.Host && n.Port == node.Port);
        if (nodeToRemove == null)
        {
            return false;
        }

        _nodes.Remove(nodeToRemove);
        return true;
    }

    private void NodePulse()
    {
        Thread.Sleep(30 * 1000);
        var nodesToRemove = new List<Node>();
        foreach(var node in _nodes)
        {
            var client = new TcpClient();
            try
            {
                client.Connect(node.Host, node.Port);
            }
            catch
            {
                nodesToRemove.Add(node);
            }
            finally
            {
                client.Close();
            }
        }
        
        foreach (var node in nodesToRemove)
        {
            _nodes.Remove(node);
        }
        
        NodePulse();
    }
    
    
}