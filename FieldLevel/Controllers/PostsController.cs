using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.Redis;
using System.IO;
using FieldLevel.Models;

namespace FieldLevel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : Controller
    {
        private readonly ILogger<PostsController> _logger;
        private readonly IRedisClient _redis;

        public PostsController(ILogger<PostsController> logger, IRedisClient redis)
        {
            _logger = logger;
            _redis = redis;
        }

        [HttpGet]
        public JsonResult Main()
        {
            var redisPosts = _redis.As<Post>();

            var testPost = new Post
            {
                Id = redisPosts.GetNextSequence(),
                UserId = 10,
                Title = "From Here to There",
                Body = "Over the river and through the woods..."
            };

            try
            {
                _redis.Store(testPost);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Json(testPost);
        }
    }
}
