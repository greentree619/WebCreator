using FireSharp.Config;
using FireSharp.Interfaces;

namespace WebCreator
{
    public static class Config
    {
        //public static KeyValuePair<string, object> Application { get; set; }
        public static String FirebaseAuthSecret = "5cGJIcs3WRWYyu8XTFyjy8zW87Ql2ey44IOZvIRO";
        public static String FirebaseBasePath = "https://websitecreator-94452-default-rtdb.firebaseio.com";
        public static String FirebaseProjectID = "websitecreator-94452";
        public static IFirebaseConfig firebaseConfig = new FirebaseConfig
        {
            AuthSecret = Config.FirebaseAuthSecret,
            BasePath = Config.FirebaseBasePath
        };

        public static String ArticleForgeKey = "b6391ee8e9105e4e26042008edf03e62";
        public static String SerpApiKey = "e98e32704f4642aa6fbf1cf0086aed75474dab0b0d1f88e2c0a68266bab57b2f";
        public static String DeepLKey = "37f5f472-d0f2-44c2-3a5d-7896694f8cdc";
        public static String PixabayKey = "27944002-ca9bbda02c769f32ad5769e81";
    }
}
