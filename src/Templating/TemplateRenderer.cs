using Fluid;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;

namespace Templating;

public sealed class TemplateRenderer
{
    private static readonly FluidParser Parser = new();
    private const int MaxOutputCharacters = 256_000;
    private const int DefaultMaxSteps = 10_000;
    private readonly int _maxSteps;

    public TemplateRenderer(int maxSteps = DefaultMaxSteps)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxSteps);
        _maxSteps = maxSteps;
    }

    public Task<string> RenderHtmlAsync(string templateSource, object model)
    {
        return RenderAsync(templateSource, model, encodeHtml: true);
    }

    public Task<string> RenderTextAsync(string templateSource, object model)
    {
        return RenderAsync(templateSource, model, encodeHtml: false);
    }

    public async Task<string> RenderAsync(string templateSource, object model, bool encodeHtml)
    {
        ArgumentNullException.ThrowIfNull(templateSource);
        ArgumentNullException.ThrowIfNull(model);

        if(!Parser.TryParse(templateSource, out var template, out var error))
        {
            throw new FormatException($"Template parsing failed: {error}");
        }

        var options = new TemplateOptions
        {
            StrictVariables = true,
            MaxSteps = _maxSteps
        };

        var context = new TemplateContext(model, options);
        using var output = new LimitedTextWriter(MaxOutputCharacters);

        if (encodeHtml)
        {
            await template.RenderAsync(output, context, HtmlEncoder.Default);
        }
        else
        {
            await template.RenderAsync(output, context);
        }

        return output.ToString();
    }

}
