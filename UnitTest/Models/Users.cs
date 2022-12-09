using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    [FirestoreData]
    public class Users
    {
        public String? Id { get; set; } // firebase unique id
        [FirestoreProperty]
        public String? Name { get; set; }
        [FirestoreProperty]
        public String? Email { get; set; }
        [FirestoreProperty]
        public String? Password { get; set; }        

        public String? Content { get; set; }
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
    }
}
