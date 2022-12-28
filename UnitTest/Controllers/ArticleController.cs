using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Cors;
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
                QuerySnapshot totalSnapshot = await articlesCol.GetSnapshotAsync();
                total = (int)Math.Ceiling((double)totalSnapshot.Count / count);

                Query query = articlesCol.OrderByDescending("CreatedTime").Offset((page - 1) * count).Limit(count);
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

        [HttpGet("{domainid}/{page}/{count}")]
        public async Task<IActionResult> GetAsync(String domainid, int page = 1, int count = 5)
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
                foreach (String articleId in articleRefKeyList) {
                    int prog = af.getApiProgress(articleId);
                    scrapStatus[articleId] = prog;
                }

                foreach (String articleDocId in articleDocumentIdsList)
                {
                    if( CommonModule.refKeyCash[articleDocId] != null 
                        && CommonModule.refKeyCash[articleDocId].ToString().Length > 0 )
                    {
                        String articleId = CommonModule.refKeyCash[articleDocId].ToString();
                        int prog = af.getApiProgress(articleId);
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

        [HttpGet("sync_status/{domain}/{articleids}")]
        public async Task<IActionResult> GetArticleSyncAsync(string domain, String articleids)
        {
            string[] articleList = articleids.Split(',');
            Dictionary<string, bool> syncStatus = new Dictionary<string, bool>();
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                Query query = articlesCol.WhereIn(FieldPath.DocumentId, articleList);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    String url = CommonModule.articleURL(domain, article.Title);
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

        [HttpGet("scrap/{articleid}/{question}")]
        public async Task<IActionResult> ScrapAsync(String articleid, String question)
        {
            question = question.Replace(";", "?");

            bool ret = await CommonModule.ScrapArticleAsync(af, question, articleid);
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

        [HttpPut("add")]
        public async Task<IActionResult> AddArticleAsync([FromBody] Article article)
        {
            bool ret = await CommonModule.AddArticle(article.ProjectId, article.Title, "1234567890",article.Content, 100);
            return Ok(ret);
        }

        [HttpPut("update_content")]
        public async Task<IActionResult> UpdateContentAsync([FromBody] Article article)
        {
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                DocumentReference docRef = articlesCol.Document(article.Id);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "Content", article.Content },
                };
                await docRef.UpdateAsync(userUpdate);
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
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                DocumentReference docRef = articlesCol.Document(articleid);
                await docRef.DeleteAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok();
        }
    }
}