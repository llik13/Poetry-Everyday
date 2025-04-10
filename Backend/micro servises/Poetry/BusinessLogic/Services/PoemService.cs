using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.Extensions.Logging; 

namespace BusinessLogic.Services
{
    public class PoemService : IPoemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PoemService> _logger;

        public PoemService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PoemDto> GetPoemByIdAsync(Guid id)
        {
            var poem = await _unitOfWork.Poems.GetByIdAsync(id);
            if (poem == null) return null;

            return new PoemDto
            {
                Id = poem.Id,
                Title = poem.Title,
                Excerpt = poem.Excerpt,
                AuthorId = poem.AuthorId,
                Content = poem.Content,
                AuthorName = poem.AuthorName,
                IsPublished = poem.IsPublished,
                CreatedAt = poem.CreatedAt,
                UpdatedAt = poem.UpdatedAt,
                Statistics = new PoemStatisticsDto
                {
                    ViewCount = poem.Statistics?.ViewCount ?? 0,
                    LikeCount = poem.Statistics?.LikeCount ?? 0,
                    CommentCount = poem.Statistics?.CommentCount ?? 0,
                    SaveCount = poem.Statistics?.SaveCount ?? 0
                },
                Tags = poem.Tags?.Select(t => t.Name).ToList() ?? new List<string>(),
                Categories = poem.Categories?.Select(c => c.Name).ToList() ?? new List<string>()
            };
        }

        public async Task<PoemDetailsDto> GetPoemDetailsAsync(Guid id, Guid? currentUserId)
        {
            var poem = await _unitOfWork.Poems.GetPoemWithDetailsAsync(id);
            if (poem == null) return null;

            var isLiked = false;

            if (currentUserId.HasValue)
            {
                var likes = await _unitOfWork.Likes.FindAsync(l => l.PoemId == id && l.UserId == currentUserId.Value);
                isLiked = likes.Any();
            }

            

            return new PoemDetailsDto
            {
                Id = poem.Id,
                Title = poem.Title,
                Excerpt = poem.Excerpt,
                Content = poem.Content,
                AuthorId = poem.AuthorId,
                AuthorName = poem.AuthorName,
                IsPublished = poem.IsPublished,
                CreatedAt = poem.CreatedAt,
                UpdatedAt = poem.UpdatedAt,
                IsLikedByCurrentUser = isLiked,
                Statistics = new PoemStatisticsDto
                {
                    ViewCount = poem.Statistics?.ViewCount ?? 0,
                    LikeCount = poem.Statistics?.LikeCount ?? 0,
                    CommentCount = poem.Statistics?.CommentCount ?? 0,
                    SaveCount = poem.Statistics?.SaveCount ?? 0
                },
                Tags = poem.Tags?.Select(t => t.Name).ToList() ?? new List<string>(),
                Categories = poem.Categories?.Select(c => c.Name).ToList() ?? new List<string>(),
                Comments = poem.Comments?.Select(c => new CommentDto
                {
                    Id = c.Id,
                    PoemId = c.PoemId,
                    UserId = c.UserId,
                    UserName = c.UserName,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                }).ToList() ?? new List<CommentDto>(),
            };
        }

        public async Task<IEnumerable<PoemDto>> GetPoemsByAuthorIdAsync(Guid authorId)
        {
            var poems = await _unitOfWork.Poems.GetPoemsByAuthorIdAsync(authorId);
            return poems.Select(p => new PoemDto
            {
                Id = p.Id,
                Title = p.Title,
                Excerpt = p.Excerpt,
                Content = p.Content,
                AuthorId = p.AuthorId,
                AuthorName = p.AuthorName,
                IsPublished = p.IsPublished,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Statistics = new PoemStatisticsDto
                {
                    ViewCount = p.Statistics?.ViewCount ?? 0,
                    LikeCount = p.Statistics?.LikeCount ?? 0,
                    CommentCount = p.Statistics?.CommentCount ?? 0,
                    SaveCount = p.Statistics?.SaveCount ?? 0
                },
                Tags = p.Tags?.Select(t => t.Name).ToList() ?? new List<string>(),
                Categories = p.Categories?.Select(c => c.Name).ToList() ?? new List<string>()
            });
        }

        public async Task<PaginatedResult<PoemDto>> SearchPoemsAsync(PoemSearchDto searchDto)
        {
            // Search poems with pagination from repository
            var (poems, totalCount) = await _unitOfWork.Poems.SearchPoemsAsync(
                searchDto.SearchTerm,
                searchDto.PageNumber,
                searchDto.PageSize,
                searchDto.SortBy,
                searchDto.SortDescending,
                searchDto.IsPublished);

            // If there are specific tag or category filters, apply them in memory
            var result = poems.AsEnumerable();


            if (searchDto.Tags != null && searchDto.Tags.Any())
            {
                result = result.Where(p =>
                    p.Tags.Any(t => searchDto.Tags.Contains(t.Name, StringComparer.OrdinalIgnoreCase)));
            }

            if (searchDto.Categories != null && searchDto.Categories.Any())
            {
                result = result.Where(p =>
                    p.Categories.Any(c => searchDto.Categories.Contains(c.Name, StringComparer.OrdinalIgnoreCase)));
            }

            if (searchDto.AuthorId.HasValue)
            {
                result = result.Where(p => p.AuthorId == searchDto.AuthorId.Value);
            }

            

            // Convert entities to DTOs
            var poemDtos = result.Select(p => new PoemDto
            {
                Id = p.Id,
                Title = p.Title,
                Excerpt = p.Excerpt,
                AuthorId = p.AuthorId,
                AuthorName = p.AuthorName,
                IsPublished = p.IsPublished,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Statistics = new PoemStatisticsDto
                {
                    ViewCount = p.Statistics?.ViewCount ?? 0,
                    LikeCount = p.Statistics?.LikeCount ?? 0,
                    CommentCount = p.Statistics?.CommentCount ?? 0,
                    SaveCount = p.Statistics?.SaveCount ?? 0
                },
                Tags = p.Tags?.Select(t => t.Name).ToList() ?? new List<string>(),
                Categories = p.Categories?.Select(c => c.Name).ToList() ?? new List<string>()
            }).ToList();

            // Calculate pagination values
            int filteredCount = poemDtos.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);

            // Return paginated result
            return new PaginatedResult<PoemDto>
            {
                Items = poemDtos,
                PageNumber = searchDto.PageNumber,
                PageSize = searchDto.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }


       


        public async Task<PoemDto> CreatePoemAsync(CreatePoemDto poemDto)
        {
            // Create poem entity
            var poem = new Poem
            {
                Title = poemDto.Title,
                Excerpt = poemDto.Excerpt,
                Content = poemDto.Content,
                AuthorId = poemDto.AuthorId,
                AuthorName = poemDto.AuthorName,
                IsPublished = false,
                Statistics = new PoemStatistics
                {
                    ViewCount = 0,
                    LikeCount = 0,
                    CommentCount = 0,
                    SaveCount = 0
                },
                Tags = new List<Tag>(),
                Categories = new List<Category>(),
            };

            // Save poem first to get an ID
            await _unitOfWork.Poems.AddAsync(poem);
            await _unitOfWork.CompleteAsync();

            // Now handle tags
            if (poemDto.Tags != null && poemDto.Tags.Any())
            {
                foreach (var tagName in poemDto.Tags)
                {
                    var tag = await _unitOfWork.Tags.GetTagByNameAsync(tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        await _unitOfWork.Tags.AddAsync(tag);
                        await _unitOfWork.CompleteAsync(); // Save each tag immediately
                    }

                    poem.Tags.Add(tag);
                }
            }

            // Handle categories
            if (poemDto.Categories != null && poemDto.Categories.Any())
            {
                foreach (var categoryName in poemDto.Categories)
                {
                    var category = await _unitOfWork.Categories.GetCategoryByNameAsync(categoryName);
                    if (category == null)
                    {
                        category = new Category { Name = categoryName, Description = "" };
                        await _unitOfWork.Categories.AddAsync(category);
                        await _unitOfWork.CompleteAsync(); // Save each category immediately
                    }

                    poem.Categories.Add(category);
                }
            }

            // Save all changes
            try
            {
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                // Log the inner exception details
                while (ex != null)
                {
                    _logger.LogError($"Error: {ex.Message}");
                    ex = ex.InnerException;
                }
                throw;
            }

            // Return DTO
            return new PoemDto
            {
                Id = poem.Id,
                Title = poem.Title,
                Excerpt = poem.Excerpt,
                Content = poemDto.Content,
                AuthorId = poem.AuthorId,
                AuthorName = poem.AuthorName,
                IsPublished = poem.IsPublished,
                CreatedAt = poem.CreatedAt,
                UpdatedAt = poem.UpdatedAt,
                Statistics = new PoemStatisticsDto
                {
                    ViewCount = 0,
                    LikeCount = 0,
                    CommentCount = 0,
                    SaveCount = 0
                },
                Tags = poemDto.Tags ?? new List<string>(),
                Categories = poemDto.Categories ?? new List<string>()
            };
        }

        public async Task<PoemDto> UpdatePoemAsync(UpdatePoemDto poemDto, Guid currentUserId)
        {
            var poem = await _unitOfWork.Poems.GetPoemWithDetailsAsync(poemDto.Id);
            if (poem == null) return null;

            // Check if user is the author
            if (poem.AuthorId != currentUserId)
            {
                throw new UnauthorizedAccessException("Only the author can update this poem");
            }         

            // Update poem properties
            poem.Title = poemDto.Title;
            poem.Excerpt = poemDto.Excerpt;
            poem.Content = poemDto.Content;
            poem.IsPublished = poemDto.IsPublished;
            poem.UpdatedAt = DateTime.UtcNow;

            // Update tags
            poem.Tags.Clear();
            if (poemDto.Tags != null && poemDto.Tags.Any())
            {
                foreach (var tagName in poemDto.Tags)
                {
                    var tag = await _unitOfWork.Tags.GetTagByNameAsync(tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        await _unitOfWork.Tags.AddAsync(tag);
                    }
                    poem.Tags.Add(tag);
                }
            }

            // Update categories
            poem.Categories.Clear();
            if (poemDto.Categories != null && poemDto.Categories.Any())
            {
                foreach (var categoryName in poemDto.Categories)
                {
                    var category = await _unitOfWork.Categories.GetCategoryByNameAsync(categoryName);
                    if (category == null)
                    {
                        category = new Category { Name = categoryName };
                        await _unitOfWork.Categories.AddAsync(category);
                    }
                    poem.Categories.Add(category);
                }
            }

            _unitOfWork.Poems.Update(poem);
            await _unitOfWork.CompleteAsync();

            // Create notifications for followers (simplified implementation)
            // In a real application, this would be handled by an event bus

            // Return updated poem
            return new PoemDto
            {
                Id = poem.Id,
                Title = poem.Title,
                Excerpt = poem.Excerpt,
                Content = poem.Content,
                AuthorId = poem.AuthorId,
                AuthorName = poem.AuthorName,
                IsPublished = poem.IsPublished,
                CreatedAt = poem.CreatedAt,
                UpdatedAt = poem.UpdatedAt,
                Statistics = new PoemStatisticsDto
                {
                    ViewCount = poem.Statistics.ViewCount,
                    LikeCount = poem.Statistics.LikeCount,
                    CommentCount = poem.Statistics.CommentCount,
                    SaveCount = poem.Statistics.SaveCount
                },
                Tags = poemDto.Tags ?? new List<string>(),
                Categories = poemDto.Categories ?? new List<string>()
            };
        }

        public async Task<bool> UnpublishPoemAsync(Guid id, Guid currentUserId)
        {
            var poem = await _unitOfWork.Poems.GetByIdAsync(id);
            if (poem == null) return false;

            // Check if user is the author
            if (poem.AuthorId != currentUserId)
            {
                throw new UnauthorizedAccessException("Only the author can delete this poem");
            }

            poem.IsPublished = false;
            _unitOfWork.Poems.Update(poem);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeletePoemAsync(Guid id, Guid currentUserId)
        {
            var poem = await _unitOfWork.Poems.GetByIdAsync(id);
            if (poem == null) return false;

            // Check if user is the author
            if (poem.AuthorId != currentUserId)
            {
                throw new UnauthorizedAccessException("Only the author can unpublish this poem");
            }

            _unitOfWork.Poems.Remove(poem);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> IncrementViewCountAsync(Guid id)
        {
            var poem = await _unitOfWork.Poems.GetPoemWithDetailsAsync(id);
            if (poem == null) return false;

            poem.Statistics.ViewCount++;
            _unitOfWork.Poems.Update(poem);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<string> GetPoemContentAsync(Guid id)
        {
            var poem = await _unitOfWork.Poems.GetByIdAsync(id);
            if (poem == null) return null;

            return poem.Content;
        }
    }
}
