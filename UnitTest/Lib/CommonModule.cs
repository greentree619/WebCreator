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

namespace UnitTest.Lib
{
    internal class CommonModule
    {
        public static Hashtable threadList = new Hashtable();
        public static Hashtable afThreadList = new Hashtable();
        public static Hashtable refKeyCash = new Hashtable();

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

        public static async Task<bool> ScrapArticleAsync(ArticleForge af, String question, String articleid) {
            bool status = false;
            try
            {
                dynamic jsonObjectParam = new JObject();
                jsonObjectParam.keyword = question;
                //jsonObjectParam.sub_keywords = "subkeyword1,subkeyword2,subkeyword3";
                jsonObjectParam.sentence_variation = 2;//a list of sub-keywords separated by comma (e.g. subkeyword1,subkeyword2,subkeyword3).
                jsonObjectParam.paragraph_variation = 2;//number of paragraph variations. It can be either 1, 2, or 3. The default value is 1.
                jsonObjectParam.shuffle_paragraphs = 1;//enable shuffle paragraphs or not.It can be either 0(disabled) or 1(enabled).The default value is 0.
                jsonObjectParam.length = "medium";//the length of the article. It can be either ‘very_short’(approximately 50 words), ‘short’(approximately 200 words), ‘medium’(approximately 500 words), or ‘long’(approximately 750 words). The default value is ‘short’.
                jsonObjectParam.title = 1;//It can be either 0 or 1. If it is set to be 0, the article generated is without titles and headings. The default value is 0.
                jsonObjectParam.image = 1.0;//the probability of adding an image into the article. It should be a float number from 0.00 to 1.00. The default value is 0.00.
                jsonObjectParam.video = 1.0;//the probability of adding a video into the article. It should be a float number from 0.00 to 1.00. The default value is 0.00.
                jsonObjectParam.quality = 3;


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
    }
}
