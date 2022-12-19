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

namespace UnitTest.Lib
{
    internal class CommonModule
    {
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
    }
}
