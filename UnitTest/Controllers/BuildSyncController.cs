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
            Task.Run(() => this.BuildPagesThreadAsync(domainid, domain));
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
            Task.Run(() => this.SyncWithServerThreadAsync(domainid, domain, ipaddr));
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
                    using (FileStream zipFile = System.IO.File.Open($"{tmpFolder}\\{domain}.zip", FileMode.Create))
                    {
                        using (var archive = new Archive())
                        {
                            DirectoryInfo info = new DirectoryInfo(curFolder);
                            FileInfo[] files = info.GetFiles("*.htm*").ToArray();
                            foreach (FileInfo file in files)
                            {
                                archive.CreateEntry(file.Name, file);
                            }
                            // Create ZIP archive
                            archive.Save(zipFile, new ArchiveSaveOptions() { Encoding = Encoding.ASCII });
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
                if (!Directory.Exists(curFolder)) { 
                    Directory.CreateDirectory(curFolder);
                }

                CollectionReference articlesCol = Config.FirebaseDB.Collection("Articles");
                Query query = articlesCol.WhereEqualTo("ProjectId", domainid);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var article = document.ConvertTo<Article>();
                    if (article.Content != null && article.Content.Length > 0) {
                        String title = article.Title.Replace("?", "").Trim();
                        using (StreamWriter writer = new StreamWriter(curFolder + "\\" + title + ".html"))
                        {
                            writer.Write(article.Content);
                        }
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
