using System;
using Xunit;
using ServiceStack.Redis;
using FieldLevel.Models;
using System.Threading.Tasks;
using System.Linq;
using FieldLevel.Helpers;
using ServiceStack.Redis.Generic;
using System.Collections.Generic;

namespace FieldLevel.Tests
{
    public class DatabaseTests : IClassFixture<GlobalTestSetup>
    {
        private GlobalTestSetup _fixture;
        private RedisManagerPool _redispool;
        private static Random random = new Random();

        public DatabaseTests(GlobalTestSetup fixture)
        {
            this._fixture = fixture;
            this._redispool = new RedisManagerPool(fixture._connectionString);
        }

        [Fact]
        public async Task CanConnectToAndStoreInRedis()
        {
            bool valid;

            await using var redis = await _redispool.GetClientAsync();
            var redisPostsClient = redis.As<Post>(); // get the typed client

            var testPost = await CreateTestPost(redisPostsClient);

            try
            {
                await redis.StoreAsync(testPost);
                valid = true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                valid = false;
            }
            

            Assert.True(valid);
        }

        [Fact]
        public async Task CacheDataExpires()
        {
            await using var redis = await _redispool.GetClientAsync();
            var redisPosts = redis.As<Post>();
            var testPost = await CreateTestPost(redisPosts);
            bool success = false;
            

            try
            {
                var result = await redisPosts.StoreAsync(testPost);
                await redisPosts.ExpireInAsync(testPost.Id, TimeSpan.FromMilliseconds(20000));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                success = false;  // TODO: force this to exist early
            }

            var retPost = await redisPosts.GetByIdAsync(testPost.Id);
            Assert.NotNull(retPost);  // not normally a fan of multiple assertions, but have to be sure

            // Wait while cache expires
            await Task.Delay(30000);
            
            // get the record again
            retPost = await redisPosts.GetByIdAsync(testPost.Id);
            Assert.Null(retPost);

        }

        [Fact]
        public async Task CachedLastPostsExpires()
        {
            await using var redis = await _redispool.GetClientAsync();
            var redisCacheClient = redis.As<PostsDataCache>();
            bool success = false;

            PostsDataCache cacheItem = new PostsDataCache()
            {
                Id = PostsDataCache.LATEST_POSTS_BY_AUTHOR,
                Data = await CreateTestPostList(2),
                LastCacheDate = DateTime.UtcNow
            };

            var helper = new CacheHelper(_redispool);

            var result = await helper.CacheLastPostsByUser(cacheItem, TimeSpan.FromSeconds(20));

            // retrieve it (got it)
            var expiringPost = await redisCacheClient.GetByIdAsync(cacheItem.Id);
            Assert.NotNull(expiringPost);

            // sleep
            await Task.Delay(22000);

            // retrieve it again (fail)
            var expiredPost = await redisCacheClient.GetByIdAsync(cacheItem.Id);
            Assert.Null(expiredPost);

        }

        private async Task<Post> CreateTestPost(IRedisTypedClientAsync<Post> redis, int userId=10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            var testPost = new Post
            {
                Id = await redis.GetNextSequenceAsync(),
                UserId = userId,
                Title = "A random test post",
                Body = new string(Enumerable.Range(1, 35).Select(_ => chars[random.Next(chars.Length)]).ToArray())
            };

            return testPost;
        }

        private async Task<List<Post>> CreateTestPostList(int total)
        {
            await using var redis = await _redispool.GetClientAsync();

            int i;
            var redisPostClient = redis.As<Post>();
            List<Post> list = new List<Post>();

            for (i = 0; i < total; i++)
            {
                list.Add(await CreateTestPost(redisPostClient, random.Next(1, 100)));
            }

            return list;
        }
    }
}
