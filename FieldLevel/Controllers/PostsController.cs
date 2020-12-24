using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceStack.Redis;
using FieldLevel.Models;
using FieldLevel.Helpers;

namespace FieldLevel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : Controller
    {
        private readonly ILogger<PostsController> _logger;
        private readonly IRedisClientsManagerAsync _manager;
        private static Random random = new Random();
        private const int CACHE_EXPIRY = 60000;

        public PostsController(ILogger<PostsController> logger, IRedisClientsManagerAsync redisManager)
        {
            _logger = logger;
            this._manager = redisManager;
        }

        [HttpGet]
        public async Task<JsonResult> GetLastPostByAuthors()
        {
            List <Post> list = new List<Post>();
            var cacheHelper = new CacheHelper(_manager);
            var typedClient = (await cacheHelper.GetRedisClientAsync()).As<PostsDataCache>();
            
            var posts = await typedClient.GetByIdAsync(PostsDataCache.LATEST_POSTS_BY_AUTHOR);

            list = posts?.Data;

            if (posts == null)
            {
                list = await HandlePostCaching(cacheHelper);
            }

            return Json(list);
        }

        private async Task<List<Post>> HandlePostCaching(CacheHelper cacheHelper)
        {
            // no posts, let's get them
            var apiHelper = new ApiHelper();
            List<Post> list = new List<Post>();
            List<Post> allPosts = new List<Post>();

            try
            {
                allPosts = await apiHelper.GetPostDataAsync();
                await cacheHelper.CachePostListAsync(allPosts); // store everything for fallback
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                // TODO: issue a pretty error, for now just blow up
                throw ex;
            }
            
            list = apiHelper.GetLastPostForEachUser(allPosts);

            // now cache them
            PostsDataCache cacheItem = new PostsDataCache(PostsDataCache.LATEST_POSTS_BY_AUTHOR, list, DateTime.UtcNow);

            var result = await cacheHelper.CacheLastPostsByUserAsync(cacheItem, TimeSpan.FromMilliseconds(CACHE_EXPIRY));
            if (result == null)
            {
                _logger.LogError("Could not store the Last Posts object.");
                // TODO: issue a pretty error
                throw new Exception("Could not store the Last Posts object.");
            }

            return result.Data;
        }
    }
}