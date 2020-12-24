using System;
using Xunit;
using FieldLevel.Services;
using System.Threading.Tasks;

namespace FieldLevel.Tests
{
    public class ExternalDataTests : IClassFixture<GlobalTestSetup>
    {
        private GlobalTestSetup _fixture;
        private ApiService _apiService;

        public ExternalDataTests(GlobalTestSetup fixture)
        {
            this._fixture = fixture;
            this._apiService = new ApiService();
        }

        [Fact]
        public async Task CanGetPostData()
        {
            var posts = await _apiService.GetPostDataAsync();

            Assert.Equal(100, posts.Count);
        }

        [Fact]
        public async Task PostResultsHaveCorrectData()
        {
            var posts = await _apiService.GetPostDataAsync();

            var uniquesByUser = _apiService.GetLastPostForEachUser(posts);

            Assert.Equal(10, uniquesByUser[0].Id);
            Assert.Equal(100, uniquesByUser[9].Id);
        }
    }
}