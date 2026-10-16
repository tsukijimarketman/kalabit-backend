using System;
using System.Collections.Generic;
using System.Text;
using Templating;
using Xunit.Abstractions;

namespace NotificationPlatform.UnitTests;


public class TemplateRendererTests
{
    private readonly ITestOutputHelper _output;
    public TemplateRendererTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region PassedTests before HTML rendering
    [Fact]
    public async Task RenderAsync_ReplacesVariableWithModelValues()
    {
        var renderer = new TemplateRenderer();
        var model = new { FirstName = "Gerick", Salutatory = "Sir" };
        var expected = "Hello Sir Gerick!";
        var actual = await renderer.RenderAsync("Hello {{ Salutatory }} {{ FirstName }}!", model);

        _output.WriteLine($"Actual: {actual}");
        _output.WriteLine($"Expected: {expected}");

        Assert.Equal(expected, actual);

    }

    [Fact]
    public async Task RenderAsync_MissingVariable_RenderAsEmptyAsDefault()
    {
        var renderer = new TemplateRenderer();
        var model = new { FirsName = "Gerick" };
        var expected = "Hello !";
        var actual = await renderer.RenderAsync("Hello {{ LastName }}!", model);

        _output.WriteLine($"Actual: {actual}");
        _output.WriteLine($"Expected: {expected}");

        Assert.Equal(expected, actual);
    }
    #endregion

    #region HTML rendering test

    [Fact]
    public async Task RenderHtmlAsync_EncodesHtmlInModelValues()
    {
        var renderer = new TemplateRenderer();

        var model = new
        {
            FirstName = "Ada",
            Message = "<script>alert(1)</script>"
        };

        var template = """
        <html>
            <body>
                <h1>Hello {{ FirstName }}!</h1>
                <p>{{ Message }}</p>
            </body>
        </html>
        """;

        var actual = await renderer.RenderHtmlAsync(template, model);

        _output.WriteLine("Rendered HTML:");
        _output.WriteLine(actual);

        Assert.Contains("<h1>Hello Ada!</h1>", actual);
        Assert.Contains("&lt;script&gt;alert(1)&lt;/script&gt;", actual);
        Assert.DoesNotContain("<script>", actual);
    }
    #endregion
}
