namespace WebCreator.Models
{
    public class Article
    {
        public String? Id { get; set; } // firebase unique id
        public String? ProjectId { get; set; }
        public String? Title { get; set; }

        public String? Content { get; set; }
    }
}
