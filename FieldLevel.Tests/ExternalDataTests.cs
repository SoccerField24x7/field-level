using System;
using Xunit;
using FieldLevel.Helpers;
using System.Threading.Tasks;

namespace FieldLevel.Tests
{
    public class ExternalDataTests
    {
        [Fact]
        public async Task CanGetPostData()
        {
            var helper = new ApiHelper();

            var posts = await helper.GetPostDataAsync();

            Assert.Equal(100, posts.Count);
        }

        [Fact]
        public async Task PostResultsHaveCorrectData()
        {
            var helper = new ApiHelper();

            var posts = await helper.GetPostDataAsync();

            var uniquesByUser = helper.GetLastPostForEachUser(posts);

            Assert.Equal(10, uniquesByUser[0].Id);
            Assert.Equal(100, uniquesByUser[9].Id);
        }
    }
}