using BusinessLogic.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface ICommentService
    {
        Task<CommentDto> AddCommentAsync(CreateCommentDto commentDto);
        Task<IEnumerable<CommentDto>> GetCommentsByPoemIdAsync(Guid poemId, int pageNumber, int pageSize);
        Task<bool> DeleteCommentAsync(Guid id, Guid currentUserId);
        Task<PaginatedResult<CommentDto>> GetCommentsByAuthorPoemsAsync(Guid authorId, int page = 1, int pageSize = 20);
    }

}
