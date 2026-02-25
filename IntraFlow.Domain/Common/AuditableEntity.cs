using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; protected set; }
    public DateTime LastUpdatedAt { get; protected set; }
}
