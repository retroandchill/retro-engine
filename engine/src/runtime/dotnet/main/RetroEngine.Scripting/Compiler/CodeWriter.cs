// // @file CodeWriter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Cysharp.Text;

namespace RetroEngine.Scripting.Compiler;

public static class CodeWriterPool
{
    private static readonly ConcurrentQueue<CodeWriter> Writers = new();

    public static CodeWriter Rent()
    {
        if (!Writers.TryDequeue(out var writer))
        {
            writer = new CodeWriter();
        }

        writer.Initialize();
        return writer;
    }

    internal static void Return(CodeWriter writer)
    {
        writer.Reset();
        Writers.Enqueue(writer);
    }
}

public sealed class CodeWriter : IDisposable
{
    private static readonly string NewLine = Environment.NewLine;
    private const int IndentSize = 4;

    private Utf16ValueStringBuilder _builder;
    private int _indentLevel;
    private bool _needToIndent;

    internal CodeWriter() { }

    internal void Initialize()
    {
        _builder = ZString.CreateStringBuilder();
        _indentLevel = 0;
        _needToIndent = true;
    }

    internal void Reset()
    {
        _builder.Dispose();
    }

    public void Append(char character)
    {
        _builder.Append(character);
    }

    public void Append(string text)
    {
        Append(text.AsSpan());
    }

    public void Append(ReadOnlySpan<char> text)
    {
        AppendIndent();
        _builder.Append(text);
    }

    public void Append<T>(T? value)
    {
        Append(value?.ToString() ?? "");
    }

    public void AppendLine()
    {
        _builder.Append(NewLine);
        _needToIndent = true;
    }

    public void AppendLine(char character)
    {
        Append(character);
        _builder.Append(NewLine);
        _needToIndent = true;
    }

    public void AppendLine(ReadOnlySpan<char> text)
    {
        Append(text);
        _builder.Append(NewLine);
        _needToIndent = true;
    }

    public void AppendLine(string text)
    {
        AppendLine(text.AsSpan());
    }

    public void AppendLine<T>(T? value)
    {
        AppendLine(value?.ToString() ?? "");
    }

    private void AppendIndent()
    {
        if (!_needToIndent)
            return;
        _builder.Append(' ', _indentLevel * IndentSize);
        _needToIndent = false;
    }

    public void EnterBlock()
    {
        AppendLine('{');
        _indentLevel++;
    }

    public IndentScope EnterBlockScope() => new(this);

    public void ExitBlock()
    {
        AppendLine('}');
        _indentLevel--;
    }

    public override string ToString() => _builder.ToString();

    public void Dispose()
    {
        CodeWriterPool.Return(this);
    }

    public readonly ref struct IndentScope : IDisposable
    {
        private readonly CodeWriter _writer;

        public IndentScope(CodeWriter writer)
        {
            _writer = writer;
            _writer.EnterBlock();
        }

        public void Dispose()
        {
            _writer.ExitBlock();
        }
    }
}
