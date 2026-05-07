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
        const string expectedOutput = """
            #nullable enable

            namespace Test.Namespace;

            public class TestClass;
            """;
        var testClass = new ScriptClassDefinition("Test.Namespace", "TestClass");
        using var writer = CodeWriterPool.Rent();
        testClass.Emit(writer);
        Assert.That(writer.ToString().Trim(), Is.EqualTo(expectedOutput));
    }

    [Test]
    public void InvalidNamespaceOrClassNameThrows()
    {
        Assert.Throws<ArgumentException>(() => _ = new ScriptClassDefinition("Test.Namespace", "Test.Class"));
        Assert.Throws<ArgumentException>(() => _ = new ScriptClassDefinition("Test Namespace", "Test.Class"));
        Assert.Throws<ArgumentException>(() => _ = new ScriptClassDefinition("Test.Namespace", "Test Class"));
    }

    [Test]
    public void GenerateClassWithParentTypes()
    {
        const string expectedOutput = """
            #nullable enable

            namespace Test.Namespace;

            public class TestClass : Test.BaseClass, Test.Interface1, Test.Interface2;
            """;
        var baseClass = new TypeSpecifier("Test.BaseClass");
        var interface1 = new TypeSpecifier("Test.Interface1");
        var interface2 = new TypeSpecifier("Test.Interface2");

        var testClass = new ScriptClassDefinition("Test.Namespace", "TestClass") { BaseType = baseClass };
        testClass.Interfaces.Add(interface1);
        testClass.Interfaces.Add(interface2);
        using var writer = CodeWriterPool.Rent();
        testClass.Emit(writer);
        Assert.That(writer.ToString().Trim(), Is.EqualTo(expectedOutput));
    }
}
