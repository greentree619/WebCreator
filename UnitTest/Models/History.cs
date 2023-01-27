using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    [FirestoreData]
    public class History
    {
        public String? Id { get; set; } // firebase unique id
        [FirestoreProperty]
        public String DomainID { get; set; }//same with projectID
        [FirestoreProperty]
        public String Category { get; set; }//Keyword, Theme
        [FirestoreProperty]
        public String Log { get; set; }

        //[FirestoreProperty]
        //public MetaInfoBase MetaInfo { get; set; }

        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
    }
}
