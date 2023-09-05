using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebCreator;
using WebCreator.Models;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;

namespace UnitTest.Lib
{
    internal class OpenAIVideoGen
    {
        String videoScript = "[Videotitel]Hvordan bliver Bitcoin lavet?[Video Introduction]Velkommen til denne video om Bitcoin. I denne video undersøger vi, hvordan Bitcoin fremstilles, og hvordan den fungerer.[Hovedindhold]Bitcoin er en digital valuta, der skabes og opbevares elektronisk. Den trykkes ikke som traditionelle valutaer, men skabes i stedet gennem en proces, der kaldes mining. Mining er processen med at verificere og tilføje transaktionsoptegnelser til den offentlige hovedbog, kendt som blockchain.Blockchain er en offentlig hovedbog, der registrerer alle Bitcoin-transaktioner. Den vedligeholdes af et netværk af computere, der konstant verificerer og opdaterer hovedbogen.For at udvinde Bitcoin bruger minearbejdere specialiserede computere til at løse komplekse matematiske problemer. Når en miner løser et problem, belønnes de med en vis mængde Bitcoin. Denne proces er kendt som proof-of-work.Mængden af Bitcoin, der belønnes for at løse et problem, halveres hvert fjerde år. Dette er kendt som &quot;halveringen&quot; og er designet til at kontrollere udbuddet af Bitcoin.Processen med mining bruges også til at sikre netværket og forhindre svindel. Minere har et incitament til at holde netværket sikkert ved at verificere transaktioner og forhindre dobbeltforbrug.[Konklusion]Så det er sådan, Bitcoin bliver lavet. Det er en interessant og kompleks proces, men den er afgørende for at holde netværket sikkert og forhindre svindel. Tak, fordi du så med!";
        String tmpFolder = CommonModule.stupidVideoFolder;
        ScrapProgress scrapProgress = null;
        int duration = 0;
        public OpenAIVideoGen()
        {            
        }

        public async Task<String> GenerateStupidVideo(String projId, String script, String imageURL, ScrapProgress scrapProgress, string searchQuery = "nature") 
        {
            String url = "";
            videoScript = script;
            int resultsPerPage = 10; // Number of results to retrieve per page
            int pageNumber = 1; // Page number of the search results
            String imgLink = imageURL;
            scrapProgress?.MoveNextStep();//5
            if (imageURL.Length == 0)
            {
                imgLink = await SearchImages(searchQuery, resultsPerPage, pageNumber);
            }

            String imgExt = Path.GetExtension(imgLink);
            imgExt = ((imgExt == null || imgExt.Length == 0) ? ".jpg" : imgExt);
            String saveFile = tmpFolder + "\\" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "-" + CommonModule.GenerateRandomString(5) + imgExt;
            bool ret = await DownloadMediaFile(imgLink, saveFile);

            scrapProgress?.MoveNextStep();//6
            var clipPath = "";
            try
            {
                var voice = (await CommonModule.elevenLabsClientAPI.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
                var defaultVoiceSettings = await CommonModule.elevenLabsClientAPI.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
                defaultVoiceSettings.Stability = 0.9f;
                clipPath = await CommonModule.elevenLabsClientAPI.TextToSpeechEndpoint.TextToSpeechAsync(videoScript, voice, defaultVoiceSettings);
            }
            catch (Exception e) {
                CommonModule.Log(projId.ToString(), $"TextToSpeechEndpoint error: {e.Message.ToString()}", "scrap");
            }
            

            if (ret) {
                url = await GenerateVideo(saveFile, clipPath, videoScript, GetTextWidthBy(700), scrapProgress);
            }

            return url;
        }

        async Task<String> GenerateVideo(String saveFile, String audioFile, String videoScript, int textWidth, ScrapProgress scrapProgress)
        {
            int duration = 90;
            String scriptFileName = Path.GetFileName(saveFile);
            String resultID = Path.GetFileNameWithoutExtension(scriptFileName);
            scriptFileName = resultID + ".txt";
            scriptFileName = Path.GetDirectoryName(saveFile) + "\\" + scriptFileName;
            videoScript = videoScript.Replace("'", "\\'").Replace("%", "\\%");
            MakeScriptFile(scriptFileName , videoScript, textWidth);

            string ffmpegPath = "./FFmpeg/ffmpeg";
            string outputFilePath = CommonModule.stupidVideoFolder + $"\\{resultID}-bkg.mp4";
            string outputAudioFilePath = CommonModule.stupidVideoFolder + $"\\{resultID}-bkg-audio.mp4";
            string finalOutputFilePath = CommonModule.stupidVideoFolder + $"\\{resultID}-completed.mp4";
            string ffmpegCommand = $"-y -progress pipe:1 -loop 1 -t {duration} -i \"{saveFile}\" -vf scale=1024:-2 \"{outputFilePath}\"";
            scrapProgress?.MoveNextStep();//7
            RunFFmpegCommand(ffmpegPath, ffmpegCommand, scrapProgress, duration);

            scrapProgress?.MoveNextStep();//8
            if (audioFile.Length > 0)
            {
                ffmpegCommand = $"-y -progress pipe:1 -i \"{outputFilePath}\" -i \"{audioFile}\" -c:v copy -map 0:v -map 1:a -y \"{outputAudioFilePath}\"";
                RunFFmpegCommand(ffmpegPath, ffmpegCommand, scrapProgress, duration);
            }
            else 
            {
                outputAudioFilePath = outputFilePath;
            }

            String scriptFileNameTmp = scriptFileName.Replace("\\", "/").Replace(":", "\\:");
            string ffmpegCommand2 = $"-y -progress pipe:1 -i \"{outputAudioFilePath}\" -filter_complex \"[0]split[txt][orig];[txt]drawtext=fontfile=tahoma.ttf:fontsize=55:fontcolor=white:x=(w-text_w)/2+20:y=h/2-30*t:textfile='{scriptFileNameTmp}':bordercolor=black:line_spacing=20:borderw=3[txt];[orig]crop=iw:50:0:0[orig];[txt][orig]overlay\" -c:v libx264 -y -preset ultrafast -t {duration} \"{finalOutputFilePath}\"";
            scrapProgress?.MoveNextStep();//9
            RunFFmpegCommand(ffmpegPath, ffmpegCommand2, scrapProgress, duration);

            scrapProgress?.MoveNextStep();//10
            UploadToS3(finalOutputFilePath, scrapProgress);

            //Delete finalOutputFilePath, outputFilePath, scriptFileName, saveFile
            File.Delete(finalOutputFilePath);
            File.Delete(outputAudioFilePath);
            File.Delete(outputFilePath);
            File.Delete(scriptFileName);
            File.Delete(saveFile);
            scrapProgress?.SetDone();

            return $"https://article-image-bucket-live.s3.us-east-2.amazonaws.com/stupid-video/{resultID}-completed.mp4";
        }

        void UploadToS3(String uploadFile, ScrapProgress _scrapProgress) {
            scrapProgress = _scrapProgress;
            string bucketName = "article-image-bucket-live";
            string keyName = "stupid-video/"+Path.GetFileName(uploadFile);
            string filePath = uploadFile;

            // Set up your AWS credentials
            BasicAWSCredentials credentials = new BasicAWSCredentials(Config.AWSAccessKey, Config.AWSSecretKey);

            // Create a new Amazon S3 client
            AmazonS3Client s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USEast2);

            try
            {
                var uploadRequest =
                    new TransferUtilityUploadRequest
                    {
                        BucketName = bucketName,
                        FilePath = filePath,
                        Key = keyName,
                        PartSize = 5 * 1024 * 1024,// 5M
                        CannedACL = S3CannedACL.PublicRead
                    };
                uploadRequest.UploadProgressEvent += new EventHandler<UploadProgressArgs>(uploadRequest_UploadPartProgressEvent);

                // Upload the file to Amazon S3
                TransferUtility fileTransferUtility = new TransferUtility(s3Client);
                fileTransferUtility.Upload(uploadRequest);//fileTransferUtility.Upload(filePath, bucketName, keyName);
                //Console.WriteLine("Upload completed!");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
        }

        void uploadRequest_UploadPartProgressEvent(object sender, UploadProgressArgs e)
        {
            // Process event.
            //Console.WriteLine("{0}/{1}", e.TransferredBytes, e.TotalBytes);
            float uploadProgress = ((float)e.TransferredBytes / (float)e.TotalBytes) * 100;
            scrapProgress?.SetProgress((int)uploadProgress);
        }

        int GetTextWidthBy(int videoWidth) { 
            return 32;
        }

        void MakeScriptFile(String fileName, String videoScript, int textWidth)
        {
            Encoding utf8Encoding = new UTF8Encoding(false);

            // Open the file for writing with UTF-8 encoding
            using (StreamWriter writer = new StreamWriter(fileName, false, utf8Encoding))
            {
                // Write the text to the file
                String videoScriptTmp = Regex.Replace(videoScript, @"([\[][^\[\]]+[\]])", "\n\n");
                videoScriptTmp = videoScriptTmp.Trim('\n');
                String[] lineString = videoScriptTmp.Split("\n");
                foreach (var line in lineString) {
                    if (line.Length <= textWidth)
                    {
                        writer.WriteLine(line);
                    }
                    else {
                        string[] words = line.Split(' ');
                        String subLineString = "";
                        foreach (var word in words) {
                            if ((subLineString.Length + word.Length) <= textWidth)
                            {
                                if (subLineString.Length > 0) subLineString += " ";
                                subLineString += word;
                            }
                            else
                            {
                                writer.WriteLine(subLineString);
                                subLineString = word;
                            }
                        }
                        writer.WriteLine(subLineString);
                    }
                }
            }
        }

        private void FfmpegOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data != null)
            {
                if (outLine.Data.Contains(" time="))
                {
                    String timeToken = outLine.Data.Split(" time=")[1].Split(" ")[0];
                    float curSeconds = (float)TimeSpan.Parse(timeToken).TotalSeconds;
                    scrapProgress?.SetProgress( (int)( curSeconds/ (float)duration * 100) );
                }
            }
        }

        void RunFFmpegCommand(string ffmpegPath, string ffmpegCommand, ScrapProgress _scrapProgress, int _duration)
        {
            scrapProgress = _scrapProgress;
            duration = _duration;
            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.FileName = ffmpegPath;
                p.StartInfo.Arguments = ffmpegCommand;
                p.OutputDataReceived += new DataReceivedEventHandler(FfmpegOutputHandler);
                p.ErrorDataReceived += new DataReceivedEventHandler(FfmpegOutputHandler);
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                //string output = p.StandardOutput.ReadToEnd();
                //string error = p.StandardError.ReadToEnd();
                p.Close();
            }

            //Omitted
            //ProcessStartInfo startInfo = new ProcessStartInfo
            //{
            //    FileName = ffmpegPath,
            //    Arguments = ffmpegCommand,
            //    RedirectStandardOutput = true,
            //    RedirectStandardError = true,
            //    UseShellExecute = false,
            //    CreateNoWindow = true
            //};

            //using (Process process = new Process())
            //{
            //    process.StartInfo = startInfo;

            //    try {
            //        process.Start();

            //        string output = process.StandardOutput.ReadToEnd();
            //        string error = process.StandardError.ReadToEnd();

            //        process.WaitForExit();
            //    }
            //    catch (Exception e) {
            //        Console.WriteLine(e.Message);
            //    }
            //}
        }

        async Task<String> SearchImages(string searchQuery, int resultsPerPage, int pageNumber)
        {
            string apiKey = Config.PixabayKey;
            string ret = "";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Construct the Pixabay API URL
                    string apiUrl = $"https://pixabay.com/api/?key={apiKey}&q={searchQuery}&per_page={resultsPerPage}&page={pageNumber}";

                    // Send GET request to the Pixabay API
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    dynamic jsonObject = JsonConvert.DeserializeObject(jsonResponse);
                    JArray hits = jsonObject.hits;
                    if (hits.Count > 0) {
                        //Console.WriteLine(hits[0]["largeImageURL"].ToString());
                        ret = hits[0]["largeImageURL"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error retrieving search results: {ex.Message}");
                }
            }
            return ret;
        }

        async Task<bool> DownloadMediaFile(string imageUrl, string savePath)
        {
            bool ret = false;
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Send GET request to Pixabay to download the image
                    HttpResponseMessage response = await client.GetAsync(imageUrl);

                    // Ensure the request was successful
                    response.EnsureSuccessStatusCode();

                    // Read the image data as a byte array
                    byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                    // Save the image data to a file
                    File.WriteAllBytes(savePath, imageData);

                    ret = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error downloading image: {ex.Message}");
                }
            }

            return ret;
        }
    }
}