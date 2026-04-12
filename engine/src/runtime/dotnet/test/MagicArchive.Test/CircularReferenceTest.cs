// // @file CircularReferenceTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using FluentAssertions;
using MagicArchive.Test.Models;

namespace MagicArchive.Test;

public class CircularReferenceTest
{
    [Test]
    public void MicrosoftExample()
    {
        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/preserve-references?pivots=dotnet-7-0

        Employee tyler = new() { Name = "Tyler Stein" };

        Employee adrian = new() { Name = "Adrian King" };

        tyler.DirectReports = [adrian];
        adrian.Manager = tyler;

        var bin = ArchiveSerializer.Serialize(tyler);
        var tylerDeserialized = ArchiveSerializer.Deserialize<Employee>(bin);

        tylerDeserialized?.DirectReports?[0].Manager.Should().BeSameAs(tylerDeserialized);
    }

    [Test]
    public void NodeTest()
    {
        var parent = new Node();
        var a1 = new Node();
        var a2 = new Node();
        var a3 = new Node();
        a1.Parent = parent;
        a2.Parent = parent;
        a3.Parent = parent;
        parent.Children = [a1, a2, a3];

        var bin = ArchiveSerializer.Serialize(parent);
        var value2 = ArchiveSerializer.Deserialize<Node>(bin);

        foreach (var item in value2!.Children!)
        {
            item.Parent.Should().BeSameAs(value2);
        }
    }

    [Test]
    public void PureNodeTest()
    {
        var node = new PureNode() { Id = 10, Id2 = 1000 };

        var bin = ArchiveSerializer.Serialize(node);
        var value2 = ArchiveSerializer.Deserialize<PureNode>(bin);

        Assert.That(value2, Is.Not.Null);
        value2.Id.Should().Be(10);
        value2.Id2.Should().Be(1000);
    }

    [Test]
    public void InHolder()
    {
        var holder = new CircularHolder { List = [], ListPure = [] };

        {
            var parent = new Node();
            var a1 = new Node();
            var a2 = new Node();
            var a3 = new Node();
            a1.Parent = parent;
            a2.Parent = parent;
            a3.Parent = parent;
            parent.Children = [a1, a2, a3];

            var parent2 = new Node { Children = [parent, a2] };

            holder.List.AddRange([parent, parent, parent2, parent, parent2]);
        }
        {
            var pure1 = new PureNode() { Id = 10, Id2 = 1000 };
            var pure2 = new PureNode() { Id = 100, Id2 = 100000 };

            holder.ListPure.Add(pure1);
            holder.ListPure.Add(pure1);
            holder.ListPure.Add(pure2);
            holder.ListPure.Add(pure2);
            holder.ListPure.Add(pure1);
        }

        var bin = ArchiveSerializer.Serialize(holder);
        var value2 = ArchiveSerializer.Deserialize<CircularHolder>(bin);

        {
            var parent = value2!.List![0];
            var parent2 = value2.List[2];
            _ = parent.Children![0];
            var a2 = parent.Children[1];
            _ = parent.Children[2];

            parent.Should().NotBeSameAs(parent2);
            parent2.Children![0].Should().BeSameAs(parent);
            parent2.Children[1].Should().BeSameAs(a2);
        }
        {
            var pure1 = value2.ListPure![0];
            var pure2 = value2.ListPure[2];

            pure1.Should().NotBeSameAs(pure2);
            pure1.Should().BeSameAs(value2.ListPure[1]);
            pure1.Should().BeSameAs(value2.ListPure[4]);
            pure2.Should().BeSameAs(value2.ListPure[3]);
        }
    }

    [Test]
    public void Sequential()
    {
        SequentialCircularReference tyler = new() { Name = "Tyler Stein" };

        SequentialCircularReference adrian = new() { Name = "Adrian King" };

        tyler.DirectReports = [adrian];
        adrian.Manager = tyler;

        var bin = ArchiveSerializer.Serialize(tyler);
        var tylerDeserialized = ArchiveSerializer.Deserialize<SequentialCircularReference>(bin);

        tylerDeserialized?.DirectReports?[0].Manager.Should().BeSameAs(tylerDeserialized);
    }
}
