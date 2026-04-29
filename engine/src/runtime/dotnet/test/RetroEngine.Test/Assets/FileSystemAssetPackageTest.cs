// // @file FileSystemAssetPackageTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RetroEngine.Assets;
using Testably.Abstractions.Testing;

namespace RetroEngine.Test.Assets;

public class FileSystemAssetPackageTest
{
    private MockFileSystem _mockFileSystem;
    private ServiceProvider _services;
    private IAssetPackageFactory _assetPackageFactory;

    [SetUp]
    public void Setup()
    {
        _mockFileSystem = new MockFileSystem(o => o.SimulatingOperatingSystem(SimulationMode.Linux));
        _mockFileSystem
            .Initialize()
            .WithSubdirectory("foo")
            .Initialized(d =>
                d.WithSubdirectory("bar")
                    .Initialized(i => i.WithFile("bar.asset"))
                    .WithFile("foo.asset")
                    .Which(f => f.HasStringContent("some file content"))
            );

        var mockDecoder = new Mock<IAssetDecoder>();
        mockDecoder.Setup(x => x.Extensions).Returns(["asset"]);

        _services = new ServiceCollection()
            .AddSingleton<IFileSystem>(_mockFileSystem)
            .AddSingleton(mockDecoder.Object)
            .AddSingleton<IAssetPackageFactory, FilesystemAssetPackageFactory>()
            .BuildServiceProvider();

        _assetPackageFactory = _services.GetRequiredService<IAssetPackageFactory>();
    }

    [TearDown]
    public void TearDown()
    {
        _services.Dispose();
    }

    [Test]
    public void ConstructFromFileSystem()
    {
        var package = _assetPackageFactory.Create("test", "/foo");
        package.Load();

        using var scope = Assert.EnterMultipleScope();
        var items = package.WalkEntriesBreadthFirst().Select(x => x.Name.ToString()).ToArray();

        Assert.That(items, Is.EquivalentTo(["bar", "foo.asset", "bar/bar.asset"]));

        items = package.WalkEntriesDepthFirst().Select(x => x.Name.ToString()).ToArray();
        Assert.That(items, Is.EquivalentTo(["bar", "bar/bar.asset", "foo.asset"]));
    }

    [Test]
    public async Task AddEntryAfterLoad()
    {
        var package = _assetPackageFactory.Create("test", "/foo");
        await package.LoadAsync();

        var taskCompletionSource = new TaskCompletionSource();
        package.OnEntryAdded += _ => taskCompletionSource.SetResult();

        _mockFileSystem.FileInfo.New("/foo/bar/baz.asset").Create();
        await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(10));

        var items = package.WalkEntriesBreadthFirst().Select(x => x.Name.ToString()).ToArray();

        Assert.That(items, Is.EquivalentTo(["bar", "foo.asset", "bar/bar.asset", "bar/baz.asset"]));
    }
}
