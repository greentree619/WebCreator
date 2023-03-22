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
        public static String FirebaseProjectID = "websitecreator-94452";
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
        public static String CloudFlareAPIKey = "vKQiwIFU0Eyz269KeOjBsliYyaaEQMqZRe3QA9TE";
        public static String CloudFlareAPIEmail= "uniqtop@gmail.com";
        public static String OpenAIKey = "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";

        public static String AWSAccessKey = "AKIAQOOJJC4OURJMH4OU";
        public static String AWSSecretKey = "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";
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
