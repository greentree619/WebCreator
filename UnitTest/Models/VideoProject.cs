using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    [FirestoreData]
    public class VideoProject
    {
        public String? Id { get; set; } //Firebase unique id
        [FirestoreProperty]
        public String Name { get; set; } //Project Name
        [FirestoreProperty]
        public String YoutubeChannel { get; set; } = "";
        [FirestoreProperty]
        public int Width { get; set; } = 1280;
        [FirestoreProperty]
        public int Height { get; set; } = 720;
        [FirestoreProperty]
        public String Keyword { get; set; } = "";
        [FirestoreProperty]
        public int QuesionsCount { get; set; } = 0;
        [FirestoreProperty]
        public bool OnQueryScrapping { get; set; } = false;
        [FirestoreProperty]
        public bool OnAFScrapping { get; set; } = false;
        [FirestoreProperty]
        public bool OnOpenAIScrapping { get; set; } = false;
        [FirestoreProperty]
        public bool OnPublishSchedule { get; set; } = false;
        [FirestoreProperty]
        public List<VideoDetail>? VideoCollection { get; set; }
        [FirestoreProperty]
        public String Language { get; set; } = "en";
        [FirestoreProperty]
        public String LanguageString { get; set; } = "English";
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
        [FirestoreProperty]
        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

        public bool IsContain(String videoTitle) {
            if (VideoCollection == null) return false;

            foreach (var vd in VideoCollection) {
                if (vd.Title.CompareTo(videoTitle) == 0) return true;
            }
            return false;
        }
    }
}
