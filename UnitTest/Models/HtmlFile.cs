using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    [FirestoreData]
    public class HtmlFile
    {
        public String? Id { get; set; } // firebase unique id
        [FirestoreProperty]
        public String? FileName { get; set; }
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
        [FirestoreProperty]
        public DateTime ModifiedTime { get; set; }
    }
}
