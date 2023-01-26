using Google.Cloud.Firestore;
using OpenAI_API;

namespace WebCreator.Models
{
    [FirestoreData]
    public class OpenAISetting
    {
        public String? Id { get; set; } // firebase unique id
        [FirestoreProperty]
        public String Model { get; set; }
        [FirestoreProperty]
        public String Prompt { get; set; }
        [FirestoreProperty]
        public int MaxTokens { get; set; }
        [FirestoreProperty]
        public float Temperature { get; set; }
        [FirestoreProperty]
        public float TopP { get; set; }
        [FirestoreProperty]
        public int N { get; set; }
        //[FirestoreProperty]
        //public Int32 Logprobs { get; set; }
        [FirestoreProperty]
        public float PresencePenalty { get; set; }
        [FirestoreProperty]
        public float FrequencyPenalty { get; set; }
        [FirestoreProperty]
        public DateTime UpdateTime { get; set; }
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
    }
}