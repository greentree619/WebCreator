using Aspose.Zip;
using Aspose.Zip.Saving;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using OpenAI_API;
using System.Collections;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnitTest.Interfaces;
using UnitTest.Lib;
using UnitTest.Services;
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

////{{REGEXPRESS TEST
//String content = "{<h1>The 5 Rules of Football</h1> <img align=\"{left|right}\" alt=\"What are the 5 rules of football\" style=\"max-width:44 %; width: auto; height: auto; max - height:272px; margin: 0px 10px; \" src=\"https://i.imgur.com/gd3d94N.jpg\">|<img align=\"{left|right}\" alt=\"What are the 5 rules of football\" style=\"max-width:44%; width:auto; height:auto; max-height:272px; margin:0px 10px;\" src=\"https://i.imgur.com/gd3d94N.jpg\"> <h1>The 5 Rules of Football</h1>} <p>The game of football has always been a favorite sport for many people, and it is easy to see why. The game involves some rules that must be followed, and if you want to play football at its highest level, you will need to be familiar with them. Below are some of the basic rules of football. By understanding these rules, you will be able to make the game more enjoyable for yourself and your team.</p> <h2>Offsides</h2><p>Offsides are a football rule that can be confusing for less experienced players. It can be frustrating for the player that is offside and also for the team that is receiving the ball. The offside rule is controversial. Some argue that it can be a good thing. Others believe it is a bad thing. Regardless, it has made the game much more interesting.</p> <p>A player is considered to be offside when he is in front of the ball and any part of his body is outside of the neutral zone. If a player jumps offside while the ball is being played, the opposing team is awarded a five-yard penalty.</p> <p>The offsides rule has many different forms. In high school and college football, offsides are called when a defensive player breaks the line of scrimmage before the snap. Similarly, an offensive player can be penalized for making an illegal motion before the snap.</p> <h2>Down markers</h2><p>A down marker aka football chain is what the name implies. These metal rods covered in padding will keep your hapless opponents from taking the blame. One or two will suffice, but three is recommended. They are available in a range of shapes and sizes, from octagonal to square. To snag a bargain, look for sales at the local sporting goods store or online. Some retailers have special deals or rebates, or you can find a mate who is willing to give you a tidbit of his trade secrets. If you are lucky, he may even let you try out his gear on him.</p> <p>There are actually more than a few football down markers to choose from, and many of them can be found for less than the cost of a bottle of booze. The trick is in picking the right one. This can be a daunting task for novices, but a little research can go a long way.</p> <h2>Punishment kicks</h2><p>Punishment kicks are awarded for infringements in the penalty area. This area is defined by the referee as a region on the field, marked by an 18-yard box.</p> <p>The penalty box is located 16.5 meters from the goal posts. In the past, a penalty kick could be awarded for offences within twelve yards of the goal line. However, in 1891, a new rule was introduced, limiting penalty kicks to fouls that took place within the penalty area.</p> <p>Penalty kicks can be awarded for a wide variety of offences. Some of the most common include physical fouls, interfering fouls, and illegal contact. If the kick is taken, the team taking the penalty must stand at the designated spot, identified by the referee.</p> <h2>Free kicks</h2><p>The free kick is a great tool for teams to attack their opponent. They are often used to set up set pieces and to score goals. There are two types of free kicks. These include direct and indirect kicks.</p> <p>A direct free kick is one that is taken directly from the field. This kick is aimed directly at the goal. It is also known as a \"one-touch\" kick.</p> <p>An indirect free kick is one that is a pass to another player. For this kick to be successful, it must be touched by a member of the opposing team.</p> <p>A free kick may not be awarded in a penalty area. However, a foul outside the penalty area may be worth a penalty kick. If the referee determines that the foul was worthy of a penalty kick, it will be awarded.</p> <h2>Goal attempts</h2><p>The main goal of football is to score a touchdown. To achieve a touchdown, the ball must be carried or passed over the goal line. However, a team can only score a touchdown against an opponent.</p> <p>Goal attempts are usually made on fourth down. In some cases, teams attempt field goals after a touchdown. If the kick fails, the field goal is recorded as a miss. There are several rules that govern field goals and extra points.</p> <p>A goal kick is awarded when the ball completely crosses the goal line. However, it can also be kicked in the air.</p> <p>Field goals are scored by the place kicker. They are calculated by adding 17 yards to the line of scrimmage plus 10 yards for the end zone.</p>";
//Regex regex = new Regex("[{]([^{}]+)[}]");
//string[] substrings = regex.Split(content);
////}}REGEXPRESS TEST

////{{OpenAI Content Generate TEST
//OpenAIAPI api = new OpenAIAPI("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");
//var result = await api.Completions.CreateCompletionAsync(
//    new CompletionRequest("Write a Blogpost for 500 words about: what is a good Amarone red wine"
//    , model: Model.DavinciText
//    , temperature: 0.1
//    , max_tokens: 1000));
//Console.WriteLine(result.ToString());
////}}OpenAI Content Generate TEST

//Refresh Article Forge Scrapping status.
Task.Run(() => new CommonModule().UpdateArticleScrappingProgress());

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IStreamFileUploadService, StreamFileUploadLocalService>();

// Register the Swagger generator, defining 1 or more Swagger documents 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "EShopping  WebAPI",
        Description = "ASP.NET Core Web API",
        TermsOfService = new Uri("https://www.linkedin.com/in/aman-toumaj-92114051/"),
        Contact = new OpenApiContact
        {
            Name = "EShopping  Web API",
            Email = string.Empty,
            Url = new Uri("https://www.linkedin.com/in/aman-toumaj-92114051/"),
        },
        License = new OpenApiLicense
        {
            Name = "Aman Toumaj",
            Url = new Uri("https://www.linkedin.com/in/aman-toumaj-92114051/"),
        }
    });

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

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
builder.Services.Configure<FormOptions>(x => x.ValueCountLimit = 50000);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Omitted app.UseHttpsRedirection();//FIXME
//{{Swagger
app.UseSwagger(c =>
{
    c.SerializeAsV2 = true;
});

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty;
});

app.UseStaticFiles();

// Register the Swagger generator and the Swagger UI middlewares
//Omitted app.UseOpenApi();

app.UseSwaggerUI();
//}}Swagger
app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
