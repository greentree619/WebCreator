using Google.Cloud.Firestore;
using Newtonsoft.Json.Linq;
using OpenAI_API;
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
    {
        ArticleForge manualAF = new ArticleForge();
        public SerpapiScrap()
        {
        }

        public async Task ScrappingThreadAsync(String _id, String keyword, Int32 count)
        {
            CommonModule.Log(_id.ToString(), "ScrappingThreadAsync Start", "question");
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
            CommonModule.Log(_id.ToString(), "ScrappingThreadAsync End", "question");
        }
        
        public async Task ScrappingAFThreadAsync(String _id, String scheduleId)
        {
            CommonModule.Log(_id.ToString(), $"ScrappingAFThreadAsync start", "scrap");
            String lang = CommonModule.project2LanguageMap[_id].ToString();
            //Omitted JObject scrapStatus = (JObject)await CommonModule.IsDomainScrappingAsync(_id);
            //Omitted             bool isScrapping = (bool)scrapStatus["afapi"];
            //Omitted if (!isScrapping)
            await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                    , _id
                                    , $"[Project ID={_id}] AF Scrapping Thread Start");
            {
                CommonModule.SetDomainAFScrappingAsync(_id, true);
                try
                {
                    Schedule schedule;
                    CollectionReference scheduleCol = Config.FirebaseDB.Collection("Schedules");
                    DocumentReference docRef = scheduleCol.Document(scheduleId);
                    DocumentSnapshot scheduleSnapshot = await docRef.GetSnapshotAsync();

                    while ( (bool)CommonModule.articleScrappingThreadList[_id] )
                    {
                        CommonModule.Log(_id.ToString(), $"ScrappingAFThreadAsync > main-proc step 1/3", "scrap");
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

                            CommonModule.Log(_id.ToString(), $"ScrappingAFThreadAsync > main-proc step 2/3", "scrap");
                            for (int i = 0; i < schedule.JustNowCount 
                                            && (bool)CommonModule.articleScrappingThreadList[_id]
                                            && scrapArticles.Count > 0; i++)
                            {
                                Article scrapAF = scrapArticles.Pop();
                                do
                                {
                                    Thread.Sleep(10000);
                                    //{{In case start manual scrap, sleep untile complete
                                    while (CommonModule.isManualAFScrapping) Thread.Sleep(5000);
                                    //}}In case start manual scrap, sleep untile complete
                                    afRet = await CommonModule.ScrapArticleAsync(_id, af, scrapAF.Title, scrapAF.Id, lang);
                                    if (afRet)
                                    {
                                        await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                        , _id
                                        , $"[Project ID={_id}] AF Article Id={scrapAF.Id} Scrapping Start[{afRet}]");
                                    }
                                    else
                                    {
                                        await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                        , _id
                                        , $"AF Error {af.error_message}");
                                    }
                                }
                                while (!afRet && (bool)CommonModule.articleScrappingThreadList[_id]);
                            }

                            CommonModule.Log(_id.ToString(), $"ScrappingAFThreadAsync > main-proc step 3/3", "scrap");
                            while ((bool)CommonModule.articleScrappingThreadList[_id]
                                && scrapArticles.Count > 0)
                            {
                                Thread.Sleep(schedule.SpanTime * schedule.SpanUnit * 1000);

                                for (int i = 0; i < schedule.EachCount 
                                    && (bool)CommonModule.articleScrappingThreadList[_id]
                                    && scrapArticles.Count > 0; i++)
                                {
                                    Article scrapAF = scrapArticles.Pop();
                                    do
                                    {
                                        Thread.Sleep(10000);
                                        //{{In case start manual scrap, sleep untile complete
                                        while (CommonModule.isManualAFScrapping) Thread.Sleep(5000);
                                        //}}In case start manual scrap, sleep untile complete
                                        afRet = await CommonModule.ScrapArticleAsync(_id, af, scrapAF.Title, scrapAF.Id, lang);
                                        if (afRet)
                                        {
                                            await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                                , _id
                                                , $"[Project ID={_id}] AF Article Id={scrapAF.Id} Scrapping Start[{afRet}]");
                                        }
                                        else
                                        {
                                            await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                            , _id
                                            , $"AF Error {af.error_message}");
                                        }
                                    }
                                    while (!afRet && (bool)CommonModule.articleScrappingThreadList[_id]);

                                }
                            }
                        }

                        await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                            , _id
                                            , $"[Project ID={_id}] AF Article Scrapping Repeat");
                        Thread.Sleep(10000);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                    , _id
                                    , $"[Project ID={_id}] Exception {ex.Message}");
                    CommonModule.Log(_id.ToString(), $"ScrappingAFThreadAsync Exception: {ex.Message.ToString()}", "scrap");
                }
                
                Console.WriteLine("AF scrapping All done.");

                CommonModule.SetDomainAFScrappingAsync(_id, false);
                CommonModule.articleScrappingThreadList[_id] = false;
                await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                    , _id
                                    , $"[Project ID={_id}] AF Scrapping Thread Stop");
            }
            CommonModule.Log(_id.ToString(), $"ScrappingAFThreadAsync end", "scrap");
        }

        public async Task ScrappingOpenAIThreadAsync(String _id, String scheduleId)
        {
            CommonModule.Log(_id.ToString(), $"ScrappingOpenAIThreadAsync start", "scrap");
            {
                await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                    , _id
                                    , $"[Project ID={_id}] OpenAI Scrapping Thread Start");
                CommonModule.SetDomainOpenAIScrappingAsync(_id, true);
                try
                {
                    Schedule schedule;
                    CollectionReference scheduleCol = Config.FirebaseDB.Collection("Schedules");
                    DocumentReference docRef = scheduleCol.Document(scheduleId);
                    DocumentSnapshot scheduleSnapshot = await docRef.GetSnapshotAsync();

                    while ( (bool)CommonModule.articleScrappingThreadList[_id] )
                    {
                        CommonModule.Log(_id.ToString(), $"ScrappingOpenAIThreadAsync step 1/3\n", "scrap");
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
                            CommonModule.Log(_id.ToString(), $"ScrappingOpenAIThreadAsync step 2/3\n", "scrap");
                            schedule = scheduleSnapshot.ConvertTo<Schedule>();

                            for (int i = 0; i < schedule.JustNowCount 
                                && (bool)CommonModule.articleScrappingThreadList[_id]
                                && scrapArticles.Count > 0; i++)
                            {
                                Article scrapAF = scrapArticles.Pop();
                                do
                                {
                                    Thread.Sleep(10000);
                                    //{{In case start manual scrap, sleep untile complete
                                    while (CommonModule.isManualOpenAIScrapping) Thread.Sleep(5000);
                                    //}}In case start manual scrap, sleep untile complete
                                    afRet = await CommonModule.ScrapArticleByOpenAIAsync(_id, CommonModule.manualOpenAI, scrapAF.Title, scrapAF.Id);
                                    await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                        , _id
                                        , $"[Project ID={_id}] OpenAI Article Id={scrapAF.Id} Scrapping Ok[{afRet}]");
                                }
                                while (!afRet && (bool)CommonModule.articleScrappingThreadList[_id]);
                            }

                            CommonModule.Log(_id.ToString(), $"ScrappingOpenAIThreadAsync step 3/3\n", "scrap");
                            while ((bool)CommonModule.articleScrappingThreadList[_id] && scrapArticles.Count > 0)
                            {
                                Thread.Sleep(schedule.SpanTime * schedule.SpanUnit * 1000);

                                for (int i = 0; i < schedule.EachCount 
                                    && (bool)CommonModule.articleScrappingThreadList[_id]
                                    && scrapArticles.Count > 0; i++)
                                {
                                    Article scrapAF = scrapArticles.Pop();
                                    do
                                    {
                                        Thread.Sleep(10000);
                                        //{{In case start manual scrap, sleep untile complete
                                        while (CommonModule.isManualOpenAIScrapping) Thread.Sleep(5000);
                                        //}}In case start manual scrap, sleep untile complete
                                        afRet = await CommonModule.ScrapArticleByOpenAIAsync(_id, CommonModule.manualOpenAI, scrapAF.Title, scrapAF.Id);
                                        await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                            , _id
                                            , $"[Project ID={_id}] OpenAI Article Id={scrapAF.Id} Scrapping Ok[{afRet}]");
                                    }
                                    while (!afRet && (bool)CommonModule.articleScrappingThreadList[_id]);

                                }
                            }
                        }

                        await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                            , _id
                                            , $"[Project ID={_id}] OpenAI Article Scrapping Repeat");
                        Thread.Sleep(10000);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                    , _id
                                    , $"[Project ID={_id}] Exception {ex.Message}");
                    CommonModule.Log(_id.ToString(), $"ScrappingOpenAIThreadAsync Exception: {ex.Message}\n", "scrap");
                }

                Console.WriteLine("OpenAI scrapping All done.");

                CommonModule.SetDomainOpenAIScrappingAsync(_id, false);
                CommonModule.articleScrappingThreadList[_id] = false;
                await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                    , _id
                                    , $"[Project ID={_id}] OpenAI Scrapping Thread Stop");
            }
            CommonModule.Log(_id.ToString(), $"ScrappingOpenAIThreadAsync end", "scrap");
        }

        public async Task ScrappingManualThreadAsync(String mode, String _id, String articleIds)
        {
            try
            {
                String lang = CommonModule.project2LanguageMap[_id].ToString();
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

                bool afRet = false;
                while (scrapArticles.Count > 0)
                {
                    Article scrapAF = scrapArticles.Pop();
                    do
                    {
                        switch(mode)
                        {
                            case "0"://AF
                                Thread.Sleep(10000);
                                afRet = await CommonModule.ScrapArticleAsync(_id, manualAF, scrapAF.Title, scrapAF.Id, lang);
                                if (!afRet)
                                {
                                    await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                    , scrapAF.ProjectId
                                    , $"AF Error {manualAF.error_message}");
                                }
                                break;
                            case "1"://OpenAI
                                afRet = await CommonModule.ScrapArticleByOpenAIAsync(_id, CommonModule.manualOpenAI, scrapAF.Title, scrapAF.Id);
                                break;
                        }
                    }
                    while ( !afRet );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if(mode == "0") CommonModule.isManualAFScrapping = false;
            else if (mode == "1") CommonModule.isManualOpenAIScrapping = false;
        }

        public async Task ManualArticlesSyncAsync(String domainId, String domainName, String ipAddr, String s3Name, String region, String articleIds)
        {
            await CommonModule.BuildPagesFromArtidleIdsAsync(domainId, domainName, articleIds, CommonModule.isAWSHosting(ipAddr), s3Name, region);
            await CommonModule.SyncWithServerThreadAsync(domainId, domainName, ipAddr, s3Name);
            CommonModule.isManualSync = false;
        }

        public async Task PublishThreadAsync(String _id, String scheduleId)
        {
            {
                await CommonModule.historyLog.LogActionHistory(CommonModule.PublishCategory
                    , _id
                    , $"[Project ID={_id}] Publish Thread Start");
                CommonModule.SetDomainPublishScheduleAsync(_id, true);
                try
                {
                    bool publishRet = false;
                    PublishSchedule schedule;
                    CollectionReference scheduleCol = Config.FirebaseDB.Collection("PublishSchedules");
                    DocumentReference docRef = scheduleCol.Document(scheduleId);
                    DocumentSnapshot scheduleSnapshot = await docRef.GetSnapshotAsync();

                    CollectionReference col2 = Config.FirebaseDB.Collection("Projects");
                    DocumentReference docRef2 = col2.Document(_id);
                    DocumentSnapshot projectSnapshot = await docRef2.GetSnapshotAsync();
                    var projInfo = projectSnapshot.ConvertTo<Project>();

                    while ( (bool)CommonModule.publishThreadList[_id] )
                    {
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

                            for (int i = 0; i < schedule.JustNowCount 
                                && (bool)CommonModule.publishThreadList[_id]
                                && scrapArticles.Count > 0; i++)
                            {
                                Article scrapAF = scrapArticles.Pop();
                                do
                                {
                                    Thread.Sleep(10000);

                                    //Incase manual sync, sleep untile complete
                                    while (CommonModule.isManualSync) Thread.Sleep(5000);

                                    //{{
                                    await CommonModule.BuildArticlePageThreadAsync(_id, projInfo.Name, scrapAF.Id, CommonModule.isAWSHosting(projInfo.Ip), projInfo.S3BucketName, projInfo.S3BucketRegion);
                                    await CommonModule.SyncWithServerThreadAsync(_id, projInfo.Name, projInfo.Ip, projInfo.S3BucketName);
                                    //}}
                                    await CommonModule.historyLog.LogActionHistory(CommonModule.PublishCategory
                                        , _id
                                        , $"[Project ID={_id}] Article Id={scrapAF.Id} Sync OK");
                                    publishRet = true;
                                }
                                while (!publishRet);
                            }

                            while ((bool)CommonModule.publishThreadList[_id] && scrapArticles.Count > 0)
                            {
                                Thread.Sleep(schedule.SpanTime * schedule.SpanUnit * 1000);

                                for (int i = 0; i < schedule.EachCount 
                                    && (bool)CommonModule.publishThreadList[_id]
                                    && scrapArticles.Count > 0; i++)
                                {
                                    Article scrapAF = scrapArticles.Pop();
                                    do
                                    {
                                        Thread.Sleep(10000);
                                        //Incase manual sync, sleep untile complete
                                        while (CommonModule.isManualSync) Thread.Sleep(5000);

                                        //{{
                                        await CommonModule.BuildArticlePageThreadAsync(_id, projInfo.Name, scrapAF.Id, CommonModule.isAWSHosting(projInfo.Ip), projInfo.S3BucketName, projInfo.S3BucketRegion);
                                        await CommonModule.SyncWithServerThreadAsync(_id, projInfo.Name, projInfo.Ip, projInfo.S3BucketName);
                                        //}}
                                        await CommonModule.historyLog.LogActionHistory(CommonModule.PublishCategory
                                        , _id
                                        , $"[Project ID={_id}] Article Id={scrapAF.Id} Sync OK");
                                        publishRet = true;
                                    }
                                    while (!publishRet);
                                }
                            }
                        }

                        Thread.Sleep(10000);
                        await CommonModule.historyLog.LogActionHistory(CommonModule.PublishCategory
                                        , _id
                                        , $"[Project ID={_id}] Publish Process Repeat");
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                    await CommonModule.historyLog.LogActionHistory(CommonModule.PublishCategory
                    , _id
                    , $"[Project ID={_id}] Exception {ex.Message}");
                }

                CommonModule.SetDomainPublishScheduleAsync(_id, false);
                CommonModule.publishThreadList[_id] = false;
                await CommonModule.historyLog.LogActionHistory(CommonModule.PublishCategory
                    , _id
                    , $"[Project ID={_id}] Publish Thread Stop");
            }
        }

        //_id: domain id
        public async Task UpdateArticleThemeThreadAsync(String domainid, String domainName, String ipaddr, String s3Name, String region)
        {
            try
            {
                await CommonModule.BuildPagesThreadAsync(domainid, domainName, CommonModule.isAWSHosting(ipaddr), s3Name, region, 3, false);
                await CommonModule.SyncWithServerThreadAsync(domainid, domainName, ipaddr, s3Name);
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
            CommonModule.Log(_id.ToString(), "GoogleSearchAsync Start", "question");
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
                CommonModule.Log(_id.ToString(), "GoogleSearchAsync > GoogleSearch", "question");
                GoogleSearch search = new GoogleSearch(ht, apiKey);
                JObject data = search.GetJson();
                JArray questions = (JArray)data["related_questions"];
                if (questions != null)
                {
                    CommonModule.Log(_id.ToString(), $"GoogleSearchAsync > GoogleSearch Count: {questions.Count.ToString()}", "question");
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
                                await CommonModule.historyLog.LogScrapKeywordAction(_id, articleData.Title);

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
                }
                    
                // close socket
                search.Close();
                CommonModule.Log(_id.ToString(), $"GoogleSearchAsync > GoogleSearch Close", "question");

                CommonModule.Log(_id.ToString(), $"GoogleSearchAsync > GoogleSearch next_page_token: {next_page_token.Length.ToString()}", "question");
                //Once more questions.
                while (next_page_token.Length > 0 && curCount < count)
                {
                    Hashtable htOncemore = new Hashtable();
                    htOncemore.Add("engine", "google_related_questions");
                    htOncemore.Add("next_page_token", next_page_token);

                    CommonModule.Log(_id.ToString(), $"GoogleSearchAsync > Next GoogleSearch", "question");
                    search = new GoogleSearch(htOncemore, apiKey);
                    data = search.GetJson();
                    questions = (JArray)data["related_questions"];
                    if (questions != null)
                    {
                        CommonModule.Log(_id.ToString(), $"GoogleSearchAsync > Next GoogleSearch count:{questions.Count.ToString()}", "question");
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
                                    await CommonModule.historyLog.LogScrapKeywordAction(_id, articleData.Title);

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
                    }   
                    // close socket
                    search.Close();
                    CommonModule.Log(_id.ToString(), $"GoogleSearchAsync > Next GoogleSearch Close", "question");
                }
            }
            catch (SerpApiSearchException ex)
            {
                Console.WriteLine("Exception:");
                Console.WriteLine(ex.ToString());
                CommonModule.Log(_id.ToString(), $"GoogleSearchAsync > Exception: {ex.ToString()}", "question");
            }

            CommonModule.Log(_id.ToString(), "GoogleSearchAsync End", "question");
            return $"Complted scrapping: DomainID={_id} Keyword={keyword} total_count={curCount}";
        }
    }
}
