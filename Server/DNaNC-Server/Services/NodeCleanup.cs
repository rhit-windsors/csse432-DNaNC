using System.Net.Sockets;
using DNaNC_Server.Objects;

namespace DNaNC_Server.Services;

public static class NodeCleanup
{
    public static bool KillCleanup = false;
    public static bool Paused = false;
    public static bool TempPause = false;

    public static void Cleanup()
    {
        while (true)
        {
            while (Paused)
            {
                //Wait
                Thread.Sleep(1000);
            }

            if (TempPause)
            {
                Thread.Sleep(5000);
                TempPause = false;
            }
            
            //If both nodes are the same and dead, then you are the only node in the network
            if (NodeManager.Successor.Id == NodeManager.Predecessor.Id && !CheckNode(NodeManager.Successor))
            {
                NodeManager.Successor = NodeManager.LocalNode;
                NodeManager.Predecessor = NodeManager.LocalNode;
                NodeManager.SuccessorSuccessor = NodeManager.LocalNode;
                NodeManager.PredecessorPredecessor = NodeManager.LocalNode;
            }
            
            //Check if your successor is still alive
            if (!CheckNode(NodeManager.Successor))
            {
                //If not, set your successor to your successor's successor
                NodeManager.Successor = FindNewSuccessor(NodeManager.LocalNode);

                //Alert your new successor that you are its predecessor
                NodeManager.AlertNodeJoin(NodeManager.Successor);
            }

            //Check if your predecessor is still alive
            if (!CheckNode(NodeManager.Predecessor))
            {
                //If not, set your predecessor to your predecessor's predecessor
                NodeManager.Predecessor = FindNewPredecessor(NodeManager.LocalNode);

                //The node will clean up the rest
            }

            
            //If all 4 are dead, assume you are the only node in the network
            if (!CheckNode(NodeManager.Successor) && !CheckNode(NodeManager.Predecessor) && !CheckNode(NodeManager.SuccessorSuccessor) && !CheckNode(NodeManager.PredecessorPredecessor))
            {
                NodeManager.Successor = NodeManager.LocalNode;
                NodeManager.Predecessor = NodeManager.LocalNode;
                NodeManager.SuccessorSuccessor = NodeManager.LocalNode;
                NodeManager.PredecessorPredecessor = NodeManager.LocalNode;
            }
            
            //If my successor is not my successor's predecessor, then my successor's predecessor is my new successor
            var successorPredecessor = NodeManager.GetPredecessor(NodeManager.Successor);
            if(NodeManager.LocalNode.Id != successorPredecessor.Id)
            {
                NodeManager.Successor = successorPredecessor;
            }
            
            //Keep only 200 entries in query history
            if (NodeManager.QueryHistory.Count > 200)
            {
                //Remove oldest 10
                for (int i = 0; i < 10; i++)
                {
                    NodeManager.QueryHistory.RemoveAt(0);
                }
            }

            //Sleep for 5 seconds
            Thread.Sleep(5000);
            //Cleanup again
            if (KillCleanup)
            {
                return;
            }
        }
    }

    private static bool CheckNode(Node? node)
    {
        if(node == null)
        {
            return false;
        }
        
        try
        {
            var client = new TcpClient();
            client.Connect(node.Host, node.Port);
            client.Close();
            return true;
        }
        catch
        {
            return false;
        }
        
    }
    
    private static Node FindNewSuccessor(Node inputNode)
    {
        try
        {
            var node = NodeManager.GetSuccessor(inputNode);
            if (!CheckNode(node))
            {
                return inputNode;
            }
            return FindNewSuccessor(node);
        }
        catch
        {
            //If we died then the node is our new successor
            return inputNode;
        }
    }
    
private static Node FindNewPredecessor(Node inputNode)
    {
        try
        {
            var node = NodeManager.GetSuccessor(inputNode);
            if (!CheckNode(node))
            {
                return inputNode;
            }

            return FindNewPredecessor(node);
        }
        catch
        {
            //If we died then the node is our new predecessor
            return inputNode;
        }
    }
}