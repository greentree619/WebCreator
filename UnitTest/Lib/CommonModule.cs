﻿using Google.Cloud.Firestore;
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

namespace UnitTest.Lib
{
    internal class CommonModule
    {
        public static Hashtable threadList = new Hashtable();
        public static Hashtable afThreadList = new Hashtable();
        public static Hashtable publishThreadList = new Hashtable();
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

        public static async Task<JObject> IsDomainScrappingAsync(String domainId)
        {
            bool isScrapping = false;
            bool isAFScrapping = false;
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Projects");
                DocumentReference docRef = articlesCol.Document(domainId);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
                if (snapshot.Exists)
                {
                    var prj = snapshot.ConvertTo<Project>();
                    isScrapping = (prj.OnScrapping != null ? prj.OnScrapping : false);
                    isAFScrapping = (prj.OnAFScrapping != null ? prj.OnAFScrapping : false);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            JObject res = new JObject();
            res["serpapi"] = isScrapping;
            res["afapi"] = isAFScrapping;

            return res;
        }

        public static async Task<bool> ScrapArticleAsync(ArticleForge af, String question, String articleid) {
            bool status = false;
            try
            {
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
                if (ref_key != null)
                {
                    CommonModule.refKeyCash[articleid] = ref_key;
                    CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                    DocumentReference docRef = articlesCol.Document(articleid);

                    Console.WriteLine($"ScrapArticleAsync ref_key={ref_key}");

                    Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                    {
                        { "ArticleId", ref_key },
                        { "Progress", 0 },
                        { "IsScrapping", true },
                        { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                    };
                    await docRef.UpdateAsync(userUpdate);
                    status = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return status;
        }

        public static async Task<bool> ScrapArticleByOpenAIAsync(OpenAIAPI openAI, String question, String articleid)
        {
            bool status = false;
            try
            {
                var result = await openAI.Completions.CreateCompletionAsync(
                    new CompletionRequest(CommonModule.openAISetting.GetPrompt(question)
                    , model: CommonModule.openAISetting.setInf.Model
                    , temperature: CommonModule.openAISetting.setInf.Temperature
                    , max_tokens: CommonModule.openAISetting.setInf.MaxTokens
                    , top_p: CommonModule.openAISetting.setInf.TopP
                    , numOutputs: CommonModule.openAISetting.setInf.N
                    , presencePenalty: CommonModule.openAISetting.setInf.PresencePenalty
                    , frequencyPenalty: CommonModule.openAISetting.setInf.FrequencyPenalty));

                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                DocumentReference docRef = articlesCol.Document(articleid);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                    {
                        { "ArticleId", "55555" },
                        { "Progress", 100 },
                        { "Content", result.ToString() },
                        { "IsScrapping", false },
                        { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) },
                    };
                await docRef.UpdateAsync(userUpdate);
                status = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return status;
        }

        public static String articleURL(String domain, String question) {
            String filename = CommonModule.GetHtmlFileName("", question);

            return $"http://{domain}/{filename}";
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
                        if (!article.IsScrapping && article.Progress == 100) continue;

                        int prog = 0;
                        if (article.ArticleId != null)
                        {
                            prog = af.getApiProgress(article.ArticleId);
                            if (prog == article.Progress) continue;
                        }
                        //if (prog == 0) continue;

                        Dictionary<string, object> update = new Dictionary<string, object>();
                        if (prog == 100)
                        {
                            article.Content = af.getApiArticleResult(article.ArticleId);
                            update["Content"] = article.Content;
                            update["IsScrapping"] = false;
                            update["Progress"] = 100;
                        }
                        else if (article.ArticleId == null)
                        {
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

        static public async Task<bool> AddArticle(Article articleParam, String articleId, Int32 progress)
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
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return ret;
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

        static public void GenerateArticleHtml(String fileName, String link, Article article, String articleTemplate)
        {
            String canonialTag = $"<link rel=\"canonical\" href=\"{link}\"/>";
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                if (article.MetaTitle == null || article.MetaTitle.Length == 0) 
                    article.MetaTitle = article.Title;
                
                if (articleTemplate.Length == 0)
                {
                    writer.WriteLine("<!DOCTYPE html>");
                    writer.WriteLine("<html>");
                    writer.WriteLine("<head>");
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

                    articleTemplate = articleTemplate.Replace("{{TITLE}}", article.MetaTitle);
                    articleTemplate = articleTemplate.Replace("{{CONTENT}}", article.Content);
                    articleTemplate = articleTemplate.Replace("{{FOOTER}}", article.Footer);
                    articleTemplate = articleTemplate.Replace("{{META_DESC}}", metaDesc);
                    articleTemplate = articleTemplate.Replace("{{META_KEYWORD}}", metaKeywd);
                    articleTemplate = articleTemplate.Replace("{{META_AUTHOR}}", metaAuthor);
                    writer.Write(articleTemplate);
                }
            }
        }

        static public async Task BuildArticlePageThreadAsync(String domainid, String domain, String articleId, bool isAWSHost, String s3Name, String region)
        {
            try
            {
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
                        GenerateArticleHtml(curFolder + "\\" + title, $"http://{hostingDomain}/{title}", article, articleTemplate);

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
                GenerateSiteMapFile(qSnapshot, curFolder, domainid, $"http://{hostingDomain}", domainMap);
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
                        GenerateArticleHtml(curFolder + "\\" + title, $"http://{hostingDomain}/{title}", article, articleTemplate);

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
                GenerateSiteMapFile(snapshot, curFolder, domainid, $"http://{hostingDomain}", domainMap);

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
                        GenerateArticleHtml(curFolder + "\\" + title, $"http://{hostingDomain}/{title}", article, articleTemplate);
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
                GenerateSiteMapFile(snapshot, curFolder, domainid, $"http://{hostingDomain}", domainMap);
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

                String tmpFolder = Directory.GetCurrentDirectory();
                tmpFolder += $"\\Temp";
                if (!Directory.Exists(tmpFolder))
                {
                    Directory.CreateDirectory(tmpFolder);
                }

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
                        String cmd = $"pscp -i {exeFolder}\\searchsystem.ppk {tmpFolder}\\{domain}.zip ubuntu@{ipaddr}:/home/ubuntu";

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
                        String cmd = $"pscp -i {exeFolder}\\searchsystem.ppk {curFolder} ubuntu@{ipaddr}:/home/ubuntu";

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

            while (tmpContent.Contains("|"))
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

            return tmpContent;
        }

        static public void GenerateURLFile(QuerySnapshot snapshot, String folder, String fileName, String selfDomainId, Hashtable domainMap, bool isAWSHost, String s3Name, String region)
        {
            DomainIpMap selfdomain = (DomainIpMap)domainMap[selfDomainId];
            using (StreamWriter writer = new StreamWriter(folder + "//" + fileName))
            {
                writer.WriteLine("<!DOCTYPE html>");
                writer.WriteLine("<html>");
                writer.WriteLine("<head>");
                writer.WriteLine($"<title>Online URLs</title>");
                writer.WriteLine("</head>");
                writer.WriteLine("<body>");
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    DomainIpMap domainInfo = (DomainIpMap)domainMap[article.ProjectId.ToString()];
                    if ( !selfdomain.ip.Equals(domainInfo.ip) ) continue;

                    String orgTitle = article.Title;
                    String titlelink = CommonModule.GetHtmlFileName(article.MetaTitle, article.Title);
                    String hostingDomain = CommonModule.GetDomain(domainInfo.domain, isAWSHost, s3Name, region);
                    String baseURL = $"http://{hostingDomain}";
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

        static public String GetDomain(String domain, bool isAWSHost, String s3Name, String s3Region= "us-east-2")
        {
            
            if (!isAWSHost) return domain;
            else
            {
                if (s3Name == null || s3Name.Length == 0) s3Name = domain;
                if (s3Region == null || s3Region.Length == 0) s3Region = "us-east-2";
                return $"{s3Name}.s3-website.{s3Region}.amazonaws.com";
            }
        }

        static public void GenerateSiteMapFile(QuerySnapshot snapshot, String folder, String selfDomainId, String baseURL, Hashtable domainMap)
        {
            DomainIpMap selfdomain = (DomainIpMap)domainMap[selfDomainId];
            String updateDate = DateTime.Now.ToString("yyyy-MM-dd");
            //robots.txt
            using (StreamWriter writer = new StreamWriter(folder + "//robots.txt"))
            {
                foreach (DictionaryEntry diMapItem in domainMap)
                {
                    DomainIpMap diMap = (DomainIpMap)diMapItem.Value;
                    if (!selfdomain.ip.Equals(diMap.ip)) continue;

                    //Sitemap: http://www.example.com/sitemap-host1.xml
                    writer.WriteLine($"Sitemap: {baseURL}/sitemap-{diMap.domain}.xml");

                    //sitemap.xml or sitemap-host.xml
                    using (StreamWriter writer2 = new StreamWriter(folder + $"//sitemap-{diMap.domain}.xml"))
                    {
                        writer2.WriteLine("<?xml version=\"1.0\" encoding=\"UTF - 8\"?>");
                        writer2.WriteLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
                        foreach (DocumentSnapshot document in snapshot.Documents)
                        {
                            var article = document.ConvertTo<Article>();
                            if (diMap.domainId != article.ProjectId) continue;
                            String fileName = CommonModule.GetHtmlFileName(article.MetaTitle, article.Title);
                            String hostingDomain = GetDomain(diMap.domain, CommonModule.isAWSHosting(diMap.ip), diMap.s3Name, diMap.s3Region);
                            writer2.WriteLine("   <url>");
                            writer2.WriteLine($"      <loc>http://{hostingDomain}/{fileName}</loc>");
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
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF - 8\"?>");
                writer.WriteLine("<sitemapindex  xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
                foreach (DictionaryEntry diMapItem in domainMap)
                {
                    DomainIpMap diMap = (DomainIpMap)diMapItem.Value;
                    if (!selfdomain.ip.Equals(diMap.ip)) continue;

                    String OriginalFileName = folder + $"//sitemap-{diMap.domain}.xml";
                    String CompressedFileName = folder + $"//sitemap-{diMap.domain}.xml.gz";
                    using FileStream originalFileStream = File.Open(OriginalFileName, FileMode.Open);
                    using FileStream compressedFileStream = File.Create(CompressedFileName);
                    using var compressor = new GZipStream(compressedFileStream, CompressionMode.Compress);
                    originalFileStream.CopyTo(compressor);
                    String hostingDomain = CommonModule.GetDomain(diMap.domain, CommonModule.isAWSHosting(diMap.ip), diMap.s3Name, diMap.s3Region);

                    writer.WriteLine($"   <sitemap>");
                    writer.WriteLine($"      <loc>http://{hostingDomain}/sitemap-{diMap.domain}.xml.gz</loc>");
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
                s3Client = new AmazonS3Client("AKIA6GFGHJFKCHWFMUWX", "6YvagXUBnahKdBSWmOjvmr5o5crZbzoiGLRNkIum", RegionEndpoint.GetBySystemName(region));
            }

            var success = await CreateBucket.CreateBucketAsync(s3Client, (string)domain, region);

            if( success )
                success = await CreateBucket.SetBucketPublicAsync(s3Client, (string)domain);

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
                s3Client = new AmazonS3Client("AKIA6GFGHJFKCHWFMUWX", "6YvagXUBnahKdBSWmOjvmr5o5crZbzoiGLRNkIum", RegionEndpoint.GetBySystemName(region));
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
    }
}