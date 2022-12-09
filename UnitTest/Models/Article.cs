using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    [FirestoreData]
    public class Article
    {
        public String? Id { get; set; } // firebase unique id        
        [FirestoreProperty]
        public String? ProjectId { get; set; }
        [FirestoreProperty]
        public String? UserId { get; set; }
        [FirestoreProperty]
        public String? ArticleId { get; set; }
        [FirestoreProperty]
        public Int32 Progress { get; set; }
        [FirestoreProperty]
        public String? Title { get; set; }
        [FirestoreProperty]

        public String? Content { get; set; }
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
    }
}
