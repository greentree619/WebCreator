using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTest.Lib;
using WebCreator;
using static WebCreator.Controllers.ProjectController;

namespace UnitTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class S3BucketController : ControllerBase
    {
        private readonly ILogger<S3BucketController> _logger;

        public S3BucketController(ILogger<S3BucketController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetBucketListAsync()
        {
            var list = new List<Hashtable>();
            try
            {
                // Get the list of buckets accessible by the new user.

                var response = await CommonModule.amazonS3Client.ListBucketsAsync();
                int index = 0;
                response.Buckets
                    .ForEach(b =>
                    {
                        index++;
                        var bucket = new Hashtable();
                        bucket["index"] = index;
                        bucket["name"] = b.BucketName;
                        bucket["createDate"] = b.CreationDate;
                        list.Add(bucket);
                        //Console.WriteLine($"Bucket name: {b.BucketName}, created on: {b.CreationDate}");
                    });
            }
            catch (AmazonS3Exception ex)
            {
                // Something else went wrong. Display the error message.
                Console.WriteLine($"Error: {ex.Message}");
            }

            foreach (Hashtable bucket in list)
            {
                String region = await CommonModule.FindBucketLocationAsync(bucket["name"].ToString());
                bucket["region"] = region;
            }

            return Ok(new { result = list });
        }

        [HttpGet("contents/{bucketName}/{region}")]
        public async Task<IActionResult> GetBucketContentAsync(string bucketName, String region)
        {
            var list = new List<Hashtable>();
            String error = "";
            try
            {
                // Get the list of buckets accessible by the new user.
                AmazonS3Client amazonS3Client = new AmazonS3Client(Config.AWSAccessKey, Config.AWSSecretKey, RegionEndpoint.GetBySystemName(region));
                var response = await CommonModule.ListBucketContentsAsync(amazonS3Client, bucketName, list);
            }
            catch (AmazonS3Exception ex)
            {
                // Something else went wrong. Display the error message.
                //Console.WriteLine($"Error: {ex.Message}");
                error = ex.Message;
            }

            return Ok(new { result = list, error = error });
        }

        [HttpPost("{bucketName}")]
        public async Task<IActionResult> CreateBucketAsync(String bucketName)
        {
            bool bret = false;
            try
            {
                bret = await new CommonModule().CreateHostBucketThreadAsync(bucketName);
            }
            catch (AmazonS3Exception ex)
            {
                // Something else went wrong. Display the error message.
                Console.WriteLine($"Error: {ex.Message}");
            }

            return Ok(new { result = bret });
        }

        [HttpPost("{bucketName}/{region}")]
        public async Task<IActionResult> CreateBucketAsync(String bucketName, String region)
        {
            bool bret = false;
            try
            {
                bret = await new CommonModule().CreateHostBucketThreadAsync(bucketName, region);
            }
            catch (AmazonS3Exception ex)
            {
                // Something else went wrong. Display the error message.
                Console.WriteLine($"Error: {ex.Message}");
            }

            return Ok(new { result = bret });
        }

        [HttpDelete("{bucketName}/{region}")]
        public async Task<IActionResult> DeleteBucketAsync(String bucketName, String region)
        {
            bool bret = false;
            try
            {
                bret = await CommonModule.DeleteBucketAsync(bucketName, region);
            }
            catch (AmazonS3Exception ex)
            {
                // Something else went wrong. Display the error message.
                Console.WriteLine($"Error: {ex.Message}");
            }

            return Ok(new { result = bret });
        }

        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        [HttpPost("Upload/{domainId}/{domainName}")]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> Upload(String domainId, String domainName)
        {
            try
            {
                if (CommonModule.onThemeUpdateCash[domainId] == null || (bool)CommonModule.onThemeUpdateCash[domainId] == false)
                {
                    CommonModule.onThemeUpdateCash[domainId] = true;
                    String curFolder = Directory.GetCurrentDirectory();
                    curFolder += $"\\Upload\\{domainName}";
                    if (!Directory.Exists(curFolder))
                    {
                        Directory.CreateDirectory(curFolder);
                    }

                    if (CommonModule.IsMultipartContentType(Request.ContentType))
                    {
                        var boundary = CommonModule.GetBoundary(Request.ContentType);
                        var reader = new MultipartReader(boundary, Request.Body);
                        var section = await reader.ReadNextSectionAsync();

                        while (section != null)
                        {
                            // process each image
                            const int chunkSize = 1024;
                            var buffer = new byte[chunkSize];
                            var bytesRead = 0;
                            var fileName = CommonModule.GetFileName(section.ContentDisposition);
                            using (var stream = new FileStream(curFolder + "/S3Data.zip", FileMode.Create))
                            {
                                do
                                {
                                    bytesRead = await section.Body.ReadAsync(buffer, 0, buffer.Length);
                                    stream.Write(buffer, 0, bytesRead);

                                } while (bytesRead > 0);
                            }

                            section = await reader.ReadNextSectionAsync();
                        }
                        await CommonModule.UploadDataToS3ThreadAsync(domainName);
                    }
                    CommonModule.onThemeUpdateCash[domainId] = false;
                }
            }
            catch (Exception ex)
            {
            }
            return Ok(true);
        }
    }
}
