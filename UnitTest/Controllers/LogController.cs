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
using WebCreator.Models;

namespace WebCreator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogController : ControllerBase
    {
        [HttpGet("{projectId}/{category}/{page}/{count}")]
        public async Task<IActionResult> GetAsync(String projectId, String category, int page = 1, int count = 5)
        {
            var list = new List<History>();
            int total = 0;
            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("Histories");
                Query query = col.WhereEqualTo("DomainID", projectId).WhereEqualTo("Category", category);

                QuerySnapshot totalSnapshot = await query.GetSnapshotAsync();
                total = (int)Math.Ceiling((double)totalSnapshot.Count / count);

                query = col.WhereEqualTo("DomainID", projectId).WhereEqualTo("Category", category).OrderByDescending("CreatedTime");
                query = query.Offset((page - 1) * count).Limit(count);
                QuerySnapshot snapshot = await query.GetSnapshotAsync();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var log = document.ConvertTo<History>();
                    log.Id = document.Id;
                    list.Add(log);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(new { curPage = page, total = total, data = list });
        }
    }
}
