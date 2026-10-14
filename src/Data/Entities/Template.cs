using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities;

public sealed class Template : AuditableEntity, IDeactivatable
{
    public required string TemplateKey { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<TemplateVersion> Versions { get; set; } = new List<TemplateVersion>();
}
