using DataAccess.Context;
using DataAccess.Entities;

namespace Poetry.IntegrationTests.TestData
{
    public static class DatabaseSeeder
    {
        public static readonly Guid TestUserId1 = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");
        public static readonly Guid TestUserId2 = Guid.Parse("11234567-89ab-cdef-0123-456789abcdef");

        public static readonly Guid TestPoemId1 = Guid.Parse("21234567-89ab-cdef-0123-456789abcdef");
        public static readonly Guid TestPoemId2 = Guid.Parse("31234567-89ab-cdef-0123-456789abcdef");

        public static readonly Guid TestCollectionId = Guid.Parse("41234567-89ab-cdef-0123-456789abcdef");
        public static readonly Guid TestCommentId = Guid.Parse("51234567-89ab-cdef-0123-456789abcdef");

        public static void Seed(PoetryDbContext context)
        {
            // Create tags
            var tag1 = new Tag { Id = Guid.NewGuid(), Name = "Love", CreatedAt = DateTime.UtcNow };
            var tag2 = new Tag { Id = Guid.NewGuid(), Name = "Nature", CreatedAt = DateTime.UtcNow };

            context.Tags.Add(tag1);
            context.Tags.Add(tag2);

            // Create categories
            var category1 = new Category { Id = Guid.NewGuid(), Name = "Sonnet", Description = "14-line poem", CreatedAt = DateTime.UtcNow };
            var category2 = new Category { Id = Guid.NewGuid(), Name = "Haiku", Description = "Japanese poem", CreatedAt = DateTime.UtcNow };

            context.Categories.Add(category1);
            context.Categories.Add(category2);

            // Create poems
            var poem1 = new Poem
            {
                Id = TestPoemId1,
                Title = "Test Poem 1",
                Content = "This is a test poem content.",
                Excerpt = "This is a test poem excerpt.",
                AuthorId = TestUserId1,
                AuthorName = "Test User 1",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow,
                Tags = new List<Tag> { tag1 },
                Categories = new List<Category> { category1 }
            };

            var poem2 = new Poem
            {
                Id = TestPoemId2,
                Title = "Test Poem 2",
                Content = "This is another test poem content.",
                Excerpt = "This is another test poem excerpt.",
                AuthorId = TestUserId2,
                AuthorName = "Test User 2",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow,
                Tags = new List<Tag> { tag2 },
                Categories = new List<Category> { category2 }
            };

            // Create statistics for each poem
            var stats1 = new PoemStatistics
            {
                Id = Guid.NewGuid(),
                PoemId = poem1.Id,
                ViewCount = 10,
                LikeCount = 5,
                CommentCount = 2,
                SaveCount = 1,
                CreatedAt = DateTime.UtcNow,
                Poem = poem1
            };

            var stats2 = new PoemStatistics
            {
                Id = Guid.NewGuid(),
                PoemId = poem2.Id,
                ViewCount = 5,
                LikeCount = 2,
                CommentCount = 1,
                SaveCount = 0,
                CreatedAt = DateTime.UtcNow,
                Poem = poem2
            };

            poem1.Statistics = stats1;
            poem2.Statistics = stats2;

            context.Poems.Add(poem1);
            context.Poems.Add(poem2);
            context.PoemStatistics.Add(stats1);
            context.PoemStatistics.Add(stats2);

            // Create a comment on poem1
            var comment = new Comment
            {
                Id = TestCommentId,
                PoemId = poem1.Id,
                UserId = TestUserId2,
                UserName = "Test User 2",
                Text = "This is a test comment.",
                CreatedAt = DateTime.UtcNow,
                Poem = poem1
            };

            context.Comments.Add(comment);

            // Create a collection for TestUser1
            var collection = new Collection
            {
                Id = TestCollectionId,
                Name = "Test Collection",
                Description = "This is a test collection.",
                UserId = TestUserId1,
                IsPublic = true,
                PublishedPoemCount = 1,
                CreatedAt = DateTime.UtcNow
            };

            context.Collections.Add(collection);

            // Add Poem2 to the collection
            var savedPoem = new SavedPoem
            {
                Id = Guid.NewGuid(),
                CollectionId = collection.Id,
                PoemId = poem2.Id,
                SavedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Collection = collection
            };

            context.SavedPoems.Add(savedPoem);

            // Like poem1
            var like = new Like
            {
                Id = Guid.NewGuid(),
                PoemId = poem1.Id,
                UserId = TestUserId2,
                CreatedAt = DateTime.UtcNow,
                Poem = poem1
            };

            context.Likes.Add(like);

            // Create a notification
            var notification = new PoemNotification
            {
                Id = Guid.NewGuid(),
                UserId = TestUserId1,
                PoemId = poem1.Id,
                Message = "Someone liked your poem",
                IsRead = false,
                Type = DataAccess.Entities.NotificationType.NewLike,
                CreatedAt = DateTime.UtcNow
            };

            context.PoemNotifications.Add(notification);

            context.SaveChanges();
        }
    }
}