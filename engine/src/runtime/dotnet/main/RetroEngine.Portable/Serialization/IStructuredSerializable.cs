// // @file IStructuredReadable.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Serialization;

public interface IStructuredReadable<out T>
    where T : IStructuredReadable<T>
{
    public static abstract T Deserialize<TReader>(ref TReader reader)
        where TReader : IStructuredReader, allows ref struct;
}

public interface IStructuredWritable<T>
    where T : IStructuredWritable<T>
{
    public static abstract void Serialize<TWriter>(ref TWriter writer, scoped in T value)
        where TWriter : IStructuredWriter, allows ref struct;
}

public interface IStructuredSerializable<T> : IStructuredReadable<T>, IStructuredWritable<T>
    where T : IStructuredSerializable<T>;
