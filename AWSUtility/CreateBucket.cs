using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace AWSUtility
{
    public class CreateBucket
    {
        static public async Task<bool> CreateBucketAsync(AmazonS3Client s3Client, string bucketName, string region = "")
        {
            bool bret = false;
            try
            {
                if (!(await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName)))
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = bucketName,
                        UseClientRegion = true
                    };

                    PutBucketResponse putBucketResponse = await s3Client.PutBucketAsync(putBucketRequest);
                    bret = true;
                }
                // Retrieve the bucket location.
                string bucketLocation = await FindBucketLocationAsync(s3Client, bucketName);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

            return bret;
        }
        static public async Task<string> FindBucketLocationAsync(AmazonS3Client client, string bucketName)
        {
            string bucketLocation;
            var request = new GetBucketLocationRequest()
            {
                BucketName = bucketName
            };
            GetBucketLocationResponse response = await client.GetBucketLocationAsync(request);
            bucketLocation = response.Location.ToString();
            return bucketLocation;
        }

        static public async Task<bool> AddWebsiteConfigurationAsync(AmazonS3Client client, string bucketName,
                                            string indexDocumentSuffix,
                                            string errorDocument)
        {
            bool bret = false;
            try
            {
                // 1. Put the website configuration.
                PutBucketWebsiteRequest putRequest = new PutBucketWebsiteRequest()
                {
                    BucketName = bucketName,
                    WebsiteConfiguration = new WebsiteConfiguration()
                    {
                        IndexDocumentSuffix = indexDocumentSuffix,
                        ErrorDocument = errorDocument
                    }
                };
                PutBucketWebsiteResponse response = await client.PutBucketWebsiteAsync(putRequest);

                // 2. Get the website configuration.
                GetBucketWebsiteRequest getRequest = new GetBucketWebsiteRequest()
                {
                    BucketName = bucketName
                };
                GetBucketWebsiteResponse getResponse = await client.GetBucketWebsiteAsync(getRequest);
                Console.WriteLine("Index document: {0}", getResponse.WebsiteConfiguration.IndexDocumentSuffix);
                Console.WriteLine("Error document: {0}", getResponse.WebsiteConfiguration.ErrorDocument);

                bret = true;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }

            return bret;
        }

        static public async Task WritingAnObjectAsync(AmazonS3Client client, string bucketName, string keyName, string Content)
        {
            try
            {
                // 1. Put object-specify only key name for the new object.
                var putRequest1 = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    ContentBody = Content
                };

                PutObjectResponse response1 = await client.PutObjectAsync(putRequest1);

                //// 2. Put the object-set ContentType and add metadata.
                //var putRequest2 = new PutObjectRequest
                //{
                //    BucketName = bucketName,
                //    Key = keyName2,
                //    FilePath = filePath,
                //    ContentType = "text/plain"
                //};

                //putRequest2.Metadata.Add("x-amz-meta-title", "someTitle");
                //PutObjectResponse response2 = await client.PutObjectAsync(putRequest2);
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object"
                        , e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object"
                    , e.Message);
            }
        }

        public static async Task<S3AccessControlList> GetBucketACLAsync(AmazonS3Client client, string bucketName)
        {
            GetACLResponse response = await client.GetACLAsync(new GetACLRequest
            {
                BucketName = bucketName,
            });

            return response.AccessControlList;
        }

        static public async Task<bool> SetBucketPublicAsync(AmazonS3Client client, string bucketName)
        {
            bool bret = false;
            string publicPolicy = "{" +
                "\"Statement\": [" +
                "{" +
                "    \"Effect\": \"Allow\"," +
                "    \"Principal\": {" +
                "        \"AWS\": \"arn:aws:iam::031023765277:role/service-role/deploayWebsiteData-role-uvadv5bp\"" +
                "    }," +
                "    \"Action\": \"s3:*\"," +
                "    \"Resource\": [" +
                $"        \"arn:aws:s3:::{bucketName}\"," +
                $"        \"arn:aws:s3:::{bucketName}/*\"" +
                "    ]," +
                "    \"Condition\": {" +
                "        \"Bool\": {" +
                "            \"aws:SecureTransport\": \"false\"" +
                "        }" +
                "    }" +
                "}" +
                ",{" +
                "\"Sid\": \"VisualEditor1\"," +
                "\"Effect\": \"Allow\"," +
                "\"Principal\": {" +
                "\"AWS\": \"*\"" +
                "}," +
                "\"Action\": \"s3:GetObject\"," +
                $"\"Resource\": \"arn:aws:s3:::{bucketName}/*\"" +
                "}" +
                "]" +
                "}";

            try
            {
                await client.PutBucketPolicyAsync(bucketName, publicPolicy);
                bret = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return bret;
        }
    }
}