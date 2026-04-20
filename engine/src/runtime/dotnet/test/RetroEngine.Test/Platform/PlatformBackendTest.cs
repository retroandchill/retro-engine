// // @file PlatformBackendTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Platform;

namespace RetroEngine.Test.Platform;

public class PlatformBackendTest
{
    [Test]
    public void CanCreateHeadlessPlatform()
    {
        Assert.DoesNotThrow(() =>
        {
            using var platform = new PlatformBackend(PlatformBackendKind.Headless, PlatformInitFlags.None);
        });
    }

    [Test]
    public void CanCreateSdlPlatform()
    {
        Assert.DoesNotThrow(() =>
        {
            using var platform = new PlatformBackend(
                PlatformBackendKind.SDL3,
                PlatformInitFlags.Video | PlatformInitFlags.Audio
            );
        });
    }

    [Test]
    public void MultipleDisposeIsSafe()
    {
        Assert.DoesNotThrow(() =>
        {
            using var platform = new PlatformBackend(PlatformBackendKind.Headless, PlatformInitFlags.None);
            // ReSharper disable once DisposeOnUsingVariable
            platform.Dispose();
        });
    }
}
