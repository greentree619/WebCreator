﻿using FireSharp.Config;
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
using Consul;
using RestSharp;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Web.Helpers;
using Microsoft.Net.Http.Headers;
using UnitTest.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebCreator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectController : ControllerBase
    {
        readonly IStreamFileUploadService _streamFileUploadService;
        //public ProjectController(IStreamFileUploadService streamFileUploadService)
        //{
        //    _streamFileUploadService = streamFileUploadService;
        //}

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
                total = (int)Math.Ceiling( (double)totalSnapshot.Count/count );
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

        [HttpGet("schedule/{domainid}")]
        public async Task<IActionResult> GetScheduleAsync(String domainid)
        {
            Schedule schedule = null;
            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("Schedules");
                Query query = col.WhereEqualTo("ProjectId", domainid).Limit(1);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                if (projectsSnapshot.Documents.Count > 0)
                {
                    schedule = projectsSnapshot.Documents[0].ConvertTo<Schedule>();
                    schedule.Id = projectsSnapshot.Documents[0].Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Ok(new { id = domainid, data = schedule });
        }

        [HttpGet("publishSchedule/{domainid}")]
        public async Task<IActionResult> GetPublishScheduleAsync(String domainid)
        {
            Schedule schedule = null;
            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("PublishSchedules");
                Query query = col.WhereEqualTo("ProjectId", domainid).Limit(1);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                if (projectsSnapshot.Documents.Count > 0)
                {
                    schedule = projectsSnapshot.Documents[0].ConvertTo<Schedule>();
                    schedule.Id = projectsSnapshot.Documents[0].Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Ok(new { id = domainid, data = schedule });
        }

        [HttpGet("isscrapping/{domainId}")]
        public async Task<IActionResult> GetIsScrapping(String domainId)
        {
            //Omitted JObject scrapStatus = (JObject)await CommonModule.IsDomainScrappingAsync(domainId);
            bool isScrapping = (CommonModule.threadList[domainId] != null ? (bool)CommonModule.threadList[domainId] : false);
            bool isAFScrapping = (CommonModule.afThreadList[domainId] != null ? (bool)CommonModule.afThreadList[domainId] : false);
            bool isPublishing = (CommonModule.publishThreadList[domainId] != null ? (bool)CommonModule.publishThreadList[domainId] : false);

            //return list;
            return Ok(new { serpapi=isScrapping, afapi= isAFScrapping, publish= isPublishing });
        }

        //{{
        private static bool IsMultipartContentType(string contentType)
        {
            return
                !string.IsNullOrEmpty(contentType) &&
                contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetBoundary(string contentType)
        {
            var elements = contentType.Split(' ');
            var element = elements.Where(entry => entry.StartsWith("boundary=")).First();
            var boundary = element.Substring("boundary=".Length);
            // Remove quotes
            if (boundary.Length >= 2 && boundary[0] == '"' &&
                boundary[boundary.Length - 1] == '"')
            {
                boundary = boundary.Substring(1, boundary.Length - 2);
            }
            return boundary;
        }

        private string GetFileName(string contentDisposition)
        {
            return contentDisposition
                .Split(';')
                .SingleOrDefault(part => part.Contains("filename"))
                .Split('=')
                .Last()
                .Trim('"');
        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
        {
            public void OnResourceExecuting(ResourceExecutingContext context)
            {
                var factories = context.ValueProviderFactories;
                factories.RemoveType<FormValueProviderFactory>();
                factories.RemoveType<JQueryFormValueProviderFactory>();
            }

            public void OnResourceExecuted(ResourceExecutedContext context)
            {
            }
        }
        //}}

        [HttpPost("themeUpload")]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> themeUpload()
        {
            try
            {
                if (IsMultipartContentType(Request.ContentType))
                {
                    var boundary = GetBoundary(Request.ContentType);
                    var reader = new MultipartReader(boundary, Request.Body);
                    var section = await reader.ReadNextSectionAsync();

                    while (section != null)
                    {
                        // process each image
                        const int chunkSize = 1024;
                        var buffer = new byte[chunkSize];
                        var bytesRead = 0;
                        var fileName = GetFileName(section.ContentDisposition);

                        using (var stream = new FileStream(fileName, FileMode.Append))
                        {
                            do
                            {
                                bytesRead = await section.Body.ReadAsync(buffer, 0, buffer.Length);
                                stream.Write(buffer, 0, bytesRead);

                            } while (bytesRead > 0);
                        }

                        section = await reader.ReadNextSectionAsync();
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return Ok(true);
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
                OnAFScrapping = false,
                OnPublishSchedule = false,
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
                    DocumentReference docRef = await projectsCol.AddAsync(project);

                    await addDefaultSchedule(docRef.Id);
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

        private async Task addDefaultSchedule(String projectId) {
            CollectionReference scheduleCol = Config.FirebaseDB.Collection("Schedules");
            var afSchedule = new Schedule
            {
                ProjectId = projectId,
                JustNowCount = 1,
                EachCount = 1,
                SpanTime = 1,
                SpanUnit = 60,
                CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                UpdateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
            };
            await scheduleCol.AddAsync(afSchedule);
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
                    Task.Run(() => new CloudFlareAPI().UpdateDnsThreadAsync(prevProj.Name, project.Name, project.Ip));
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

        [HttpPut("updateSchedule")]
        public async Task<ActionResult> UpdateScheduleAsync([FromBody] Schedule schedule)
        {
            Dictionary<string, object> userUpdate = new Dictionary<string, object>()
            {
                { "ProjectId", schedule.ProjectId },
                { "JustNowCount", schedule.JustNowCount },
                { "EachCount", schedule.EachCount },
                { "SpanTime", schedule.SpanTime },
                { "SpanUnit", schedule.SpanUnit },
                { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
            };

            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Schedules");
                DocumentReference docRef = articlesCol.Document(schedule.Id);                
                await docRef.UpdateAsync(userUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                userUpdate["CreatedTime"] = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                CollectionReference scheduleCol = Config.FirebaseDB.Collection("Schedules");
                await scheduleCol.AddAsync(userUpdate);
            }

            return Ok(true);
        }

        [HttpPut("updatePublishSchedule")]
        public async Task<ActionResult> UpdatePublishScheduleAsync([FromBody] PublishSchedule schedule)
        {
            Dictionary<string, object> userUpdate = new Dictionary<string, object>()
            {
                { "ProjectId", schedule.ProjectId },
                { "JustNowCount", schedule.JustNowCount },
                { "EachCount", schedule.EachCount },
                { "SpanTime", schedule.SpanTime },
                { "SpanUnit", schedule.SpanUnit },
                { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
            };

            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("PublishSchedules");
                DocumentReference docRef = articlesCol.Document(schedule.Id);
                await docRef.UpdateAsync(userUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                userUpdate["CreatedTime"] = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                CollectionReference scheduleCol = Config.FirebaseDB.Collection("PublishSchedules");
                await scheduleCol.AddAsync(userUpdate);
            }

            return Ok(true);
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

                colRef = Config.FirebaseDB.Collection("Schedules");
                query = colRef.WhereEqualTo("ProjectId", projectid);
                snapshot = await query.GetSnapshotAsync();

                writeBatch = Config.FirebaseDB.StartBatch();
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
            if (CommonModule.threadList[_id] == null || (bool)CommonModule.threadList[_id] == false)
            {
                CommonModule.threadList[_id] = true;
                Task.Run(() => new SerpapiScrap().ScrappingThreadAsync(_id, keyword, count));
            }
            return Ok(true);
        }

        //{{
        [HttpGet("startaf/{_id}/{sid}")]
        //==
        //[Route("serpapi")]
        //[HttpPost]
        //}}
        public ActionResult StartAFapi(String _id, String sid)
        {
            bool ret = false;
            if (CommonModule.afThreadList[_id] == null || (bool)CommonModule.afThreadList[_id] == false)
            {
                CommonModule.afThreadList[_id] = true;
                Task.Run(() => new SerpapiScrap().ScrappingAFThreadAsync(_id, sid));
                ret = true;
            }
            else
            {
                CommonModule.afThreadList[_id] = false;
                ret = false;
            }
            return Ok(ret);
        }

        [HttpGet("startPublish/{_id}/{sid}")]
        public ActionResult StartPublishapi(String _id, String sid)
        {
            bool ret = false;
            if (CommonModule.publishThreadList[_id] == null || (bool)CommonModule.publishThreadList[_id] == false)
            {
                CommonModule.publishThreadList[_id] = true;
                Task.Run(() => new SerpapiScrap().PublishThreadAsync(_id, sid));
                ret = true;
            }
            else
            {
                CommonModule.publishThreadList[_id] = false;
                ret = false;
            }
            return Ok(ret);
        }
    }
}