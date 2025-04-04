using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/poems")]
    public class PoemsController : ControllerBase
    {
        private readonly IPoemService _poemService;
        private readonly ICommentService _commentService;
        private readonly ILikeService _likeService;

        public PoemsController(
            IPoemService poemService,
            ICommentService commentService,
            ILikeService likeService)
        {
            _poemService = poemService;
            _commentService = commentService;
            _likeService = likeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PoemDto>>> GetPoems([FromQuery] PoemSearchDto searchDto)
        {
            var poems = await _poemService.SearchPoemsAsync(searchDto);
            return Ok(poems);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PoemDetailsDto>> GetPoem(Guid id)
        {
            Guid? currentUserId = null;
            if (User.Identity.IsAuthenticated)
            {
                currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            var poem = await _poemService.GetPoemDetailsAsync(id, currentUserId);
            if (poem == null)
            {
                return NotFound();
            }

            // Increment view count asynchronously
            _ = _poemService.IncrementViewCountAsync(id);

            return Ok(poem);
        }

        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<IEnumerable<PoemDto>>> GetPoemsByAuthor(Guid authorId)
        {
            var poems = await _poemService.GetPoemsByAuthorIdAsync(authorId);
            return Ok(poems);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<PoemDto>> CreatePoem(CreatePoemDto poemDto)
        {
            // Set current user as author
            poemDto.AuthorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            poemDto.AuthorName = User.FindFirstValue(ClaimTypes.Name);

            var poem = await _poemService.CreatePoemAsync(poemDto);
            return CreatedAtAction(nameof(GetPoem), new { id = poem.Id }, poem);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<PoemDto>> UpdatePoem(Guid id, UpdatePoemDto poemDto)
        {
            if (id != poemDto.Id)
            {
                return BadRequest();
            }

            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var poem = await _poemService.UpdatePoemAsync(poemDto, currentUserId);
                if (poem == null)
                {
                    return NotFound();
                }

                return Ok(poem);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePoem(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var result = await _poemService.DeletePoemAsync(id, currentUserId);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpGet("{id}/content")]
        public async Task<ActionResult<string>> GetPoemContent(Guid id)
        {
            var content = await _poemService.GetPoemContentAsync(id);
            if (content == null)
            {
                return NotFound();
            }

            return Ok(content);
        }

        [HttpGet("{id}/versions")]
        public async Task<ActionResult<IEnumerable<PoemVersionDto>>> GetPoemVersions(Guid id)
        {
            var versions = await _poemService.GetPoemVersionsAsync(id);
            return Ok(versions);
        }

        [HttpGet("versions/{versionId}/content")]
        public async Task<ActionResult<string>> GetPoemVersionContent(Guid versionId)
        {
            var content = await _poemService.GetPoemVersionContentAsync(versionId);
            if (content == null)
            {
                return NotFound();
            }

            return Ok(content);
        }

        [HttpGet("{id}/comments")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetPoemComments(
            Guid id,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var comments = await _commentService.GetCommentsByPoemIdAsync(id, pageNumber, pageSize);
            return Ok(comments);
        }

        [Authorize]
        [HttpPost("{id}/comments")]
        public async Task<ActionResult<CommentDto>> AddComment(Guid id, [FromBody] string commentText)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userName = User.FindFirstValue(ClaimTypes.Name);

            var commentDto = new CreateCommentDto
            {
                PoemId = id,
                UserId = currentUserId,
                UserName = userName,
                Text = commentText
            };

            try
            {
                var comment = await _commentService.AddCommentAsync(commentDto);
                return CreatedAtAction(nameof(GetPoemComments), new { id }, comment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpDelete("comments/{commentId}")]
        public async Task<ActionResult> DeleteComment(Guid commentId)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var result = await _commentService.DeleteCommentAsync(commentId, currentUserId);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("{id}/like")]
        public async Task<ActionResult> LikePoem(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userName = User.FindFirstValue(ClaimTypes.Name);

            var result = await _likeService.LikePoemAsync(id, currentUserId, userName);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}/like")]
        public async Task<ActionResult> UnlikePoem(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _likeService.UnlikePoemAsync(id, currentUserId);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize]
        [HttpGet("{id}/liked")]
        public async Task<ActionResult<bool>> IsPoemLiked(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _likeService.IsPoemLikedByUserAsync(id, currentUserId);
            return Ok(result);
        }
    }
}
