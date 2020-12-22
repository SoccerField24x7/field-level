using System;
using System.Collections.Generic;
using System.Linq;

namespace FieldLevel.Models
{
    public class PostsDataCache : ICacheData
    {
        public static readonly Guid POSTS_CACHE = Guid.Parse("00da1868-6e81-4ac3-bf34-c9e7d175db7e");
        public static readonly Guid LATEST_POSTS_BY_AUTHOR = Guid.Parse("07a4b97d-5029-4f2e-9a52-4415e0192708");

        public PostsDataCache()
        {
        }

        public Guid Id { get; set; }

        public List<Post> Data { get; set; }

        public DateTime? LastCacheDate { get; set; }
    }
}
