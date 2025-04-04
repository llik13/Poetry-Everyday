using DataAccess.Entities;

public class PoemNotification : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PoemId { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; }
}
