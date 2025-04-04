using DataAccess.Context;
using DataAccess.Interfaces;
using DataAccess.Repositories;
namespace DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly PoetryDbContext _context;
        private bool _disposed = false;

        public IPoemRepository Poems { get; private set; }
        public ITagRepository Tags { get; private set; }
        public ICategoryRepository Categories { get; private set; }
        public ICommentRepository Comments { get; private set; }
        public ICollectionRepository Collections { get; private set; }
        public ILikeRepository Likes { get; private set; }
        public IPoemVersionRepository PoemVersions { get; private set; }
        public ISavedPoemRepository SavedPoems { get; private set; }
        public IPoemNotificationRepository Notifications { get; private set; }

        public UnitOfWork(
            PoetryDbContext context,
            PoemRepository poemRepository,
            TagRepository tagRepository,
            CategoryRepository categoryRepository,
            CommentRepository commentRepository,
            CollectionRepository collectionRepository,
            LikeRepository likeRepository,
            PoemVersionRepository poemVersionRepository,
            SavedPoemRepository savedPoemRepository,
            PoemNotificationRepository poemNotificationRepository
            )
        {
            _context = context;
            Poems = poemRepository;
            Tags = tagRepository;
            Categories = categoryRepository;
            Comments = commentRepository;
            Collections = collectionRepository;
            Likes = likeRepository;
            PoemVersions = poemVersionRepository;
            SavedPoems = savedPoemRepository;
            Notifications = poemNotificationRepository;
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}