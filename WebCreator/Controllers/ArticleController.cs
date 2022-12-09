using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebCreator.Models;

namespace WebCreator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArticleController : ControllerBase
    {
        //IFirebaseConfig config = new FirebaseConfig
        //{
        //    AuthSecret = Config.FirebaseAuthSecret,
        //    BasePath = Config.FirebaseBasePath
        //};
        IFirebaseClient client;

        private readonly ILogger<ProjectController> _logger;

        public ArticleController(ILogger<ProjectController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Article> Get()
        {
            client = new FireSharp.FirebaseClient(Config.firebaseConfig);
            FirebaseResponse response = client.Get("Articles");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<Article>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    list.Add(JsonConvert.DeserializeObject<Article>(((JProperty)item).Value.ToString()));
                }
            }

            return list.ToArray();
        }
    }
}