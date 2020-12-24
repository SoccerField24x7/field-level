using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FieldLevel.Models;
using ServiceStack.Redis;

namespace FieldLevel.Services
{
    public class CacheService
    {
        private IRedisClientsManagerAsync _manager;

        public CacheService(IRedisClientsManagerAsync redisManager = null)
        {
            this._manager = redisManager;
        }

        /// <summary>
        /// Store a single post object to the Redis cache
        /// </summary>
        /// <param name="post">The post object to be stored</param>
        /// <param name="span">Optional TimeSpan - controls if/when the stored object expires</param>
        /// <returns>True if successful, False if fails</returns>
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

        /// <summary>
        /// Stores a list of Post objects, as individual Posts, to the Redis cache 
        /// </summary>
        /// <param name="posts">List of Post objects</param>
        /// <param name="span">Optional TimeSpan - controls if/when the stored object expires</param>
        /// <returns>True if successful, False if fails</returns>
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

        /// <summary>
        /// Stores the PostDataCache single object, which contains most recent post
        /// for each user, in the Redis cache
        /// </summary>
        /// <param name="cacheObject">PostDataCache</param>
        /// <param name="span">Optional TimeSpan - controls if/when the stored object expires</param>
        /// <returns>PostDataCache on success, null on failure</returns>
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
                // TODO: what?
            }          

            return result;
        }

        /// <summary>
        /// Gets a Redis client from the ClientsManagerPool
        /// </summary>
        /// <returns>The client or null if not successful</returns>
        public async Task<IRedisClientAsync> GetRedisClientAsync()
        {
            return await _manager.GetClientAsync() ?? null;
        }
    }
}
