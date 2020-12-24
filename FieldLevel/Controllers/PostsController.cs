using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServiceStack.Redis;
using FieldLevel.Models;
using FieldLevel.Services;

namespace FieldLevel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : Controller
    {
        // injected dependencies
        private readonly ILogger<PostsController> _logger;
        private readonly CacheService _cacheHelper;
        private readonly ApiService _apiHelper;
        
        private static Random random = new Random();
        private const int CACHE_EXPIRY = 60000;

        public PostsController(ILogger<PostsController> logger, CacheService cacheHelper, ApiService apiHelper) // rename to CacheService
        {
            this._logger = logger;
            this._cacheHelper = cacheHelper;
            this._apiHelper = apiHelper;
        }

        [HttpGet]
        public async Task<JsonResult> GetLastPostByAuthors()
        {
            List <Post> list = new List<Post>();
            var typedClient = (await _cacheHelper.GetRedisClientAsync()).As<PostsDataCache>();
            
            var posts = await typedClient.GetByIdAsync(PostsDataCache.LATEST_POSTS_BY_AUTHOR);

            list = posts?.Data;

            if (posts == null)
            {
                list = await HandlePostCaching();
            }

            return Json(list);
        }

        private async Task<List<Post>> HandlePostCaching()
        {
            // no posts, let's get them
            List<Post> list = new List<Post>();
            List<Post> allPosts = new List<Post>();

            try
            {
                allPosts = await _apiHelper.GetPostDataAsync();
                await _cacheHelper.CachePostListAsync(allPosts); // store everything for fallback
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex; // TODO: issue a pretty error, for now just blow up
            }
            
            list = _apiHelper.GetLastPostForEachUser(allPosts);

            // now cache them
            PostsDataCache cacheItem = new PostsDataCache(PostsDataCache.LATEST_POSTS_BY_AUTHOR, list, DateTime.UtcNow);
            var result = await _cacheHelper.CacheLastPostsByUserAsync(cacheItem, TimeSpan.FromMilliseconds(CACHE_EXPIRY));
            
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