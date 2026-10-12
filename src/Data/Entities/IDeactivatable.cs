using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities;

public interface IDeactivatable
{
    bool IsActive { get; set; }
}
