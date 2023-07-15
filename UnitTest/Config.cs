using FireSharp.Config;
using FireSharp.Interfaces;
using Google.Cloud.Firestore;
using System.Text.Json.Serialization;

namespace WebCreator
{
    public static class Config
    {
        //public static KeyValuePair<string, object> Application { get; set; }
        public static String FirebaseAuthSecret = "5cGJIcs3WRWYyu8XTFyjy8zW87Ql2ey44IOZvIRO";
        public static String FirebaseBasePath = "https://websitecreator-94452-default-rtdb.firebaseio.com";
        public static String FirebaseCredentialJson = "";
        public static FirestoreDb FirebaseDB = null;
        public static IFirebaseConfig firebaseConfig = new FirebaseConfig
        {
            AuthSecret = Config.FirebaseAuthSecret,
            BasePath = Config.FirebaseBasePath
        };

        public static String ArticleForgeKey = "b6391ee8e9105e4e26042008edf03e62";
        public static String SerpApiKey = "e98e32704f4642aa6fbf1cf0086aed75474dab0b0d1f88e2c0a68266bab57b2f";
        public static String DeepLKey = "37f5f472-d0f2-44c2-3a5d-7896694f8cdc";
        public static String PixabayKey = "27944002-ca9bbda02c769f32ad5769e81";
        public static String ElevenLabsKey = "2f761d0275bf7b9da485e68f6e58af09";
        public static String CloudFlareAPIKey = "vKQiwIFU0Eyz269KeOjBsliYyaaEQMqZRe3QA9TE";
        public static String CloudFlareAPIEmail= "uniqtop@gmail.com";
        public static String OpenAIKey = "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";
        public static String YoutubeUploadCredential = "client_secret_863136570251-i28otmjbk57b280m56af2a6o7onchp36.apps.googleusercontent.com.json";
        public static String YoutubeAPIKey = "AIzaSyAU6kILgFASsoGBwZZH2UwuYUG4Sid6YUY";

#if false //As default, let stage
        //Stage
        /*1*/public static String Principal = "arn:aws:iam::975306180948:role/service-role/deploayWebsiteData-role-tnl6812t";//Stage
        /*2*/public static String UploadURL = "https://e7bkawidfg.execute-api.us-east-2.amazonaws.com/prod/";
        /*3*/public static String FirebaseCredential = "websitecreator-94452-firebase-adminsdk-l9yoo-962812244e.json";
        /*4*/public static String FirebaseProjectId = "websitecreator-94452";
        /*5*/public static String FrontendForCORS = "http://18.222.223.99";
        /*6*/public static String AWSAccessKey = "AKIA6GFGHJFKCHWFMUWX";
        /*7*/public static String AWSSecretKey = "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";
        /*8*/public static String EC2UploadKey = "searchsystem.ppk";
#else
        //Live
        /*1*/public static String Principal = "arn:aws:iam::031023765277:role/service-role/deploayWebsiteData-role-uvadv5bp";//Live
        /*2*/public static String UploadURL = "https://u7xal5o551.execute-api.us-east-2.amazonaws.com/prod/";
        /*3*/public static String FirebaseCredential = "webcreator-dc8f8-35607d000566.json";
        /*4*/public static String FirebaseProjectId = "webcreator-dc8f8";
        /*5*/public static String FrontendForCORS = "http://3.138.165.211";
        /*6*/public static String AWSAccessKey = "AKIAQOOJJC4OURJMH4OU";
        /*7*/public static String AWSSecretKey = "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";
        /*8*/public static String EC2UploadKey = "live-article-server.ppk";
#endif
    }

    public class FirebaseSettings
    {
        [JsonPropertyName("project_id")]
        public string ProjectId => "websitecreator-94452";

        [JsonPropertyName("private_key_id")]
        public string PrivateKeyId => "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";

        // ... and so on
    }
}
