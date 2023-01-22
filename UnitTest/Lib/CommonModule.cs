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

namespace UnitTest.Lib
{
    internal class CommonModule
    {
        public static Hashtable threadList = new Hashtable();
        public static Hashtable afThreadList = new Hashtable();
        public static Hashtable publishThreadList = new Hashtable();
        public static Hashtable refKeyCash = new Hashtable();
        public static Hashtable onThemeUpdateCash = new Hashtable();
        public static ArticleForgeSetting afSetting = new ArticleForgeSetting();
        public static bool isManualAFScrapping = false;

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

        public static String articleURL(String domain, String question) {
            String filename = question.Replace("?", "");
            filename = Uri.EscapeUriString(filename + ".html");
            return $"http://{domain}/{filename}";
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

        static public void GenerateArticleHtml(String fileName, Article article, String articleTemplate)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                if (articleTemplate.Length == 0)
                {
                    writer.Write("<!DOCTYPE html>");
                    writer.Write("<html>");
                    writer.Write("<head>");
                    writer.Write($"<title>{article.Title}</title>");
                    if (article.MetaDescription != null && article.MetaDescription.Length > 0)
                    {
                        writer.Write($"<meta name=\"description\" content=\"{article.MetaDescription}\">");
                    }

                    if (article.MetaKeywords != null && article.MetaKeywords.Length > 0)
                    {
                        writer.Write($"<meta name=\"keywords\" content=\"{article.MetaKeywords}\">");
                    }

                    if (article.MetaAuthor != null && article.MetaAuthor.Length > 0)
                    {
                        writer.Write($"<meta name=\"author\" content=\"{article.MetaAuthor}\">");
                    }

                    writer.Write($"<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                    writer.Write("</head>");
                    writer.Write("<body>");
                    writer.Write(article.Content);
                    if (article.Footer != null && article.Footer.Length > 0)
                    {
                        writer.Write("<footer>");
                        writer.Write(article.Footer);
                        writer.Write("</footer>");
                    }
                    writer.Write("</body>");
                    writer.Write("</html>");
                }
                else
                {
                    if (article.MetaDescription == null) article.MetaDescription = "";
                    if (article.MetaKeywords == null) article.MetaKeywords = "";
                    if (article.MetaAuthor == null) article.MetaAuthor = "";

                    String metaDesc = $"<meta name=\"description\" content=\"{article.MetaDescription}\">";
                    String metaKeywd = $"<meta name=\"keywords\" content=\"{article.MetaKeywords}\">";
                    String metaAuthor = $"<meta name=\"author\" content=\"{article.MetaAuthor}\">";

                    articleTemplate = articleTemplate.Replace("{{TITLE}}", article.Title);
                    articleTemplate = articleTemplate.Replace("{{CONTENT}}", article.Content);
                    articleTemplate = articleTemplate.Replace("{{FOOTER}}", article.Footer);
                    articleTemplate = articleTemplate.Replace("{{META_DESC}}", metaDesc);
                    articleTemplate = articleTemplate.Replace("{{META_KEYWORD}}", metaKeywd);
                    articleTemplate = articleTemplate.Replace("{{META_AUTHOR}}", metaAuthor);
                    writer.Write(articleTemplate);
                }
            }
        }

        static public async Task BuildArticlePageThreadAsync(String domainid, String domain, String articleId)
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
                        String title = article.Title.Replace("?", "").Trim();
                        title = title.Replace(" ", "-");
                        String articleTemplate = GetArticleTemplate(domain);
                        GenerateArticleHtml(curFolder + "\\" + title + ".html", article, articleTemplate);

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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public async Task BuildPagesThreadAsync(String domainid, String domain, int defaultState=2, bool force = true)
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
                        String title = article.Title.Replace("?", "").Trim();
                        title = title.Replace(" ", "-");
                        GenerateArticleHtml(curFolder + "\\" + title + ".html", article, articleTemplate);
                        //using (StreamWriter writer = new StreamWriter(curFolder + "\\" + title + ".html"))
                        //{
                        //    writer.Write("<!DOCTYPE html>");
                        //    writer.Write("<html>");
                        //    writer.Write("<head>");
                        //    writer.Write($"<title>{article.Title}</title>");
                        //    if (article.MetaDescription != null && article.MetaDescription.Length > 0)
                        //    {
                        //        writer.Write($"<meta name=\"description\" content=\"{article.MetaDescription}\">");
                        //    }

                        //    if (article.MetaKeywords != null && article.MetaKeywords.Length > 0)
                        //    {
                        //        writer.Write($"<meta name=\"keywords\" content=\"{article.MetaKeywords}\">");
                        //    }

                        //    if (article.MetaAuthor != null && article.MetaAuthor.Length > 0)
                        //    {
                        //        writer.Write($"<meta name=\"author\" content=\"{article.MetaAuthor}\">");
                        //    }

                        //    writer.Write($"<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
                        //    writer.Write("</head>");
                        //    writer.Write("<body>");
                        //    writer.Write(article.Content);
                        //    if (article.Footer != null && article.Footer.Length > 0)
                        //    {
                        //        writer.Write("<footer>");
                        //        writer.Write(article.Footer);
                        //        writer.Write("</footer>");
                        //    }
                        //    writer.Write("</body>");
                        //    writer.Write("</html>");
                        //}

                        //{{Update state
                        if (article.State == 2)
                        {
                            if (articleUpdateStateIds.Length > 0) articleUpdateStateIds += ",";
                            articleUpdateStateIds += document.Id;
                        }
                        //}}Update state
                    }
                }

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

        static public async Task SyncWithServerThreadAsync(String domainid, String domain, String ipaddr)
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

                    String cmd = $"pscp -i {exeFolder}\\searchsystem.ppk {tmpFolder}\\{domain}.zip ubuntu@{ipaddr}:/home/ubuntu";

                    Console.WriteLine("SyncWithServerThreadAsync --> " + cmd);
                    ExecuteCmd.ExecuteCommandAsync(cmd);
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
                    String cmd = $"pscp -i {exeFolder}\\searchsystem.ppk {curFolder} ubuntu@{ipaddr}:/home/ubuntu";

                    Console.WriteLine("SyncWithServerThreadAsync --> " + cmd);
                    ExecuteCmd.ExecuteCommandAsync(cmd);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
