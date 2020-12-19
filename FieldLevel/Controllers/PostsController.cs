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
        private readonly IRedisClientsManagerAsync _manager;
        private static Random random = new Random();

        public PostsController(ILogger<PostsController> logger, IRedisClientsManagerAsync redisManager)
        {
            _logger = logger;
            this._manager = redisManager;
        }

        [HttpGet]
        public async Task<JsonResult> Main()
        {
            await using var redis = await _manager.GetClientAsync();
            var redisPosts = redis.As<Post>();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var testPost = new Post
            {
                Id = await redisPosts.GetNextSequenceAsync(),
                UserId = 10,
                Title = "From Here to There",
                Body = new string(Enumerable.Range(1, 35).Select(_ => chars[random.Next(chars.Length)]).ToArray())
            };

            try
            {
                await redis.StoreAsync(testPost);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Json(testPost);
        }
    }
}
