﻿using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnitTest.Lib;
using WebCreator.Models;

namespace WebCreator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArticleController : ControllerBase
    {
        public class Item
        {
            public int curPage { get; set; }
            public int total { get; set; }
            public IEnumerable<Article> data { get; set; }
        }

        IFirebaseClient client;

        private readonly ILogger<ProjectController> _logger;
        ArticleForge af = new ArticleForge();

        public ArticleController(ILogger<ProjectController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{page}/{count}")]
        public async Task<IActionResult> GetAsync(int page = 1, int count = 5)
        {
            if (page < 0) page = 1;
            if (count < 0) count = 5;
            var list = new List<Article>();
            int total = 0;
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                QuerySnapshot totalSnapshot = await articlesCol.WhereNotEqualTo("State", 4).GetSnapshotAsync();
                total = (int)Math.Ceiling((double)totalSnapshot.Count / count);

                Query query = articlesCol.OrderByDescending("State").WhereNotEqualTo("State", 4).OrderByDescending("CreatedTime").Offset((page - 1) * count).Limit(count);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    article.Id = document.Id;
                    list.Add(article);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new OkObjectResult(new Item { curPage = page, total = total, data = list });
        }

        private Query filterByState(Query query, int state, bool sortByCreateTime=false)
        {
            if (state == 0)
            {//missed content
                query = query.OrderBy("State").WhereNotEqualTo("State", 4);
            }
            else if (state == 1)
            {//missed content
                query = query.OrderBy("State").WhereNotIn("State", new Int32[]{2, 3, 4});
            }
            else if (state == 2)
            {// Ready for approval
                query = query.WhereEqualTo("State", 2);
            }
            else if (state == 3)
            {// Online on server
                query = query.WhereEqualTo("State", 3);
            }

            if (sortByCreateTime) query = query.OrderByDescending("CreatedTime");
            return query;
        }

        [HttpGet("{domainid}/{state}/{page}/{count}")]
        public async Task<IActionResult> GetAsync(String domainid, int state, int page = 1, int count = 5, String? keyword = "")
        {
            if (page < 0) page = 1;
            if (count < 0) count = 5;
            var list = new List<Article>();
            int total = 0;
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                Query query = articlesCol.WhereEqualTo("ProjectId", domainid);
                query = filterByState(query, state);
                
                QuerySnapshot totalSnapshot = await query.GetSnapshotAsync();
                total = (int)Math.Ceiling((double)totalSnapshot.Count / count);
                if (keyword != null && keyword.Length > 0)
                {
                    int tmpTotal = 0;
                    foreach (DocumentSnapshot document in totalSnapshot.Documents)
                    {
                        var article = document.ConvertTo<Article>();
                        if (article.Title.IndexOf(keyword) >= 0) tmpTotal++;
                        total = (int)Math.Ceiling((double)tmpTotal / count);
                    }
                }

                query = articlesCol.WhereEqualTo("ProjectId", domainid);
                query = filterByState(query, state, true);

                query = query.Offset((page - 1) * count).Limit(count);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    article.Id = document.Id;
                    if (keyword != null && keyword.Length > 0)
                    {
                        if( article.Title.IndexOf(keyword) >= 0 )
                            list.Add(article);
                    }
                    else list.Add(article);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new OkObjectResult(new Item { curPage = page, total = total, data = list });
        }

        [HttpGet("valid/{domainid}/{page}/{count}")]
        public async Task<IActionResult> GetValidArticleAsync(String domainid, int page = 1, int count = 5)
        {
            if (page < 0) page = 1;
            if (count < 0) count = 5;
            var list = new List<Article>();
            int total = 0;
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                Query query = articlesCol.WhereEqualTo("ProjectId", domainid);
                QuerySnapshot totalSnapshot = await query.GetSnapshotAsync();
                total = (int)Math.Ceiling((double)totalSnapshot.Count / count);

                query = articlesCol.WhereEqualTo("ProjectId", domainid).OrderByDescending("CreatedTime").Offset((page - 1) * count).Limit(count);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    article.Id = document.Id;
                    list.Add(article);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new OkObjectResult(new Item { curPage = page, total = total, data = list });
        }

        [HttpGet("scrap_status/{articleids}")]
        public async Task<IActionResult> GetScrapStatusAsync(String articleids)
        {
            string[] idsList = articleids.Split(';');
            string[] articleRefKeyList = idsList[0].Split(',');
            string[] articleDocumentIdsList = idsList[1].Split(',');
            Dictionary<string, int> scrapStatus = new Dictionary<string, int>();
            int total = 0;
            try
            {
#if false
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                Query query = articlesCol.WhereIn(FieldPath.DocumentId, articleList);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    article.Id = document.Id;
                    scrapStatus[article.ArticleId.ToString()] = article.Progress;
                }
#else
                String refArticleId = "";
                foreach (String articleId in articleRefKeyList) {
                    refArticleId = articleId;
                    int prog = af.getApiProgress(ref refArticleId);
                    scrapStatus[articleId] = prog;
                }

                foreach (String articleDocId in articleDocumentIdsList)
                {
                    if( CommonModule.refKeyCash[articleDocId] != null 
                        && CommonModule.refKeyCash[articleDocId].ToString().Length > 0 )
                    {
                        String articleId = CommonModule.refKeyCash[articleDocId].ToString();
                        int prog = af.getApiProgress(ref articleId);
                        scrapStatus[articleDocId] = prog;
                    }
                }

                
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new OkObjectResult(scrapStatus);
        }

        [HttpGet("sync_status/{domain}/{articleids}/{isAWS}")]
        public async Task<IActionResult> GetArticleSyncAsync(string domain, String articleids, int isAWS,String? s3Name="", String? region="", String? domainActivate = "", String? domainIP = "")
        {
            string[] articleList = articleids.Split(',');
            Dictionary<string, bool> syncStatus = new Dictionary<string, bool>();
            try
            {
                if (domainActivate != "active") domain = domainIP;
                String hostDomain = CommonModule.GetDomain(domain, (isAWS == 1), s3Name, region);
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                Query query = articlesCol.WhereIn(FieldPath.DocumentId, articleList);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    String url = CommonModule.articleURL(hostDomain, article.Title, (bool)CommonModule.project2UseHttpsMap[article.ProjectId]);
                    syncStatus[document.Id] = CommonModule.RemoteFileExists(url);
                    Console.WriteLine($"url:{url} -> {syncStatus[document.Id].ToString()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return new OkObjectResult(syncStatus);
        }

        [HttpGet("fromid/{articleid}")]
        public async Task<IActionResult> GetArticleAsync(String articleid)
        {
            Article article = null;
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                DocumentReference docRef = articlesCol.Document(articleid);
                DocumentSnapshot articleSnapshot = await docRef.GetSnapshotAsync();
                if (articleSnapshot.Exists) {
                    article = articleSnapshot.ConvertTo<Article>();
                    article.Id = articleSnapshot.Id;
                    article.Content = CommonModule.PreAdjustForTitleImage(article.Content);
                }
#if false
                if (article.Progress == 0 
                    || article.ArticleId == null 
                    || article.ArticleId.Length == 0)
                {
                    if (article.ArticleId == null
                        || article.ArticleId.Length == 0) {
                        String ref_key = af.initiateArticle(JObject.Parse("{\"keyword\":\"" + article.Title + "\"}"));
                        article.ArticleId = ref_key;

                        Dictionary<string, object> update = new Dictionary<string, object>()
                        {
                            { "ArticleId", ref_key },
                            { "IsScrapping", true },
                            { "Progress", 0 },
                            { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                        };
                        docRef.UpdateAsync(update);
                    }

                    int prog = af.getApiProgress(article.ArticleId);
                    article.Progress = prog;
                    if (prog == 100) 
                    {
                        article.Content = af.getApiArticleResult(article.ArticleId);
                        Dictionary<string, object> update = new Dictionary<string, object>()
                        {
                            { "Content", article.Content },
                            { "IsScrapping", false },
                            { "Progress", 100 },
                            { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                        };
                        docRef.UpdateAsync(update);
                    }
                    else article.Content = "Article Forge process : "+prog+"%";
                }
#else
                if (article.IsScrapping && article.Progress == 0 && article.ArticleId != null)
                {
                    String refArticleId = article.ArticleId;
                    int prog = af.getApiProgress(ref refArticleId);
                    //article.ArticleId = refArticleId;
                    article.Progress = prog;
                    if (prog == 100)
                    {
                        article.Content = await af.getApiArticleResult(article.ArticleId
                            , CommonModule.project2LanguageMap[article.ProjectId].ToString());
                        Dictionary<string, object> update = new Dictionary<string, object>()
                        {
                            { "Content", article.Content },
                            { "IsScrapping", false },
                            { "Progress", 100 },
                            { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                        };
                        docRef.UpdateAsync(update);

                        await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                    , article.ProjectId
                                    , $"[Project ID={article.ProjectId}] AF Article Id={article.Id} Scrapping Done");
                    }
                    else article.Content = "Article Forge process : " + prog + "%";
                }
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(new { id = articleid, data = article });
        }

        [HttpGet("scrap/{articleid}/{question}/{mode}")]
        public async Task<IActionResult> ScrapAsync(String articleid, String question, int mode, String? lang = "EN")
        {
            question = question.Replace(";", "?");

            bool ret = false;// await CommonModule.ScrapArticleAsync(af, question, articleid, lang);

            switch (mode)
            {
                case 0://AF
                    ret = await CommonModule.ScrapArticleAsync("", af, question, articleid, lang);
                    break;
                case 1://OpenAI
                    ret = await CommonModule.ScrapArticleByOpenAIAsync("", CommonModule.manualOpenAI, question, articleid);
                    break;
            }

            return Ok(ret);
        }

        [HttpPut("{articleid}/{userid}")]
        public async Task<IActionResult> UpdateUserIdAsync(String articleid, String userid)
        {
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                DocumentReference docRef = articlesCol.Document(articleid);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "UserId", userid },
                };
                await docRef.UpdateAsync(userUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(true);
        }

        [HttpPut("UpdateState/{articleid}/{state}")]
        public async Task<IActionResult> UpdateStateAsync(String articleid, Int32 state)
        {
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                DocumentReference docRef = articlesCol.Document(articleid);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "State", state },
                };
                await docRef.UpdateAsync(userUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="articleids"></param>
        /// <param name="state">0: Unknow, 1: unapproval, 2: approval, 3:online, 4:delete</param>
        /// <returns></returns>
        [HttpPut("UpdateBatchState/{domainId}/{domainName}/{ipAddr}/{articleids}/{state}")]
        public async Task<IActionResult> UpdateBatchStateAsync(String domainId, String domainName, String ipAddr
                                                            , String articleids, Int32 state, String? s3Name="", String? region="")
        {
            bool ret = false;
            if (state == 1 || state == 2 || state == 4 || 
                (state == 3 && !CommonModule.isManualSync))
            {
                if(state == 3)
                {
                    CommonModule.isManualSync = true;
                    Task.Run(() => new SerpapiScrap().ManualArticlesSyncAsync(domainId, domainName, ipAddr, s3Name, region, articleids));
                }

                ret = await CommonModule.UpdateBatchState(articleids, state);
            }

            return Ok(ret);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mode">0: AF, 1: OpenAI</param>
        /// <param name="domainId"></param>
        /// <param name="articleids"></param>
        /// <returns></returns>
        [HttpGet("scrapArticleManual/{mode}/{domainId}/{articleids}")]
        public async Task<IActionResult> ScrapAFManualAsync(String mode, String domainId, String articleids)
        {
            bool ret = false;
            if (mode == "0" && CommonModule.isManualAFScrapping == false || mode == "1" && CommonModule.isManualOpenAIScrapping == false)
            {
                if(mode == "0") CommonModule.isManualAFScrapping = true;
                else if (mode == "1") CommonModule.isManualOpenAIScrapping = true;
                Task.Run(() => new SerpapiScrap().ScrappingManualThreadAsync(mode, domainId, articleids));

                ret = true;
            }
            return Ok(ret);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddArticleAsync([FromBody] Article article)
        {
            await CommonModule.historyLog.LogKeywordAction(article.ProjectId, article.Title, true, true);

            Article artResult = await CommonModule.AddArticle(article, "1234567890", 100);
            return Ok(new { data = artResult });
        }

        [HttpGet("AddArticlesByTitle/{domainId}/{keywords}")]
        public async Task<IActionResult> AddArticleAsync(String domainId, String keywords)
        {
            Task.Run(() => this.ManualAddArticleAsync(domainId, keywords));
            return Ok(true);
        }

        [HttpPut("update_content/{domainId}/{domainName}/{ipAddr}")]
        public async Task<IActionResult> UpdateContentAsync(string domainId, string domainName, string ipAddr, [FromBody] Article article, String? s3Name="", String? region="")
        {
            try 
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                DocumentReference docRef = articlesCol.Document(article.Id);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                    {
                        { "MetaTitle", article.MetaTitle },
                        { "MetaAuthor", article.MetaAuthor },
                        { "MetaDescription", article.MetaDescription },
                        { "MetaKeywords", article.MetaKeywords },
                        { "Content", article.Content },
                        { "ImageArray", article.ImageArray },
                        { "ThumbImageArray", article.ThumbImageArray },
                        { "Footer", article.Footer },
                    };
                await docRef.UpdateAsync(userUpdate);

                //If article is online, build & sync
                if (article.State == 3)
                {
                    CommonModule.isManualSync = true;
                    await CommonModule.BuildArticlePageThreadAsync(domainId, domainName, article.Id, CommonModule.isAWSHosting(ipAddr), s3Name, region);
                    await CommonModule.SyncWithServerThreadAsync(domainId, domainName, ipAddr, s3Name);
                    CommonModule.isManualSync = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(true);
        }

        [HttpDelete("{articleid}")]
        public async Task<IActionResult> DeleteArticleAsync(String articleid)
        {
            try
            {
                //{{
                //CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                //DocumentReference docRef = articlesCol.Document(articleid);
                //await docRef.DeleteAsync();
                //==
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                DocumentReference docRef = articlesCol.Document(articleid);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "State", 4 },
                };
                await docRef.UpdateAsync(userUpdate);
                //}}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok();
        }

        private async Task<string> ManualAddArticleAsync(String _id, String keywords)
        {
            await CommonModule.historyLog.LogKeywordAction(_id, keywords, true, true);

            //Console.WriteLine($"GoogleSearchAsync keyword={keywords}");
            keywords = keywords.Replace(';', '?');
            keywords = keywords.Replace('&', ';');
            String[] questions = keywords.Split(";");
            try
            {
                foreach (String question in questions)
                {
                    var article = new Article
                    {
                        ProjectId = _id,
                        Title = question,
                        IsScrapping = false,
                        ArticleId = "1234567890",
                        Progress = 100,
                        State = 0,
                        UpdateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                        CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                    };
                    var articleData = article;

                    CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                    Query query = articlesCol.OrderByDescending("CreatedTime")
                        .WhereEqualTo("ProjectId", articleData.ProjectId)
                        .WhereEqualTo("Title", articleData.Title).Limit(1);
                    QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                    if (projectsSnapshot.Documents.Count == 0)
                    {
                        await articlesCol.AddAsync(articleData);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return $"Complted scrapping: DomainID={_id}";
        }
    }
}