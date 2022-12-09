namespace WebCreator.Models
{
    public class Project
    {
        public String? Id { get; set; } // firebase unique id
        public String Name { get; set; }
        public String Keyword { get; set; }
        public int QuesionsCount { get; set; }
        public String Language { get; set; }
    }
}
