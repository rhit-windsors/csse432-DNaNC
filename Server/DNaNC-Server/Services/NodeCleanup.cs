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
            
            //Check if your successor is still alive
            if (!CheckNode(NodeManager.Successor))
            {
                //If not, set your successor to your successor's successor
                NodeManager.Successor = NodeManager.SuccessorSuccessor;

                //Set your new Successor's successor
                NodeManager.SuccessorSuccessor = NodeManager.GetSuccessor(NodeManager.Successor);

                //Alert your new successor that you are its predecessor
                NodeManager.AlertNodeJoin(NodeManager.Successor);
            }

            //Check if your predecessor is still alive
            if (!CheckNode(NodeManager.Predecessor))
            {
                //If not, set your predecessor to your predecessor's predecessor
                NodeManager.Predecessor = NodeManager.PredecessorPredecessor;

                //Set your new Predecessor's predecessor
                NodeManager.PredecessorPredecessor = NodeManager.GetPredecessor(NodeManager.Predecessor);

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
}