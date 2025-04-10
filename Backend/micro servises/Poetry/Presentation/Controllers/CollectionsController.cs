using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/collections")]
    //[Authorize]
    public class CollectionsController : ControllerBase
    {
        private readonly ICollectionService _collectionService;

        public CollectionsController(ICollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CollectionDto>>> GetUserCollections()
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var collections = await _collectionService.GetUserCollectionsAsync(currentUserId);
            return Ok(collections);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CollectionDetailsDto>> GetCollection(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var collection = await _collectionService.GetCollectionAsync(id, currentUserId);
                if (collection == null)
                {
                    return NotFound();
                }

                return Ok(collection);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<CollectionDto>> CreateCollection(CreateCollectionDto collectionDto)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            collectionDto.UserId = currentUserId;

            var collection = await _collectionService.CreateCollectionAsync(collectionDto);
            return CreatedAtAction(nameof(GetCollection), new { id = collection.Id }, collection);
        }

        [HttpPost("{collectionId}/poems/{poemId}")]
        public async Task<ActionResult> AddPoemToCollection(Guid collectionId, Guid poemId)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var result = await _collectionService.AddPoemToCollectionAsync(collectionId, poemId, currentUserId);
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

        [HttpDelete("{collectionId}/poems/{poemId}")]
        public async Task<ActionResult> RemovePoemFromCollection(Guid collectionId, Guid poemId)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var result = await _collectionService.RemovePoemFromCollectionAsync(collectionId, poemId, currentUserId);
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCollection(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            try
            {
                var result = await _collectionService.DeleteCollectionAsync(id, currentUserId);
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
    }


}
