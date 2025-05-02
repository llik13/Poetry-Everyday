using DataAccess.Context;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoetryTesting.Repository
{
    public class TagRepositoryTest
    {
        private async Task<PoetryDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<PoetryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new PoetryDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task Should_ReturnTag_When_TagNameExists()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var tag = new Tag { Id = Guid.NewGuid(), Name = "Nature", CreatedAt = DateTime.Now };
            context.Tags.Add(tag);
            await context.SaveChangesAsync();

            var repo = new TagRepository(context);

            // Act
            var result = await repo.GetTagByNameAsync("nature");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Nature", result.Name);
        }

        [Fact]
        public async Task Should_ReturnPopularTags_OrderedByPoemCount()
        {
            // Arrange
            var context = await GetDatabaseContext();

            var tag1 = new Tag { Id = Guid.NewGuid(), Name = "Spring", CreatedAt = DateTime.Now };
            var tag2 = new Tag { Id = Guid.NewGuid(), Name = "Winter", CreatedAt = DateTime.Now };

            tag1.Poems.Add(new Poem { Id = Guid.NewGuid(), Title = "Poem 1", CreatedAt = DateTime.Now, IsPublished = true });
            tag1.Poems.Add(new Poem { Id = Guid.NewGuid(), Title = "Poem 2", CreatedAt = DateTime.Now, IsPublished = true });
            tag2.Poems.Add(new Poem { Id = Guid.NewGuid(), Title = "Poem 3", CreatedAt = DateTime.Now, IsPublished = true });

            context.Tags.AddRange(tag1, tag2);
            context.SaveChangesAsync();

            var repo = new TagRepository(context);

            // Act
            var result = (await repo.GetPopularTagsAsync(2)).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result[0].Poems.Count);
            Assert.Equal("Spring", result[0].Name); // т.к. 2 поеми
        }


    }
}
