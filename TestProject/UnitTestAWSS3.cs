using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using AWSUtility;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace TestProject
{
    public class UnitTestAWSS3
    {
        AmazonS3Client client = null;

        [SetUp]
        public async Task SetupAsync()
        {
            Console.WriteLine("\nPress <Enter> to list the S3 buckets using the new user.\n");
            //Console.ReadLine();

            // Creating a client that works with this user.
            client = new AmazonS3Client("AKIA6GFGHJFKCHWFMUWX", "6YvagXUBnahKdBSWmOjvmr5o5crZbzoiGLRNkIum", RegionEndpoint.USEast2);
        }

        [Test]
        public async Task Test1Async()
        {
            var success = await CreateBucket.CreateBucketAsync(client, (string)"onlineshop893418736419873649187349187.com");
            //Omitted success = await CreateBucket.SetBucketPublicAsync(client, (string)"onlineshop893418736419873649187349187.com");
            success = await CreateBucket.AddWebsiteConfigurationAsync(client, (string)"onlineshop893418736419873649187349187.com", "index.html", "error.html");
            if (success)
            {
                Console.WriteLine($"Successfully created bucket: .\n");
            }
            else
            {
                Console.WriteLine($"Could not create bucket: .\n");
            }

            // Get the list of buckets accessible by the new user.
            var response = await client.ListBucketsAsync();
            // Loop through the list and print each bucket's name
            // and creation date.
            Console.WriteLine("\n--------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("Listing S3 buckets:\n");
            response.Buckets
                .ForEach(b => Console.WriteLine($"Bucket name: {b.BucketName}, created on: {b.CreationDate}"));

            await CreateBucket.WritingAnObjectAsync(client, (string)"onlineshop893418736419873649187349187.com", "test/index.html", "Hello World!");

            Assert.Pass();
        }

        [Test]
        public async Task Test2Async()
        {
            var success = await CreateBucket.CreateBucketAsync(client, (string)"onlineshoptest234752693876529");
            if (success)
            {
                Console.WriteLine($"Successfully created bucket: .\n");
            }
            else
            {
                Console.WriteLine($"Could not create bucket: .\n");
            }

            // Get the list of buckets accessible by the new user.
            var response = await client.ListBucketsAsync();
            // Loop through the list and print each bucket's name
            // and creation date.
            Console.WriteLine("\n--------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("Listing S3 buckets:\n");
            response.Buckets
                .ForEach(b => Console.WriteLine($"Bucket name: {b.BucketName}, created on: {b.CreationDate}"));

            Assert.Pass();
        }
    }
}