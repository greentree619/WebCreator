using Google.Cloud.Firestore;
using Newtonsoft.Json.Linq;
using OpenAI_API;
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
    internal class OpenAIAPISetting
    {
        public OpenAISetting setInf = new OpenAISetting();

        public OpenAIAPISetting()
        {
            setInf.Model = Model.DavinciText;
            setInf.Prompt = "Write a Blogpost for 500 words about:{{Q}}";
            setInf.MaxTokens = 500;
            setInf.Temperature = 0.1f;
            setInf.TopP = 1;
            setInf.N = 1;
            //setInf.Logprobs = 0;
            setInf.PresencePenalty = 0;
            setInf.FrequencyPenalty = 0;

            Task.Run(() => initOpenAISettingFromFirebase());
        }

        public async Task initOpenAISettingFromFirebase() 
        {
            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("OpenAISetting");
                Query query = col.OrderByDescending("CreatedTime").Limit(1);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                if (snapshot.Count > 0)
                {
                    DocumentSnapshot snapShot = snapshot.Documents.ElementAt(0);
                    setInf = snapShot.ConvertTo<OpenAISetting>();
                    setInf.Id = snapShot.Id;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task SaveSettingAsync(OpenAISetting setInf)
        {
            try
            {
                this.setInf = setInf;
                CollectionReference col = Config.FirebaseDB.Collection("OpenAISetting");
                Query query = col.OrderByDescending("CreatedTime").Limit( 1 );
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                if (snapshot.Count <= 0)
                {
                    setInf.UpdateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                    setInf.CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                    await col.AddAsync(setInf);
                }
                else
                {
                    DocumentSnapshot snapShot = snapshot.Documents.ElementAt(0);
                    Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                    {
                        { "Model", setInf.Model},
                        { "Prompt", setInf.Prompt},
                        { "MaxTokens", setInf.MaxTokens},
                        { "Temperature", setInf.Temperature},
                        { "TopP", setInf.TopP},
                        { "N", setInf.N},
                        //{ "Logprobs", setInf.Logprobs},
                        { "PresencePenalty", setInf.PresencePenalty},
                        { "FrequencyPenalty", setInf.FrequencyPenalty},
                        { "UpdateTime", DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)}
                    };
                    await snapShot.Reference.UpdateAsync(userUpdate);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public String GetPrompt(String question)
        {
            String prompt = question;
            if(setInf.Prompt != null && setInf.Prompt.Length > 0) prompt = setInf.Prompt.Replace("{{Q}}", question);
            return prompt;
        }

        public String GetVideoScriptPrompt(String question)
        {
            String prompt = "Write a youtube video script with interesting facts include 40~50 words packed about: {{Q}}";
            prompt = prompt.Replace("{{Q}}", question);
            return prompt;
        }

        public async Task<OpenAISetting> GetSettingAsync()
        {
            await initOpenAISettingFromFirebase();
            return setInf;
        }
    }
}