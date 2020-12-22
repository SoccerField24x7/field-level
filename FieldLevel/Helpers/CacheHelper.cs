using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FieldLevel.Models;
using ServiceStack.Redis;

namespace FieldLevel.Helpers
{
    public class CacheHelper
    {
        private IRedisClientsManagerAsync _manager;

        public CacheHelper(IRedisClientsManagerAsync redisManager = null)
        {
            this._manager = redisManager;
        }

        public async Task CachePostList(List<Post> posts, TimeSpan? span = null)
        {
            await using var redis = await _manager.GetClientAsync();
            var redisPosts = redis.As<Post>();

            try
            {
                await redisPosts.StoreAllAsync(posts);
                if (span != null)
                {
                    foreach (var post in posts)
                    {
                        await redisPosts.ExpireInAsync(post.Id, (TimeSpan)span);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<PostsDataCache> CacheLastPostsByUser(PostsDataCache cacheObject, TimeSpan? span = null)
        {
            await using var redis = await _manager.GetClientAsync();

            var cacheType = redis.As<PostsDataCache>();

             var result = await cacheType.StoreAsync(cacheObject);

            if (span != null)
                await cacheType.ExpireInAsync(cacheObject.Id, (TimeSpan)span);

            return result;
        }

        public async Task<IRedisClientAsync> GetRedisClient()
        {
            return await _manager.GetClientAsync() ?? null;
        }
    }
}
