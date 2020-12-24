using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using FieldLevel.Models;
using Newtonsoft.Json;

namespace FieldLevel.Services
{
    public class ApiService
    {
        private readonly HttpClient _client;
        private const string POST_ENDPOINT = "https://jsonplaceholder.typicode.com/posts";       

        public ApiService()
        {
            this._client = new HttpClient();
        }

        /// <summary>
        /// Retrieves all post data in JSON format from the API
        /// </summary>
        /// <returns>List of Post objects</returns>
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

        /// <summary>
        /// Receives a List of Post objects and returns the last (most recent) Post for each unique user.
        /// </summary>
        /// <param name="posts"></param>
        /// <returns>List of Posts - one each, for distinct users</returns>
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
