using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using WebCreator;
using WebCreator.Models;

namespace UnitTest.Lib
{
    internal class HistoryManagement
    {
        public async Task LogKeywordAction(String DomainId, String keywords, bool isAdd = true, bool isManual = true)
        {
            String addStr = isAdd ? "Added" : "Updated";
            String manualStr = isManual ? "Manual" : "Scrapping";
            var history = new History
            {
                Category = "Keyword",
                DomainID = DomainId,
                Log = $"{addStr} {manualStr}-Keywords: {keywords}",
                CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
            };

            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("Histories");
                DocumentReference docRef = await col.AddAsync(history);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task LogScrapKeywordAction(String DomainId, String keyword)
        {
            var history = new History
            {
                Category = "Keyword",
                DomainID = DomainId,
                Log = $"Scrapped query: '{keyword}' from Google.",
                CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
            };

            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("Histories");
                DocumentReference docRef = await col.AddAsync(history);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task ThemeUploadAction(String DomainId, String fileName)
        {
            var history = new History
            {
                Category = "Theme",
                DomainID = DomainId,
                Log = $"Upload theme: '{fileName}'",
                CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc),
            };

            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("Histories");
                DocumentReference docRef = await col.AddAsync(history);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
