using Google.Cloud.Firestore;
using Newtonsoft.Json.Linq;
using SerpApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCreator;
using WebCreator.Models;

namespace UnitTest.Lib
{
    internal class SerpapiScrap
    {   public SerpapiScrap()
        {
        }

        public async Task ScrappingThreadAsync(String _id, String keyword, Int32 count)
        {
            //Omitted JObject scrapStatus = (JObject)await CommonModule.IsDomainScrappingAsync(_id);
            //Omitted bool isScrapping = (bool)scrapStatus["serpapi"];
            //Omitted if (!isScrapping)
            {
                CommonModule.SetDomainScrappingAsync(_id, true);

                keyword = keyword.Replace(';', '?');
                keyword = keyword.Replace('&', ';');
                String[] keywords = keyword.Split(";");

                Console.WriteLine("Starting up SerpAPI scrapping...");
                var tasks = new List<Task>();
                int eachCount = count / keywords.Length;
                int taskIdx = 0;
                foreach (String kword in keywords)
                {
                    taskIdx++;
                    int qCount = eachCount;
                    if (taskIdx == keywords.Length) qCount = count - (eachCount * (taskIdx - 1));
                    //create and start tasks, then add them to the list
                    tasks.Add(Task.Run(() => new SerpapiScrap().GoogleSearchAsync(_id, kword, qCount)).ContinueWith(LogResult));
                }

                await Task.WhenAll(tasks);
                Console.WriteLine("All done.");

                CommonModule.SetDomainScrappingAsync(_id, false);
                CommonModule.threadList[_id] = false;
            }
        }
        
        public async Task ScrappingAFThreadAsync(String _id, String scheduleId)
        {
            //Omitted JObject scrapStatus = (JObject)await CommonModule.IsDomainScrappingAsync(_id);
            //Omitted             bool isScrapping = (bool)scrapStatus["afapi"];
            //Omitted if (!isScrapping)
            {
                CommonModule.SetDomainAFScrappingAsync(_id, true);
                try
                {
                    Schedule schedule;
                    CollectionReference scheduleCol = Config.FirebaseDB.Collection("Schedules");
                    DocumentReference docRef = scheduleCol.Document(scheduleId);
                    DocumentSnapshot scheduleSnapshot = await docRef.GetSnapshotAsync();

                    CollectionReference col = Config.FirebaseDB.Collection("Articles");
                    Query query = col.WhereEqualTo("ProjectId", _id).WhereEqualTo("Progress", 0).WhereEqualTo("IsScrapping", false).OrderBy("CreatedTime");
                    QuerySnapshot totalSnapshot = await query.GetSnapshotAsync();

                    Stack<Article> scrapArticles = new Stack<Article>();
                    foreach (DocumentSnapshot document in totalSnapshot.Documents)
                    {
                        var article = document.ConvertTo<Article>();
                        article.Id = document.Id;
                        scrapArticles.Push(article);
                    }

                    ArticleForge af = new ArticleForge();
                    bool afRet = false;
                    if (scheduleSnapshot.Exists && scrapArticles.Count > 0)
                    {
                        schedule = scheduleSnapshot.ConvertTo<Schedule>();

                        for (int i = 0; i < schedule.JustNowCount && (bool)CommonModule.afThreadList[_id]; i++)
                        {
                            Article scrapAF = scrapArticles.Pop();
                            do {
                                Thread.Sleep(10000);
                                //{{In case start manual scrap, sleep untile complete
                                while( CommonModule.isManualAFScrapping ) Thread.Sleep(5000);
                                //}}In case start manual scrap, sleep untile complete
                                afRet = await CommonModule.ScrapArticleAsync(af, scrapAF.Title, scrapAF.Id);
                            }
                            while (!afRet && (bool)CommonModule.afThreadList[_id]);
                        }

                        while ( (bool)CommonModule.afThreadList[_id] )
                        {
                            Thread.Sleep(schedule.SpanTime * schedule.SpanUnit * 1000);

                            for (int i = 0; i < schedule.EachCount && (bool)CommonModule.afThreadList[_id]; i++)
                            {
                                Article scrapAF = scrapArticles.Pop();
                                do {
                                    Thread.Sleep(10000);
                                    //{{In case start manual scrap, sleep untile complete
                                    while (CommonModule.isManualAFScrapping) Thread.Sleep(5000);
                                    //}}In case start manual scrap, sleep untile complete
                                    afRet = await CommonModule.ScrapArticleAsync(af, scrapAF.Title, scrapAF.Id);
                                }
                                while ( !afRet && (bool)CommonModule.afThreadList[_id]);
                                
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
                Console.WriteLine("AF scrapping All done.");

                CommonModule.SetDomainAFScrappingAsync(_id, false);
                CommonModule.afThreadList[_id] = false;
            }
        }

        public async Task ScrappingManualAFThreadAsync(String _id, String articleIds)
        {
            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("Articles");
                Query query = col.WhereIn(FieldPath.DocumentId, articleIds.Split(','));
                QuerySnapshot totalSnapshot = await query.GetSnapshotAsync();

                Stack<Article> scrapArticles = new Stack<Article>();
                foreach (DocumentSnapshot document in totalSnapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    article.Id = document.Id;
                    scrapArticles.Push(article);
                }

                ArticleForge af = new ArticleForge();
                bool afRet = false;
                while (scrapArticles.Count > 0)
                {
                    Article scrapAF = scrapArticles.Pop();
                    do
                    {
                        Thread.Sleep(10000);
                        afRet = await CommonModule.ScrapArticleAsync(af, scrapAF.Title, scrapAF.Id);
                    }
                    while ( !afRet );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            CommonModule.isManualAFScrapping = false;
        }

        public async Task ManualArticlesSyncAsync(String domainId, String domainName, String ipAddr, String articleIds)
        {
            await CommonModule.BuildPagesFromArtidleIdsAsync(domainId, domainName, articleIds);
            await CommonModule.SyncWithServerThreadAsync(domainId, domainName, ipAddr);
            CommonModule.isManualSync = false;
        }

        public async Task PublishThreadAsync(String _id, String scheduleId)
        {
            {
                CommonModule.SetDomainPublishScheduleAsync(_id, true);
                try
                {
                    PublishSchedule schedule;
                    CollectionReference scheduleCol = Config.FirebaseDB.Collection("PublishSchedules");
                    DocumentReference docRef = scheduleCol.Document(scheduleId);
                    DocumentSnapshot scheduleSnapshot = await docRef.GetSnapshotAsync();

                    CollectionReference col2 = Config.FirebaseDB.Collection("Projects");
                    DocumentReference docRef2 = col2.Document(_id);
                    DocumentSnapshot projectSnapshot = await docRef2.GetSnapshotAsync();
                    var projInfo = projectSnapshot.ConvertTo<Project>();

                    CollectionReference col = Config.FirebaseDB.Collection("Articles");
                    Query query = col.WhereEqualTo("ProjectId", _id).WhereEqualTo("State", 2).OrderBy("UpdateTime");
                    QuerySnapshot totalSnapshot = await query.GetSnapshotAsync();

                    Stack<Article> scrapArticles = new Stack<Article>();
                    foreach (DocumentSnapshot document in totalSnapshot.Documents)
                    {
                        var article = document.ConvertTo<Article>();
                        article.Id = document.Id;
                        scrapArticles.Push(article);
                    }

                    ArticleForge af = new ArticleForge();
                    if (scheduleSnapshot.Exists && scrapArticles.Count > 0)
                    {
                        schedule = scheduleSnapshot.ConvertTo<PublishSchedule>();

                        for (int i = 0; i < schedule.JustNowCount && (bool)CommonModule.publishThreadList[_id]; i++)
                        {
                            Article scrapAF = scrapArticles.Pop();
                            do
                            {
                                Thread.Sleep(10000);
                                
                                //Incase manual sync, sleep untile complete
                                while (CommonModule.isManualSync) Thread.Sleep(5000);

                                //{{
                                await CommonModule.BuildArticlePageThreadAsync(_id, projInfo.Name, scrapAF.Id);
                                await CommonModule.SyncWithServerThreadAsync(_id, projInfo.Name, projInfo.Ip);
                                //}}
                            }
                            while ((bool)CommonModule.publishThreadList[_id]);
                        }

                        while ((bool)CommonModule.publishThreadList[_id])
                        {
                            Thread.Sleep(schedule.SpanTime * schedule.SpanUnit * 1000);

                            for (int i = 0; i < schedule.EachCount && (bool)CommonModule.publishThreadList[_id]; i++)
                            {
                                Article scrapAF = scrapArticles.Pop();
                                do
                                {
                                    Thread.Sleep(10000);
                                    //Incase manual sync, sleep untile complete
                                    while (CommonModule.isManualSync) Thread.Sleep(5000);

                                    //{{
                                    await CommonModule.BuildArticlePageThreadAsync(_id, projInfo.Name, scrapAF.Id);
                                    await CommonModule.SyncWithServerThreadAsync(_id, projInfo.Name, projInfo.Ip);
                                    //}}
                                }
                                while ((bool)CommonModule.publishThreadList[_id]);

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                CommonModule.SetDomainPublishScheduleAsync(_id, false);
                CommonModule.publishThreadList[_id] = false;
            }
        }

        //_id: domain id
        public async Task UpdateArticleThemeThreadAsync(String domainid, String domainName, String ipaddr)
        {
            try
            {
                await CommonModule.BuildPagesThreadAsync(domainid, domainName, 3, false);
                await CommonModule.SyncWithServerThreadAsync(domainid, domainName, ipaddr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void LogResult(Task<string> task)
        {
            Console.WriteLine($"Is Valid: {task.Result}");
        }

        public async Task<string> GoogleSearchAsync(String _id, String keyword, int count)
        {
            Console.WriteLine($"GoogleSearchAsync keyword={keyword} count={count}");
            // secret api key from https://serpapi.com/dashboard
            String apiKey = Config.SerpApiKey;
            int curCount = 0;
            String next_page_token = "";

            // Localized search for Coffee shop in Austin Texas
            keyword += "?";
            Hashtable ht = new Hashtable();
            //Omitted ht.Add("location", "Austin, Texas, United States");
            ht.Add("engine", "google");
            ht.Add("gl", "us");
            ht.Add("q", keyword);

            try
            {
                GoogleSearch search = new GoogleSearch(ht, apiKey);
                JObject data = search.GetJson();
                JArray questions = (JArray)data["related_questions"];
                if (questions != null)
                    foreach (JObject question in questions)
                    {
                        if (curCount >= count) break;

                        next_page_token = (String)question["next_page_token"];
                        //Console.WriteLine("Question: " + question["question"]);
                        var article = new Article
                        {
                            ProjectId = _id,
                            Title = (String)question["question"],
                            IsScrapping = false,
                            Progress = 0,
                            State = 0,
                            UpdateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                        };
                        var articleData = article;

                        try
                        {
                            CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                            Query query = articlesCol.OrderByDescending("CreatedTime")
                                .WhereEqualTo("ProjectId", articleData.ProjectId)
                                .WhereEqualTo("Title", articleData.Title).Limit(1);
                            QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                            if (projectsSnapshot.Documents.Count == 0)
                            {
                                await articlesCol.AddAsync(articleData);
                                curCount++;
                            }
                            else curCount += 1;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                // close socket
                search.Close();

                //Once more questions.
                while(next_page_token.Length > 0 && curCount < count)
                {
                    Hashtable htOncemore = new Hashtable();
                    htOncemore.Add("engine", "google_related_questions");
                    htOncemore.Add("next_page_token", next_page_token);
                    
                    search = new GoogleSearch(htOncemore, apiKey);
                    data = search.GetJson();
                    questions = (JArray)data["related_questions"];
                    if (questions != null)
                        foreach (JObject question in questions)
                        {
                            if (curCount >= count) break;

                            next_page_token = (String)question["next_page_token"];
                            //Console.WriteLine("Question: " + question["question"]);
                            var article = new Article
                            {
                                ProjectId = _id,
                                Title = (String)question["question"],
                                IsScrapping = false,
                                Progress = 0,
                                State = 0,
                                UpdateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                                CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                            };
                            var articleData = article;

                            try
                            {
                                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                                Query query = articlesCol.OrderByDescending("CreatedTime")
                                    .WhereEqualTo("ProjectId", articleData.ProjectId)
                                    .WhereEqualTo("Title", articleData.Title).Limit(1);
                                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                                if (projectsSnapshot.Documents.Count == 0)
                                {
                                    await articlesCol.AddAsync(articleData);
                                    curCount++;
                                }
                                else curCount += 1;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                    // close socket
                    search.Close();
                }
            }
            catch (SerpApiSearchException ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.ToString());
            }

            return $"Complted scrapping: DomainID={_id} Keyword={keyword} total_count={curCount}";
        }
    }
}
