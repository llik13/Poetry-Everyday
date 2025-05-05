namespace BusinessLogic.DTOs
{
    public class CollectionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid UserId { get; set; }
        public bool IsPublic { get; set; }
        public int PoemCount { get; set; }
    }

}
