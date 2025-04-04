namespace DataAccess.Entities
{
    public class Poem : BaseEntity
    {
        public string Title { get; set; }
        public string Excerpt { get; set; }
        public string Content { get; set; } // Full poem content stored directly in DB
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
        public bool IsPublished { get; set; }
        public PoemStatistics Statistics { get; set; }
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<PoemVersion> Versions { get; set; } = new List<PoemVersion>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }

}
