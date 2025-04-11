using BusinessLogic.DTOs;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class CollectionService : ICollectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPoemService _poemService;

        public CollectionService(IUnitOfWork unitOfWork, IPoemService poemService)
        {
            _unitOfWork = unitOfWork;
            _poemService = poemService;
        }

        public async Task<CollectionDto> CreateCollectionAsync(CreateCollectionDto collectionDto)
        {
            var collection = new Collection
            {
                Name = collectionDto.Name,
                Description = collectionDto.Description,
                UserId = collectionDto.UserId,
                IsPublic = collectionDto.IsPublic,
                PublishedPoemCount = 0
            };

            await _unitOfWork.Collections.AddAsync(collection);
            await _unitOfWork.CompleteAsync();

            return new CollectionDto
            {
                Id = collection.Id,
                Name = collection.Name,
                Description = collection.Description,
                UserId = collection.UserId,
                IsPublic = collection.IsPublic,
                PoemCount = 0
            };
        }

        public async Task<IEnumerable<CollectionDto>> GetUserCollectionsAsync(Guid userId)
        {
            var collections = await _unitOfWork.Collections.GetUserCollectionsAsync(userId);
            return collections.Select(c => new CollectionDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                UserId = c.UserId,
                IsPublic = c.IsPublic,
                PoemCount = c.PublishedPoemCount
            });
        }

        public async Task<CollectionDetailsDto> GetCollectionAsync(Guid id, Guid userId)
        {
            var collection = await _unitOfWork.Collections.GetCollectionWithPoemsAsync(id);

            if (collection == null) return null;

            // Check if the user has access
            if (collection.UserId != userId && !collection.IsPublic)
            {
                throw new UnauthorizedAccessException("You don't have access to this collection");
            }

            var poemDtos = new List<PoemDto>();
            foreach (var savedPoem in collection.SavedPoems)
            {
                var poem = await _poemService.GetPoemByIdAsync(savedPoem.PoemId);
                if (poem != null)
                {
                    poemDtos.Add(poem);
                }
            }

            return new CollectionDetailsDto
            {
                Id = collection.Id,
                Name = collection.Name,
                Description = collection.Description,
                UserId = collection.UserId,
                IsPublic = collection.IsPublic,
                PoemCount = poemDtos.Count,
                Poems = poemDtos
            };
        }

        public async Task<bool> AddPoemToCollectionAsync(Guid collectionId, Guid poemId, Guid userId)
        {
            var collection = await _unitOfWork.Collections.GetByIdAsync(collectionId);
            if (collection == null) return false;

            // Check if the user owns the collection
            if (collection.UserId != userId)
            {
                throw new UnauthorizedAccessException("Only the collection owner can add poems");
            }
            // Check if poem exists
            var poem = await _unitOfWork.Poems.GetPoemWithDetailsAsync(poemId);
            if (poem == null) return false;

            // Check if already in collection
            var savedPoems = await _unitOfWork.SavedPoems.FindAsync(
                sp => sp.CollectionId == collectionId && sp.PoemId == poemId);

            if (savedPoems.Any()) return true; // Already in collection

            // Add to collection
            var savedPoem = new SavedPoem
            {
                CollectionId = collectionId,
                PoemId = poemId,
                SavedAt = DateTime.UtcNow
            };
            await _unitOfWork.SavedPoems.AddAsync(savedPoem);

            // Update save count
            poem.Statistics.SaveCount++;
            _unitOfWork.Poems.Update(poem);

            // Update published poem count if the poem is published
            if (poem.IsPublished)
            {
                collection.PublishedPoemCount++;
                _unitOfWork.Collections.Update(collection);
            }

            // Create notification for poem author
            if (poem.AuthorId != userId)
            {
                var notification = new PoemNotification
                {
                    UserId = poem.AuthorId,
                    PoemId = poem.Id,
                    Message = $"Someone saved your poem \"{poem.Title}\" to their collection",
                    Type = NotificationType.PoemSaved,
                    IsRead = false
                };
                await _unitOfWork.Notifications.AddAsync(notification);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> RemovePoemFromCollectionAsync(Guid collectionId, Guid poemId, Guid userId)
        {
            var collection = await _unitOfWork.Collections.GetByIdAsync(collectionId);
            if (collection == null) return false;

            // Check if the user owns the collection
            if (collection.UserId != userId)
            {
                throw new UnauthorizedAccessException("Only the collection owner can remove poems");
            }

            // Find saved poem
            var savedPoems = await _unitOfWork.SavedPoems.FindAsync(
                sp => sp.CollectionId == collectionId && sp.PoemId == poemId);

            var savedPoem = savedPoems.FirstOrDefault();
            if (savedPoem == null) return false;

            // Get the poem to check if it's published
            var poem = await _unitOfWork.Poems.GetByIdAsync(poemId);

            // Remove from collection
            _unitOfWork.SavedPoems.Remove(savedPoem);

            // Update save count
            if (poem != null && poem.Statistics != null)
            {
                poem.Statistics.SaveCount = Math.Max(0, poem.Statistics.SaveCount - 1);
                _unitOfWork.Poems.Update(poem);

                // Update published poem count if the poem is published
                if (poem.IsPublished)
                {
                    collection.PublishedPoemCount = Math.Max(0, collection.PublishedPoemCount - 1);
                    _unitOfWork.Collections.Update(collection);
                }
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> DeleteCollectionAsync(Guid id, Guid userId)
        {
            var collection = await _unitOfWork.Collections.GetByIdAsync(id);
            if (collection == null) return false;

            // Check if the user owns the collection
            if (collection.UserId != userId)
            {
                throw new UnauthorizedAccessException("Only the collection owner can delete this collection");
            }

            // Soft delete
            _unitOfWork.Collections.Remove(collection);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }

}
