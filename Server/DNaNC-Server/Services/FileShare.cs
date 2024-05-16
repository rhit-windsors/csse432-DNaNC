using System.Text;
using DNaNC_Server.Objects;

namespace DNaNC_Server.Services;

public static class FileShare
{
    public static List<FileInfo> SharedFiles { get; set; } = new List<FileInfo>();
    public static List<LocatedFile> LocatedFiles { get; set; } = new List<LocatedFile>();
    
    public static void ShareFile(string path)
    {
        var file = new FileInfo(path);
        SharedFiles.Add(file);
    }
    
    public static void UnshareFile(string path)
    {
        var file = SharedFiles.FirstOrDefault(f => f.FullName == path);
        if (file != null)
        {
            SharedFiles.Remove(file);
        }
    }
    
    public static string ListSharedFiles()
    {
        var sb = new StringBuilder();
        foreach (var file in SharedFiles)
        {
            sb.AppendLine(file.FullName);
        }
        
        return sb.ToString();
    }

    public static string ListFoundFiles()
    {
        var sb = new StringBuilder();
        foreach (var file in LocatedFiles)
        {
            sb.AppendLine("[" + LocatedFiles.IndexOf(file) + "]: " + file.FileName + " at " + file.FileLocation.Host + ":" + file.FileLocation.Port);
        }

        return sb.ToString();
    }
    
    public static List<FileInfo> FileExists(string name)
    {
        //Decide the separator
        var separator = Path.DirectorySeparatorChar;
        return SharedFiles.Where(f => f.FullName.Split(separator).Last().Contains(name)).ToList();
    }
    
}