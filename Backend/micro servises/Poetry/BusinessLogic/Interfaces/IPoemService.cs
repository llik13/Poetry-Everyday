using BusinessLogic.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IPoemService
    {
        Task<PoemDto> GetPoemByIdAsync(Guid id);
        Task<PoemDetailsDto> GetPoemDetailsAsync(Guid id, Guid? currentUserId);
        Task<IEnumerable<PoemDto>> GetPoemsByAuthorIdAsync(Guid authorId);
        Task<PaginatedResult<PoemDto>> SearchPoemsAsync(PoemSearchDto searchDto);
        Task<PoemDto> CreatePoemAsync(CreatePoemDto poemDto);
        Task<PoemDto> UpdatePoemAsync(UpdatePoemDto poemDto, Guid currentUserId);
        Task<bool> DeletePoemAsync(Guid id, Guid currentUserId);
        Task<bool> IncrementViewCountAsync(Guid id);
        Task<string> GetPoemContentAsync(Guid id);
        Task<IEnumerable<PoemVersionDto>> GetPoemVersionsAsync(Guid poemId);
        Task<string> GetPoemVersionContentAsync(Guid versionId);
    }


}
