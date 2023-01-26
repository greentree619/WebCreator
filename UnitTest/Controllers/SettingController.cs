using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnitTest.Lib;
using WebCreator.Models;

namespace WebCreator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SettingController : ControllerBase
    {
        private readonly ILogger<SettingController> _logger;

        public SettingController(ILogger<SettingController> logger)
        {
            _logger = logger;
        }

        [HttpGet("afsetting")]
        public async Task<IActionResult> GetAsync()
        {
            AFSetting setting = await CommonModule.afSetting.GetSettingAsync();
            return Ok(new { data = setting });
        }

        [HttpPut("afsetting")]
        public async Task<IActionResult> PutAsync([FromBody] AFSetting afSet)
        {
            await CommonModule.afSetting.SaveSettingAsync(afSet);
            return Ok(true);
        }

        [HttpGet("openAISetting")]
        public async Task<IActionResult> GetOpenAISettingAsync()
        {
            OpenAISetting setting = await CommonModule.openAISetting.GetSettingAsync();
            return Ok(new { data = setting });
        }

        [HttpPut("openAISetting")]
        public async Task<IActionResult> PutOpenAISettingAsync([FromBody] OpenAISetting openAISet)
        {
            await CommonModule.openAISetting.SaveSettingAsync(openAISet);
            return Ok(true);
        }
    }
}