using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    [FirestoreData]
    public class PublishSchedule
    {
        public String? Id { get; set; } // firebase unique id        
        [FirestoreProperty]
        public String ProjectId { get; set; }
        [FirestoreProperty]
        public int JustNowCount { get; set; }
        [FirestoreProperty]
        public int EachCount { get; set; }
        [FirestoreProperty]
        public int SpanTime { get; set; }
        [FirestoreProperty]
        public int SpanUnit { get; set; }
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
        [FirestoreProperty]
        public DateTime UpdateTime { get; set; }
    }
}
