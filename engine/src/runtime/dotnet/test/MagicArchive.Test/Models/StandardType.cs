namespace MagicArchive.Test.Models;

[Archivable]
public partial class StandardTypeZero { }

[Archivable]
public partial class StandardTypeOne
{
    public int One { get; set; }
}

[Archivable]
public partial class StandardTypeTwo
{
    public int One { get; set; }
    public int Two { get; set; }

    public StandardTypeTwo() { }

    [Archivable]
    public partial struct StandardUnmanagedStruct
    {
        public int MyProperty { get; set; }
    }

    [Archivable]
    public partial struct StandardStruct
    {
        public string MyProperty { get; set; }

        public StandardStruct()
        {
            MyProperty = default!;
        }
    }

    public partial class NestedContainer
    {
        [Archivable]
        public partial class StandardTypeNested
        {
            public int One { get; set; }
        }
    }

    public partial class DoublyNestedContainer
    {
        public partial class DoublyNestedContainerInner
        {
            [Archivable]
            public partial class StandardTypeDoublyNested
            {
                public int One { get; set; }
            }
        }
    }

    [Archivable]
    public partial class WithArray
    {
        public StandardTypeOne[]? One { get; set; }
    }
}
