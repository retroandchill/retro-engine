// // @file NativeTaskScheduler.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Interop;

namespace RetroEngine.Async;

internal sealed partial class NativeTaskScheduler : IDisposable
{
    private IntPtr _nativeScheduler = NativeCreate();

    public void PumpTasks(int maxTasks = int.MaxValue)
    {
        NativePumpTasks(_nativeScheduler, maxTasks, out var error);
        error.ThrowIfError();
    }

    public void Dispose()
    {
        if (_nativeScheduler == IntPtr.Zero)
            return;

        NativeDestroy(_nativeScheduler);
        _nativeScheduler = IntPtr.Zero;
    }

    public Scope CreateThreadScope() => new(this);

    [StructLayout(LayoutKind.Sequential)]
    public ref struct Scope : IDisposable
    {
        private IntPtr _previous;

        internal Scope(NativeTaskScheduler scheduler)
        {
            NativeCreateScope(scheduler._nativeScheduler, out this);
        }

        public void Dispose()
        {
            if (_previous == IntPtr.Zero)
                return;

            NativeDestroyScope(ref this);
            _previous = IntPtr.Zero;
        }
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_manual_task_scheduler_create")]
    private static partial IntPtr NativeCreate();

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_manual_task_scheduler_destroy")]
    private static partial void NativeDestroy(IntPtr scheduler);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_manual_task_scheduler_create_scope")]
    private static partial void NativeCreateScope(IntPtr previous, out Scope scope);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_manual_task_scheduler_destroy_scope")]
    private static partial void NativeDestroyScope(scoped ref Scope scheduler);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_manual_task_scheduler_pump_tasks")]
    private static partial void NativePumpTasks(IntPtr scheduler, int maxTasks, out InteropError error);
}
