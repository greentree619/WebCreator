using Google.Cloud.Firestore;
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

namespace WebCreator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DNSController : ControllerBase
    {
        private readonly ILogger<DNSController> _logger;
        private CloudFlareAPI cloudFlareAPI = new CloudFlareAPI();
        public DNSController(ILogger<DNSController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{page}/{count}")]
        public async Task<IActionResult> GetAsync(int page = 1, int count = 5)
        {
            if (page < 0) page = 1;
            if (count < 0) count = 5;

            String sResult = "";
            WebResponse response = cloudFlareAPI.ListZone(page, count);
            using (var streamReader = new StreamReader(response.GetResponseStream()))
                sResult = (streamReader.ReadToEnd());

            return Content(sResult, "application/json");
        }

        [HttpGet("byname/{domainName}")]
        public async Task<IActionResult> GetByNameAsync(String domainName)
        {
            String sResult = "{\"success\": false}";
            String[] domainInfo = domainName.Split(".");
            if (domainInfo.Length >= 2)
            {
                String zoneName = domainInfo[domainInfo.Length - 2] + "." + domainInfo[domainInfo.Length - 1];
                WebResponse response = cloudFlareAPI.ListZoneByName(zoneName);
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                    sResult = (streamReader.ReadToEnd());
            }

            return Content(sResult, "application/json");
        }

        [HttpGet("{zoneid}/{page}/{count}")]
        public async Task<IActionResult> GetAsync(string zoneId, int page = 1, int count = 5)
        {
            if (page < 0) page = 1;
            if (count < 0) count = 5;

            String sResult = "";
            WebResponse response = cloudFlareAPI.ListDns(zoneId, page, count);
            using (var streamReader = new StreamReader(response.GetResponseStream()))
                sResult = (streamReader.ReadToEnd());

            return Content(sResult, "application/json");
        }
    }
}
