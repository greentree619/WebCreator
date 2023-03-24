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
    public class EC2IPAddressController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var list = new List<EC2IPAddress>();
            int total = 0;
            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("EC2IPAddress");
                QuerySnapshot snapshot = await col.GetSnapshotAsync();
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var ipAddr = document.ConvertTo<EC2IPAddress>();
                    ipAddr.Id = document.Id;
                    list.Add(ipAddr);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(new { curPage = 1, total = 1, data = list });
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] EC2IPAddress ec2IPAddress)
        {
            bool addOK = false;
            ec2IPAddress.CreatedTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("EC2IPAddress");
                Query query = col.WhereEqualTo("IPAddress", ec2IPAddress.IPAddress).Limit(1);
                QuerySnapshot projectsSnapshot = await query.GetSnapshotAsync();
                if (projectsSnapshot.Documents.Count == 0)
                {
                    DocumentReference docRef = await col.AddAsync(ec2IPAddress);
                    addOK = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(addOK);
        }

        [HttpPut]
        public async Task<ActionResult> PutAsync([FromBody] EC2IPAddress ec2IPAddress)
        {
            bool putOK = false;
            try
            {
                CollectionReference col = Config.FirebaseDB.Collection("EC2IPAddress");
                DocumentReference docRef = col.Document(ec2IPAddress.Id);
                Dictionary<string, object> userUpdate = new Dictionary<string, object>()
                {
                    { "IPAddress", ec2IPAddress.IPAddress },
                };
                await docRef.UpdateAsync(userUpdate);
                putOK = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(putOK);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAsync(String docId)
        {
            bool putOK = false;
            try
            {
                CollectionReference colRef = Config.FirebaseDB.Collection("EC2IPAddress");
                DocumentReference docRef = colRef.Document(docId);
                await docRef.DeleteAsync();
                putOK = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return Ok(putOK);
        }
    }
}
