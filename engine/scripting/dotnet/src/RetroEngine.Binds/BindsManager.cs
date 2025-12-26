namespace RetroEngine.Binds;

public static unsafe class BindsManager
{
    private static delegate* unmanaged[Cdecl]<
        char*,
        int,
        char*,
        int,
        int,
        IntPtr> _getBoundFunction = null;

    public static void Initialize(IntPtr bindsCallbacks)
    {
        if (_getBoundFunction != null)
        {
            throw new Exception("BindsManager.Initialize called twice");
        }

        _getBoundFunction = (delegate* unmanaged[Cdecl]<
            char*,
            int,
            char*,
            int,
            int,
            IntPtr>)bindsCallbacks;
    }

    public static IntPtr GetBoundFunction(
        ReadOnlySpan<char> moduleName,
        ReadOnlySpan<char> functionName,
        int parameterSize
    )
    {
        if (_getBoundFunction is null)
        {
            throw new InvalidOperationException(
                "BindsManager.Initialize must be called before GetBoundFunction"
            );
        }

        IntPtr functionPtr;
        fixed (char* moduleNamePtr = moduleName)
        fixed (char* functionNamePtr = functionName)
        {
            functionPtr = _getBoundFunction(
                moduleNamePtr,
                moduleName.Length,
                functionNamePtr,
                functionName.Length,
                parameterSize
            );
        }

        return functionPtr != IntPtr.Zero
            ? functionPtr
            : throw new Exception($"Failed to find bound function {functionName} in {moduleName}");
    }
}
