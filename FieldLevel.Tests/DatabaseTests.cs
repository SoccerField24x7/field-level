using System;
using Xunit;
using ServiceStack.Redis;
using FieldLevel.Models;
using System.Threading.Tasks;

namespace FieldLevel.Tests
{
    public class DatabaseTests : IClassFixture<GlobalTestSetup>
    {
        GlobalTestSetup _fixture;
        RedisManagerPool _redispool;
        public DatabaseTests(GlobalTestSetup fixture)
        {
            this._fixture = fixture;
            this._redispool = new RedisManagerPool(fixture._connectionString);
        }

        [Fact]
        public async Task CanConnectRedis()
        {
            bool valid;

            await using var redis = await _redispool.GetClientAsync();

            var redisGuids = redis.As<Post>();

            var testGuid = new Post
            {
                Id = 1,
                UserId = 1,
                Title = "From Here to There",
                Body = "Over the river and through the woods to Grandmother's house we go."
            };

            try
            {
                await redisGuids.StoreAsync(testGuid, TimeSpan.FromSeconds(60));
                valid = true;
            }
            catch (Exception)
            {
                valid = false;
            }
            

            Assert.True(valid);
        }


    }
}
