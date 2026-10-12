using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
