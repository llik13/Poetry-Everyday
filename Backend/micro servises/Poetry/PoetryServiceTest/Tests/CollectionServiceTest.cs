using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using BusinessLogic.Services;
using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using Moq;
using System.Linq.Expressions;

namespace PoetryServiceTest.Tests
{
    public class CollectionServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPoemService> _mockPoemService;
        private readonly CollectionService _collectionService;

        public CollectionServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPoemService = new Mock<IPoemService>();
            _collectionService = new CollectionService(_mockUnitOfWork.Object, _mockPoemService.Object);
        }

        [Fact]
        public async Task CreateCollectionAsync_Should_CreateCollection_And_ReturnDto()
        {
            // Arrange
            var collection = new CreateCollectionDto
            {
                Name = "Test Collection",
                Description = "Test Description",
                UserId = Guid.NewGuid(),
                IsPublic = true
            };

            Collection savedCollection = null;

            var collectionRepo = new Mock<ICollectionRepository>();
            collectionRepo.Setup(repo => repo.AddAsync(It.IsAny<Collection>()))
                          .Callback<Collection>(c => savedCollection = c)
                          .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(uow => uow.Collections).Returns(collectionRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _collectionService.CreateCollectionAsync(collection);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(collection.Name, result.Name);
            Assert.Equal(collection.Description, result.Description);
            Assert.Equal(collection.UserId, result.UserId);
            Assert.Equal(collection.IsPublic, result.IsPublic);
            Assert.Equal(0, result.PoemCount);

            
            collectionRepo.Verify(repo =>
                repo.AddAsync(It.Is<Collection>(c =>
                    c.Name == collection.Name &&
                    c.Description == collection.Description &&
                    c.UserId == collection.UserId &&
                    c.IsPublic == collection.IsPublic &&
                    c.PublishedPoemCount == 0
                )), Times.Once);

            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUserCollectionsAsync_WhenUserId_ShouldReturnCollections()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var collections = new List<Collection> {
        new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Collection 1",
            Description = "Description 1",
            UserId = userId,
            IsPublic = true,
            PublishedPoemCount = 3
        },
        new Collection
        {
            Id = Guid.NewGuid(),
            Name = "Collection 2",
            Description = "Description 2",
            UserId = userId,
            IsPublic = false,
            PublishedPoemCount = 5
        }
    };

            var collectionRepo = new Mock<ICollectionRepository>();
            collectionRepo.Setup(repo => repo.GetUserCollectionsAsync(userId))
                         .ReturnsAsync(collections);

            _mockUnitOfWork.Setup(uow => uow.Collections)
                          .Returns(collectionRepo.Object);

            // Act
            var result = await _collectionService.GetUserCollectionsAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());

            // Compare collections with returned DTOs
            var resultList = result.ToList();
            for (int i = 0; i < collections.Count; i++)
            {
                Assert.Equal(collections[i].Id, resultList[i].Id);
                Assert.Equal(collections[i].Name, resultList[i].Name);
                Assert.Equal(collections[i].Description, resultList[i].Description);
                Assert.Equal(collections[i].UserId, resultList[i].UserId);
                Assert.Equal(collections[i].IsPublic, resultList[i].IsPublic);
                Assert.Equal(collections[i].PublishedPoemCount, resultList[i].PoemCount);
            }

            // Verify repository was called with correct userId
            collectionRepo.Verify(repo => repo.GetUserCollectionsAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetCollectionAsync_WhenCollectioinAndUserId_Should_ReturnCollection()
        {
            // Arrange
            var collectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var poemIds = new List<Guid>();

            var savedPoems = new List<SavedPoem>();
            for (int i = 0; i < 3; i++)
            {
                var poemId = Guid.NewGuid();
                poemIds.Add(poemId);
                savedPoems.Add(new SavedPoem
                {
                    Id = Guid.NewGuid(),
                    CollectionId = collectionId,
                    PoemId = poemId,
                    SavedAt = DateTime.UtcNow
                });
            }

            var collection = new Collection
            {
                Id = collectionId,
                Name = "Poetry Favorites",
                Description = "My favorite poems",
                UserId = userId,
                IsPublic = true,
                PublishedPoemCount = 3,
                SavedPoems = savedPoems
            };
 
            var poemDtos = new List<PoemDto>();
            foreach (var poemId in poemIds)
            {
                poemDtos.Add(new PoemDto
                {
                    Id = poemId,
                    Title = $"Poem {poemId}",
                    Excerpt = "An excerpt",
                    Content = "Poem content",
                    AuthorId = Guid.NewGuid(),
                    AuthorName = "Poet Name",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow,
                    Statistics = new PoemStatisticsDto
                    {
                        ViewCount = 10,
                        LikeCount = 5,
                        CommentCount = 3,
                        SaveCount = 2
                    },
                    Tags = new List<string> { "nature", "love" },
                    Categories = new List<string> { "lyric" }
                });
            }

            var collectionRepo = new Mock<ICollectionRepository>();
            // Add a debug setup to see what's being called
            collectionRepo.Setup(repo => repo.GetCollectionWithPoemsAsync(collectionId))
                         .ReturnsAsync(collection);

            var poemRepo = new Mock<PoemRepository>();

            _mockUnitOfWork.Setup(uow => uow.Collections)
                  .Returns(collectionRepo.Object);

            foreach (var poemId in poemIds)
            {
                _mockPoemService.Setup(service => service.GetPoemByIdAsync(poemId))
                               .ReturnsAsync(poemDtos.First(p => p.Id == poemId));
            }

            // Act
            var result = await _collectionService.GetCollectionAsync(collectionId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(collection.Id, result.Id);
            Assert.Equal(collection.Name, result.Name);
            Assert.Equal(collection.Description, result.Description);
            Assert.Equal(collection.UserId, result.UserId);
            Assert.Equal(collection.IsPublic, result.IsPublic);
            Assert.Equal(poemDtos.Count, result.PoemCount);

            // Verify poems are included
            Assert.NotNull(result.Poems);
            Assert.Equal(poemDtos.Count, result.Poems.Count);
        }

        [Fact]
        public async Task GetCollectionAsync_When_IncorrectCollectionId_ShouldReturnNull()
        {
            // Assert
            var collectionId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var collectionRepo = new Mock<ICollectionRepository>();
            collectionRepo.Setup(repo => repo.GetCollectionWithPoemsAsync(collectionId)).ReturnsAsync((Collection)null);
            _mockUnitOfWork.Setup(uow => uow.Collections).Returns(collectionRepo.Object);

            // Act
            var result = await _collectionService.GetCollectionAsync(collectionId, userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCollectionAsync_When_InappropriateUserIdAndNotPublic()
        {
            // Assert
            var userId = Guid.NewGuid();
            var collectionId = Guid.NewGuid();

            var collection = new Collection
            {
                Id = collectionId,
                Name = "Poetry Favorites",
                Description = "My favorite poems",
                UserId = Guid.NewGuid(),
                IsPublic = false,
                PublishedPoemCount = 0,
            };

            var collectionRepo = new Mock<ICollectionRepository>();
            collectionRepo.Setup(repo => repo.GetCollectionWithPoemsAsync(collectionId)).ReturnsAsync(collection);
            _mockUnitOfWork.Setup(uow => uow.Collections).Returns(collectionRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _collectionService.GetCollectionAsync(collectionId, userId));

        }

        [Fact]
        public async Task AddPoemToCollectionAsync_ShouldAddPoemToCollection_WhenUserIdIsValid()
        {
            // Assert
            Guid userId = Guid.NewGuid();
            Guid collectionId = Guid.NewGuid();
            Guid poemId = Guid.NewGuid();

            Collection collection = new Collection
            {
                Id = collectionId,
                Name = "Poetry Favorites",
                Description = "My favorite poems",
                UserId = Guid.NewGuid(),
                IsPublic = false,
                PublishedPoemCount = 0,
            };

            Poem poem = new Poem
            {
                Id = poemId,
                Title = $"Poem {poemId}",
                Excerpt = "An excerpt",
                Content = "Poem content",
                AuthorId = Guid.NewGuid(),
                AuthorName = "Poet Name",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow,
                Statistics = new PoemStatistics
                {
                    ViewCount = 10,
                    LikeCount = 5,
                    CommentCount = 3,
                    SaveCount = 2
                }
            };

            SavedPoem savedPoem = new SavedPoem
            {
                CollectionId = collectionId,
                PoemId = poemId,
                SavedAt = DateTime.UtcNow
            };

            PoemNotification notification = new PoemNotification
            {
                UserId = poem.AuthorId,
                PoemId = poem.Id,
                Message = $"Someone saved your poem \"{poem.Title}\" to their collection",
                Type = NotificationType.PoemSaved,
                IsRead = false
            };

            var collectionRepo = new Mock<ICollectionRepository>();
            collectionRepo.Setup(repo => repo.GetByIdAsync(collectionId)).ReturnsAsync(collection);

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId)).ReturnsAsync(poem);

            var savedPoemRepo = new Mock<ISavedPoemRepository>();
            savedPoemRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<SavedPoem, bool>>>())).ReturnsAsync(() => null);
            savedPoemRepo.Setup(repo => repo.AddAsync(savedPoem));

            var notificationRepo = new Mock<IPoemNotificationRepository>();
            notificationRepo.Setup(repo => repo.AddAsync(notification));

            _mockUnitOfWork.Setup(uow => uow.Collections).Returns(collectionRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.SavedPoems).Returns(savedPoemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Notifications).Returns(notificationRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = _collectionService.AddPoemToCollectionAsync(collectionId, poemId, userId);

            // Assert
            Assert.Equal(result.Result, true);
        }
    }
}