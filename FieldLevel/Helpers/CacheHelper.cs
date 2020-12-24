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

        public async Task<bool> CachePostAsync(Post post, TimeSpan? span = null)
        {
            if (_manager == null)
                throw new Exception("Redis connection manager not found.");

            await using var redis = await _manager.GetClientAsync();
            var redisPosts = redis.As<Post>();

            try
            {
                await redisPosts.StoreAsync(post);
                if (span != null)
                    await redisPosts.ExpireInAsync(post.Id, (TimeSpan)span);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> CachePostListAsync(List<Post> posts, TimeSpan? span = null)
        {
            if (_manager == null)
                throw new Exception("Redis connection manager not found.");

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
                return false;
            }

            return true;
        }

        public async Task<PostsDataCache> CacheLastPostsByUserAsync(PostsDataCache cacheObject, TimeSpan? span = null)
        {
            if (_manager == null)
                throw new Exception("Redis connection manager not found.");

            await using var redis = await _manager.GetClientAsync();
            var cacheType = redis.As<PostsDataCache>();
            PostsDataCache result = null;

            try
            {
                result = await cacheType.StoreAsync(cacheObject);

                if (span != null)
                {
                    await cacheType.ExpireInAsync(cacheObject.Id, (TimeSpan)span);
                }
            }
            catch (Exception ex)
            {
                // change this to log exception
            }          

            return result;
        }

        public async Task<IRedisClientAsync> GetRedisClientAsync()
        {
            return await _manager.GetClientAsync() ?? null;
        }
    }
}
