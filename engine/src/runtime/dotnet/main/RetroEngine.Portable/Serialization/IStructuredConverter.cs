// // @file IStructuredConverter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Serialization;

public interface IStructuredConverter;

public interface IStructuredConverter<T> : IStructuredConverter
{
    T Read<TReader>(ref TReader reader)
        where TReader : IStructuredReader, allows ref struct;
    void Write<TWriter>(ref TWriter writer, T value)
        where TWriter : IStructuredWriter, allows ref struct;
}
