using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AiFashionStudio.Platform.Domain.Common
{
    /// <summary>
    ///  Entity chung có thể được cập nhật, có thuộc tính UpdatedAt để lưu thời gian cập nhật cuối cùng
    /// </summary>
    public abstract class UpdatableEntity : BaseEntity
    {
        public DateTime UpdatedAt { get; protected set; } = DateTime.UtcNow;

        public void Update() => UpdatedAt = DateTime.UtcNow;
    }
}