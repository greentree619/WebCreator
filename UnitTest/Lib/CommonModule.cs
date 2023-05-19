using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCreator;
using WebCreator.Models;
using Newtonsoft.Json;
using System.Collections;
using System.Net;
using System.Web;
using System.IO.Compression;
using System.Text.RegularExpressions;
using OpenAI_API;
using UnitTest.Models;
using Amazon.S3;
using Amazon;
using AWSUtility;
using Amazon.S3.Model;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace UnitTest.Lib
{
    internal class CommonModule
    {
        public static Hashtable threadList = new Hashtable();
        public static Hashtable articleScrappingThreadList = new Hashtable();
        public static Hashtable publishThreadList = new Hashtable();
        public static Hashtable project2LanguageMap = new Hashtable();
        public static Hashtable project2UseHttpsMap = new Hashtable();
        public static Hashtable project2ImageAutoGenInfoMap = new Hashtable();
        public static Hashtable refKeyCash = new Hashtable();
        public static Hashtable onThemeUpdateCash = new Hashtable();
        public static Hashtable domainScrappingScheduleStatus = new Hashtable();
        public static ArticleForgeSetting afSetting = new ArticleForgeSetting();
        public static OpenAIAPISetting openAISetting = new OpenAIAPISetting();
        public static bool isManualAFScrapping = false;
        public static bool isManualOpenAIScrapping = false;
        public static bool isManualSync = false;
        public static HistoryManagement historyLog = new HistoryManagement();
        public static AmazonS3Client amazonS3Client = new AmazonS3Client( Config.AWSAccessKey, Config.AWSSecretKey, RegionEndpoint.USEast2);//us-east-2
        public static DeepLTranslate deepLTranslate = new DeepLTranslate();
        public static String baseLanguage = "EN";
        public static OpenAIAPI manualOpenAI = new OpenAIAPI(Config.OpenAIKey);
        public static int OpenAIImageHeight = 1024;
        public static int OpenAIImageWidth = 1024;//1280;
        public static int PixabayImageWidth = 1280;
        public static String PublishCategory = "Publish";
        public static String ArticleScrapCategory = "ArticleScrap";
        public static String OpenAIScrapCategory = "OpenAIScrap";
        public static Hashtable questionTransMap = new Hashtable();

        public static async Task SetDomainScrappingAsync(String domainId, bool isScrapping)
        {
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Projects");
                DocumentReference docRef = articlesCol.Document(domainId);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "OnScrapping",  isScrapping},
                    { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                };
                await docRef.UpdateAsync(userUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task SetDomainAFScrappingAsync(String domainId, bool isScrapping)
        {
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Projects");
                DocumentReference docRef = articlesCol.Document(domainId);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "OnAFScrapping",  isScrapping},
                    { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                };
                await docRef.UpdateAsync(userUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task SetDomainOpenAIScrappingAsync(String domainId, bool isScrapping)
        {
            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("Projects");
                DocumentReference docRef = col.Document(domainId);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "OnOpenAIScrapping",  isScrapping},
                    { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                };
                await docRef.UpdateAsync(userUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task SetDomainPublishScheduleAsync(String domainId, bool isRunning)
        {
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Projects");
                DocumentReference docRef = articlesCol.Document(domainId);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "OnPublishSchedule",  isRunning},
                    { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                };
                await docRef.UpdateAsync(userUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }        

        //public static async Task<JObject> IsDomainScrappingAsync(String domainId)
        //{
        //    bool isScrapping = false;
        //    bool isAFScrapping = false;
        //    try
        //    {
        //        CollectionReference articlesCol = Config.FirebaseDB.Collection("Projects");
        //        DocumentReference docRef = articlesCol.Document(domainId);
        //        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        //        if (snapshot.Exists)
        //        {
        //            var prj = snapshot.ConvertTo<Project>();
        //            isScrapping = (prj.OnScrapping != null ? prj.OnScrapping : false);
        //            isAFScrapping = (prj.OnAFScrapping != null ? prj.OnAFScrapping : false);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }

        //    JObject res = new JObject();
        //    res["serpapi"] = isScrapping;
        //    res["afapi"] = isAFScrapping;

        //    return res;
        //}

        public static async Task<bool> ScrapArticleAsync(String domainId, ArticleForge af, String question, String articleid, String language) {
            bool status = false;
            CommonModule.Log(domainId.ToString(), $"ScrapArticleAsync Start", "scrap");
            try
            {
                question = await deepLTranslate.TranslateForQuestion(question, language);

                dynamic jsonObjectParam = new JObject();
                jsonObjectParam.keyword = question;
                //jsonObjectParam.sub_keywords = "subkeyword1,subkeyword2,subkeyword3";
                jsonObjectParam.sentence_variation = (Int32)CommonModule.afSetting.setInf.SentenceVariation;//a list of sub-keywords separated by comma (e.g. subkeyword1,subkeyword2,subkeyword3).
                jsonObjectParam.paragraph_variation = (Int32)CommonModule.afSetting.setInf.ParagraphVariation;//number of paragraph variations. It can be either 1, 2, or 3. The default value is 1.
                jsonObjectParam.shuffle_paragraphs = (Int32)CommonModule.afSetting.setInf.ShuffleParagraphs;//enable shuffle paragraphs or not.It can be either 0(disabled) or 1(enabled).The default value is 0.
                jsonObjectParam.length = CommonModule.afSetting.GetLengthString();//the length of the article. It can be either ‘very_short’(approximately 50 words), ‘short’(approximately 200 words), ‘medium’(approximately 500 words), or ‘long’(approximately 750 words). The default value is ‘short’.
                jsonObjectParam.title = (Int32)CommonModule.afSetting.setInf.Title;//It can be either 0 or 1. If it is set to be 0, the article generated is without titles and headings. The default value is 0.
                jsonObjectParam.image = Math.Round((Double)CommonModule.afSetting.setInf.Image, 2);//the probability of adding an image into the article. It should be a float number from 0.00 to 1.00. The default value is 0.00.
                jsonObjectParam.video = Math.Round((Double)CommonModule.afSetting.setInf.Video, 2);//the probability of adding a video into the article. It should be a float number from 0.00 to 1.00. The default value is 0.00.
                jsonObjectParam.quality = (Int32)CommonModule.afSetting.setInf.Quality;

                String ref_key = af.initiateArticle(jsonObjectParam);
                CommonModule.Log(domainId.ToString(), $"ScrapArticleAsync ref_key:{ref_key}", "scrap");

                List<String> imageArray = new List<String>();
                List<String> thumbImageArray = new List<String>();
                if (ref_key != null)
                {
                    CommonModule.refKeyCash[articleid] = ref_key;
                    CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                    DocumentReference docRef = articlesCol.Document(articleid);
                    DocumentSnapshot articleSnapshot = await docRef.GetSnapshotAsync();
                    var article = articleSnapshot.ConvertTo<Article>();
                    _ImageAutoGenInfo imageAutoGenInfo = (_ImageAutoGenInfo)project2ImageAutoGenInfoMap[article.ProjectId];
                    String InsteadOfTitle = imageAutoGenInfo.InsteadOfTitle;
                    if (InsteadOfTitle.Length > 0
                        && language.ToUpper().CompareTo(CommonModule.baseLanguage) != 0)
                    {
                        InsteadOfTitle = await deepLTranslate.TranslateForQuestion(InsteadOfTitle, language);
                    }

                    ScrapArticleImages(article.ProjectId, question, InsteadOfTitle, ref imageArray, ref thumbImageArray);//Image auto generation
                    Console.WriteLine($"ScrapArticleAsync ref_key={ref_key}");

                    Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                    {
                        { "ArticleId", ref_key },
                        { "Progress", 0 },
                        { "IsScrapping", true },
                        { "ImageArray", imageArray },
                        { "ThumbImageArray", thumbImageArray },
                        { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                    };
                    await docRef.UpdateAsync(userUpdate);
                    status = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                CommonModule.Log(domainId.ToString(), $"ScrapArticleAsync Exception: {ex.Message}", "scrap");
            }

            CommonModule.Log(domainId.ToString(), $"ScrapArticleAsync End", "scrap");
            return status;
        }

        public static void ScrapArticleImages(String projectId, String question, String InsteadOfTitle, ref List<String> imageArray, ref List<String> thumbImageArray)
        {
            String imageGenToken = question;
            Int32 pixabayPageSize = 100;
            _ImageAutoGenInfo imageAutoGenInfo = (_ImageAutoGenInfo)project2ImageAutoGenInfoMap[projectId];
            String pixabayUrl = "https://pixabay.com/api/?key=14748885-e58fd7b3b1c4bf5ae18c651f6&q=" +question.Replace(" ", "+").Replace("?", "")+"&image_type=photo&min_width=" 
                + PixabayImageWidth + "&min_height=" + OpenAIImageHeight + "&per_page="+ pixabayPageSize + "&page=1";

             String pixabay2thUrl = "https://pixabay.com/api/?key=14748885-e58fd7b3b1c4bf5ae18c651f6&q=" + InsteadOfTitle.Replace(";", "+") + "&image_type=photo&min_width="
                + PixabayImageWidth + "&min_height=" + OpenAIImageHeight + "&per_page="+ pixabayPageSize + "&page=1";

            if ((imageAutoGenInfo.ScrappingFrom == _ImageAutoGenInfo.ImageScrapingFrom.Pixabay
                || imageAutoGenInfo.ScrappingFrom == _ImageAutoGenInfo.ImageScrapingFrom.Pixabay_OpenAI)
                && imageAutoGenInfo.ImageNumber > 0)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        var response = client.GetAsync(pixabayUrl).Result;
                        string content = response.Content.ReadAsStringAsync().Result;
                        JObject json = JObject.Parse(content);
                        Int32 totalHits = Convert.ToInt32(json["totalHits"].ToString());
                        JArray hitsArray = (JArray)json["hits"];
                        if (totalHits <= 0 && InsteadOfTitle.Length > 0)
                        {
                            response = client.GetAsync(pixabay2thUrl).Result;
                            content = response.Content.ReadAsStringAsync().Result;
                            json = JObject.Parse(content);
                            totalHits = Convert.ToInt32(json["totalHits"].ToString());
                            hitsArray = (JArray)json["hits"];
                        }

                        totalHits = (totalHits > pixabayPageSize ? pixabayPageSize : totalHits);
                        if (totalHits > 0)
                        {
                            var random = new Random();
                            while (imageArray.Count < imageAutoGenInfo.ImageNumber)
                            {
                                int imgNo = random.Next(totalHits);
                                imageArray.Add(hitsArray[imgNo]["largeImageURL"].ToString());
                                thumbImageArray.Add(hitsArray[imgNo]["previewURL"].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.Message);
                }
            }

            if ((imageAutoGenInfo.ScrappingFrom == _ImageAutoGenInfo.ImageScrapingFrom.OpenAI
                || imageAutoGenInfo.ScrappingFrom == _ImageAutoGenInfo.ImageScrapingFrom.Pixabay_OpenAI)
                && imageAutoGenInfo.ImageNumber > 0
                && imageArray.Count < imageAutoGenInfo.ImageNumber)
            {
                String thumbFolder = Directory.GetCurrentDirectory() + "\\Thumbnails";
                List<Hashtable> urls = GetImageFromOpenAI(imageGenToken, imageAutoGenInfo.ImageNumber, thumbFolder);
                if (urls.Count <= 0 && InsteadOfTitle.Length > 0)
                {
                    urls = GetImageFromOpenAI("Images for " + InsteadOfTitle.Replace(";", " "), imageAutoGenInfo.ImageNumber, thumbFolder);
                }

                foreach (var img in urls)
                {
                    imageArray.Add(img["url"].ToString());
                    thumbImageArray.Add(img["thumb"].ToString());
                }
            }
        }

        public static async Task<bool> ScrapArticleByOpenAIAsync(String domainId, OpenAIAPI openAI, String question, String articleid)
        {
            bool status = false;
            String orgLangQuetion = question;
            String articleContent = "";
            List<String> imageArray = new List<string>();
            List<String> thumbImageArray = new List<string>();
            CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
            DocumentReference docRef = articlesCol.Document(articleid);

            CommonModule.Log(domainId.ToString(), $"ScrapArticleByOpenAIAsync start\n", "scrap");
            try
            {   
                DocumentSnapshot articleSnapshot = await docRef.GetSnapshotAsync();
                var article = articleSnapshot.ConvertTo<Article>();

                _ImageAutoGenInfo imageAutoGenInfo = (_ImageAutoGenInfo)project2ImageAutoGenInfoMap[article.ProjectId];
                String InsteadOfTitle = imageAutoGenInfo.InsteadOfTitle;
                if (InsteadOfTitle.Length > 0
                    && project2LanguageMap[article.ProjectId].ToString().ToUpper()
                    .CompareTo(CommonModule.baseLanguage) != 0)
                {
                    InsteadOfTitle = await deepLTranslate.TranslateForQuestion(InsteadOfTitle
                        , project2LanguageMap[article.ProjectId].ToString().ToUpper());
                }

                if (project2LanguageMap[article.ProjectId].ToString()
                    .CompareTo(CommonModule.baseLanguage) != 0)
                {
                    question = await CommonModule.deepLTranslate.TranslateForQuestion(question
                        , project2LanguageMap[article.ProjectId].ToString());
                }

                CommonModule.Log(domainId.ToString(), $"ScrapArticleByOpenAIAsync step 1/2\n", "scrap");
                var result = await openAI.Completions.CreateCompletionAsync(
                    new CompletionRequest(CommonModule.openAISetting.GetPrompt(question)
                    , model: CommonModule.openAISetting.setInf.Model
                    , temperature: CommonModule.openAISetting.setInf.Temperature
                    , max_tokens: CommonModule.openAISetting.setInf.MaxTokens
                    , top_p: CommonModule.openAISetting.setInf.TopP
                    , numOutputs: CommonModule.openAISetting.setInf.N
                    , presencePenalty: CommonModule.openAISetting.setInf.PresencePenalty
                    , frequencyPenalty: CommonModule.openAISetting.setInf.FrequencyPenalty));

                String content = result.ToString();
                CommonModule.Log(domainId.ToString(), $"ScrapArticleByOpenAIAsync step 2/2 content len: {content.Length.ToString()}\n", "scrap");
                if (content.Length > 0 && CommonModule.project2LanguageMap[article.ProjectId].ToString()
                    .CompareTo(CommonModule.baseLanguage) != 0)
                {
                    content = await CommonModule.deepLTranslate.Translate(content
                        , CommonModule.project2LanguageMap[article.ProjectId].ToString());
                }
                articleContent += "<br>" + content;

                CommonModule.Log(domainId.ToString(), $"ScrapArticleByOpenAIAsync > scrap image\n", "scrap");
                ScrapArticleImages(article.ProjectId, question, InsteadOfTitle, ref imageArray, ref thumbImageArray);//Image auto generation
                CommonModule.Log(domainId.ToString(), $"ScrapArticleByOpenAIAsync > scrap image end\n", "scrap");

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "ArticleId", "55555" },
                    { "Progress", 100 },
                    { "Content", articleContent },
                    { "ImageArray", imageArray },
                    { "ThumbImageArray", thumbImageArray },
                    { "IsScrapping", false },
                    { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                };
                await docRef.UpdateAsync(userUpdate);

                status = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                CommonModule.Log(domainId.ToString(), $"ScrapArticleByOpenAIAsync exception: {ex.Message}\n", "scrap");
            }

            CommonModule.Log(domainId.ToString(), $"ScrapArticleByOpenAIAsync end\n", "scrap");
            return status;
        }

        public static String articleURL(String domain, String question, bool useHttps) {
            String filename = CommonModule.GetHtmlFileName("", question);
            String httpPrefix = ( useHttps ? "https" : "http" );

            return $"{httpPrefix}://{domain}/{filename}";
            //if ( isAWS == 0 ) return $"http://{domain}/{filename}";
            //else return $"http://{domain}.s3-website.us-east-2.amazonaws.com/{filename}";
        }

        public static bool RemoteFileExists(string url)
        {
            bool code = false;
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                string text;
                HttpStatusCode status;

                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse) { 
                     code = (response.StatusCode == HttpStatusCode.OK);
                }
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }

            return code;
        }

        public async Task InitProject2LanguageMapAsync()
        {
            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("Projects");
                QuerySnapshot projectsSnapshot = await col.GetSnapshotAsync();

                foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                {
                    var project = document.ConvertTo<Project>();
                    project2LanguageMap[document.Id] = project.Language.ToUpper();
                    project2UseHttpsMap[document.Id] = (project.UseHttps == null ? false : project.UseHttps);
                    project2ImageAutoGenInfoMap[document.Id] = (project.ImageAutoGenInfo == null ? new _ImageAutoGenInfo() : project.ImageAutoGenInfo);
                }

                Task.Run(() => UpdateArticleScrappingProgress());//Refresh Article Forge Scrapping status.
                Task.Run(() => RestartBackgroundThreads());//If publish thread and scrapping thread is on, resume the threads.
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task RestartBackgroundThreads()
        {
            try
            {
                CollectionReference projectsCol = Config.FirebaseDB.Collection("Projects");
                QuerySnapshot projectsSnapshot = await projectsCol.GetSnapshotAsync();
                foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                {
                    var project = document.ConvertTo<Project>();
                    project.Id = document.Id;
                    
                    if (project.OnPublishSchedule)
                    { //Resume publish
                        CommonModule.publishThreadList[project.Id] = true;
                        Schedule schedule = await CommonModule.GetPublishScheduleAsync(project.Id);
                        Task.Run(() => new SerpapiScrap().PublishThreadAsync(project.Id, schedule.Id));
                    }

                    if (project.OnOpenAIScrapping || project.OnAFScrapping)
                    { //Resume AF Scrapping || Resume OpenAI Scrapping
                        CommonModule.articleScrappingThreadList[project.Id] = true;
                        CommonModule.domainScrappingScheduleStatus[project.Id] = 
                            new ScrappingScheduleStatus { isRunning = true, mode = (project.OnAFScrapping ? 0 : 1) };
                        Schedule schedule = await CommonModule.GetScheduleAsync(project.Id);

                        if ( project.OnAFScrapping )
                        {
                            Task.Run(() => new SerpapiScrap().ScrappingAFThreadAsync(project.Id, schedule.Id));
                        }
                        else if ( project.OnOpenAIScrapping )
                        {
                            Task.Run(() => new SerpapiScrap().ScrappingOpenAIThreadAsync(project.Id, schedule.Id));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task UpdateArticleScrappingProgress() {
            try
            {
                ArticleForge af = new ArticleForge();
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                
                while ( true )
                {
                    Query query = articlesCol.WhereEqualTo("IsScrapping", true).OrderByDescending("CreatedTime");
                    QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();

                    foreach (DocumentSnapshot document in projectsSnapshot.Documents)
                    {
                        var article = document.ConvertTo<Article>();
                        article.Id = document.Id;
                        if (!article.IsScrapping && article.Progress == 100) continue;

                        int prog = 0;
                        if (article.ArticleId != null
                            && (article.ArticleId.CompareTo("55555") == 0 || article.ArticleId.CompareTo("1234567890") == 0)) continue;

                        if (article.ArticleId != null)
                        {
                            String refArticleId = article.ArticleId;
                            prog = af.getApiProgress(ref refArticleId);
                            article.ArticleId = refArticleId;
                            if (refArticleId != null && prog == article.Progress) continue;
                        }
                        //if (prog == 0) continue;

                        Dictionary<string, object> update = new Dictionary<string, object>();
                        if (prog == 100)
                        {
                            article.Content = await af.getApiArticleResult(article.ArticleId, project2LanguageMap[article.ProjectId].ToString());
                            update["Content"] = article.Content;
                            update["IsScrapping"] = false;
                            update["Progress"] = 100;

                            await CommonModule.historyLog.LogActionHistory(CommonModule.ArticleScrapCategory
                                    , article.ProjectId
                                    , $"[Project ID={article.ProjectId}] AF Article Id={article.Id} Scrapping Done");
                        }
                        else if (article.ArticleId == null)
                        {
                            update["ArticleId"] = null;
                            update["IsScrapping"] = false;
                            update["Progress"] = 0;
                        }
                        else
                        {
                            update["Progress"] = prog;
                        }

                        update["UpdateTime"] = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                        DocumentReference docRef = articlesCol.Document(document.Id);
                        await docRef.UpdateAsync(update);
                    }

                    Thread.Sleep(5 * 1000);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        static public void DeleteAllContentInFolder(String folder)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(folder);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        static public async Task<Article> AddArticle(Article articleParam, String articleId, Int32 progress)
        {
            bool ret = false;
            var article = new Article
            {
                ProjectId = articleParam.ProjectId,
                Title = articleParam.Title,
                MetaDescription = articleParam.MetaDescription,
                MetaKeywords = articleParam.MetaKeywords,
                MetaAuthor = articleParam.MetaAuthor,
                ArticleId = articleId,
                Content = articleParam.Content,
                Footer = articleParam.Footer,
                IsScrapping = false,
                Progress = progress,
                State = 0,
                ImageArray = articleParam.ImageArray,
                ThumbImageArray = articleParam.ThumbImageArray,
                UpdateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
                CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
            };

            Article articleData = article;

            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                Query query = articlesCol.OrderByDescending("CreatedTime")
                    .WhereEqualTo("ProjectId", articleData.ProjectId)
                    .WhereEqualTo("Title", articleData.Title).Limit(1);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                if (projectsSnapshot.Documents.Count == 0)
                {
                    DocumentReference docRef = await articlesCol.AddAsync(articleData);
                    articleData.Id = docRef.Id;
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if ( !ret ) articleData = null;
            return articleData;
        }

        public static async Task<bool> UpdateBatchState(String articleids, int state)
        {
            bool ret = false;
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");

                int elemSize = 10;
                int pageNo = 0;
                String[] ids = articleids.Split(",");
                do
                {
                    var subIds = ids.Skip(elemSize * pageNo).Take(elemSize);
                    pageNo++;

                    if (subIds.Count() > 0)
                    {
                        Query query = articlesCol.WhereIn(FieldPath.DocumentId, subIds);
                        QuerySnapshot snapshot = await query.GetSnapshotAsync();

                        WriteBatch updateBatch = Config.FirebaseDB.StartBatch();
                        Dictionary<string, object> articleUpdate = new Dictionary<string, object>()
                        {
                            { "State", state },
                        };

                        foreach (DocumentSnapshot document in snapshot.Documents)
                        {
                            updateBatch.Update(document.Reference, articleUpdate);
                        }
                        await updateBatch.CommitAsync();
                    }
                    else break;
                } while (true);
                ret = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ret;
        }

        static public String GetArticleTemplate(String domainName)
        {
            String templateHtml = "";
            String templateFile = Directory.GetCurrentDirectory();
            templateFile += $"\\Theme\\{domainName}\\theme\\articlepage.html";
            if (File.Exists(templateFile))
            {
                templateHtml = File.ReadAllText(templateFile);
            }
            
            return templateHtml;
        }

        static public void GenerateArticleHtml(String fileName, String link, Article article, String articleTemplate, String lang)
        {
            lang = lang.ToLower();
            String canonialTag = $"<link rel=\"canonical\" href=\"{link}\"/>";
            String metaUtf8 = "<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\">";
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                if (article.MetaTitle == null || article.MetaTitle.Length == 0) 
                    article.MetaTitle = article.Title;
                
                if (articleTemplate.Length == 0)
                {
                    writer.WriteLine("<!DOCTYPE html>");
                    writer.WriteLine($"<html lang=\"{lang}\">");
                    writer.WriteLine("<head>");
                    writer.WriteLine(metaUtf8);
                    writer.WriteLine($"<title>{article.MetaTitle}</title>");

                    String metaDesc = article.MetaDescription;
                    if (metaDesc == null || metaDesc.Length <= 0)
                    {// auto fill
                        metaDesc = PickupMetaDescription(article.Content);
                    }
                    writer.WriteLine($"<meta name=\"description\" content=\"{metaDesc}\">");

                    if (article.MetaKeywords != null && article.MetaKeywords.Length > 0)
                    {
                        writer.WriteLine($"<meta name=\"keywords\" content=\"{article.MetaKeywords}\">");
                    }

                    if (article.MetaAuthor != null && article.MetaAuthor.Length > 0)
                    {
                        writer.WriteLine($"<meta name=\"author\" content=\"{article.MetaAuthor}\">");
                    }

                    writer.WriteLine($"<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                    writer.WriteLine(canonialTag);

                    writer.WriteLine("</head>");
                    writer.WriteLine("<body>");
                    writer.WriteLine(article.Content);
                    if (article.ImageArray != null && article.ImageArray.Count > 0)
                    {//Image replace
                        foreach(String url in article.ImageArray)
                        {
                            writer.WriteLine("<img src=\"" + url + "\" width=\"100%\">");
                        }
                    }

                    if (article.Footer != null && article.Footer.Length > 0)
                    {
                        writer.WriteLine("<footer>");
                        writer.WriteLine(article.Footer);
                        writer.WriteLine("</footer>");
                    }
                    writer.WriteLine("</body>");
                    writer.WriteLine("</html>");
                }
                else
                {
                    if (article.MetaDescription == null) article.MetaDescription = "";
                    if (article.MetaKeywords == null) article.MetaKeywords = "";
                    if (article.MetaAuthor == null) article.MetaAuthor = "";

                    String metaDescContent = article.MetaDescription;
                    if (metaDescContent == null || metaDescContent.Length <= 0)
                    {// auto fill
                        metaDescContent = PickupMetaDescription(article.Content);
                    }

                    String htmlLang = $"<html lang=\"{lang}\">";
                    articleTemplate = Regex.Replace(articleTemplate, @"([<]html[^<>]+[>])", htmlLang);

                    String htmlUtf8Meta = $"<head>\n  " + metaUtf8;
                    articleTemplate = Regex.Replace(articleTemplate, @"([<]head[>])", htmlUtf8Meta);

                    String metaDesc = $"<meta name=\"description\" content=\"{metaDescContent}\">";
                    String metaKeywd = $"<meta name=\"keywords\" content=\"{article.MetaKeywords}\">";
                    String metaAuthor = $"<meta name=\"author\" content=\"{article.MetaAuthor}\">";

                    articleTemplate = Regex.Replace(articleTemplate, @"([<]meta\s+name=""description""\s+[^<>]+[>])", metaDesc);
                    articleTemplate = Regex.Replace(articleTemplate, @"([<]meta\s+name=""keywords""\s+[^<>]+[>])", metaKeywd);
                    articleTemplate = Regex.Replace(articleTemplate, @"([<]meta\s+name=""author""\s+[^<>]+[>])", metaAuthor);

                    //<link rel=\"canonical\" href=\"{link}\"/>
                    articleTemplate = Regex.Replace(articleTemplate, @"([<]link\s+rel=""canonical""\s+[^<>]+[>])", canonialTag);
                    Match m = Regex.Match(articleTemplate, @"[<]link\s+rel=""canonical""", RegexOptions.IgnoreCase);
                    if (!m.Success)
                    {
                        articleTemplate = Regex.Replace(articleTemplate, @"([<][/]head[>])", "  " + canonialTag + "\n" + "</head>");
                    }

                    if (article.ImageArray != null && article.ImageArray.Count > 0)
                    {//Image replace
                        String imagePattern = @"(['""]?{{IMAGE}}['""]?)";
                        Match im = Regex.Match(articleTemplate, imagePattern, RegexOptions.IgnoreCase);
                        int i = 0, sz = article.ImageArray.Count;
                        Regex rgx = new Regex(imagePattern, RegexOptions.IgnoreCase);
                        while ( im.Success )
                        {
                            articleTemplate = rgx.Replace(articleTemplate, "\"" + article.ImageArray[i].ToString() + "\"", 1);
                            im = Regex.Match(articleTemplate, imagePattern, RegexOptions.IgnoreCase);
                            i++;i %= sz;
                        }
                    }

                    //Auto replace for title
                    articleTemplate = Regex.Replace(articleTemplate, @"[<]title[>]([^<>]+)[<][/]title[>]", "<title>" + article.MetaTitle + "</title>");

                    articleTemplate = articleTemplate.Replace("{{TITLE}}", "<title>" + article.MetaTitle + "</title>");
                    articleTemplate = articleTemplate.Replace("{{CONTENT}}", article.Content);
                    articleTemplate = articleTemplate.Replace("{{FOOTER}}", article.Footer);
                    articleTemplate = articleTemplate.Replace("{{META_DESC}}", metaDesc);
                    articleTemplate = articleTemplate.Replace("{{META_KEYWORD}}", metaKeywd);
                    articleTemplate = articleTemplate.Replace("{{META_AUTHOR}}", metaAuthor);
                    writer.Write(articleTemplate);
                }
            }
        }

        static public async Task BuildArticlePageAsEmptyThreadAsync(String domainid, String domain)
        {
            try
            {
                String curFolder = Directory.GetCurrentDirectory();
                curFolder += $"\\Build\\{domain}";
                if (!Directory.Exists(curFolder))
                {
                    Directory.CreateDirectory(curFolder);
                }
                else
                {
                    CommonModule.DeleteAllContentInFolder(curFolder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public async Task BuildArticlePageThreadAsync(String domainid, String domain, String articleId, bool isAWSHost, String s3Name, String region)
        {
            try
            {
                String httpPrefix = (((bool)project2UseHttpsMap[domainid]) ? "https" : "http");
                String lang = CommonModule.project2LanguageMap[domainid].ToString();
                String hostingDomain = CommonModule.GetDomain(domain, isAWSHost, s3Name, region);
                String curFolder = Directory.GetCurrentDirectory();
                curFolder += $"\\Build\\{domain}";
                if (!Directory.Exists(curFolder))
                {
                    Directory.CreateDirectory(curFolder);
                }
                else
                {
                    CommonModule.DeleteAllContentInFolder(curFolder);
                }

                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                DocumentReference docRef = articlesCol.Document(articleId);

                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    var article = snapshot.ConvertTo<Article>();
                    if (article.Content != null && article.Content.Length > 0
                        && (article.State == 2 || article.State == 3))
                    {
                        //{{Stop When update theme
                        while (CommonModule.onThemeUpdateCash[domainid] != null && (bool)CommonModule.onThemeUpdateCash[domainid]) Thread.Sleep(500);
                        //}}Stop When update theme
                        String title = CommonModule.GetHtmlFileName(article.MetaTitle, article.Title);
                        String articleTemplate = GetArticleTemplate(domain);
                        GenerateArticleHtml(curFolder + "\\" + title, $"{httpPrefix}://{hostingDomain}/{title}", article, articleTemplate, lang);

                        //{{Update state
                        if (article.State != 3)
                        {
                            Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                            {
                                { "State", 3 },
                            };
                            await docRef.UpdateAsync(userUpdate);
                        }
                        //}}Update state
                    }
                }

                CollectionReference col = Config.FirebaseDB.Collection("Projects");
                QuerySnapshot snapshot2 = await col.GetSnapshotAsync();
                Hashtable domainMap = new Hashtable();
                foreach (DocumentSnapshot document in snapshot2.Documents)
                {
                    var proj = document.ConvertTo<Project>();
                    domainMap[document.Id] = new DomainIpMap{ domainId = document.Id
                                                    , domain= proj.Name
                                                    , ip = proj.Ip
                                                    , s3Name = proj.S3BucketName
                                                    , s3Region = proj.S3BucketRegion
                    };
                }

                Query query = articlesCol.WhereEqualTo("State", 3).OrderBy("Title");
                QuerySnapshot qSnapshot = await query.GetSnapshotAsync();

                GenerateURLFile(qSnapshot, curFolder, "url.html", domainid, domainMap, isAWSHost, s3Name, region);
                GenerateSiteMapFile(qSnapshot, curFolder, domainid, $"{httpPrefix}://{hostingDomain}", domainMap);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public async Task BuildPagesThreadAsync(String domainid, String domain, bool isAWSHost, String s3Name, String region, int defaultState =2, bool force = true)
        {
            try
            {
                String httpPrefix = (((bool)CommonModule.project2UseHttpsMap[domainid]) ? "https" : "http");
                String lang = CommonModule.project2LanguageMap[domainid].ToString();
                String hostingDomain = CommonModule.GetDomain(domain, isAWSHost, s3Name, region);
                String curFolder = Directory.GetCurrentDirectory();
                curFolder += $"\\Build\\{domain}";
                if (!Directory.Exists(curFolder))
                {
                    Directory.CreateDirectory(curFolder);
                }
                else
                {
                    CommonModule.DeleteAllContentInFolder(curFolder);
                }

                String articleTemplate = GetArticleTemplate(domain);

                String articleUpdateStateIds = "";
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                Query query = articlesCol.WhereEqualTo("ProjectId", domainid);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    if (article.Content != null && article.Content.Length > 0
                                && (
                                (force && (article.State == 2 || article.State == 3)) || article.State == defaultState)
                                )
                    {
                        //{{Stop When update theme
                        while (CommonModule.onThemeUpdateCash[domainid] != null && (bool)CommonModule.onThemeUpdateCash[domainid]) Thread.Sleep(500);
                        //}}Stop When update theme
                        String title = CommonModule.GetHtmlFileName(article.MetaTitle, article.Title);
                        GenerateArticleHtml(curFolder + "\\" + title, $"{httpPrefix}://{hostingDomain}/{title}", article, articleTemplate, lang);

                        //{{Update state
                        if (article.State == 2)
                        {
                            if (articleUpdateStateIds.Length > 0) articleUpdateStateIds += ",";
                            articleUpdateStateIds += document.Id;
                        }
                        //}}Update state
                    }
                }

                CollectionReference col = Config.FirebaseDB.Collection("Projects");
                QuerySnapshot snapshot2 = await col.GetSnapshotAsync();
                Hashtable domainMap = new Hashtable();
                foreach (DocumentSnapshot document in snapshot2.Documents)
                {
                    var proj = document.ConvertTo<Project>();
                    domainMap[document.Id] = new DomainIpMap { domainId = document.Id, domain = proj.Name, ip = proj.Ip,
                        s3Name = proj.S3BucketName,s3Region = proj.S3BucketRegion};
                }

                query = articlesCol.WhereEqualTo("State", 3).OrderBy("Title");
                snapshot = await query.GetSnapshotAsync();

                GenerateURLFile(snapshot, curFolder, "url.html", domainid, domainMap, isAWSHost, s3Name, region);
                GenerateSiteMapFile(snapshot, curFolder, domainid, $"{httpPrefix}://{hostingDomain}", domainMap);

                if (articleUpdateStateIds.Length > 0)
                {
                    await CommonModule.UpdateBatchState(articleUpdateStateIds, 3);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public async Task BuildPagesFromArtidleIdsAsync(String domainid, String domain, String articleIds, bool isAWS, String s3Name, String region)
        {
            try
            {
                String httpPrefix = (((bool)CommonModule.project2UseHttpsMap[domainid]) ? "https" : "http");
                String lang = CommonModule.project2LanguageMap[domainid].ToString();
                String hostingDomain = CommonModule.GetDomain(domain, isAWS, s3Name, region);
                String[] aids = articleIds.Split(',');
                String curFolder = Directory.GetCurrentDirectory();
                curFolder += $"\\Build\\{domain}";
                if (!Directory.Exists(curFolder))
                {
                    Directory.CreateDirectory(curFolder);
                }
                else
                {
                    CommonModule.DeleteAllContentInFolder(curFolder);
                }

                String articleTemplate = GetArticleTemplate(domain);

                String articleUpdateStateIds = "";
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                Query query = articlesCol.WhereIn(FieldPath.DocumentId, aids);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    if (article.Content != null && article.Content.Length > 0)
                    {
                        //{{Stop When update theme
                        while (CommonModule.onThemeUpdateCash[domainid] != null && (bool)CommonModule.onThemeUpdateCash[domainid]) Thread.Sleep(500);
                        //}}Stop When update theme
                        String title = CommonModule.GetHtmlFileName(article.MetaTitle, article.Title);

                        GenerateArticleHtml(curFolder + "\\" + title, $"{httpPrefix}://{hostingDomain}/{title}", article, articleTemplate, lang);
                    }
                }

                CollectionReference col = Config.FirebaseDB.Collection("Projects");
                QuerySnapshot snapshot2 = await col.GetSnapshotAsync();
                Hashtable domainMap = new Hashtable();
                foreach (DocumentSnapshot document in snapshot2.Documents)
                {
                    var proj = document.ConvertTo<Project>();
                    domainMap[document.Id] = new DomainIpMap { domainId = document.Id, domain = proj.Name, ip = proj.Ip
                    , s3Name = proj.S3BucketName, s3Region = proj.S3BucketRegion};
                }

                query = articlesCol.WhereEqualTo("State", 3).OrderBy("Title");
                snapshot = await query.GetSnapshotAsync();

                GenerateURLFile(snapshot, curFolder, "url.html", domainid, domainMap, isAWS, s3Name, region);
                GenerateSiteMapFile(snapshot, curFolder, domainid, $"{httpPrefix}://{hostingDomain}", domainMap);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public async Task SyncWithServerThreadAsync(String domainid, String domain, String ipaddr, String s3Name)
        {
            try
            {
                String curFolder = Directory.GetCurrentDirectory();
                String exeFolder = curFolder;
                curFolder += $"\\Build\\{domain}";

                //Omitted
                String tmpFolder = Directory.GetCurrentDirectory() + "\\Temp";
                //tmpFolder += $"\\Temp";
                //if (!Directory.Exists(tmpFolder))
                //{
                //    Directory.CreateDirectory(tmpFolder);
                //}

                Console.WriteLine("SyncWithServerThreadAsync Zip starting...");

                if (Directory.Exists(curFolder))
                {
                    string dirRoot = curFolder;
                    string[] filesToZip = Directory.GetFiles(dirRoot, "*.*");
                    string zipFileName = string.Format($"{domain}.zip", DateTime.Now);

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

                    if (ipaddr.CompareTo("0.0.0.0") != 0)
                    {
                        String cmd = $"pscp -i {exeFolder}\\{Config.EC2UploadKey} {tmpFolder}\\{domain}.zip ubuntu@{ipaddr}:/home/ubuntu";

                        Console.WriteLine("SyncWithServerThreadAsync --> " + cmd);
                        ExecuteCmd.ExecuteCommandAsync(cmd);
                    }
                    else
                    {
                        await new AWSUpload().start(s3Name, $"{domain}.zip", $"{tmpFolder}\\");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public async Task SyncThemeWithServerThreadAsync(String domain, String ipaddr)
        {
            try
            {
                String curFolder = Directory.GetCurrentDirectory();
                String exeFolder = curFolder;
                curFolder += $"\\Theme\\{domain}\\theme.zip";

                if (File.Exists(curFolder))
                {
                    if (!CommonModule.isAWSHosting(ipaddr))
                    {
                        String cmd = $"pscp -i {exeFolder}\\{Config.EC2UploadKey} {curFolder} ubuntu@{ipaddr}:/home/ubuntu";

                        Console.WriteLine("SyncWithServerThreadAsync --> " + cmd);
                        ExecuteCmd.ExecuteCommandAsync(cmd);
                    }
                    else
                    {
                        await new AWSUpload().start(domain, "theme.zip", $"{exeFolder}\\Theme\\{domain}\\");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public async Task UploadDataToS3ThreadAsync(String domain)
        {
            try
            {
                String curFolder = Directory.GetCurrentDirectory();
                String exeFolder = curFolder;
                curFolder += $"\\Upload\\{domain}\\S3Data.zip";

                if (File.Exists(curFolder))
                {
                    await new AWSUpload().start(domain, "S3Data.zip", $"{exeFolder}\\Upload\\{domain}\\");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public String PreAdjustForTitleImage(String content)
        {
            String tmpContent = content;
            Regex regex = new Regex("([{][^{}]+[}])");

            while (tmpContent != null && tmpContent.Contains("|"))
            {
                string[] subContents = regex.Split(tmpContent);
                tmpContent = "";
                foreach (String sub in subContents)
                {
                    if (sub.Length == 0) continue;

                    if (sub[0] == '{' && sub[sub.Length - 1] == '}') tmpContent += sub.Split("|")[0].Substring(1);
                    else tmpContent += sub;
                }
            }
            
            return tmpContent;
        }

        static public String PickupMetaDescription(String content, int limit = 160)
        {
            String tmpContent = "";
            Regex regex = new Regex("[<]p[>]([^<>]+)[<][/]p[>]");

            string[] subContents = regex.Split(content);
            tmpContent = "";

            foreach (String para in subContents)
            {
                if (para.Trim().Length > 0 && para.IndexOf("<") == -1)
                {
                    tmpContent = para;
                    break;
                }
            }

            if (tmpContent.Length > limit) {
                string[] subSentence = tmpContent.Split('.');
                tmpContent = "";
                foreach (String sentence in subSentence)
                {
                    if (tmpContent.Length + sentence.Length > limit)
                    {
                        break;
                    }
                    tmpContent += sentence + ".";
                }
            }

            tmpContent = tmpContent.Trim().Replace("\n", "").Replace("\r", "");
            return tmpContent;
        }

        static public void GenerateURLFile(QuerySnapshot snapshot, String folder, String fileName, String selfDomainId, Hashtable domainMap, bool isAWSHost, String s3Name, String region)
        {
            DomainIpMap selfdomain = (DomainIpMap)domainMap[selfDomainId];
            String lang = CommonModule.project2LanguageMap[selfDomainId].ToString().ToLower();
            String selfHostingDomain = CommonModule.GetDomain(selfdomain.domain, CommonModule.isAWSHosting( selfdomain.ip ), s3Name, region);
            using (StreamWriter writer = new StreamWriter(folder + "//" + fileName))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine($"<html lang=\"{lang}\">");
                writer.WriteLine("<head>");
                writer.WriteLine($"<title>Online URLs</title>");
                writer.WriteLine("</head>");
                writer.WriteLine("<body>");
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    DomainIpMap domainInfo = (DomainIpMap)domainMap[article.ProjectId.ToString()];
                    if (domainInfo == null) continue;

                    String hostingDomain = CommonModule.GetDomain(domainInfo.domain, isAWSHost, s3Name, region);
                    if ( !selfdomain.ip.Equals(domainInfo.ip) 
                        || !selfHostingDomain.Equals(hostingDomain) ) continue;

                    String orgTitle = article.Title;
                    String titlelink = CommonModule.GetHtmlFileName(article.MetaTitle, article.Title);
                    String httpPrefix = (((bool)CommonModule.project2UseHttpsMap[article.ProjectId]) ? "https" : "http");

                    String baseURL = $"{httpPrefix}://{hostingDomain}";
                    writer.WriteLine($"<a href='{baseURL}/{titlelink}'>{orgTitle}</a><br/>");
                }
                writer.WriteLine("</body>");
                writer.WriteLine("</html>");
            }
        }

        static public String GetHtmlFileName(String metaTitle, String title)
        {
            if (metaTitle != null && metaTitle.Length > 0) title = metaTitle;
            title = title.Replace("?", "").Trim();
            title = title.Replace(" ", "-");
            return title + ".html";
        }

        static public String GetDomain(DomainIpMap diMap)
        {   
            return GetDomain(diMap.domain
                , CommonModule.isAWSHosting(diMap.ip)
                , diMap.s3Name
                , diMap.s3Region);
        }

        static public String GetDomain(String domain, bool isAWSHost, String s3Name, String s3Region= "us-east-2")
        {
            
            if (!isAWSHost) return domain;
            else
            {
                if (s3Name == null || s3Name.Length == 0) s3Name = domain;
                if (s3Region == null || s3Region.Length == 0) s3Region = "us-east-2";
                return $"{s3Name}.s3.{s3Region}.amazonaws.com";
            }
        }

        static public void GenerateSiteMapFile(QuerySnapshot snapshot, String folder, String selfDomainId, String baseURL, Hashtable domainMap)
        {
            DomainIpMap selfdomain = (DomainIpMap)domainMap[selfDomainId];
            String selfHostingDomain = CommonModule.GetDomain(selfdomain);
            String updateDate = DateTime.Now.ToString("yyyy-MM-dd");
            //robots.txt
            using (StreamWriter writer = new StreamWriter(folder + "//robots.txt"))
            {
                foreach (DictionaryEntry diMapItem in domainMap)
                {
                    DomainIpMap diMap = (DomainIpMap)diMapItem.Value;
                    String hostingDomain = CommonModule.GetDomain(diMap);
                    if ( !selfdomain.ip.Equals(diMap.ip)
                        || !selfHostingDomain.Equals(hostingDomain) ) continue;

                    //Sitemap: http://www.example.com/sitemap-host1.xml
                    writer.WriteLine($"Sitemap: {baseURL}/sitemap-{diMap.domain}.xml");

                    //sitemap.xml or sitemap-host.xml
                    using (StreamWriter writer2 = new StreamWriter(folder + $"//sitemap-{diMap.domain}.xml"))
                    {
                        writer2.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                        writer2.WriteLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
                        foreach (DocumentSnapshot document in snapshot.Documents)
                        {
                            var article = document.ConvertTo<Article>();
                            if (diMap.domainId != article.ProjectId) continue;
                            String httpPrefix = (((bool)CommonModule.project2UseHttpsMap[article.ProjectId]) ? "https" : "http");
                            String fileName = CommonModule.GetHtmlFileName(article.MetaTitle, article.Title);
                            writer2.WriteLine("   <url>");
                            writer2.WriteLine($"      <loc>{httpPrefix}://{hostingDomain}/{fileName}</loc>");
                            writer2.WriteLine($"      <lastmod>{updateDate}</lastmod>");
                            //always
                            //hourly
                            //daily
                            //weekly
                            //monthly
                            //yearly
                            //never
                            writer2.WriteLine("      <changefreq>monthly</changefreq>");
                            writer2.WriteLine("      <priority>0.5</priority>");
                            writer2.WriteLine("   </url>");
                        }
                        writer2.WriteLine("</urlset>");
                    }
                }
            }

            String lastMod = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffffffK");
            int num = 1;
            //siteindex.xml
            using (StreamWriter writer = new StreamWriter(folder + "//siteindex.xml"))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine("<sitemapindex  xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
                foreach (DictionaryEntry diMapItem in domainMap)
                {
                    DomainIpMap diMap = (DomainIpMap)diMapItem.Value;
                    String hostingDomain = CommonModule.GetDomain(diMap);

                    if ( !selfdomain.ip.Equals(diMap.ip)
                        || !selfHostingDomain.Equals(hostingDomain) ) continue;

                    String OriginalFileName = folder + $"//sitemap-{diMap.domain}.xml";
                    String CompressedFileName = folder + $"//sitemap-{diMap.domain}.xml.gz";
                    String httpPrefix = (((bool)CommonModule.project2UseHttpsMap[diMap.domainId]) ? "https" : "http");
                    using FileStream originalFileStream = File.Open(OriginalFileName, FileMode.Open);
                    using FileStream compressedFileStream = File.Create(CompressedFileName);
                    using var compressor = new GZipStream(compressedFileStream, CompressionMode.Compress);
                    originalFileStream.CopyTo(compressor);

                    writer.WriteLine($"   <sitemap>");
                    writer.WriteLine($"      <loc>{httpPrefix}://{hostingDomain}/sitemap-{diMap.domain}.xml.gz</loc>");
                    writer.WriteLine($"      <lastmod>{lastMod}</lastmod>");
                    writer.WriteLine($"   </sitemap>");
                    num++;
                }
                writer.WriteLine("</sitemapindex>");
            }
        }

        public async Task<bool> CreateHostBucketThreadAsync(String domain, String region = "")
        {
            AmazonS3Client s3Client = CommonModule.amazonS3Client;
            if (region.Length > 0)
            {
                s3Client = new AmazonS3Client(Config.AWSAccessKey, Config.AWSSecretKey, RegionEndpoint.GetBySystemName(region));
            }

            var success = await CreateBucket.CreateBucketAsync(s3Client, (string)domain, region);

            if( success )
                success = await CreateBucket.SetBucketPublicAsync(s3Client, (string)domain, Config.Principal);

            if (success)
                success = await CreateBucket.AddWebsiteConfigurationAsync(s3Client, (string)domain, "index.html", "error.html");
            //if (success)
            //{
            //    Console.WriteLine($"Successfully created bucket: .\n");
            //}
            //else
            //{
            //    Console.WriteLine($"Could not create bucket: .\n");
            //}

            //// Get the list of buckets accessible by the new user.
            //var response = await client.ListBucketsAsync();
            //// Loop through the list and print each bucket's name
            //// and creation date.
            //Console.WriteLine("\n--------------------------------------------------------------------------------------------------------------");
            //Console.WriteLine("Listing S3 buckets:\n");
            //response.Buckets
            //    .ForEach(b => Console.WriteLine($"Bucket name: {b.BucketName}, created on: {b.CreationDate}"));
            return success;
        }

        static public bool isAWSHosting(String ipAddr, String val = "0.0.0.0")
        { 
            if(ipAddr.CompareTo(val) == 0) return true;

            return false;
        }

        public static async Task<bool> DeleteBucketAsync(string bucketName, string region)
        {
            bool bret = false;
            AmazonS3Client s3Client = CommonModule.amazonS3Client;
            if (region.Length > 0)
            {
                s3Client = new AmazonS3Client(Config.AWSAccessKey, Config.AWSSecretKey, RegionEndpoint.GetBySystemName(region));
            }
            try
            {
                var request = new DeleteBucketRequest
                {
                    BucketName = bucketName,
                };

                var response = await s3Client.DeleteBucketAsync(request);
                bret = true;
            }
            catch (Exception e)
            { 

            }
            return bret;            
        }

        public static async Task<bool> EmptyBucketAsync(string bucketName, string region)
        {
            bool bret = false;
            AmazonS3Client s3Client = CommonModule.amazonS3Client;
            if (region.Length > 0)
            {
                s3Client = new AmazonS3Client(Config.AWSAccessKey, Config.AWSSecretKey, RegionEndpoint.GetBySystemName(region));
            }
            var keysAndVersions = await GetAllKeysAsync(s3Client, bucketName);

            DeleteObjectsRequest multiObjectDeleteRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = keysAndVersions // This includes the object keys and null version IDs.
            };
            try
            {
                if (keysAndVersions.Count > 0)
                {
                    DeleteObjectsResponse response = await s3Client.DeleteObjectsAsync(multiObjectDeleteRequest);
                    //Console.WriteLine("Successfully deleted all the {0} items", response.DeletedObjects.Count);
                }
                bret = true;
            }
            catch (DeleteObjectsException e)
            {
                //PrintDeletionErrorStatus(e);
            }

            return bret;
        }

        static async Task<List<KeyVersion>> GetAllKeysAsync(AmazonS3Client s3Client, string bucketName)
        {
            List<KeyVersion> keys = new List<KeyVersion>();
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                MaxKeys = 100,
                Prefix = "",
                Delimiter = "",
            };

            ListObjectsV2Response response;
            do
            {
                response = await s3Client.ListObjectsV2Async(request);
                response.S3Objects
                    .ForEach(obj => {
                        //Console.WriteLine($"{obj.Key,-35}{obj.LastModified.ToShortDateString(),10}{obj.Size,10}")
                        KeyVersion keyVersion = new KeyVersion
                        {
                            Key = obj.Key,
                            // For non-versioned bucket operations, we only need object key.
                            // VersionId = response.VersionId
                        };
                        keys.Add(keyVersion);
                    });
                request.ContinuationToken = response.NextContinuationToken;
            }
            while (response.IsTruncated);

            return keys;
        }

        static public async Task<string> FindBucketLocationAsync(String bucketName)
        {
            string bucketLocation;
            var request = new GetBucketLocationRequest()
            {
                BucketName = bucketName,

            };
            GetBucketLocationResponse response = await amazonS3Client.GetBucketLocationAsync(request);
            bucketLocation = response.Location.ToString();
            if (bucketLocation.CompareTo("EU") == 0) bucketLocation = "eu-west-1";//Patch manually.
            return bucketLocation;
        }

        public static async Task<bool> ListBucketContentsAsync(AmazonS3Client s3Client, string bucketName, List<Hashtable> list, int maxKeys = 20)
        {
            try
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    MaxKeys = maxKeys,
                    Prefix = "",
                    Delimiter = "",
                };

                ListObjectsV2Response response;

                do
                {
                    response = await s3Client.ListObjectsV2Async(request);

                    response.S3Objects
                        .ForEach(obj => {
                            //Console.WriteLine($"{obj.Key,-35}{obj.LastModified.ToShortDateString(),10}{obj.Size,10}")
                            Hashtable objTbl = new Hashtable();
                            objTbl["key"] = obj.Key;
                            objTbl["date"] = obj.LastModified.ToShortDateString();
                            objTbl["size"] = obj.Size;
                            list.Add(objTbl);
                            });

                    // If the response is truncated, set the request ContinuationToken
                    // from the NextContinuationToken property of the response.
                    request.ContinuationToken = response.NextContinuationToken;
                }
                while (response.IsTruncated);

                return true;
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' getting list of objects.");
                return false;
            }
        }

        public static async Task<bool> ListBucketObjectsAsync(AmazonS3Client s3Client, string bucketName)
        {
            ListObjectsRequest listObjectsRequest = new ListObjectsRequest{
                BucketName = bucketName,
                Prefix = "",
                Delimiter = "/*/",
            };

            List<String> keys = new List<String>();

            ListObjectsResponse response = await s3Client.ListObjectsAsync(listObjectsRequest);
            response.S3Objects
                        .ForEach(obj => Console.WriteLine($"{obj.Key,-35}{obj.LastModified.ToShortDateString(),10}{obj.Size,10}"));

            return true;
        }
        public static bool IsMultipartContentType(string contentType)
        {
            return
                !string.IsNullOrEmpty(contentType) &&
                contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string GetBoundary(string contentType)
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
        public static string GetFileName(string contentDisposition)
        {
            return contentDisposition
                .Split(';')
                .SingleOrDefault(part => part.Contains("filename"))
                .Split('=')
                .Last()
                .Trim('"');
        }

        public static String GetImageFromOpenAI(String question)
        {
            String ret = "";
            HttpWebRequest request = WebRequest.CreateHttp("https://api.openai.com/v1/images/generations");
            request.Method = "Post";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer " + Config.OpenAIKey);

            string json = $"{{\"prompt\": \"{question}\",\"n\": 1,\"size\": \"{OpenAIImageWidth}x{OpenAIImageHeight}\"}}";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            try
            {
                string sResult = "";
                using (WebResponse response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
                dynamic jResult = JsonConvert.DeserializeObject(sResult);
                JArray imgUrls = (JArray)jResult.data;
                if (imgUrls.Count > 0)
                {
                    ret = imgUrls[0]["url"].ToString();
                }
            }
            catch (WebException e)
            { }

            return ret;
        }

        public static List<Hashtable> GetImageFromOpenAI(String question, int n, String thumbFolder)
        {
            String ret = "";
            var list = new List<Hashtable>();
            HttpWebRequest request = WebRequest.CreateHttp("https://api.openai.com/v1/images/generations");
            request.Method = "Post";
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", "Bearer " + Config.OpenAIKey);

            string json = $"{{\"prompt\": \"{question}\",\"n\": {n},\"size\": \"{OpenAIImageWidth}x{OpenAIImageHeight}\"}}";
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            try
            {
                string sResult = "";
                using (WebResponse response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
                dynamic jResult = JsonConvert.DeserializeObject(sResult);
                JArray imgUrls = (JArray)jResult.data;
                foreach (JObject imgUrl in imgUrls)
                {
                    Hashtable hashTbl = new Hashtable();
                    hashTbl["url"] = imgUrl["url"].ToString();
                    using (WebClient wc = new WebClient())
                    {
                        using (Stream s = wc.OpenRead( imgUrl["url"].ToString() ))
                        {
                            using (Bitmap bmp = new Bitmap(s))
                            {
                                String fileName = question.Trim().Replace(" ", "").Replace("?", "");
                                fileName = $"{thumbFolder}\\{fileName}.jpg";
                                bmp.Save(fileName);
                                hashTbl["thumb"] = ThumbnailBase64Image(bmp, 256, 256);
                            }
                        }
                    }
                    list.Add( hashTbl );
                }
            }
            catch (WebException e)
            { }

            return list;
        }

        public static String ThumbnailBase64Image(Bitmap imgBitmap, int width, int height)
        {
            String SigBase64 = "";
            Bitmap srcBmp = imgBitmap;
            float ratio = srcBmp.Width / srcBmp.Height;
            SizeF newSize = new SizeF(width, height * ratio);
            Bitmap target = new Bitmap((int)newSize.Width, (int)newSize.Height);
            using (Graphics graphics = Graphics.FromImage(target))
            {
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImage(srcBmp, 0, 0, newSize.Width, newSize.Height);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    target.Save(memoryStream, ImageFormat.Jpeg);
                    byte[] byteImage = memoryStream.ToArray();
                    SigBase64 = Convert.ToBase64String(byteImage); // Get Base64
                }
            }

            return SigBase64;
        }

        public static async Task<Schedule> GetPublishScheduleAsync(String projectId)
        {
            Schedule schedule = null;
            CollectionReference col = Config.FirebaseDB.Collection("PublishSchedules");
            Query query = col.WhereEqualTo("ProjectId", projectId).Limit(1);
            QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
            if (projectsSnapshot.Documents.Count > 0)
            {
                schedule = projectsSnapshot.Documents[0].ConvertTo<Schedule>();
                schedule.Id = projectsSnapshot.Documents[0].Id;
            }
            return schedule;
        }

        public static async Task<Schedule> GetScheduleAsync(String projectId)
        {
            Schedule schedule = null;
            CollectionReference col = Config.FirebaseDB.Collection("Schedules");
            Query query = col.WhereEqualTo("ProjectId", projectId).Limit(1);
            QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
            if (projectsSnapshot.Documents.Count > 0)
            {
                schedule = projectsSnapshot.Documents[0].ConvertTo<Schedule>();
                schedule.Id = projectsSnapshot.Documents[0].Id;
            }
            return schedule;
        }

        public static void Log(String domainId, String log, String tag)
        {
            if (domainId.Length == 0) return;

            String logFile = Directory.GetCurrentDirectory() + $"\\Log\\{domainId}-{tag}.log";
            String logContent = $"[{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")}] {log}\n";
            File.AppendAllText(logFile, logContent);
        }
    }
}