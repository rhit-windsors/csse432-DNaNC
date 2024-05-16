using System.Security.Cryptography;
using System.Text;

namespace DNaNC_Server.Objects
{
    public class Node
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public UInt64 Id { get; set; }

        public Node(string host, int port)
        {
            Host = host;
            Port = port;
            //Set Id
            Id = GenerateId(Host, Port);
        }
        
        private UInt64 GenerateId(string host, int port)
        {
            //Generate the id for the node
            var hash = SHA1.HashData(Encoding.UTF8.GetBytes($"{host}:{port}"));
            return BitConverter.ToUInt64(hash, 0);
        }
        
        //DONE: Implement Compare
        public static int Compare(Node? x, Node? y)
        {
            if (x == null && y == null)
            {
                return 0;
            }
            if (x == null)
            {
                return -1;
            }
            if (y == null)
            {
                return 1;
            }
            return x.Id.CompareTo(y.Id);
        }
        
        //DONE: Implement Equals
        public static bool Equals(Node? x, Node? y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x == y;
        }
        
        public string GetNodeString()
        {
            return $"{this.Host}:{this.Port}";
        }

        //TODO: Implement ToString
        
        //TODO: Implement GetHashCode
    }
}
