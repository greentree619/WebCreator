using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    [FirestoreData]
    public class Project
    {
        public String? Id { get; set; } // firebase unique id
        [FirestoreProperty]
        public String Name { get; set; }//Domain
        [FirestoreProperty]
        public String Ip { get; set; }//Ip address
        [FirestoreProperty]
        public String Keyword { get; set; }
        [FirestoreProperty]
        public int QuesionsCount { get; set; }
        [FirestoreProperty]
        public bool OnScrapping { get; set; }
        [FirestoreProperty]
        public bool OnAFScrapping { get; set; }
        [FirestoreProperty]
        public String Language { get; set; }
        [FirestoreProperty]
        public String LanguageString { get; set; }
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
        [FirestoreProperty]
        public DateTime UpdateTime { get; set; }
    }
}
