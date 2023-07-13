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
using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Net;
using UnitTest.Models;

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
                total = (int)Math.Ceiling( (double)totalSnapshot.Count/count );
                Query query = projectsCol.OrderByDescending("CreatedTime").Offset((page-1)*count).Limit(count);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                
                foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                {
                    var project = document.ConvertTo<Project>();
                    project.Id = document.Id;
                    if (project.ContactInfo == null) project.ContactInfo = new _ContactInfo();
                    if (project.ImageAutoGenInfo == null) project.ImageAutoGenInfo = new _ImageAutoGenInfo();
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
                schedule = await CommonModule.GetScheduleAsync(domainid);
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
                schedule = await CommonModule.GetPublishScheduleAsync(domainid);
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
            bool isArticleScrapping = (CommonModule.articleScrappingThreadList[domainId] != null ? (bool)CommonModule.articleScrappingThreadList[domainId] : false);
            bool isPublishing = (CommonModule.publishThreadList[domainId] != null ? (bool)CommonModule.publishThreadList[domainId] : false);
            int scrappingScheduleMode = (CommonModule.domainScrappingScheduleStatus[domainId] != null ? 
                ((ScrappingScheduleStatus)CommonModule.domainScrappingScheduleStatus[domainId]).mode : 0);

            //return list;
            return Ok(new { serpapi=isScrapping, afapi= isArticleScrapping, publish= isPublishing, scrappingScheduleMode = scrappingScheduleMode });
        }

        [HttpGet("allDownload/{domainId}/{domainName}/{domainIp}")]
        public async Task<FileResult> AllDownload(String domainId, String domainName, String domainIp, String? s3Name="", String? region="")
        {
            String curFolder = Directory.GetCurrentDirectory();
            curFolder += $"\\Build\\{domainName}";
            String themeFolder = Directory.GetCurrentDirectory();
            themeFolder += $"\\Theme\\{domainName}\\theme";

            await CommonModule.BuildPagesThreadAsync(domainId, domainName, CommonModule.isAWSHosting(domainIp), s3Name, region);
            try
            {
                FileSystem.CopyDirectory(themeFolder, curFolder, true);
            }
            catch (Exception e) { }

            //Omitted
            String tmpFolder = Directory.GetCurrentDirectory() + "\\Temp";
            //tmpFolder += $"\\Temp";
            //if (!Directory.Exists(tmpFolder))
            //{
            //    Directory.CreateDirectory(tmpFolder);
            //}

            string dirRoot = curFolder;
            string[] filesToZip = Directory.GetFiles(dirRoot, "*.*", System.IO.SearchOption.AllDirectories);
            string zipFileName = string.Format($"{domainName}.zip", DateTime.Now);

            using (MemoryStream zipMS = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(zipMS, ZipArchiveMode.Create, true, Encoding.UTF8))
                {
                    //loop through files to add
                    foreach (string fileToZip in filesToZip)
                    {
                        //read the file bytes
                        byte[] fileToZipBytes = System.IO.File.ReadAllBytes(fileToZip);
                        String zipEntry = fileToZip.Replace(dirRoot + "\\", "");
                        ZipArchiveEntry zipFileEntry = zipArchive.CreateEntry(zipEntry);

                        //add the file contents
                        using (Stream zipEntryStream = zipFileEntry.Open())
                        using (BinaryWriter zipFileBinary = new BinaryWriter(zipEntryStream))
                        {
                            zipFileBinary.Write(fileToZipBytes);
                        }
                    }
                }

                using (FileStream finalZipFileStream = new FileStream($"{tmpFolder}\\" + zipFileName, FileMode.Create))
                {
                    zipMS.Seek(0, SeekOrigin.Begin);
                    zipMS.CopyTo(finalZipFileStream);
                }
            }

            String tempOutput = $"{tmpFolder}\\" + zipFileName;
            byte[] finalResult = System.IO.File.ReadAllBytes(tempOutput);
            if (System.IO.File.Exists(tempOutput))
            {
                System.IO.File.Delete(tempOutput);
            }
            if (finalResult == null || !finalResult.Any())
            {
                throw new Exception(String.Format("Nothing found"));
            }

            return File(finalResult, "application/zip", zipFileName);
        }

        //{{
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
        {
            public void OnResourceExecuting(ResourceExecutingContext context)
            {
                var factories = context.ValueProviderFactories;
                factories.RemoveType<FormValueProviderFactory>();
                factories.RemoveType<FormFileValueProviderFactory>();
                factories.RemoveType<JQueryFormValueProviderFactory>();
            }

            public void OnResourceExecuted(ResourceExecutedContext context)
            {
            }
        }
        //}}

        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        [HttpPost("themeUpload/{domainId}/{domainName}/{ipaddr}")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> themeUpload(String domainId, String domainName, String ipaddr, String? s3Name="", String? region="")
        {
            bool ret = true;
            try
            {
                if (CommonModule.onThemeUpdateCash[domainId] == null || (bool)CommonModule.onThemeUpdateCash[domainId] == false)
                {
                    CommonModule.onThemeUpdateCash[domainId] = true;
                }
                String curFolder = Directory.GetCurrentDirectory();
                curFolder += $"\\Theme\\{domainName}";
                if (!Directory.Exists(curFolder))
                {
                    Directory.CreateDirectory(curFolder);
                }

                if (CommonModule.IsMultipartContentType(Request.ContentType))
                {
                    var boundary = CommonModule.GetBoundary(Request.ContentType);
                    var reader = new MultipartReader(boundary, Request.Body);
                    var section = await reader.ReadNextSectionAsync();

                    while (section != null)
                    {
                        // process each image
                        const int chunkSize = 1024;
                        var buffer = new byte[chunkSize];
                        var bytesRead = 0;
                        var fileName = CommonModule.GetFileName(section.ContentDisposition);
                        await CommonModule.historyLog.ThemeUploadAction(domainId, fileName);

                        using (var stream = new FileStream(curFolder + "/theme.zip", FileMode.Create))
                        {
                            do
                            {
                                bytesRead = await section.Body.ReadAsync(buffer, 0, buffer.Length);
                                stream.Write(buffer, 0, bytesRead);

                            } while (bytesRead > 0);
                        }

                        section = await reader.ReadNextSectionAsync();
                    }

                    if (System.IO.File.Exists(curFolder + "/theme.zip"))
                    {
                        if (System.IO.Directory.Exists(curFolder + "/theme"))
                            CommonModule.DeleteAllContentInFolder(curFolder + "/theme");
                        System.IO.Compression.ZipFile.ExtractToDirectory(curFolder + "/theme.zip", curFolder + "/theme", true);
                        //using (ZipArchive archive = ZipFile.OpenRead(curFolder + "/theme.zip"))
                        //{
                        //    foreach (ZipArchiveEntry entry in archive.Entries)
                        //    {
                        //        entry.ExtractToFile(Path.Combine(curFolder + "/theme", entry.FullName));
                        //    }
                        //}

                        String templ = CommonModule.GetArticleTemplate(domainName);
                        if (templ.Length == 0)
                        {
                            ret = false;
                        }
                    }
                    else ret = false;

                    CommonModule.onThemeUpdateCash[domainId] = false;

                    await CommonModule.SyncThemeWithServerThreadAsync(domainName, ipaddr);//Task.Run(() => this.SyncWithServerThreadAsync(domainid, domain, ipaddr));
                    Task.Run(() => new SerpapiScrap().UpdateArticleThemeThreadAsync(domainId, domainName, ipaddr, s3Name, region));//Only for online article
                }
            }
            catch (Exception ex)
            {
            }
            return Ok(ret);
        }

        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        [HttpPost("keywordTranslate")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> KeywordTranslate(String? lang= "EN-GB")
        {
            String keywordFile = "";
            try
            {
                using (StreamReader reader
                  = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    keywordFile = await reader.ReadToEndAsync();
                }

                keywordFile = await CommonModule.deepLTranslate.Translate(keywordFile, lang.ToUpper());
            }
            catch (Exception ex)
            {
            }
            return Ok(new { data = keywordFile });
        }

        [HttpGet("translateKeyword")]
        public async Task<IActionResult> TranslateKeyword(String keyword)
        {
            String transKeyword = "";
            try
            {
                transKeyword = await CommonModule.deepLTranslate.Translate(keyword);
            }
            catch (Exception ex)
            {}
            return Ok(new { data = transKeyword });
        }

        [HttpPost]
        public async Task<ActionResult> AddProjectAsync([FromBody] Project projectInput)
        {
            bool addOK = false;
            projectInput.UseHttps = (projectInput.UseHttps == null ? false : projectInput.UseHttps);
            var project = new Project
            {
                Name = projectInput.Name,
                Keyword = projectInput.Keyword,
                QuesionsCount = projectInput.QuesionsCount,
                Language = projectInput.Language,
                S3BucketName = (projectInput.S3BucketName == null ? "" : projectInput.S3BucketName),
                S3BucketRegion = (projectInput.S3BucketRegion == null ? "" : projectInput.S3BucketRegion),
                Ip = projectInput.Ip,
                UseHttps = (projectInput.Ip.CompareTo("0.0.0.0") == 0 ? true : projectInput.UseHttps),
                OnScrapping = false,
                OnAFScrapping = false,
                OnOpenAIScrapping = false,
                OnPublishSchedule = false,
                LanguageString = projectInput.LanguageString,
                ContactInfo = projectInput.ContactInfo,
                ImageAutoGenInfo = projectInput.ImageAutoGenInfo,
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

                    CommonModule.project2LanguageMap[docRef.Id] = project.Language;
                    CommonModule.project2UseHttpsMap[docRef.Id] = (project.UseHttps == null ? false : project.UseHttps);
                    CommonModule.project2ImageAutoGenInfoMap[docRef.Id] = (project.ImageAutoGenInfo == null ? new _ImageAutoGenInfo() : project.ImageAutoGenInfo);
                    await addDefaultSchedule(docRef.Id);
                    addOK = true;
                }

                Task.Run(() => new CloudFlareAPI().CreateDnsThreadAsync(project.Name, project.Ip));
                
                if(project.Ip.CompareTo("0.0.0.0") == 0)
                    Task.Run(() => new CommonModule().CreateHostBucketThreadAsync(project.Name));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(new { result = addOK, error = "" });
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
            String error = "";
            var project = new Project
            {
                Id = projectInput.Id,
                UseHttps = projectInput.UseHttps,
                Name = projectInput.Name,
                Ip = projectInput.Ip,
                S3BucketName = (projectInput.S3BucketName == null ? "" : projectInput.S3BucketName),
                S3BucketRegion = (projectInput.S3BucketRegion == null ? "" : projectInput.S3BucketRegion),
                Keyword = projectInput.Keyword,
                QuesionsCount = projectInput.QuesionsCount,
                Language = projectInput.Language,
                LanguageString = projectInput.LanguageString,
                ContactInfo = projectInput.ContactInfo,
                ImageAutoGenInfo = projectInput.ImageAutoGenInfo,
                CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
            };

            if (projectInput.Ip.CompareTo("0.0.0.0") == 0
                && (projectInput.UseHttps == null || projectInput.UseHttps == false))
            {
                error = "You have incorrect setting with HTTPS. Maybe you can't deploy to S3 Bukect.";
                CommonModule.Notification(projectInput.Id, error);
            }

            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Projects");
                DocumentReference docRef = articlesCol.Document(projectInput.Id);

                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    var prevProj = snapshot.ConvertTo<Project>();
                    Task.Run(() => new CloudFlareAPI().UpdateDnsThreadAsync(prevProj.Name, project.Name, project.Ip));

                    if (projectInput.UseHttps == true
                        && prevProj.UseHttps != projectInput.UseHttps
                        && projectInput.Ip.CompareTo("0.0.0.0") != 0)
                    { //Configure https on ec2 erver.
                        await CommonModule.BuildArticlePageAsEmptyThreadAsync( projectInput.Id, projectInput.Name );
                        await CommonModule.SyncWithServerThreadAsync(projectInput.Id, projectInput.Name, projectInput.Ip, projectInput.S3BucketName);
                    }
                }

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "Name", projectInput.Name },
                    { "Ip", projectInput.Ip },
                    { "UseHttps", projectInput.UseHttps },
                    { "S3BucketName", project.S3BucketName },
                    { "S3BucketRegion", project.S3BucketRegion },
                    { "Keyword", projectInput.Keyword },
                    { "QuesionsCount", projectInput.QuesionsCount },
                    { "Language", projectInput.Language },
                    { "LanguageString", projectInput.LanguageString },
                    { "ContactInfo", projectInput.ContactInfo },
                    { "ImageAutoGenInfo", projectInput.ImageAutoGenInfo },
                    { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                };
                await docRef.UpdateAsync(userUpdate);
                CommonModule.project2LanguageMap[docRef.Id] = projectInput.Language;
                CommonModule.project2UseHttpsMap[docRef.Id] = projectInput.UseHttps;
                CommonModule.project2ImageAutoGenInfoMap[docRef.Id] = projectInput.ImageAutoGenInfo;

                if (projectInput.Ip.CompareTo("0.0.0.0") == 0)
                    Task.Run(() => new CommonModule().CreateHostBucketThreadAsync(projectInput.Name));
                addOK = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(new { result = addOK, error = error });
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
        public ActionResult SerpAPI(String _id, String keyword, Int32 count)
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
            String lang = CommonModule.project2LanguageMap[_id].ToString();
            if (CommonModule.articleScrappingThreadList[_id] == null || (bool)CommonModule.articleScrappingThreadList[_id] == false)
            {
                CommonModule.articleScrappingThreadList[_id] = true;
                if (CommonModule.domainScrappingScheduleStatus[_id] == null)
                    CommonModule.domainScrappingScheduleStatus[_id] = new ScrappingScheduleStatus { isRunning = true, mode = 0 };
                else {
                    ScrappingScheduleStatus status = (ScrappingScheduleStatus)CommonModule.domainScrappingScheduleStatus[_id];
                    status.isRunning = true;
                    status.mode = 0;
                }
                Task.Run(() => new SerpapiScrap().ScrappingAFThreadAsync(_id, sid));
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

                Task.Run(() => new SerpapiScrap().ScrappingOpenAIThreadAsync(_id, sid));
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