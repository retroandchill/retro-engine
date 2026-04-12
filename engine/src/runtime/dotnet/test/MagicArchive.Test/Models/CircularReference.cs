namespace MagicArchive.Test.Models;

[Archivable(GenerateType.CircularReference)]
public partial class Node
{
    [ArchiveOrder(0)]
    public Node? Parent { get; set; }

    [ArchiveOrder(1)]
    public Node[]? Children { get; set; }
}

[Archivable(GenerateType.CircularReference)]
public partial class PureNode
{
    [ArchiveOrder(0)]
    public int Id { get; set; }

    [ArchiveOrder(1)]
    public ulong Id2 { get; set; }
}

[Archivable]
public partial class CircularHolder
{
    public List<Node>? List { get; set; }
    public List<PureNode>? ListPure { get; set; }
}

// https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/preserve-references?pivots=dotnet-7-0
[Archivable(GenerateType.CircularReference)]
public partial class Employee
{
    [ArchiveOrder(0)]
    public string? Name { get; set; }

    [ArchiveOrder(1)]
    public Employee? Manager { get; set; }

    [ArchiveOrder(2)]
    public List<Employee>? DirectReports { get; set; }
}

[Archivable(GenerateType.CircularReference, SerializeLayout.Sequential)]
public partial class SequentialCircularReference
{
    public string? Name { get; set; }
    public SequentialCircularReference? Manager { get; set; }
    public List<SequentialCircularReference>? DirectReports { get; set; }
}
