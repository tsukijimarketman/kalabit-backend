using System;
using System.Collections.Generic;
using System.Text;
using Lib.Enums;

namespace Data.Entities;

public sealed class TemplateVersion : Entity
{
    public Guid TemplateId { get; set; }
    public int VersionNumber { get; set; }
    public required string SubjectTemplate { get; set; }
    public required string HtmlBodyTemplate { get; set; }
    public required string TextBodyTemplate { get; set; }
    public required string VariablesSchemaJson { get; set; }
    public TemplateVersionStatus Status { get; set; } = TemplateVersionStatus.Draft;
    public required string ContentHash { get; set; }
    public Template Template { get; set; } = null!;
}
