using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using FluentAssertions;

// ReSharper disable AccessToModifiedClosure

namespace MagicArchive.Test;

public class CollectionFormatterTest
{
    private static void CollectionEqual<T>(T value)
        where T : IEnumerable<int>
    {
        var bin = ArchiveSerializer.Serialize(value);
        var value2 = ArchiveSerializer.Deserialize<T>(bin);
        value2.Should().Equal(value);
    }

    private static void CollectionEqualReference<T>(ref T? value, Action<T?> clear)
        where T : class, IEnumerable<int>
    {
        var bin = ArchiveSerializer.Serialize(value);
        var original = value;
        clear(value);
        var expected = ArchiveSerializer.Deserialize<T>(bin);
        ArchiveSerializer.Deserialize(bin, ref value);
        value.Should().Equal(expected);
        value.Should().BeSameAs(original);
    }

    [Test]
    public void List()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var bin = ArchiveSerializer.Serialize(list);

        // no ref
        ArchiveSerializer.Deserialize<List<int>>(bin).Should().Equal(list);

        // ref and same length
        var list2 = new List<int>() { 10, 20, 30, 40, 50 };
        ArchiveSerializer.Deserialize(bin, ref list2);
        list2.Should().Equal(list);

        // ref and differenct length
        var list3 = new List<int>() { 99, 98, 97 };
        ArchiveSerializer.Deserialize(bin, ref list3);
        list3.Should().Equal(list);
    }

    [Test]
    public void Stack()
    {
        void Push(Stack<int> stack, params int[] values)
        {
            foreach (var item in values)
            {
                stack.Push(item);
            }
        }

        var stack = new Stack<int>();
        Push(stack, 1, 2, 3, 4, 5);
        var bin = ArchiveSerializer.Serialize(stack);

        // no ref
        ArchiveSerializer.Deserialize<Stack<int>>(bin).Should().Equal(stack);

        // ref and same length
        var stack2 = new Stack<int>();
        Push(stack2, 10, 20, 30, 40, 50);
        ArchiveSerializer.Deserialize(bin, ref stack2);
        stack2.Should().Equal(stack);

        // ref and differenct length
        var stack3 = new Stack<int>();
        Push(stack3, 99, 98, 97);
        ArchiveSerializer.Deserialize(bin, ref stack3);
        stack3.Should().Equal(stack);
    }

    [Test]
    public void Queue()
    {
        var q = new Queue<int>();
        q.Enqueue(1);
        q.Enqueue(2);
        q.Enqueue(3);
        q.Enqueue(4);
        q.Enqueue(5);

        CollectionEqual(q);
        CollectionEqualReference(ref q, x => x!.Clear());
    }

    [Test]
    public void LinkedList()
    {
        var list = new LinkedList<int>();
        list.AddLast(1);
        list.AddLast(2);
        list.AddLast(3);
        list.AddLast(4);
        list.AddLast(5);

        CollectionEqual(list);
        CollectionEqualReference(ref list, _ => list!.Clear());
    }

    [Test]
    public void HashSet()
    {
        var collection = new HashSet<int> { 1, 2, 3, 4, 5 };

        CollectionEqual(collection);
        CollectionEqualReference(ref collection, _ => collection!.Clear());
    }

    [Test]
    public void PriorityQueue()
    {
        var collection = new PriorityQueue<int, int>();
        collection.Enqueue(1, 10);
        collection.Enqueue(2, 4);
        collection.Enqueue(3, 1231);
        collection.Enqueue(4, 5);
        collection.Enqueue(5, 7);

        var bin = ArchiveSerializer.Serialize(collection);
        var v2 = ArchiveSerializer.Deserialize<PriorityQueue<int, int>>(bin);

        Assert.That(v2, Is.Not.Null);
        collection.Dequeue().Should().Be(v2.Dequeue());
        collection.Dequeue().Should().Be(v2.Dequeue());
        collection.Dequeue().Should().Be(v2.Dequeue());
        collection.Dequeue().Should().Be(v2.Dequeue());
        collection.Dequeue().Should().Be(v2.Dequeue());
    }

    [Test]
    public void Collection()
    {
        {
            var collection = new ObservableCollection<int> { 1, 2, 3, 4, 5 };
            CollectionEqual(collection);
            CollectionEqualReference(ref collection, _ => collection!.Clear());
        }
        {
            var collection = new Collection<int> { 1, 2, 3, 4, 5 };
            CollectionEqual(collection);
            CollectionEqualReference(ref collection, _ => collection!.Clear());
        }
        {
            var collection = new ConcurrentQueue<int>();
            collection.Enqueue(1);
            collection.Enqueue(2);
            collection.Enqueue(3);
            collection.Enqueue(4);
            collection.Enqueue(5);
            CollectionEqual(collection);
            CollectionEqualReference(ref collection, _ => collection!.Clear());
        }
        {
            var collection = new ConcurrentStack<int>();
            collection.Push(1);
            collection.Push(2);
            collection.Push(3);
            collection.Push(4);
            collection.Push(5);
            CollectionEqual(collection);
            CollectionEqualReference(ref collection, _ => collection!.Clear());
        }
        {
            var collection = new ConcurrentBag<int> { 1, 2, 3, 4, 5 };

            var bin = ArchiveSerializer.Serialize(collection);
            // not gurantees order
            ArchiveSerializer.Deserialize<Stack<int>>(bin).Should().BeEquivalentTo(collection);
        }
        {
            var collection = new ReadOnlyCollection<int>([1, 2, 3, 4, 5]);
            CollectionEqual(collection);
        }
        {
            var collection = new ReadOnlyObservableCollection<int>([1, 2, 3, 4, 5]);
            CollectionEqual(collection);
        }
        {
            var collection = new BlockingCollection<int>() { 1, 2, 3, 4, 5 };
            CollectionEqual(collection);
        }
    }

    [Test]
    public void Dictionary()
    {
        using var scope = Assert.EnterMultipleScope();

        {
            var dict = new Dictionary<int, int>
            {
                { 1, 2 },
                { 3, 4 },
                { 4, 5 },
                { 6, 7 },
                { 8, 9 },
            };

            var bin = ArchiveSerializer.Serialize(dict);
            ArchiveSerializer.Deserialize<Dictionary<int, int>>(bin).Should().BeEquivalentTo(dict);
        }
        {
            var dict = new SortedDictionary<int, int>()
            {
                { 1, 2 },
                { 3, 4 },
                { 4, 5 },
                { 6, 7 },
                { 8, 9 },
            };

            var bin = ArchiveSerializer.Serialize(dict);
            ArchiveSerializer.Deserialize<SortedDictionary<int, int>>(bin).Should().BeEquivalentTo(dict);
        }
        {
            var dict = new SortedList<int, int>()
            {
                { 1, 2 },
                { 3, 4 },
                { 4, 5 },
                { 6, 7 },
                { 8, 9 },
            };

            var bin = ArchiveSerializer.Serialize(dict);
            ArchiveSerializer.Deserialize<SortedList<int, int>>(bin).Should().BeEquivalentTo(dict);
        }
        {
            var dict = new ConcurrentDictionary<int, int>();
            dict.TryAdd(1, 2);
            dict.TryAdd(2, 4);
            dict.TryAdd(30, 5);
            dict.TryAdd(4, 8);

            var bin = ArchiveSerializer.Serialize(dict);
            ArchiveSerializer.Deserialize<ConcurrentDictionary<int, int>>(bin).Should().BeEquivalentTo(dict);
        }
    }
}
