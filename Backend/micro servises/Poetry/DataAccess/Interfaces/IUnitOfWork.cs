namespace DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IPoemRepository Poems { get; }
        ITagRepository Tags { get; }
        ICategoryRepository Categories { get; }
        ICommentRepository Comments { get; }
        ICollectionRepository Collections { get; }
        ILikeRepository Likes { get; }
        ISavedPoemRepository SavedPoems { get; }
        IPoemNotificationRepository Notifications { get; }
        Task<int> CompleteAsync();
    }

}
