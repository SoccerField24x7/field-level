using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using FieldLevel.Models;
using Newtonsoft.Json;

namespace FieldLevel.Helpers
{
    public class ApiHelper
    {
        private HttpClient _client;
        private const string POST_ENDPOINT = "https://jsonplaceholder.typicode.com/posts";       

        public ApiHelper()
        {
            this._client = new HttpClient();
        }

        public async Task<List<Post>> GetPostDataAsync()
        {
            var response = await _client.GetAsync(POST_ENDPOINT);
            var results = new List<Post>();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException();
            }

            var content = await response.Content.ReadAsStringAsync();
            results = await Task.Run(() => JsonConvert.DeserializeObject<List<Post>>(content));

            return results;
        }

        public List<Post> GetLastPostForEachUser(List<Post> posts)
        {
            return posts.GroupBy(s => s.UserId)
                        .Select(s => s.OrderByDescending(x => x.Id)
                        .FirstOrDefault())
                        .OrderBy(x => x.UserId)
                        .ToList();
        }
    }
}
