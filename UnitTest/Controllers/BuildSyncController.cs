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

        [HttpPost("{domainid}/{domain}/domainIp/{domainIp}")]
        public async Task<IActionResult> BuildPages(string domainid, string domain, string domainIp)
        {
            //Task.Run(() => this.BuildPagesThreadAsync(domainid, domain));
            await CommonModule.BuildPagesThreadAsync(domainid, domain, CommonModule.isAWSHosting(domainIp));
            return Ok(true);
        }

        [HttpPost("{domainid}/{domain}/{articleId}/{domainIp}")]
        public async Task<IActionResult> BuildArticlePage(string domainid, string domain, string articleId, string domainIp)
        {
            await CommonModule.BuildArticlePageThreadAsync(domainid, domain, articleId, CommonModule.isAWSHosting(domainIp));//Task.Run(() => this.BuildArticlePageThreadAsync(domainid, domain, articleId));
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
            await CommonModule.SyncWithServerThreadAsync(domainid, domain, ipaddr);//Task.Run(() => this.SyncWithServerThreadAsync(domainid, domain, ipaddr));
            return Ok(true);
        }
    }
}
