﻿using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/mypoems")]
    [Authorize]
    public class UserPoemsController : ControllerBase
    {
        private readonly IPoemService _poemService;

        public UserPoemsController(IPoemService poemService)
        {
            _poemService = poemService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<PoemDto>>> GetMyPoems()
        {
            // Extract user ID from the JWT claims
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get poems authored by the current user
            var poems = await _poemService.GetPoemsByAuthorIdAsync(currentUserId);
            return Ok(poems);
        }

        [HttpGet("drafts")]
        public async Task<ActionResult<IEnumerable<PoemDto>>> GetMyDrafts()
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Create a search with unpublished filter for the current user
            var searchDto = new PoemSearchDto
            {
                AuthorId = currentUserId,
                PageNumber = 1,
                PageSize = 100,
                SortBy = "UpdatedAt",
                SortDescending = true
            };

            var result = await _poemService.SearchPoemsAsync(searchDto);

            // Filter for unpublished poems (drafts) in memory
            var drafts = result.Items.Where(p => !p.IsPublished).ToList();

            return Ok(new PaginatedResult<PoemDto>
            {
                Items = drafts,
                PageNumber = 1,
                PageSize = drafts.Count,
                TotalCount = drafts.Count,
                TotalPages = 1
            });
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

        [HttpPost]
        public async Task<ActionResult<PoemDto>> CreatePoem(CreatePoemDto poemDto)
        {
            // Set current user as author
            poemDto.AuthorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            poemDto.AuthorName = User.FindFirstValue(ClaimTypes.Name);

            var poem = await _poemService.CreatePoemAsync(poemDto);
            return CreatedAtAction(nameof(GetPoem), new { id = poem.Id }, poem);
        }


        //[Authorize]
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

        [HttpPost("publish/{id}")]
        public async Task<ActionResult> PublishPoem(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Get the poem first
            var poem = await _poemService.GetPoemByIdAsync(id);

            if (poem == null)
                return NotFound();

            // Check if the user is the author
            if (poem.AuthorId != currentUserId)
                return Forbid("Only the author can publish this poem");

            // Create the update DTO
            var updateDto = new UpdatePoemDto
            {
                Id = id,
                Title = poem.Title,
                Content = poem.Content,
                Excerpt = poem.Excerpt,
                IsPublished = true, // Set to published
                Tags = poem.Tags,
                Categories = poem.Categories,
                ChangeNotes = "Published poem"
            };

            var updatedPoem = await _poemService.UpdatePoemAsync(updateDto, currentUserId);
            return Ok(updatedPoem);
        }
    }
}