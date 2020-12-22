using System;
using Xunit;
using FieldLevel.Helpers;
using System.Threading.Tasks;
using System.Linq;

namespace FieldLevel.Tests
{
    public class ExternalDataTests
    {
        [Fact]
        public async Task CanGetPostData()
        {
            var client = new ApiHelper();

            var posts = await client.GetPostDataAsync();

            Assert.Equal(100, posts.Count);
        }

        [Fact]
        public async Task PostResultsHaveCorrectData()
        {
            var client = new ApiHelper();

            var posts = await client.GetPostDataAsync();

            var uniquesByUser = posts.GroupBy(s => s.UserId)
                                    .Select(s => s.OrderByDescending(x => x.Id)
                                    .FirstOrDefault()                                    )
                                    .OrderBy(x => x.UserId)
                                    .ToList(); // SO 470440

            Assert.Equal(10, uniquesByUser[0].Id);
            Assert.Equal(100, uniquesByUser[9].Id);
        }
    }
}