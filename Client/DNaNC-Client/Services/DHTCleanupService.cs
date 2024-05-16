using DNaNC_Client.Objects;

namespace DNaNC_Client.Services;

public static class DHTCleanupService
{
     public static bool RejoinRun { get; set; } 
     public static Node? InitialNode { get; set; }
}