using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnitTest.Lib;
using WebCreator;

WebCreator.Config.FirebaseCredentialJson = File.ReadAllText("websitecreator-94452-firebase-adminsdk-l9yoo-962812244e.json");
try
{
    Config.FirebaseDB = FirestoreDb.Create("websitecreator-94452"
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
