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

namespace UnitTest.Lib
{
    internal class CommonModule
    {
        public static Hashtable threadList = new Hashtable();
        public static Hashtable afThreadList = new Hashtable();

        public static async Task SetDomainScrappingAsync(String domainId, bool isScrapping)
        {
            try
            {
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Projects");
                DocumentReference docRef = articlesCol.Document(domainId);

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "OnScrapping",  isScrapping},
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

        public static async Task ScrapArticleAsync(ArticleForge af, String question, String articleid) {
            try
            {
                String ref_key = af.initiateArticle(JObject.Parse("{\"keyword\":\"" + question + "\"}"));
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                DocumentReference docRef = articlesCol.Document(articleid);

                Console.WriteLine($"ScrapArticleAsync ref_key={ref_key}");

                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "ArticleId", ref_key },
                    { "Progress", 0 },
                    { "IsScrapping", true },
                };
                await docRef.UpdateAsync(userUpdate);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
    }
}
