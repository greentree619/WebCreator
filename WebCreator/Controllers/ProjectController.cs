using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebCreator.Models;
using SerpApi;
using System.Collections;
using Microsoft.AspNetCore.Cors;

namespace WebCreator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectController : ControllerBase
    {
        //IFirebaseConfig config = new FirebaseConfig
        //{
        //    AuthSecret = "5cGJIcs3WRWYyu8XTFyjy8zW87Ql2ey44IOZvIRO",
        //    BasePath = "https://websitecreator-94452-default-rtdb.firebaseio.com"
        //};
        IFirebaseClient client;

        private readonly ILogger<ProjectController> _logger;

        public ProjectController(ILogger<ProjectController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Project> Get()
        {
            client = new FireSharp.FirebaseClient(Config.firebaseConfig);
            FirebaseResponse response = client.Get("Projects");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<Project>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    list.Add(JsonConvert.DeserializeObject<Project>(((JProperty)item).Value.ToString()));
                }
            }

            return list.ToArray();
        }

        [HttpPost]
        public ActionResult AddProject([FromBody] Project projectInput)
        {
            bool addOK = false;
            var project = new Project
            {
                Name = projectInput.Name,
                Keyword = projectInput.Keyword,                
                QuesionsCount = projectInput.QuesionsCount,
                Language = projectInput.Language,
            };

            try
            {
                client = new FireSharp.FirebaseClient(Config.firebaseConfig);
                var data = project;
                //{{Unique Domain
                FirebaseResponse resp = client.Get("Projects", FireSharp.QueryBuilder.New().OrderBy("Name").StartAt(data.Name).LimitToFirst(1));
                dynamic allProject = JsonConvert.DeserializeObject<dynamic>(resp.Body);
                //List<Project> sameWebsites =  allProject.Where(proj => proj.Name == data.Name).ToList();
                //}}Unique Domain
                //if(sameWebsites.Count == 0)
                {
                    PushResponse response = client.Push("Projects/", data);
                    data.Id = response.Result.name;
                    SetResponse setResponse = client.Set("Projects/" + data.Id, data);

                    if (setResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        ModelState.AddModelError(string.Empty, "Added Succesfully");
                        addOK = true;
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Something went wrong!!");
                    }
                }
            }
            catch (Exception ex)
            {

                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return Ok(addOK);
        }

        //{{
        [HttpGet("serpapi/{_id}/{keyword}")]
        //==
        //[Route("serpapi")]
        //[HttpPost]
        //}}
        public ActionResult serpapi(String _id, String keyword)
        {
            GoogleSearch(_id, keyword);
            return Ok();
        }

        public void GoogleSearch(String _id, String keyword)
        {
            // secret api key from https://serpapi.com/dashboard
            String apiKey = Config.SerpApiKey;

            // Localized search for Coffee shop in Austin Texas
            Hashtable ht = new Hashtable();
            ht.Add("location", "Austin, Texas, United States");
            ht.Add("q", keyword);

            try
            {
                client = new FireSharp.FirebaseClient(Config.firebaseConfig);
                GoogleSearch search = new GoogleSearch(ht, apiKey);
                JObject data = search.GetJson();
                JArray questions = (JArray)data["related_questions"];
                if( questions != null )
                    foreach (JObject question in questions)
                    {
                        //Console.WriteLine("Question: " + question["question"]);
                        var article = new Article
                        {
                            ProjectId = _id,
                            Title = (String)question["question"],
                        };
                        var articleData = article;
                        PushResponse response = client.Push("Articles/", articleData);
                        articleData.Id = response.Result.name;
                        SetResponse setResponse = client.Set("Articles/" + articleData.Id, articleData);
                    }
                // close socket
                search.Close();
            }
            catch (SerpApiSearchException ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}