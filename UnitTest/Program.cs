using Aspose.Zip;
using Aspose.Zip.Saving;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.IO.Compression;
using System.Text;
using UnitTest.Lib;
using WebCreator;

WebCreator.Config.FirebaseCredentialJson = File.ReadAllText("webcreator-dc8f8-35607d000566.json");
try
{
    Config.FirebaseDB = FirestoreDb.Create("webcreator-dc8f8"
        , (new FirestoreClientBuilder { JsonCredentials = Config.FirebaseCredentialJson }).Build());
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

//{{Article Forge UnitTest
//ArticleForge af = new ArticleForge();
//af.viewArticles();
//af.initiateArticle(JObject.Parse("{\"keyword\":\"What do you mean by Style?\"}"));
//af.getApiArticleResult("689026196");
//int ff = af.getApiProgress("689026196");
//}}Article Forge UnitTest

////{{Thread Test
//Console.WriteLine("Starting up PDF validations");
//var tasks = new List<Task>();
//for (int i = 0; i <= 2; i++)
//{
//    int instanceNumber = i;
//    //create and start tasks, then add them to the list
//    tasks.Add(Task.Run(() => new SerpapiScrap(instanceNumber).ValidateFile()).ContinueWith(LogResult));
//}
//Console.WriteLine("Now waiting for results...");
////wait until all the tasks in the list are completed
//await Task.WhenAll(tasks);
//Console.WriteLine("All done.");
//Console.ReadKey();

//static void LogResult(Task<string> task)
//{
//    Console.WriteLine($"Is Valid: {task.Result}");
//}
////}}Thread Test

//{{CloudFlare test
//bool b = CloudFlareAPI.VerifyTokenAsync();
//CloudFlareAPI api = new CloudFlareAPI();
//String b = api.CreateZone("testzone.net");

//api.CreateDns("testzone.com", "example4.com");
//api.ListDns("testzone.com", "example.com");
//api.DeleteDns("testzone.com", "example.com");
//api.UpdateDns("testzone.com", "testzone.com", "example6.com");
//}}CloudFlare test

////{{ZIP TEST
//String tmpFolder = "D:\\Workstation\\TonniProjects\\WebCreatorGit\\UnitTest\\bin\\Release\\net6.0\\Temp";
//String curFolder = "D:\\Workstation\\TonniProjects\\WebCreatorGit\\UnitTest\\bin\\Release\\net6.0\\Build\\traepiller.net";
////what folder to zip - include trailing slash
//string dirRoot = curFolder;
//string[] filesToZip = Directory.GetFiles(dirRoot, "*.*");
//string zipFileName = string.Format("zipfile-{0:yyyy-MM-dd_hh-mm-ss-tt}.zip", DateTime.Now);

//using (MemoryStream zipMS = new MemoryStream())
//{
//    using (ZipArchive zipArchive = new ZipArchive(zipMS, ZipArchiveMode.Create, true))
//    {
//        //loop through files to add
//        foreach (string fileToZip in filesToZip)
//        {
//            if (new FileInfo(fileToZip).Extension == ".zip") continue;
//            if (fileToZip.Contains("node_modules")) continue;

//            //read the file bytes
//            byte[] fileToZipBytes = System.IO.File.ReadAllBytes(fileToZip);
//            String zipEntry = fileToZip.Replace(dirRoot + "\\", "");
//            ZipArchiveEntry zipFileEntry = zipArchive.CreateEntry( zipEntry );

//            //add the file contents
//            using (Stream zipEntryStream = zipFileEntry.Open())
//            using (BinaryWriter zipFileBinary = new BinaryWriter(zipEntryStream))
//            {
//                zipFileBinary.Write(fileToZipBytes);
//            }
//            //lstLog.Items.Add("zipped: " + fileToZip);
//        }
//    }

//    using (FileStream finalZipFileStream = new FileStream($"{tmpFolder}\\" + zipFileName, FileMode.Create))
//    {
//        zipMS.Seek(0, SeekOrigin.Begin);
//        zipMS.CopyTo(finalZipFileStream);
//    }
//}
////}}ZIP TEST


/////{{ Patch Firebase Article.State
//CollectionReference colRef = Config.FirebaseDB.Collection("Articles");
//QuerySnapshot snapshot = await colRef.GetSnapshotAsync();
//WriteBatch updateBatch = Config.FirebaseDB.StartBatch();
//Dictionary<string, object> articleUpdate = new Dictionary<string, object>()
//{
//    { "State", 0 },
//};

//foreach (DocumentSnapshot document in snapshot.Documents)
//{
//    updateBatch.Update( document.Reference, articleUpdate );
//}
//await updateBatch.CommitAsync();
///// 
/////}} Patch Firebase Article.State

//Refresh Article Forge Scrapping status.
Task.Run(() => new CommonModule().UpdateArticleScrappingProgress());

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://18.222.223.99", "https://localhost:44496")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

// Add services to the container.

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Omitted app.UseHttpsRedirection();//FIXME
app.UseStaticFiles();
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
