namespace MagicArchive.Test.Models;

[Archivable]
public partial class Overwrite
{
    public int MyProperty1 { get; set; }
    public int MyProperty2 { get; set; }
    public string? MyProperty3 { get; set; }
    public string? MyProperty4 { get; set; }
}

[Archivable]
public partial struct Overwrite2
{
    public int MyProperty1 { get; set; }
    public int MyProperty2 { get; set; }
    public string? MyProperty3 { get; set; }
    public string? MyProperty4 { get; set; }
}

[Archivable]
public partial class Overwrite3(int myProperty1, int myProperty2)
{
    public int MyProperty1 { get; set; } = myProperty1;
    public int MyProperty2 { get; set; } = myProperty2;
    public string? MyProperty3 { get; set; }
    public string? MyProperty4 { get; set; }
}

[Archivable]
public partial class Overwrite4
{
    public int MyProperty1 { get; set; }
    public Overwrite? MyProperty2 { get; set; }
    public List<int>? MyProperty3 { get; set; }
}
