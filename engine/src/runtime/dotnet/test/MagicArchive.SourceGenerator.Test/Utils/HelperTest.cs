// // @file TemplateHelperTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HandlebarsDotNet;
using MagicArchive.SourceGenerator.Utils;

namespace MagicArchive.SourceGenerator.Test.Utils;

public class HelperTest
{
    [Test]
    public void CanJoinTemplateItemsWithASeparator()
    {
        var handlebars = Handlebars.Create();
        handlebars.Configuration.TextEncoder = null;
        handlebars.RegisterHelper("Joined", Helpers.Joined);

        var items = new[] { "foo", "bar", "baz" };

        const string template = "{{#Joined \", \" @this}}{{.}}{{/Joined}}";
        var compiledTemplate = handlebars.Compile(template);
        var result = compiledTemplate(items);
        Assert.That(result, Is.EqualTo("foo, bar, baz"));
    }
}
