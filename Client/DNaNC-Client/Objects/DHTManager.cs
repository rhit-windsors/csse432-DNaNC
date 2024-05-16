using DNaNC_Client.Services;

namespace DNaNC_Client.Objects;

public class DHTManager
{
    public FingerTable FingerTable = null!;
    //Store old successors
    public Node?[] SuccessorStore = null!;
    public string Host { get; set; } = DHTService.Local.Host;
    public int Port { get; set; } = DHTService.Local.Port;
    public UInt64 Id { get; set; } = DHTService.Local.Id;

    public Node? Successor
    {
        get => SuccessorStore[0];
        set
        {
            if (value == null) return;
            SuccessorStore[0] = value;
        }
    }
    
    public Node? Predecessor { get; set; } = null;
    
    
    public bool Join(Node? node, string host, int port)
    {
        DHTService.Local = new Node(host, port);

        DHTCleanupService.RejoinRun = false;
        DHTCleanupService.InitialNode = node;
        
        //Create the new finger table
        this.FingerTable = new FingerTable(DHTService.Local);

        this.SuccessorStore = new Node[3];
        for (int i = 0; i < 3; i++)
        {
            this.SuccessorStore[i] = node;
        }

        //This is the first node in the network
        if (node == null)
        {
            //TODO: Start a cleanup service
            
            return true;
        }
        
        var dhtManager = DHTService.GetDHTManager(node);
        
        if(dhtManager == null)
        {
            return false;
        }

        if (DHTService.CheckManagerValid(dhtManager))
        {
            try
            {
                this.Successor = dhtManager.FindSuccessor(this.Id);
            }
            catch(Exception e)
            {
                DHTService.Log("Failed to find successor while joining!");
                return false;
            }
        }
        else
        {
            return false;
        }

        //TODO: Start a cleanup service
        
        DHTService.Log("Joined network successfully!");
        
        return true;
    }

    public void Leave()
    {
        //TODO: Stop cleanup service

        try
        {
            //Alert the successor that we are leaving
            DHTService.AlertLeaving(this.Successor, null, this.Predecessor);

            //Alert the predecessor that we are leaving
            DHTService.AlertLeaving(this.Predecessor, this.Successor, null);
        }
        catch (Exception e)
        {
            DHTService.Log("Failed to leave network!");
        }
        finally
        {
            //Reset the node
            ResetNode();
        }
        
        DHTService.Log("Left network successfully!");
    }

    public Node? FindSuccessor(UInt64 id)
    {
        if (DHTService.IdValid(id, this.Id, this.Successor.Id))
        {
            return this.Successor;
        }
        else
        {
            //Find the closest finger
            Node closestPrecedingFinger = FindClosestPrecedingFinger(id);
            //Ask the closest finger for its successor
            return DHTService.RemoteFindSuccessor(closestPrecedingFinger, id);
        }
    }
    
    private Node FindClosestPrecedingFinger(UInt64 id)
    {
        //Check finger table
        for (int i = FingerTable.Length - 1; i >= 0; i--)
        {
            if (FingerTable.Successors[i] == DHTService.Local) continue;
            if (!DHTService.FingerValid(FingerTable.Successors[i].Id, this.Id, id)) continue;
            var dhtManager = DHTService.GetDHTManager(FingerTable.Successors[i]);
            if (dhtManager == null) continue;
            if (DHTService.CheckManagerValid(dhtManager))
            {
                return FingerTable.Successors[i];
            }
        }
        
        //Check successor cache
        foreach (var node in SuccessorStore)
        {
            if (node == null || node == DHTService.Local) continue;
            if (!DHTService.FingerValid(node.Id, this.Id, id)) continue;
            var dhtManager = DHTService.GetDHTManager(node);
            if (dhtManager == null) continue;
            if (DHTService.CheckManagerValid(dhtManager))
            {
                return node;
            }
        }

        return DHTService.Local;
    }

    private void ResetNode()
    {
        this.Successor = DHTService.Local;
        this.Predecessor = DHTService.Local;
        this.FingerTable = new FingerTable(DHTService.Local);
        for (var i = 0; i < SuccessorStore.Length; i++)
        {
            this.SuccessorStore[i] = DHTService.Local;
        }
    }
}