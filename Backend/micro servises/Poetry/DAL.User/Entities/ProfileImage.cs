namespace DAL.User.Entities
{
    public class ProfileImage
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public byte[] ImageData { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ApplicationUser User { get; set; }
    }
}