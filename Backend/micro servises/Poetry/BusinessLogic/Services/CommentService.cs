using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;


namespace BusinessLogic.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CommentDto> AddCommentAsync(CreateCommentDto commentDto)
        {
            var poem = await _unitOfWork.Poems.GetPoemWithDetailsAsync(commentDto.PoemId);
            if (poem == null) throw new KeyNotFoundException("Poem not found");

            var comment = new Comment
            {
                PoemId = commentDto.PoemId,
                UserId = commentDto.UserId,
                UserName = commentDto.UserName,
                Text = commentDto.Text
            };

            await _unitOfWork.Comments.AddAsync(comment);

            // Update comment count
            poem.Statistics.CommentCount++;
            _unitOfWork.Poems.Update(poem);

            // Create notification for poem author
            if (poem.AuthorId != commentDto.UserId)
            {
                var notification = new PoemNotification
                {
                    UserId = poem.AuthorId,
                    PoemId = poem.Id,
                    Message = $"{commentDto.UserName} commented on your poem \"{poem.Title}\"",
                    Type = NotificationType.NewComment,
                    IsRead = false
                };
                await _unitOfWork.Notifications.AddAsync(notification);
            }

            await _unitOfWork.CompleteAsync();

            return new CommentDto
            {
                Id = comment.Id,
                PoemId = comment.PoemId,
                UserId = comment.UserId,
                UserName = comment.UserName,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsByPoemIdAsync(Guid poemId, int pageNumber, int pageSize)
        {
            var comments = await _unitOfWork.Comments.GetCommentsByPoemIdAsync(poemId, pageNumber, pageSize);
            return comments.Select(c => new CommentDto
            {
                Id = c.Id,
                PoemId = c.PoemId,
                UserId = c.UserId,
                UserName = c.UserName,
                Text = c.Text,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task<bool> DeleteCommentAsync(Guid id, Guid currentUserId)
        {
            var comment = await _unitOfWork.Comments.GetByIdAsync(id);
            if (comment == null) return false;
            
            var poem = await _unitOfWork.Poems.GetByIdAsync(comment.PoemId);
            if (poem == null) return false;

            if (comment.UserId != currentUserId && poem.AuthorId != currentUserId)
            {
                throw new UnauthorizedAccessException("Only the comment author or poem author can delete this comment");
            }

            // Soft delete the comment
            _unitOfWork.Comments.SoftDelete(comment);

            poem = await _unitOfWork.Poems.GetPoemWithDetailsAsync(comment.PoemId);
            if (poem.Statistics != null)
            {
                poem.Statistics.CommentCount = Math.Max(0, poem.Statistics.CommentCount - 1);
                _unitOfWork.Poems.Update(poem);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<PaginatedResult<CommentDto>> GetCommentsByAuthorPoemsAsync(Guid authorId, int page = 1, int pageSize = 20)
        {
            // Получаем все стихи автора
            var poems = await _unitOfWork.Poems.GetPoemsByAuthorIdAsync(authorId);

            // Извлекаем их ID
            var poemIds = poems.Select(p => p.Id).ToList();

            // Создаем список для хранения всех комментариев
            var allComments = new List<CommentDto>();

            // Для каждого стихотворения получаем комментарии
            foreach (var poemId in poemIds)
            {
                var comments = await _unitOfWork.Comments.FindAsync(c => c.PoemId == poemId);

                allComments.AddRange(comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    PoemId = c.PoemId,
                    UserId = c.UserId,
                    UserName = c.UserName,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                }));
            }

            // Сортируем по дате создания (сначала новые) и применяем пагинацию
            var totalCount = allComments.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginatedComments = allComments
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResult<CommentDto>
            {
                Items = paginatedComments,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }


    }
}
