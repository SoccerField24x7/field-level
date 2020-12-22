using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FieldLevel.Models;
using ServiceStack.Redis;
//using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
//using Newtonsoft.Json;

namespace FieldLevel.Helpers
{
    public class ApiHelper
    {
        private HttpClient _client;
        private IRedisClientsManagerAsync _manager;
        private readonly string POST_ENDPOINT = "https://jsonplaceholder.typicode.com/posts";       

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
                //what?
            }

            var content = await response.Content.ReadAsStringAsync();
            results = await Task.Run(() => JsonConvert.DeserializeObject<List<Post>>(content));

            return results;
        }
    }
}
