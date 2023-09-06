using Amazon;
using Amazon.S3;
using Google.Cloud.Firestore;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTest.Lib;
using UnitTest.Models;
using WebCreator;
using WebCreator.Models;
using static WebCreator.Controllers.ProjectController;

namespace UnitTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly ILogger<VideoController> _logger;

        public VideoController(ILogger<VideoController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var list = new List<VideoProject>();
            int total = 0;
            try
            {
                CollectionReference projectsCol = Config.FirebaseDB.Collection("VideoProjects");
                QuerySnapshot totalSnapshot = await projectsCol.GetSnapshotAsync();
                Query query = projectsCol.OrderByDescending("CreatedTime");
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                {
                    var project = document.ConvertTo<VideoProject>();
                    project.Id = document.Id;
                    list.Add(project);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //return list;
            return Ok(new { total = total, data = list });
        }

        [HttpGet("{prjId}/{state}/{page}/{count}")]
        public async Task<IActionResult> GetAsync(String prjId, int state, int page = 1, int count = 5, String? keyword = "")
        {
            if (page < 0) page = 1;
            if (count < 0) count = 5;
            var list = new List<VideoDetail>();
            int total = 0;
            try
            {
                CollectionReference projectsCol = Config.FirebaseDB.Collection("VideoProjects");
                DocumentReference docRef = projectsCol.Document(prjId);
                DocumentSnapshot prjSnapshot = await docRef.GetSnapshotAsync();
                if (prjSnapshot.Exists)
                {
                    var vPrj = prjSnapshot.ConvertTo<VideoProject>();
                    if (vPrj.VideoCollection != null)
                    {
                        foreach (var vd in vPrj.VideoCollection)
                        {
                            if (state != 0 && vd.State != state) continue;

                            list.Add(vd);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(new { curPage = page, total = total, data = list });
        }

        [HttpGet("byTitle/{prjId}/{title}")]
        public async Task<ActionResult> GetVideoByTitleAsync(String prjId, String title)
        {
            VideoDetail result = null;
            try
            {
                CollectionReference projectsCol = Config.FirebaseDB.Collection("VideoProjects");
                DocumentReference docRef = projectsCol.Document(prjId);
                DocumentSnapshot prjSnapshot = await docRef.GetSnapshotAsync();
                if (prjSnapshot.Exists)
                {
                    var vPrj = prjSnapshot.ConvertTo<VideoProject>();
                    if (vPrj.VideoCollection != null)
                    {
                        foreach (var vd in vPrj.VideoCollection)
                        {
                            var titleTmp = vd.Title.Trim('?');
                            if (titleTmp.CompareTo(title) == 0) 
                                result = vd;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(new { data = result });
        }

        [HttpPost]
        public async Task<ActionResult> AddProjectAsync([FromBody] VideoProject projectInput)
        {
            bool addOK = false;
            try
            {
                CollectionReference projectsCol = Config.FirebaseDB.Collection("VideoProjects");
                Query query = projectsCol.OrderByDescending("CreatedTime").WhereEqualTo("Name", projectInput.Name).Limit(1);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                if (projectsSnapshot.Documents.Count == 0)
                {
                    DocumentReference docRef = await projectsCol.AddAsync(projectInput);
                    addOK = true;
                }
            }
            catch (RpcException ex)
            {
                if (ex.Status.StatusCode == Grpc.Core.StatusCode.FailedPrecondition) {
                    Console.WriteLine(ex.Status.Detail);
                }
            }

            return Ok(new { result = addOK, error="" });
        }

        [HttpPut]
        public async Task<ActionResult> UpdateProjectAsync([FromBody] VideoProject projectInput)
        {
            bool updateOK = false;
            try
            {
                CollectionReference projectsCol = Config.FirebaseDB.Collection("VideoProjects");
                DocumentReference docRef = projectsCol.Document(projectInput.Id);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "Name", projectInput.Name },
                    { "YoutubeChannel", projectInput.YoutubeChannel },
                    { "Width", projectInput.Width },
                    { "Height", projectInput.Height },
                    { "Keyword", projectInput.Keyword },
                    { "QuesionsCount", projectInput.QuesionsCount },
                    { "Language", projectInput.Language },
                    { "LanguageString", projectInput.LanguageString },
                    { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                };
                await docRef.UpdateAsync(userUpdate);
                CommonModule.project2LanguageMap[docRef.Id] = projectInput.Language;

                updateOK = true;
            }
            catch (RpcException ex)
            {
                if (ex.Status.StatusCode == Grpc.Core.StatusCode.FailedPrecondition)
                {
                    Console.WriteLine(ex.Status.Detail);
                }
            }

            return Ok(new { result = updateOK, error = "" });
        }

        [HttpPut("update_content/{prjId}")]
        public async Task<IActionResult> UpdateVideoDetailAsync(string prjId, [FromBody] VideoDetail videoDetail)
        {
            CollectionReference projectCol = Config.FirebaseDB.Collection("VideoProjects");
            DocumentReference docRef = projectCol.Document(prjId);
            DocumentSnapshot articleSnapshot = await docRef.GetSnapshotAsync();
            var vCol = new List<VideoDetail>();
            VideoProject vPrj = null;
            Hashtable videoListMap = new Hashtable();
            if (articleSnapshot.Exists)
            {
                vPrj = articleSnapshot.ConvertTo<VideoProject>();
                vPrj.Id = articleSnapshot.Id;
                if (vPrj.VideoCollection != null)
                {
                    foreach (var vd in vPrj.VideoCollection)
                    {
                        vCol.Add(vd);
                        videoListMap[vd.Title.Trim('?').ToString()] = vd;
                    }
                }
            }

            try
            {
                var videoObj = (VideoDetail)videoListMap[videoDetail.Title.Trim('?')];
                if (videoObj != null) {
                    videoObj.Script = videoDetail.Script;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Dictionary<string, object> userUpdate = new Dictionary<string, object>(){
                { "VideoCollection", vCol }
            };
            await docRef.UpdateAsync(userUpdate);

            return Ok(true);
        }

        [HttpDelete("{projectid}")]
        public async Task<IActionResult> DeleteProjectAsync(String projectid)
        {
            try
            {
                CollectionReference colRef = Config.FirebaseDB.Collection("VideoProjects");
                DocumentReference docRef = colRef.Document(projectid);
                docRef.DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok();
        }

        [HttpGet("AddArticlesByTitle/{projectId}/{keywords}")]
        public async Task<IActionResult> AddVideoDetailsAsync(String projectId, String keywords)
        {
            _ = Task.Run(async () =>
             {
                 await CommonModule.historyLog.LogKeywordAction(projectId, keywords, true, true);

                 CollectionReference projectCol = Config.FirebaseDB.Collection("VideoProjects");
                 DocumentReference docRef = projectCol.Document(projectId);
                 DocumentSnapshot articleSnapshot = await docRef.GetSnapshotAsync();
                 var vCol = new List<VideoDetail>();
                 VideoProject vPrj = null;
                 if (articleSnapshot.Exists)
                 {
                     vPrj = articleSnapshot.ConvertTo<VideoProject>();
                     vPrj.Id = articleSnapshot.Id;
                     if (vPrj.VideoCollection != null)
                     {
                         foreach (var vd in vPrj.VideoCollection)
                         {
                             vCol.Add(vd);
                         }
                     }
                 }

                 //Console.WriteLine($"GoogleSearchAsync keyword={keywords}");
                 keywords = keywords.Replace(';', '?');
                 keywords = keywords.Replace('&', ';');
                 String[] questions = keywords.Split(";");
                 try
                 {
                     foreach (String question in questions)
                     {
                         if (vPrj != null && vPrj.IsContain(question)) continue;

                         var video = new VideoDetail
                         {
                             Title = question,
                             IsScrapping = false,
                             Progress = 0,
                             State = 0,
                             UpdateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                             CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                         };

                         vCol.Add(video);
                     }
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine(ex.Message);
                 }

                 Dictionary<string, object> userUpdate = new Dictionary<string, object>(){
                    { "VideoCollection", vCol }
                 };
                 await docRef.UpdateAsync(userUpdate);

                 return $"Complted scrapping: DomainID={projectId}";
             });
            return Ok(true);
        }

        [HttpGet("serpapi/{_id}/{keyword}/{count}")]
        public ActionResult SerpAPI(String _id, String keyword, Int32 count)
        {
            if (CommonModule.threadList[_id] == null || (bool)CommonModule.threadList[_id] == false)
            {
                CommonModule.threadList[_id] = true;
                Task.Run(() => new SerpapiScrap().VideoScrappingThreadAsync(_id, keyword, count));
            }
            return Ok(true);
        }

        [HttpGet("scrapContentManual/{mode}/{prjId}/{titles}")]
        public async Task<IActionResult> ScrapContentManualAsync(String mode, String prjId, String titles)
        {
            bool ret = false;
            if (mode == "0" && CommonModule.isManualAFScrapping == false || mode == "1" && CommonModule.isManualOpenAIScrapping == false)
            {
                if (mode == "0") CommonModule.isManualAFScrapping = true;
                else if (mode == "1") CommonModule.isManualOpenAIScrapping = true;
                Task.Run(() => new SerpapiScrap().ScrappingVideoManualThreadAsync(mode, prjId, titles));

                ret = true;
            }
            return Ok(ret);
        }

        [HttpPost("scrapProgressState")]
        public async Task<IActionResult> scrapProgressState([FromBody] VideoTitles videoTitles)
        {
            Dictionary<string, ScrapProgress> scrapStatus = new Dictionary<string, ScrapProgress>();
            string[] titleAry = videoTitles.articleIds.Split("+NEXT+");
            foreach (var tl in titleAry)
            {
                var titleToken = tl.Trim('?');
                if (CommonModule.videoScrapProgress[videoTitles.projectId] != null && ((Hashtable)CommonModule.videoScrapProgress[videoTitles.projectId])[titleToken] != null) {
                    scrapStatus[titleToken] = (ScrapProgress)((Hashtable)CommonModule.videoScrapProgress[videoTitles.projectId])[titleToken];
                }
            }
            
            return new OkObjectResult(scrapStatus);
        }

        [HttpGet("schedule/{prjId}")]
        public async Task<IActionResult> GetScrapScheduleAsync( String prjId )
        {
            Schedule schedule = null;
            try
            {
                schedule = await CommonModule.GetScheduleAsync(prjId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Ok(new { id = prjId, data = schedule });
        }

        [HttpGet("publishSchedule/{prjId}")]
        public async Task<IActionResult> GetPublishScheduleAsync(String prjId)
        {
            Schedule schedule = null;
            try
            {
                schedule = await CommonModule.GetPublishScheduleAsync(prjId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Ok(new { id = prjId, data = schedule });
        }

        //_id: project Id
        //sid: schedule Id
        [HttpGet("startOpenAI/{_id}/{sid}")]
        public ActionResult StartOpenAIapi(String _id, String sid)
        {
            bool ret = false;
            if (CommonModule.articleScrappingThreadList[_id] == null || (bool)CommonModule.articleScrappingThreadList[_id] == false)
            {
                CommonModule.articleScrappingThreadList[_id] = true;
                if (CommonModule.domainScrappingScheduleStatus[_id] == null)
                    CommonModule.domainScrappingScheduleStatus[_id] = new ScrappingScheduleStatus { isRunning = true, mode = 1 };
                else
                {
                    ScrappingScheduleStatus status = (ScrappingScheduleStatus)CommonModule.domainScrappingScheduleStatus[_id];
                    status.isRunning = true;
                    status.mode = 1;
                }

                Task.Run(() => new SerpapiScrap().VideoScrappingOpenAIThreadAsync(_id, sid));
                ret = true;
            }
            else
            {
                CommonModule.articleScrappingThreadList[_id] = false;
                ScrappingScheduleStatus status = (ScrappingScheduleStatus)CommonModule.domainScrappingScheduleStatus[_id];
                status.isRunning = false;
                ret = false;
            }
            return Ok(ret);
        }
    }
}
