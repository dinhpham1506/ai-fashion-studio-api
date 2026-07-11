using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Domain.Common
{
    /// <summary>
    /// Entity chung để các entity khác kế thừa, có Id và CreatedAt
    /// </summary>
    public abstract class BaseEntity
    {
        public Guid Id { get; protected set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    }
}