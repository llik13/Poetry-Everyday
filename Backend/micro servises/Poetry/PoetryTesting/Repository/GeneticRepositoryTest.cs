
using DataAccess.Context;
using DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Validations;
using PoetryTesting.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PoetryTesting.Repository
{
    public class GeneticRepositoryTest
    {
        private DbContextOptions<TestDbContext> GetInMemoryOptions(string dbName)
        {
            return new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
        }

        private async Task SeedTestData(TestDbContext context)
        {
            context.AddRange(new[]
            {
                new TestEntity { Id = Guid.NewGuid(), CreatedAt = DateTime.Now, IsDeleted = false },
                new TestEntity { Id = Guid.NewGuid(), CreatedAt = DateTime.Now, IsDeleted = true }
            });

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntities()
        {
            var options = GetInMemoryOptions("GetAllTestDb");

            using(var context = new TestDbContext(options))
            {
                await SeedTestData(context);

                var repo = new GenericRepository<TestEntity>(context);
                var result = await repo.GetAllAsync();

                Assert.Equal(2, result.Count());
            }
        }

        [Fact]
        public async Task FindAsync_CheckCorrectReturn()
        {
            var options = GetInMemoryOptions("FindAsyncTestDb");
          
            using (var context = new TestDbContext(options))
            {
                await SeedTestData(context);
           
                var repo = new GenericRepository<TestEntity>(context);
                var result = await repo.FindAsync(e => e.IsDeleted == true);

                var deleted = result.First();
                Assert.True(deleted.IsDeleted);
            }
        }


        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectEntity()
        {
            var options = GetInMemoryOptions("GetByIdTestDb");

            var testId = Guid.NewGuid();

            using (var context = new TestDbContext(options))
            {
                context.TestEntities.AddRange(new[]
                {
                    new TestEntity { Id = testId, CreatedAt = DateTime.Now, IsDeleted = false },
                    new TestEntity { Id = Guid.NewGuid(), CreatedAt = DateTime.Now, IsDeleted = true }
                });

                await context.SaveChangesAsync();
                var repo = new GenericRepository<TestEntity>(context);
                var result = await repo.GetByIdAsync(testId);

                Assert.NotNull(result);
                Assert.Equal(testId, result.Id);
            }
        }

        [Fact]
        public async Task Should_AddEntityToDatabase_When_AddAsync()
        {
            var options = GetInMemoryOptions("AddToTestDb");

            var testId = Guid.NewGuid();

            using (var context = new TestDbContext(options))
            {
                var entity = new TestEntity { Id = testId, CreatedAt = DateTime.Now, IsDeleted = false };
                var repo = new GenericRepository<TestEntity>(context);

                await repo.AddAsync(entity);
                await context.SaveChangesAsync();

                var addedEntity = await context.TestEntities.FindAsync(testId);
                Assert.NotNull(addedEntity);
                Assert.Equal(testId, addedEntity.Id);
            }
        }

        [Fact]
        public async Task Should_UpdateEntity_When_Update()
        {
            var options = GetInMemoryOptions("UpdateAsyncTestDb");
            var testId = Guid.NewGuid();

            using (var context = new TestDbContext(options))
            {
                context.TestEntities.Add(new TestEntity
                {
                    Id = testId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                });
                await context.SaveChangesAsync();

                var repo = new GenericRepository<TestEntity>(context);
                var entity = await repo.GetByIdAsync(testId);
                entity.IsDeleted = true;
                repo.Update(entity);
                await context.SaveChangesAsync();
    

                var updatedEntity = await context.TestEntities.FindAsync(testId);


                Assert.True(updatedEntity.IsDeleted);
            }
        }

        [Fact]
        public async Task Should_DeleteEntoty_When_Remove()
        {

            var options = GetInMemoryOptions("UpdateAsyncTestDb");
            var testId = Guid.NewGuid();

            using (var context = new TestDbContext(options))
            {
                context.TestEntities.Add(new TestEntity
                {
                    Id = testId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                });
                await context.SaveChangesAsync();

                var repo = new GenericRepository<TestEntity>(context);
                var entity = await repo.GetByIdAsync(testId);

                repo.Remove(entity);
                await context.SaveChangesAsync();

                var result = await repo.GetByIdAsync(testId);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task Should_SoftDeleteEntity_When_SoftDeleteCalled()
        {
            var options = GetInMemoryOptions("SoftDeleteTestDb");
            var testId = Guid.NewGuid();

            using (var context = new TestDbContext(options))
            {
                context.TestEntities.Add(new TestEntity
                {
                    Id = testId,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                });
                await context.SaveChangesAsync();
            }

            using (var context = new TestDbContext(options))
            {
                var repo = new GenericRepository<TestEntity>(context);
                var entity = await repo.GetByIdAsync(testId);

                repo.SoftDelete(entity);
                await context.SaveChangesAsync();
            }

            using (var context = new TestDbContext(options))
            {
                var repo = new GenericRepository<TestEntity>(context);
                var result = await repo.GetByIdAsync(testId);

                Assert.NotNull(result);
                Assert.True(result.IsDeleted);
            }
        }
    }
}
