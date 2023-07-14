using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    [FirestoreData]
    public class VideoDetail
    {
        public String? Id { get; set; } // firebase unique id      
        [FirestoreProperty]
        public Int32 Progress { get; set; }
        [FirestoreProperty]
        public Boolean IsScrapping { get; set; }
        [FirestoreProperty]
        public String? Title { get; set; }
        [FirestoreProperty]
        public String? Script { get; set; }
        [FirestoreProperty]
        public String? BackgroundImage { get; set; }
        [FirestoreProperty]
        public String? BackgroundThumbImage { get; set; }
        [FirestoreProperty]
        public String? awsVideoLink { get; set; }
        [FirestoreProperty]
        public String? youtubeVideoLink { get; set; }
        [FirestoreProperty]
        public int State { get; set; }//0-Unknow, 1-UnApproved, 2-Approved , 3-Online, 4 - Delete
        [FirestoreProperty]
        public DateTime UpdateTime { get; set; }
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
        
    }
}
