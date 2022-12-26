using Google.Cloud.Firestore;

namespace WebCreator.Models
{
    public enum SentenceVariationType
    {
        Sentence1 = 1,
        Sentence2,
        Sentence3
    };
    public enum ParagraphVariationType
    {
        Paragraph1 = 1,
        Paragraph2,
        Paragraph3
    };
    public enum ShuffleParagraphsType
    {
        Disable,
        Enable
    };
    public enum LengthType
    {
        VeryShort = 1,
        Short,
        Medium,
        Long
    };

    public enum TitleType
    {
        Diable,
        Enable
    };

    public enum QualityType
    {
        Regular = 1,
        Unique,
        VeryUnique,
        Readable,
        VeryReadable
    };

    [FirestoreData]
    public class AFSetting
    {
        public String? Id { get; set; } // firebase unique id        
        [FirestoreProperty]
        public SentenceVariationType SentenceVariation { get; set; }
        [FirestoreProperty]
        public ParagraphVariationType ParagraphVariation { get; set; }
        [FirestoreProperty]
        public ShuffleParagraphsType ShuffleParagraphs { get; set; }
        [FirestoreProperty]
        public LengthType Length { get; set; }
        [FirestoreProperty]
        public TitleType Title { get; set; }
        [FirestoreProperty]
        public float Image { get; set; }
        [FirestoreProperty]
        public float Video { get; set; }
        [FirestoreProperty]
        public QualityType Quality { get; set; }
        [FirestoreProperty]
        public DateTime UpdateTime { get; set; }
        [FirestoreProperty]
        public DateTime CreatedTime { get; set; }
    }
}