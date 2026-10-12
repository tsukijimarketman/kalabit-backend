using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities;

public abstract class AuditableEntity : Entity
{
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public uint RowVersion { get; set; }
}
