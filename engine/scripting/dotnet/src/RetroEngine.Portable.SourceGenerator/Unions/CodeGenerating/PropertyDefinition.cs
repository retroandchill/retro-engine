// // @file PropertyDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;

namespace RetroEngine.Portable.SourceGenerator.Unions.CodeGenerating;

public sealed partial class PropertyDefinition
{
    public Accessibility? Accessibility { get; init; }

    public bool IsStatic { get; init; }

    public required TypeName TypeName { get; init; }

    public required string Name { get; init; }

    public PropertyAccessor? Getter { get; init; }

    public PropertyAccessor? Setter { get; init; }

    public bool IsInitOnly { get; init; }

    public string? Initializer { get; init; }

    public IReadOnlyList<string> Attributes { get; init; } = [];

    public readonly record struct PropertyAccessor(Accessibility? Accessibility, PropertyAccessorImpl Impl);

    public readonly struct PropertyAccessorImpl
    {
        private Action<PropertyDefinition, CodeWriter>? BodyWriter { get; init; }

        public static PropertyAccessorImpl Auto() => new();

        public static PropertyAccessorImpl Bodied(Action<PropertyDefinition, CodeWriter> bodyWriter)
        {
            return new PropertyAccessorImpl { BodyWriter = bodyWriter };
        }

        public void Match(Action autoCase, Action<Action<PropertyDefinition, CodeWriter>> bodiedCase)
        {
            if (BodyWriter is null)
            {
                autoCase();
            }
            else
            {
                bodiedCase(BodyWriter);
            }
        }
    }
}
