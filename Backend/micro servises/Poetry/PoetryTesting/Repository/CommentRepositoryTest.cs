using DataAccess.Context;
using DataAccess.Entities;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PoetryTesting.Repository
{
    public class CommentRepositoryTest
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

        private async Task SeedDatabaseAsync(PoetryDbContext context)
        {
            if (await context.Categories.AnyAsync())
                return;

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                UserName = "Uncle Bob",
                Text = "Hi Bob",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                Poem = new Poem { Id = Guid.NewGuid()}
            };

            context.Comments.Add(comment);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Should_ReturnComment_When_PoemIdPass()
        {
            // Arrange
            var context = await GetDatabaseContext();
            SeedDatabaseAsync(context);
            var repo = new CommentRepository(context);
            var poemId = context.Comments.First().PoemId;

            // Act
            var result = (await repo.GetCommentsByPoemIdAsync(poemId, 1, 5)).ToList();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.True(result.All(c => c.PoemId == poemId));
            Assert.True(result.Count <= 5);
            Assert.True(result.SequenceEqual(result.OrderByDescending(c => c.CreatedAt)));
        }

    }
}
