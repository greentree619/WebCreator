using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnitTest.Lib;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using WebCreator.Models;
using Aspose.Zip;
using Aspose.Zip.Saving;
using System.IO.Compression;
using System.Runtime.CompilerServices;

namespace WebCreator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BuildSyncController : ControllerBase
    {
        private readonly ILogger<BuildSyncController> _logger;
        public BuildSyncController(ILogger<BuildSyncController> logger)
        {
            _logger = logger;
        }

        [HttpPost("{domainid}/{domain}")]
        public async Task<IActionResult> BuildPages(string domainid, string domain)
        {
            //Task.Run(() => this.BuildPagesThreadAsync(domainid, domain));
            await BuildPagesThreadAsync(domainid, domain);
            return Ok(true);
        }

        [HttpPost("{domainid}/{domain}/{articleId}")]
        public async Task<IActionResult> BuildArticlePage(string domainid, string domain, string articleId)
        {
            await BuildArticlePageThreadAsync(domainid, domain, articleId);//Task.Run(() => this.BuildArticlePageThreadAsync(domainid, domain, articleId));
            return Ok(true);
        }

        [HttpGet("{domainid}/{domain}")]
        public async Task<IActionResult> GetBuildPages(string domainid, string domain)
        {
            var list = new List<HtmlFile>();
            String curFolder = Directory.GetCurrentDirectory();
            curFolder += $"\\Build\\{domain}\\";
            
            DirectoryInfo info = new DirectoryInfo(curFolder);
            FileInfo[] files = info.GetFiles("*.htm*").OrderBy(p => p.CreationTime).ToArray();
            foreach (FileInfo file in files)
            {
                var html = new HtmlFile();
                html.FileName = file.Name;
                html.CreatedTime = file.CreationTime;
                html.ModifiedTime = file.LastWriteTime;
                list.Add(html);
            }

            return Ok(new { curPage = 1, total = 1, data = list });
        }

        [HttpPost("sync/{domainid}/{domain}/{ipaddr}")]
        public async Task<IActionResult> SyncWithServer(string domainid, string domain, string ipaddr)
        {
            await SyncWithServerThreadAsync(domainid, domain, ipaddr);//Task.Run(() => this.SyncWithServerThreadAsync(domainid, domain, ipaddr));
            return Ok(true);
        }

        public async Task SyncWithServerThreadAsync(String domainid, String domain, String ipaddr)
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

        public async Task BuildPagesThreadAsync(String domainid, String domain)
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

                String articleUpdateStateIds = "";
                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                Query query = articlesCol.WhereEqualTo("ProjectId", domainid);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    if (article.Content != null && article.Content.Length > 0 
                                && (article.State == 2 || article.State == 3)) {

                        String title = article.Title.Replace("?", "").Trim();
                        using (StreamWriter writer = new StreamWriter(curFolder + "\\" + title + ".html"))
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

        public async Task BuildArticlePageThreadAsync(String domainid, String domain, String articleId)
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
                        String title = article.Title.Replace("?", "").Trim();
                        using (StreamWriter writer = new StreamWriter(curFolder + "\\" + title + ".html"))
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

                        //{{Update state
                        if(article.State != 3)
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
    }
}
