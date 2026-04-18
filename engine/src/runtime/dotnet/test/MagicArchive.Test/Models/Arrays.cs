namespace MagicArchive.Test.Models;

[Archivable]
public partial class ArrayCheck
{
    public int[]? Array1 { get; set; }
    public int?[]? Array2 { get; set; }
    public string[]? Array3 { get; set; }
    public string?[]? Array4 { get; set; }
}

[Archivable]
public partial class ArrayOptimizeCheck
{
    public StandardTypeTwo?[]? Array1 { get; set; }
    public List<StandardTypeTwo?>? List1 { get; set; }
}

[Archivable]
public partial class BitPackData
{
    [BitPackFormatter]
    public bool[]? Data { get; set; }
    public int AAA { get; set; }
}

[Archivable]
public partial class BitPackSingleData
{
    [BitPackFormatter]
    public bool[]? Data { get; set; }
}
