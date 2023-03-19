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
        public Boolean IsScrapping { get; set; }
        [FirestoreProperty]
        public String? Title { get; set; }
        [FirestoreProperty]
        public String? MetaTitle { get; set; }
        [FirestoreProperty]
        public String? MetaDescription { get; set; }
        [FirestoreProperty]
        public String? MetaKeywords { get; set; }
        [FirestoreProperty]
        public String? MetaAuthor { get; set; }
        [FirestoreProperty]
        public String? Content { get; set; }
        [FirestoreProperty]
        public String? Footer { get; set; }
        [FirestoreProperty]
        public List<String>? ImageArray { get; set; }
        [FirestoreProperty]
        public List<String>? ThumbImageArray { get; set; }
        [FirestoreProperty]
        public int State { get; set; }//0-Unknow, 1-UnApproved, 2-Approved , 3-Online, 4 - Delete
        [FirestoreProperty]
        public DateTime UpdateTime { get; set; }
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
    }
}
