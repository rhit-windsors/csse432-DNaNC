namespace DNaNC_Server.Objects;

public class LocatedFile(Node fileLocation, string fileName)
{
    public Node FileLocation { get; set; } = fileLocation;
    public string FileName { get; set; } = fileName;
}