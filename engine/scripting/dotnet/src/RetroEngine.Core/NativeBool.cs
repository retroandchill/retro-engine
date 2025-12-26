namespace RetroEngine.Core;

public enum NativeBool : byte
{
    False = 0,
    True = 1,
}

public static class BoolConverter
{
    public static NativeBool ToNativeBool(this bool value)
    {
        return value ? NativeBool.True : NativeBool.False;
    }

    public static bool ToManagedBool(this NativeBool value)
    {
        var byteValue = (byte)value;
        return byteValue != 0;
    }
}
