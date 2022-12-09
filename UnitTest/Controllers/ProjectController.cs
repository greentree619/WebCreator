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
using Microsoft.Extensions.Logging;
using FirebaseSharp.Portable;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;
using System.Threading.Channels;
using System.IO;
using System.Linq;
using System.Web;
using UnitTest.Lib;

namespace WebCreator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectController : ControllerBase
    {
        public class Item
        {
            public int curPage { get; set; }
            public int total { get; set; }
            public IEnumerable<Project> data { get; set; }
        }

        private readonly ILogger<ProjectController> _logger;

        public ProjectController(ILogger<ProjectController> logger)
        {
            _logger = logger;            
        }

        [HttpGet("{page}/{count}")]
        public async Task<IActionResult> GetAsync(int page=1, int count=5)
        {
            if (page < 0) page = 1;
            if (count < 0) count = 5;
            var list = new List<Project>();
            int total = 0;
            try
            {
                CollectionReference projectsCol = Config.FirebaseDB.Collection("Projects");
                QuerySnapshot totalSnapshot = await projectsCol.GetSnapshotAsync();
                total = (int)Math.Round( (double)totalSnapshot.Count/count );
                Query query = projectsCol.OrderByDescending("CreatedTime").Offset((page-1)*count).Limit(count);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                
                foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                {
                    var project = document.ConvertTo<Project>();
                    project.Id = document.Id;
                    list.Add(project);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //return list;
            return new OkObjectResult(new Item { curPage = page, total = total, data = list });
        }

        [HttpPost]
        public async Task<ActionResult> AddProjectAsync([FromBody] Project projectInput)
        {
            bool addOK = false;
            var project = new Project
            {
                Name = projectInput.Name,
                Keyword = projectInput.Keyword,                
                QuesionsCount = projectInput.QuesionsCount,
                Language = projectInput.Language,
                Ip = projectInput.Ip,
                OnScrapping = false,
                LanguageString = projectInput.LanguageString,
                CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                UpdateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
            };

            try
            {
                CollectionReference projectsCol = Config.FirebaseDB.Collection("Projects");
                
                Query query = projectsCol.OrderByDescending("CreatedTime").WhereEqualTo("Name", project.Name).Limit(1);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                if (projectsSnapshot.Documents.Count == 0) {
                    await projectsCol.AddAsync(project);
                    addOK = true;
                }

                Task.Run(() => new CloudFlareAPI().CreateDnsThreadAsync(project.Name, project.Ip));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(addOK);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateProjectAsync([FromBody] Project projectInput)
        {
            bool addOK = false;
            var project = new Project
            {
                Id = projectInput.Id,
                Name = projectInput.Name,
                Ip = projectInput.Ip,
                Keyword = projectInput.Keyword,
                QuesionsCount = projectInput.QuesionsCount,
                Language = projectInput.Language,
                LanguageString = projectInput.LanguageString,
                CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
            };

            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Projects");
                DocumentReference docRef = articlesCol.Document(projectInput.Id);

                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    var prevProj = snapshot.ConvertTo<Project>();
                    //Omitted Task.Run(() => new CloudFlareAPI().UpdateDnsThreadAsync(prevProj.Name, project.Name, project.Ip));
                }

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "Name", projectInput.Name },
                    { "Ip", projectInput.Ip },
                    { "Keyword", projectInput.Keyword },
                    { "QuesionsCount", projectInput.QuesionsCount },
                    { "Language", projectInput.Language },
                    { "LanguageString", projectInput.LanguageString },
                    { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                };
                await docRef.UpdateAsync(userUpdate);
                addOK = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(addOK);
        }

        [HttpDelete("{projectid}")]
        public async Task<IActionResult> DeleteProjectAsync(String projectid)
        {
            try
            {
                CollectionReference colRef= Config.FirebaseDB.Collection("Projects");
                DocumentReference docRef = colRef.Document(projectid);
                DocumentSnapshot snapshotPrj = await docRef.GetSnapshotAsync();
                var prevProj = snapshotPrj.ConvertTo<Project>();
                Task.Run(() => new CloudFlareAPI().DeleteDnsThreadAsync(prevProj.Name, prevProj.Ip));
                await docRef.DeleteAsync();

                colRef = Config.FirebaseDB.Collection("Articles");
                Query query = colRef.WhereEqualTo("ProjectId", projectid);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                WriteBatch writeBatch = Config.FirebaseDB.StartBatch();
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    writeBatch.Delete(document.Reference);
                }
                await writeBatch.CommitAsync();                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok();
        }

        //{{
        [HttpGet("serpapi/{_id}/{keyword}/{count}")]
        //==
        //[Route("serpapi")]
        //[HttpPost]
        //}}
        public ActionResult serpapi(String _id, String keyword, Int32 count)
        {
            Task.Run(() => new SerpapiScrap().ScrappingThreadAsync(_id, keyword, count));
            return Ok(true);
        }
    }
}