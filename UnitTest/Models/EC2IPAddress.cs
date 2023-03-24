using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    [FirestoreData]
    public class EC2IPAddress
    {
        public String? Id { get; set; } // firebase unique id
        [FirestoreProperty]
        public String IPAddress { get; set; }
        [FirestoreProperty]
        public String? ProjectId { get; set; }

        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
    }
}
