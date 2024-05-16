namespace DNaNC_Client.Objects;

public class FingerTable
{
    public UInt64[] StartVals = new UInt64[64];
    public Node[] Successors = new Node[64];
    public int Length;
    
    public FingerTable(Node node)
    {
        Length = Successors.Length;
        
        for (int i = 0; i < Length; i++)
        {
            StartVals[i] = (node.Id + (UInt64)Math.Pow(2, i)) % UInt64.MaxValue;
            Successors[i] = node;
        }
    }
}