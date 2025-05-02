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
    public class CategoryRepositoryTest
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

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Spring",
                Description = "Poems about spring season",
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }


        [Fact]
        public async Task Should_ReturnCategory_When_PassName()
        {
            // Arrange
            var context = await GetDatabaseContext();
            SeedDatabaseAsync(context);
            var repo = new CategoryRepository(context);

            // Act
            var result = await repo.GetCategoryByNameAsync("Spring");
             
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Spring", result.Name);
        }

        [Fact]
        public async Task Should_ReturnNull_When_CategoryNotFound()
        {
            // Arrange
            var context = await GetDatabaseContext();
            var repo = new CategoryRepository(context);

            // Act
            var result = await repo.GetCategoryByNameAsync("NonExistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Should_FindCategory_IgnoringCase()
        {
            // Arrange
            var context = await GetDatabaseContext();
            SeedDatabaseAsync(context);
            var repo = new CategoryRepository(context);

            // Act
            var result = await repo.GetCategoryByNameAsync("SpRIng");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Spring", result.Name);
        }


    }
}
