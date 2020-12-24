using System;
using System.Collections.Generic;

namespace FieldLevel.Models
{
    public interface ICacheData
    {
        public Guid Id { get; set; }
        public List<Post> Data { get; set; }
        public DateTime? LastCacheDate { get; set; }
    }
}
