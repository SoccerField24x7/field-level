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
            await using var redis = await _redispool.GetClientAsync();
            var redisPostsClient = redis.As<Post>(); // get the typed client

            var testPost = await CreateTestPost();

            var result = await redis.StoreAsync(testPost);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CacheDataExpires()
        {
            await using var redis = await _redispool.GetClientAsync();
            var redisPosts = redis.As<Post>();
            var testPost = await CreateTestPost();
            var helper = new CacheHelper(_redispool);

            await helper.CachePostAsync(testPost, TimeSpan.FromMilliseconds(10000));  // test millisecs

            var result = await redisPosts.GetByIdAsync(testPost.Id);
            Assert.NotNull(result);  // not normally a fan of multiple assertions, but have to be sure

            // Wait while cache expires
            await Task.Delay(15000);
            
            // get the record again
            result = await redisPosts.GetByIdAsync(testPost.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task CachePostList()
        {
            var helper = new CacheHelper(_redispool);
            var list = await CreateTestPostList(100);

            var result = await helper.CachePostListAsync(list);

            Assert.True(result);

            // clean-up
            await (await helper.GetRedisClientAsync())
                .As<Post>()
                .DeleteByIdsAsync(list.Select(x => x.Id));
        }

        [Fact]
        public async Task CachedLastPostsExpires()
        {
            await using var redis = await _redispool.GetClientAsync();
            var redisCacheClient = redis.As<PostsDataCache>();
            
            PostsDataCache cacheItem = new PostsDataCache()
            {
                Id = PostsDataCache.LATEST_POSTS_BY_AUTHOR,
                Data = await CreateTestPostList(2),
                LastCacheDate = DateTime.UtcNow
            };

            var helper = new CacheHelper(_redispool);

            var result = await helper.CacheLastPostsByUserAsync(cacheItem, TimeSpan.FromSeconds(10)); // test secs

            var expiringPost = await redisCacheClient.GetByIdAsync(cacheItem.Id);
            Assert.NotNull(expiringPost);

            await Task.Delay(15000);

            var expiredPost = await redisCacheClient.GetByIdAsync(cacheItem.Id);
            Assert.Null(expiredPost);
        }

        [Fact]
        public async Task CanGetRedisClient()
        {
            var helper = new CacheHelper(_redispool);
            await using var control = await _redispool.GetClientAsync();
            var redisClient = await helper.GetRedisClientAsync();

            Assert.IsType(control.GetType(), redisClient);
        }

        private async Task<Post> CreateTestPost(int userId=10)
        {
            await using var redis = await _redispool.GetClientAsync();
            var redisPostClient = redis.As<Post>();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            
            var testPost = new Post
            {
                Id = await redisPostClient.GetNextSequenceAsync(),
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
                list.Add(await CreateTestPost(random.Next(1, 100)));
            }

            return list;
        }
    }
}
