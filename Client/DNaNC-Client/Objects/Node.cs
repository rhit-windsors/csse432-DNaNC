using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DNaNC_Client.Objects
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
        
        //TODO: Implement Compare
        
        //TODO: Implement Equals
        
        //TODO: Implement ToString
        
        //TODO: Implement GetHashCode
    }
}
