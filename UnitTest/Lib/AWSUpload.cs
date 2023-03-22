using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Razor.Parser.SyntaxTree;

namespace UnitTest.Lib
{
    internal class AWSUpload
    {
        public String uploadURL = "https://u7xal5o551.execute-api.us-east-2.amazonaws.com/prod/";
        public bool useTransferAcceleration = true;
        public int chunkSize = 1024 * 1024 * 5;
        public int threadsQuantity = 5;
        JArray partNumberETags = new JArray();
        int onUploadCnt = 0;

        public AWSUpload()
        {
        }

        public async Task start(String domain, String fileName, String path)
        {
            await StartUpload( domain, fileName, path );
        }

        public async void PartUpload(byte[] zipFile, JObject partObj, int len)
        {
            int partIndex = Int32.Parse(partObj["PartNumber"].ToString());
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(partObj["signedUrl"].ToString());
            request.Method = "PUT";
            request.Timeout = 1000 * 60;
            //request.ContentLength = len;
            using (var stream = request.GetRequestStream())
            {
                stream.Write(zipFile, (partIndex-1) * chunkSize, len);
            }

            try
            {
                using (var resp = (HttpWebResponse)request.GetResponse())
                {
                    using (var s = new StreamReader(resp.GetResponseStream()))
                    {
                        JObject part = new JObject();
                        part["PartNumber"] = partIndex;
                        part["ETag"] = resp.Headers["ETag"].Replace("\"", "");
                        partNumberETags.Add(part);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        public void UploadCompleted(Task task)
        {
            Console.WriteLine("UploadCompleted");
            onUploadCnt--;
        }

        public async Task StartUpload(String Domain, String zipFileName, String path)
        {
            String tempOutput = path + zipFileName;
            byte[] fileContents = File.ReadAllBytes(tempOutput);

            try
            {
                //================================================================================
                HttpWebRequest request = WebRequest.CreateHttp(uploadURL + "initialize");
                request.Method = "Post";
                request.ContentType = "application/json";
                string json = $"{{\"name\":\"{zipFileName}\",\"domain\":\"{Domain}\"}}";
                request.ContentLength = json.Length;
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {   
                    //string json = "{\"name\":\"" + item2 + "\"," + "\"email\":\"" + item1 + "\"}";
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                string sResult = "";
                using (WebResponse response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
                dynamic jResult = JsonConvert.DeserializeObject(sResult);

                string fileId = jResult.fileId;
                string fileKey = jResult.fileKey;

                int numberOfparts = (int)Math.Ceiling(fileContents.Length / (double)chunkSize);

                try
                {
                    //================================================================================
                    request = WebRequest.CreateHttp(uploadURL + "getPreSignedTAUrls");
                    request.Method = "Post";
                    request.ContentType = "application/json";
                    json = $"{{\"fileId\": \"{fileId}\", \"fileKey\": \"{fileKey}\", \"parts\": {numberOfparts}}}";
                    request.ContentLength = json.Length;
                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {   
                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                    using (WebResponse response = request.GetResponse())
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                        sResult = (streamReader.ReadToEnd());
                    jResult = JsonConvert.DeserializeObject(sResult);
                }
                catch (Exception ex)
                { 
                }

                //================================================================================                
                partNumberETags.Clear();

                var tasks = new List<Task>();
                int idx = 0;
                Stack<JObject> partStack = new Stack<JObject>();
                foreach (JObject part  in ((JObject)jResult)["parts"])
                {
                    partStack.Push(part);
                }

                while (partStack.Count > 0)
                {
                    while (onUploadCnt < threadsQuantity && partStack.Count > 0)
                    {
                        JObject part = partStack.Pop();
                        Console.WriteLine("Start Upload...");

                        int partIndex = Int32.Parse(part["PartNumber"].ToString());
                        int restSize = (int)fileContents.Length - (partIndex * chunkSize);
                        int sizeToRead = (restSize >= 0 ? chunkSize : chunkSize + restSize);
                        try
                        {
                            tasks.Add(Task.Run(() => this.PartUpload(fileContents, part, sizeToRead)).ContinueWith(this.UploadCompleted));
                            onUploadCnt++;
                        }
                        catch (Exception e)
                        { 
                        }
                    }
                    Thread.Sleep(1);
                }

                await Task.WhenAll(tasks);

                //================================================================================
                request = WebRequest.CreateHttp(uploadURL + "finalize");
                request.Method = "Post";
                request.ContentType = "application/json";                
                String parts = JsonConvert.SerializeObject(partNumberETags);
                json = $"{{\"fileId\": \"{fileId}\", \"fileKey\": \"{fileKey}\", \"parts\": {parts}}}";
                request.ContentLength = json.Length;
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {   
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                using (WebResponse response = request.GetResponse())
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
                jResult = JsonConvert.DeserializeObject(sResult);
            }
            catch (WebException e)
            {
            }
        }
    }
}
