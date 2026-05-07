// // @file CodeGenerationTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Scripting.Compiler;
using RetroEngine.Scripting.Model;

namespace RetroEngine.Scripting.Test.Compiler;

public class CodeGenerationTest
{
    [Test]
    public void EmptyClassHasNoBody()
    {
        var testClass = new ScriptClassDefinition("TestClass");
        using var writer = CodeWriterPool.Rent();
        testClass.Emit(writer);
        Assert.That(writer.ToString().Trim(), Is.EqualTo("public class TestClass;"));
    }
}
