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
    public class CollectionRepositoryTest
    {
        private async Task<PoetryDbContext> GetDatabaseContextWithCollectionsAsync()
        {
            var options = new DbContextOptionsBuilder<PoetryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new PoetryDbContext(options);
            context.Database.EnsureCreated();

            var userId = Guid.NewGuid();
            var poemId = Guid.NewGuid();

            var collection = new Collection
            {
                Id = Guid.NewGuid(),
                Name = "My Poems",
                Description = "A collection of my favorite poems",
                UserId = userId,
                IsPublic = true,
                PublishedPoemCount = 1,
                SavedPoems = new List<SavedPoem>
            {
                new SavedPoem
                {
                    Id = Guid.NewGuid(),
                    PoemId = poemId,
                    SavedAt = DateTime.UtcNow
                }
            },
                CreatedAt = DateTime.UtcNow
            };

            context.Collections.Add(collection);
            await context.SaveChangesAsync();

            return context;
        }

        [Fact]
        public async Task Should_ReturnUserCollections_When_UserIdProvided()
        {
            // Arrange
            var context = await GetDatabaseContextWithCollectionsAsync();
            var repository = new CollectionRepository(context);
            var userId = context.Collections.First().UserId;

            // Act
            var collections = await repository.GetUserCollectionsAsync(userId);

            // Assert
            Assert.NotNull(collections);
            Assert.Single(collections);
            Assert.Equal("My Poems", collections.First().Name);
        }

        [Fact]
        public async Task Should_ReturnCollectionWithSavedPoems_When_CollectionIdProvided()
        {
            // Arrange
            var context = await GetDatabaseContextWithCollectionsAsync();
            var repository = new CollectionRepository(context);
            var collectionId = context.Collections.First().Id;

            // Act
            var collection = await repository.GetCollectionWithPoemsAsync(collectionId);

            // Assert
            Assert.NotNull(collection);
            Assert.Equal("My Poems", collection.Name);
            Assert.NotEmpty(collection.SavedPoems);
        }
    }
}
