using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnitTest.Lib;
using Microsoft.AspNetCore.Mvc;

namespace WebCreator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DNSController : ControllerBase
    {
        private readonly ILogger<DNSController> _logger;
        private CloudFlareAPI cloudFlareAPI = new CloudFlareAPI();
        public DNSController(ILogger<DNSController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{page}/{count}")]
        public async Task<IActionResult> GetAsync(int page = 1, int count = 5)
        {
            if (page < 0) page = 1;
            if (count < 0) count = 5;

            String sResult = "";
            WebResponse response = cloudFlareAPI.ListZone(page, count);
            using (var streamReader = new StreamReader(response.GetResponseStream()))
                sResult = (streamReader.ReadToEnd());

            return Content(sResult, "application/json");
        }

        [HttpGet("{zoneid}/{page}/{count}")]
        public async Task<IActionResult> GetAsync(string zoneId, int page = 1, int count = 5)
        {
            if (page < 0) page = 1;
            if (count < 0) count = 5;

            String sResult = "";
            WebResponse response = cloudFlareAPI.ListDns(zoneId, page, count);
            using (var streamReader = new StreamReader(response.GetResponseStream()))
                sResult = (streamReader.ReadToEnd());

            return Content(sResult, "application/json");
        }

        //[HttpGet("{domainid}/{page}/{count}")]
        //public async Task<IActionResult> GetAsync(String domainid, int page = 1, int count = 5)
        //{
        //    if (page < 0) page = 1;
        //    if (count < 0) count = 5;
        //    var list = new List<Article>();
        //    int total = 0;
        //    try
        //    {
        //        CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
        //        Query query = articlesCol.WhereEqualTo("ProjectId", domainid);
        //        QuerySnapshot totalSnapshot = await query.GetSnapshotAsync();
        //        total = (int)Math.Round((double)totalSnapshot.Count / count);

        //        query = articlesCol.WhereEqualTo("ProjectId", domainid).OrderByDescending("CreatedTime").Offset((page - 1) * count).Limit(count);
        //        QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();

        //        foreach (DocumentSnapshot document in projectsSnapshot.Documents)
        //        {
        //            var article = document.ConvertTo<Article>();
        //            article.Id = document.Id;
        //            list.Add(article);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }

        //    return new OkObjectResult(new Item { curPage = page, total = total, data = list });
        //}

        //[HttpGet("fromid/{articleid}")]
        //public async Task<IActionResult> GetArticleAsync(String articleid)
        //{
        //    Article article = null;
        //    try
        //    {
        //        CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
        //        DocumentReference docRef = articlesCol.Document(articleid);
        //        DocumentSnapshot articleSnapshot = await docRef.GetSnapshotAsync();
        //        if (articleSnapshot.Exists)
        //        {
        //            article = articleSnapshot.ConvertTo<Article>();
        //        }

        //        if (article.Progress == 0
        //            || article.ArticleId == null
        //            || article.ArticleId.Length == 0)
        //        {
        //            if (article.ArticleId == null
        //                || article.ArticleId.Length == 0)
        //            {
        //                String ref_key = af.initiateArticle(JObject.Parse("{\"keyword\":\"" + article.Title + "\"}"));
        //                article.ArticleId = ref_key;

        //                Dictionary<string, object> update = new Dictionary<string, object>()
        //                {
        //                    { "ArticleId", ref_key },
        //                    { "Progress", 0 },
        //                };
        //                docRef.UpdateAsync(update);
        //            }

        //            int prog = af.getApiProgress(article.ArticleId);
        //            article.Progress = prog;
        //            if (prog == 100)
        //            {
        //                article.Content = af.getApiArticleResult(article.ArticleId);
        //                Dictionary<string, object> update = new Dictionary<string, object>()
        //                {
        //                    { "Content", article.Content },
        //                    { "Progress", 100 },
        //                };
        //                docRef.UpdateAsync(update);
        //            }
        //            else article.Content = "Article Forge process : " + prog + "%";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }

        //    return Ok(new { id = articleid, data = article });
        //}
    }
}
