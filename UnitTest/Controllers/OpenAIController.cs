using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebCreator.Models;
using SerpApi;
using System.Collections;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Logging;
using FirebaseSharp.Portable;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore.V1;
using System.Threading.Channels;
using System.IO;
using System.Linq;
using System.Web;
using UnitTest.Lib;
using Consul;
using RestSharp;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Web.Helpers;
using Microsoft.Net.Http.Headers;
using UnitTest.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;
using System.Text;
using System.Net;
using UnitTest.Models;

namespace UnitTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OpenAIController : ControllerBase
    {
        private readonly ILogger<OpenAIController> _logger;

        public OpenAIController(ILogger<OpenAIController> logger)
        {
            _logger = logger;
        }

        [HttpGet("image/{n}")]
        public async Task<IActionResult> GetImageAsync(int n, String? prompt="")
        {
            if (prompt == null || prompt.Length == 0) return Ok(new { data = new List<String>() });

            String thumbFolder = Directory.GetCurrentDirectory() + "\\Thumbnails";
            List<Hashtable> urls = CommonModule.GetImageFromOpenAI(prompt, n, thumbFolder);
            return Ok(new { data = urls });
        }

        [HttpGet("video/{n}")]
        public async Task<IActionResult> GetVideoAsync(int n, String? prompt = "")
        {
            if (prompt == null || prompt.Length == 0) return Ok(new { data = new List<String>() });
            return Ok(new { data = new List<String>() });

            //List<Hashtable> urls = CommonModule.GetImageFromOpenAI(prompt, n, thumbFolder);
            //return Ok(new { data = urls });
        }
    }
}
