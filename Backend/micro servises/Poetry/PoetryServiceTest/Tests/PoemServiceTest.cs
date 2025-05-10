using BusinessLogic.DTOs;
using BusinessLogic.Services;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PoetryServiceTest.Tests
{
    public class PoemServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly PoemService _poemService;

        public PoemServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _poemService = new PoemService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetPoemByIdAsync_ShouldReturnPoemDTO()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var authorId = Guid.NewGuid();

            var poem = new Poem
            {
                Id = poemId,
                Title = "The Raven",
                Excerpt = "Once upon a midnight dreary...",
                Content = "Once upon a midnight dreary, while I pondered, weak and weary...",
                AuthorId = authorId,
                AuthorName = "Edgar Allan Poe",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                Statistics = new PoemStatistics
                {
                    ViewCount = 100,
                    LikeCount = 50,
                    CommentCount = 10,
                    SaveCount = 5
                },
                Tags = new List<Tag>
                {
                    new Tag { Name = "Gothic" },
                    new Tag { Name = "Horror" }
                },
                Categories = new List<Category>
                {
                    new Category { Name = "Classic" }
                }
            };

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetByIdAsync(poemId))
                .ReturnsAsync(poem);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);

            // Act
            var result = await _poemService.GetPoemByIdAsync(poemId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(poemId, result.Id);
            Assert.Equal("The Raven", result.Title);
            Assert.Equal("Once upon a midnight dreary...", result.Excerpt);
            Assert.Equal("Once upon a midnight dreary, while I pondered, weak and weary...", result.Content);
            Assert.Equal(authorId, result.AuthorId);
            Assert.Equal("Edgar Allan Poe", result.AuthorName);
            Assert.True(result.IsPublished);
            Assert.Equal(poem.CreatedAt, result.CreatedAt);
            Assert.Equal(poem.UpdatedAt, result.UpdatedAt);

            // Check statistics
            Assert.Equal(100, result.Statistics.ViewCount);
            Assert.Equal(50, result.Statistics.LikeCount);
            Assert.Equal(10, result.Statistics.CommentCount);
            Assert.Equal(5, result.Statistics.SaveCount);

            // Check tags and categories
            Assert.Equal(2, result.Tags.Count);
            Assert.Contains("Gothic", result.Tags);
            Assert.Contains("Horror", result.Tags);
            Assert.Single(result.Categories);
            Assert.Contains("Classic", result.Categories);
        }

        [Fact]
        public async Task GetPoemByIdAsync_WhenPoemNotFound_ShouldReturnNull()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetByIdAsync(poemId))
                .ReturnsAsync((Poem)null);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);

            // Act
            var result = await _poemService.GetPoemByIdAsync(poemId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPoemDetailsAsync_ShouldReturnPoemDetailsDTO()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var poem = new Poem
            {
                Id = poemId,
                Title = "The Raven",
                Excerpt = "Once upon a midnight dreary...",
                Content = "Once upon a midnight dreary, while I pondered, weak and weary...",
                AuthorId = Guid.NewGuid(),
                AuthorName = "Edgar Allan Poe",
                IsPublished = true,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5),
                Statistics = new PoemStatistics
                {
                    ViewCount = 100,
                    LikeCount = 50,
                    CommentCount = 10,
                    SaveCount = 5
                },
                Tags = new List<Tag>
                {
                    new Tag { Name = "Gothic" },
                    new Tag { Name = "Horror" }
                },
                Categories = new List<Category>
                {
                    new Category { Name = "Classic" }
                },
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.NewGuid(),
                        UserName = "Reader1",
                        Text = "Brilliant work!",
                        CreatedAt = DateTime.UtcNow.AddDays(-3)
                    }
                }
            };

            var likes = new List<Like>
            {
                new Like
                {
                    Id = Guid.NewGuid(),
                    PoemId = poemId,
                    UserId = userId
                }
            };

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId))
                .ReturnsAsync(poem);

            var likeRepo = new Mock<ILikeRepository>();
            likeRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Like, bool>>>()))
                .ReturnsAsync(likes);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Likes).Returns(likeRepo.Object);

            // Act
            var result = await _poemService.GetPoemDetailsAsync(poemId, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(poemId, result.Id);
            Assert.Equal("The Raven", result.Title);
            Assert.True(result.IsLikedByCurrentUser);
            Assert.Single(result.Comments);
            Assert.Equal("Reader1", result.Comments.First().UserName);
            Assert.Equal("Brilliant work!", result.Comments.First().Text);
        }

        [Fact]
        public async Task CreatePoemAsync_ShouldCreateNewPoemWithTagsAndCategories()
        {
            // Arrange
            var authorId = Guid.NewGuid();
            var newPoemId = Guid.NewGuid();

            var createPoemDto = new CreatePoemDto
            {
                Title = "New Poem",
                Excerpt = "An excerpt...",
                Content = "Full poem content...",
                AuthorId = authorId,
                AuthorName = "Poet",
                IsPublished = false,
                Tags = new List<string> { "Nature", "Life" },
                Categories = new List<string> { "Modern" }
            };

            var savedPoem = new Poem
            {
                Id = newPoemId,
                Title = createPoemDto.Title,
                Excerpt = createPoemDto.Excerpt,
                Content = createPoemDto.Content,
                AuthorId = createPoemDto.AuthorId,
                AuthorName = createPoemDto.AuthorName,
                IsPublished = createPoemDto.IsPublished,
                CreatedAt = DateTime.UtcNow,
                Statistics = new PoemStatistics(),
                Tags = new List<Tag>(),
                Categories = new List<Category>()
            };

            var tagRepo = new Mock<ITagRepository>();
            tagRepo.Setup(repo => repo.GetTagByNameAsync("Nature"))
                .ReturnsAsync((Tag)null); // Tag doesn't exist yet
            tagRepo.Setup(repo => repo.GetTagByNameAsync("Life"))
                .ReturnsAsync(new Tag { Name = "Life" }); // Tag already exists

            var categoryRepo = new Mock<ICategoryRepository>();
            categoryRepo.Setup(repo => repo.GetCategoryByNameAsync("Modern"))
                .ReturnsAsync((Category)null); // Category doesn't exist yet

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.AddAsync(It.IsAny<Poem>()))
                .Callback<Poem>(p =>
                {
                    p.Id = newPoemId;
                    p.CreatedAt = DateTime.UtcNow;
                    p.Statistics = new PoemStatistics();
                })
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(uow => uow.Tags).Returns(tagRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Categories).Returns(categoryRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _poemService.CreatePoemAsync(createPoemDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newPoemId, result.Id);
            Assert.Equal("New Poem", result.Title);
            Assert.Equal("An excerpt...", result.Excerpt);
            Assert.Equal("Full poem content...", result.Content);
            Assert.Equal(authorId, result.AuthorId);
            Assert.Equal("Poet", result.AuthorName);
            Assert.False(result.IsPublished);

            // Verify tags and categories
            Assert.Equal(2, result.Tags.Count);
            Assert.Contains("Nature", result.Tags);
            Assert.Contains("Life", result.Tags);
            Assert.Single(result.Categories);
            Assert.Contains("Modern", result.Categories);

            // Verify repository calls
            poemRepo.Verify(repo => repo.AddAsync(It.IsAny<Poem>()), Times.Once);
            tagRepo.Verify(repo => repo.GetTagByNameAsync("Nature"), Times.Once);
            tagRepo.Verify(repo => repo.GetTagByNameAsync("Life"), Times.Once);
            categoryRepo.Verify(repo => repo.GetCategoryByNameAsync("Modern"), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task UpdatePoemAsync_ShouldUpdateExistingPoem()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var authorId = Guid.NewGuid();

            var updatePoemDto = new UpdatePoemDto
            {
                Id = poemId,
                Title = "Updated Title",
                Excerpt = "Updated excerpt...",
                Content = "Updated content...",
                IsPublished = true,
                Tags = new List<string> { "Updated" },
                Categories = new List<string> { "NewCategory" },
                ChangeNotes = "Made some updates"
            };

            // Make sure to initialize all collections properly
            var existingPoem = new Poem
            {
                Id = poemId,
                Title = "Original Title",
                Excerpt = "Original excerpt...",
                Content = "Original content...",
                AuthorId = authorId,
                AuthorName = "Poet",
                IsPublished = false,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = null,
                Statistics = new PoemStatistics
                {
                    ViewCount = 5,
                    LikeCount = 2,
                    CommentCount = 1,
                    SaveCount = 0
                },
                Tags = new List<Tag> { new Tag { Name = "Original", Id = Guid.NewGuid() } },
                Categories = new List<Category> { new Category { Name = "OldCategory", Id = Guid.NewGuid() } }
            };

            var tag = new Tag { Name = "Updated", Id = Guid.NewGuid() };
            var category = new Category { Name = "NewCategory", Id = Guid.NewGuid(), Description = "New Category Description" };

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId))
                .ReturnsAsync(existingPoem);

            var tagRepo = new Mock<ITagRepository>();
            tagRepo.Setup(repo => repo.GetTagByNameAsync("Updated"))
                .ReturnsAsync(tag);

            var categoryRepo = new Mock<ICategoryRepository>();
            categoryRepo.Setup(repo => repo.GetCategoryByNameAsync("NewCategory"))
                .ReturnsAsync((Category)null); // Category doesn't exist yet
            categoryRepo.Setup(repo => repo.AddAsync(It.IsAny<Category>()))
                .Callback<Category>(c => { c.Id = Guid.NewGuid(); })
                .Returns(Task.CompletedTask);

            // Create saved poems for testing
            var savedPoemRepo = new Mock<ISavedPoemRepository>();
            savedPoemRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<SavedPoem, bool>>>()))
                .ReturnsAsync(new List<SavedPoem>());

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Tags).Returns(tagRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.Categories).Returns(categoryRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.SavedPoems).Returns(savedPoemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _poemService.UpdatePoemAsync(updatePoemDto, authorId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(poemId, result.Id);
            Assert.Equal("Updated Title", result.Title);
            Assert.Equal("Updated excerpt...", result.Excerpt);
            Assert.Equal("Updated content...", result.Content);
            Assert.True(result.IsPublished);

            // Verify tags and categories
            Assert.Single(result.Tags);
            Assert.Contains("Updated", result.Tags);
            Assert.Single(result.Categories);
            Assert.Contains("NewCategory", result.Categories);

            // Verify repository calls
            poemRepo.Verify(repo => repo.Update(It.Is<Poem>(p =>
                p.Id == poemId &&
                p.Title == "Updated Title" &&
                p.IsPublished == true)), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.AtLeastOnce);
        }

        [Fact]
        public async Task UpdatePoemAsync_WhenNotAuthor_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid(); // Different from author ID

            var updatePoemDto = new UpdatePoemDto
            {
                Id = poemId,
                Title = "Updated Title",
                Excerpt = "Updated excerpt...",
                Content = "Updated content...",
                IsPublished = true,
                Tags = new List<string>(),
                Categories = new List<string>()
            };

            var existingPoem = new Poem
            {
                Id = poemId,
                Title = "Original Title",
                AuthorId = authorId, // Different from currentUserId
                IsPublished = false
            };

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId))
                .ReturnsAsync(existingPoem);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _poemService.UpdatePoemAsync(updatePoemDto, currentUserId));
        }

        [Fact]
        public async Task DeletePoemAsync_ShouldDeletePoem()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var authorId = Guid.NewGuid();

            var poem = new Poem
            {
                Id = poemId,
                Title = "Poem to Delete",
                AuthorId = authorId,
                IsPublished = true
            };

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetByIdAsync(poemId))
                .ReturnsAsync(poem);

            var savedPoemRepo = new Mock<ISavedPoemRepository>();
            savedPoemRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<SavedPoem, bool>>>()))
                .ReturnsAsync(new List<SavedPoem>());

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.SavedPoems).Returns(savedPoemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _poemService.DeletePoemAsync(poemId, authorId);

            // Assert
            Assert.True(result);

            // Verify poem was deleted
            poemRepo.Verify(repo => repo.Remove(poem), Times.Once);

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task DeletePoemAsync_WhenNotAuthor_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid(); // Different from author ID

            var poem = new Poem
            {
                Id = poemId,
                Title = "Poem to Delete",
                AuthorId = authorId, // Different from currentUserId
                IsPublished = true
            };

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetByIdAsync(poemId))
                .ReturnsAsync(poem);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _poemService.DeletePoemAsync(poemId, currentUserId));
        }

        [Fact]
        public async Task IncrementViewCountAsync_ShouldIncrementCount()
        {
            // Arrange
            var poemId = Guid.NewGuid();

            var poem = new Poem
            {
                Id = poemId,
                Title = "Popular Poem",
                Statistics = new PoemStatistics
                {
                    ViewCount = 99
                }
            };

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetPoemWithDetailsAsync(poemId))
                .ReturnsAsync(poem);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _poemService.IncrementViewCountAsync(poemId);

            // Assert
            Assert.True(result);
            Assert.Equal(100, poem.Statistics.ViewCount);

            // Verify poem was updated
            poemRepo.Verify(repo => repo.Update(poem), Times.Once);

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }

        [Fact]
        public async Task SearchPoemsAsync_ShouldReturnFilteredPoems()
        {
            // Arrange
            var searchDto = new PoemSearchDto
            {
                SearchTerm = "nature",
                Tags = new List<string> { "Spring" },
                Categories = new List<string> { "Seasonal" },
                PageNumber = 1,
                PageSize = 10,
                SortBy = "CreatedAt",
                SortDescending = true,
                IsPublished = true
            };

            var poems = new List<Poem>
            {
                new Poem
                {
                    Id = Guid.NewGuid(),
                    Title = "Spring Nature",
                    Content = "About nature in spring",
                    AuthorId = Guid.NewGuid(),
                    AuthorName = "Poet1",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow,
                    Statistics = new PoemStatistics(),
                    Tags = new List<Tag> { new Tag { Name = "Spring" } },
                    Categories = new List<Category> { new Category { Name = "Seasonal" } }
                }
            };

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.SearchPoemsAsync(
                    searchDto.SearchTerm,
                    searchDto.PageNumber,
                    searchDto.PageSize,
                    searchDto.SortBy,
                    searchDto.SortDescending,
                    searchDto.IsPublished))
                .ReturnsAsync((poems, 1));

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);

            // Act
            var result = await _poemService.SearchPoemsAsync(searchDto);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(1, result.TotalPages);
            Assert.False(result.HasPrevious);
            Assert.False(result.HasNext);
        }

        [Fact]
        public async Task GetPoemContentAsync_ShouldReturnContent()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var content = "The poem's full content...";

            var poem = new Poem
            {
                Id = poemId,
                Content = content
            };

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetByIdAsync(poemId))
                .ReturnsAsync(poem);

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);

            // Act
            var result = await _poemService.GetPoemContentAsync(poemId);

            // Assert
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task UnpublishPoemAsync_ShouldUnpublishPoem()
        {
            // Arrange
            var poemId = Guid.NewGuid();
            var authorId = Guid.NewGuid();

            var poem = new Poem
            {
                Id = poemId,
                Title = "Published Poem",
                AuthorId = authorId,
                IsPublished = true
            };

            var poemRepo = new Mock<IPoemRepository>();
            poemRepo.Setup(repo => repo.GetByIdAsync(poemId))
                .ReturnsAsync(poem);

            var savedPoemRepo = new Mock<ISavedPoemRepository>();
            savedPoemRepo.Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<SavedPoem, bool>>>()))
                .ReturnsAsync(new List<SavedPoem>());

            _mockUnitOfWork.Setup(uow => uow.Poems).Returns(poemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.SavedPoems).Returns(savedPoemRepo.Object);
            _mockUnitOfWork.Setup(uow => uow.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _poemService.UnpublishPoemAsync(poemId, authorId);

            // Assert
            Assert.True(result);
            Assert.False(poem.IsPublished);

            // Verify poem was updated
            poemRepo.Verify(repo => repo.Update(poem), Times.Once);

            // Verify changes were saved
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Once);
        }
    }
}
