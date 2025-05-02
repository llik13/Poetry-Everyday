using DataAccess.Context;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;

namespace PoetryTesting.Repository
{
    public class PoemRepositoryTest
    {
        private async Task<PoetryDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<PoetryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new PoetryDbContext(options);
            context.Database.EnsureCreated();

            if (!await context.Poems.AnyAsync())
            {
                var category = new Category { Name = "Nature", Description = "Poems about nature" };
                var tag = new Tag { Name = "Spring" };

                var poem = new Poem
                {
                    Id = Guid.NewGuid(),
                    Title = "The Blossoming Tree",
                    Excerpt = "A tree in bloom...",
                    Content = "A tree in bloom with flowers bright...",
                    AuthorId = Guid.NewGuid(),
                    AuthorName = "John Doe",
                    IsPublished = true,
                    CreatedAt = DateTime.Now,
                    Statistics = new PoemStatistics
                    {
                        ViewCount = 120,
                        LikeCount = 10,
                        CommentCount = 2
                    },
                    Tags = new List<Tag> { tag },
                    Categories = new List<Category> { category },
                    Comments = new List<Comment>
            {
                new Comment
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    UserName = "Alice",
                    Text = "Beautiful imagery!",
                    CreatedAt = DateTime.Now
                },
                new Comment
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.NewGuid(),
                    UserName = "Bob",
                    Text = "Really touched me.",
                    CreatedAt = DateTime.Now
                }
            },
                    Likes = new List<Like>
            {
                new Like { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CreatedAt = DateTime.Now }
            }
                };

                context.Poems.Add(poem);
                await context.SaveChangesAsync();
            }

            return context;
        }

        [Fact]
        public async Task Should_ReturnFullPoem_WhenPassingAuthorId()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var repo = new PoemRepository(context);

            var targetAuthorId = Guid.NewGuid();
            var poemByAuthor = new Poem
            {
                Id = Guid.NewGuid(),
                Title = "By Author",
                Excerpt = "Excerpt 1",
                Content = "Content 1",
                AuthorId = targetAuthorId,
                AuthorName = "Author A",
                IsPublished = true,
                CreatedAt = DateTime.Now,
                Statistics = new PoemStatistics(),
                Tags = new List<Tag> { new Tag { Name = "Test" } },
                Categories = new List<Category> { new Category { Name = "TestCat", Description = "desc" } }
            };

            var otherPoem = new Poem
            {
                Id = Guid.NewGuid(),
                Title = "Not by Author",
                Excerpt = "Excerpt 2",
                Content = "Content 2",
                AuthorId = Guid.NewGuid(),
                AuthorName = "Author B",
                IsPublished = true,
                CreatedAt = DateTime.Now.AddHours(-1),
                Statistics = new PoemStatistics()
            };

            context.Poems.AddRange(poemByAuthor, otherPoem);
            await context.SaveChangesAsync();

            // Act
            var result = await repo.GetPoemsByAuthorIdAsync(targetAuthorId);

            // Assert
            Assert.Single(result);
            Assert.Equal(poemByAuthor.Id, result.First().Id);
        }

        [Fact]
        public async Task Should_ReturnFullPoem_When_GetPoemsByTagsAsync()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var repo = new PoemRepository(context);
            var tagList = new List<string> { "Spring" };

            // Act
            var result = await repo.GetPoemsByTagsAsync(tagList);

            // Assert
            Assert.NotNull(result);
            var poem = result.FirstOrDefault();
            Assert.NotNull(poem);
            Assert.True(poem.Tags.Any(t => tagList.Contains(t.Name)));
            Assert.True(poem.IsPublished);
            Assert.NotNull(poem.Statistics);
            Assert.NotEmpty(poem.Categories);
        }

        [Fact]
        public async Task Should_ReturnFullPoem_When_GetPoemsByCategoryAsync()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var repo = new PoemRepository(context);

            // Act
            var result = await repo.GetPoemsByCategoryAsync("Nature");

            // Assert
            Assert.NotNull(result);
            var poem = result.FirstOrDefault();
            Assert.True(poem.Categories.Any(c => c.Name.Equals("Nature")));
            Assert.True(poem.IsPublished);
            Assert.NotNull(poem.Statistics);
            Assert.NotEmpty(poem.Categories);
        }

        [Fact]
        public async Task Should_ReturnPoemWithDetails_When_PassId()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var repo = new PoemRepository(context);
            var testId = Guid.NewGuid();
            var fullPoem = new Poem
            {
                Id = testId,
                Title = "By Author",
                Excerpt = "Excerpt 1",
                Content = "Content 1",
                AuthorId = Guid.NewGuid(),
                AuthorName = "Author A",
                IsPublished = true,
                CreatedAt = DateTime.Now,
                Statistics = new PoemStatistics(),
                Tags = new List<Tag> { new Tag { Name = "Test" } },
                Categories = new List<Category> { new Category { Name = "TestCat", Description = "desc" } }
            };
            context.Poems.Add(fullPoem);
            context.SaveChanges();

            //Act
            var result = await repo.GetPoemWithDetailsAsync(testId);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(testId, result.Id);
            Assert.NotNull(result.Statistics);
            Assert.NotNull(result.Tags);
            Assert.NotNull(result.Categories);
            Assert.NotEmpty(result.Categories);
        }

        [Fact]
        public async Task Should_ReturnFilteredPagedSortedPoems_When_SearchPoemsAsync()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var repo = new PoemRepository(context);

            string searchTerm = "blossom";      // слово з Title або Content
            int pageNumber = 1;
            int pageSize = 5;
            string sortBy = "createdAt";
            bool sortDescending = true;
            bool isPublished = true;

            // Act
            var (poems, totalCount) = await repo.SearchPoemsAsync(
                searchTerm,
                pageNumber,
                pageSize,
                sortBy,
                sortDescending,
                isPublished);

            // Assert
            Assert.NotNull(poems);
            Assert.True(totalCount >= poems.Count());
            Assert.All(poems, poem =>
            {
                Assert.True(poem.IsPublished);
                Assert.True(
                    poem.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    poem.Excerpt.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    poem.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    poem.AuthorName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    poem.Tags.Any(t => t.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                );
            });
        }

        [Fact]
        public async Task Should_GetCountOfPoems_When_PassSearchTerm()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var repo = new PoemRepository(context);

            // Act
            var count = await repo.GetTotalPoemsCountAsync("bloom");

            // Assert
            Assert.True(count > 0);
            Assert.Equal(1, count);
        }
    }
}
