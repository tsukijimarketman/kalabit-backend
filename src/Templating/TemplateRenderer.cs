using Fluid;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;

namespace Templating;

public sealed class TemplateRenderer
{
    private static readonly FluidParser Parser = new();

    public async Task<string> RenderAsync(string templateSource, object model)
    {
        ArgumentNullException.ThrowIfNull(templateSource);
        ArgumentNullException.ThrowIfNull(model);

        if (!Parser.TryParse(templateSource, out var template, out var error))
        {
            throw new FormatException($"Template parsing failed: {error}");
        }

        var context = new TemplateContext(model);
        return await template.RenderAsync(context);
    }

    public async Task<string> RenderHtmlAsync(string templateSource, object model)
    {
        ArgumentNullException.ThrowIfNull(templateSource);
        ArgumentNullException.ThrowIfNull(model);

        if(!Parser.TryParse(templateSource, out var template, out var error))
        {
            throw new FormatException($"Template parsing failed: {error}");
        }

        var context = new TemplateContext(model);
        return await template.RenderAsync(context, HtmlEncoder.Default);
    }

    public async Task<string> RenderHtmlAsyncv2(string templateSource, object model)
    {
        ArgumentNullException.ThrowIfNull(templateSource);
        ArgumentNullException.ThrowIfNull(model);

        if (!Parser.TryParse(templateSource, out var template, out var error))
        {
            throw new FormatException($"Template parsing failed: {error}");
        }

        var options = new TemplateOptions { StrictVariables = true };
        var context = new TemplateContext(model, options);
        return await template.RenderAsync(context, HtmlEncoder.Default);
    }
}
