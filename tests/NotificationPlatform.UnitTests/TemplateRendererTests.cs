using System;
using System.Collections.Generic;
using System.Text;
using Templating;
using Xunit.Abstractions;
using Fluid;

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
        var actual = await renderer.RenderHtmlAsync("Hello {{ Salutatory }} {{ FirstName }}!", model);

        _output.WriteLine($"Actual: {actual}");
        _output.WriteLine($"Expected: {expected}");

        Assert.Equal(expected, actual);

    }

    [Fact]
    public async Task RenderHtmlAsync_StopsWhenFluidSafetyLimitIsReached()
    {
        var renderer = new TemplateRenderer();
        var model = new { FirsName = "Gerick" };
        var expected = "Hello !";
        //var actual = await renderer.RenderHtmlAsync("Hello {{ LastName }}!", model);

        var exception = await Assert.ThrowsAsync<FluidException>(
            () => renderer.RenderHtmlAsync("Hello {{ LastName }}!", model));
        
        //_output.WriteLine($"Actual: {actual}");
        _output.WriteLine($"Expected: {expected}");
        _output.WriteLine($"Actual: {exception}");

        Assert.Contains("LastName", exception.Message);
        //Assert.Equal(expected, exception.Message);
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

    #region variable is missing

    [Fact]
    public async Task RenderHtmlAsync_ThrowsWhenModelVariableIsMissing()
    {
        var renderer = new TemplateRenderer();

        var model = new { FirsName = "Ada" };

        var exception = await Assert.ThrowsAsync<FluidException>(
            () => renderer.RenderHtmlAsync("Hello {{ LastName }}!", model));

        _output.WriteLine($"Expected a missing-variable error: {exception.Message}");

        Assert.Contains("LastName", exception.Message);
    }
    #endregion

    [Fact]
    public async Task RenderHtmlAsync_StopsWhenTemplateExceedsRecursionLimit()
    {
        var renderer = new TemplateRenderer();
        var template = "{% for number in (1..100000) %}{{ number }}{% endfor %}";

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => renderer.RenderHtmlAsync(template, new { }));

        _output.WriteLine($"Rendering stopped: {exception.Message}");
        Assert.Contains("maximum level of recursion", exception.Message);
    }

    [Fact]
    public async Task RenderHtmlAsync_ThrowsWhenOutputExceedsLimit()
    {
        var renderer = new TemplateRenderer();
        var model = new { Message = new string('x', 256_001) };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => renderer.RenderHtmlAsync("{{ Message }}", model));

        _output.WriteLine($"Output limit enforced: {exception.Message}");
        Assert.Contains("exceeds the limit", exception.Message);
    }


    [Fact]
    public async Task RenderTextAsync_InterpolatesValuesWithoutHtmlEncoding()
    {
        var renderer = new TemplateRenderer();
        var model = new { FirstName = "Ada", Message = "<script>alert(1)</script>" };

        var actual = await renderer.RenderTextAsync(
            "Hello {{ FirstName }}!\n{{ Message }}",
            model);

        var expected = "Hello Ada!\n<script>alert(1)</script>";

        _output.WriteLine($"Actual plain-text body:\n{actual}");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task RenderHtmlAsync_StopsWhenTemplateExceedsConfiguredStepLimit()
    {
        var renderer = new TemplateRenderer(maxSteps: 5);
        var template = string.Concat(
            Enumerable.Repeat("{% assign value = FirstName %}", 7));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => renderer.RenderHtmlAsync(template, new { FirstName = "Ada" }));

        _output.WriteLine($"Rendering stopped: {exception.Message}");
    }
}
