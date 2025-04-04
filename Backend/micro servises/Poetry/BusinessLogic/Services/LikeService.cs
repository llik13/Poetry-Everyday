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
    public class LikeService : ILikeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LikeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> LikePoemAsync(Guid poemId, Guid userId, string userName)
        {
            var poem = await _unitOfWork.Poems.GetPoemWithDetailsAsync(poemId);
            if (poem == null) return false;

            // Check if already liked
            var existingLikes = await _unitOfWork.Likes.FindAsync(l => l.PoemId == poemId && l.UserId == userId);
            if (existingLikes.Any()) return true; // Already liked

            // Add like
            var like = new Like
            {
                PoemId = poemId,
                UserId = userId
            };
            await _unitOfWork.Likes.AddAsync(like);

            // Update like count
            poem.Statistics.LikeCount++;
            _unitOfWork.Poems.Update(poem);

            // Create notification for poem author
            if (poem.AuthorId != userId)
            {
                var notification = new PoemNotification
                {
                    UserId = poem.AuthorId,
                    PoemId = poem.Id,
                    Message = $"{userName} liked your poem \"{poem.Title}\"",
                    Type = NotificationType.NewLike,
                    IsRead = false
                };
                await _unitOfWork.Notifications.AddAsync(notification);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> UnlikePoemAsync(Guid poemId, Guid userId)
        {
            var likes = await _unitOfWork.Likes.FindAsync(l => l.PoemId == poemId && l.UserId == userId);
            var like = likes.FirstOrDefault();
            if (like == null) return false; // Not liked yet

            _unitOfWork.Likes.Remove(like);

            // Update like count
            var poem = await _unitOfWork.Poems.GetPoemWithDetailsAsync(poemId);
            if (poem != null && poem.Statistics != null)
            {
                poem.Statistics.LikeCount = Math.Max(0, poem.Statistics.LikeCount - 1);
                _unitOfWork.Poems.Update(poem);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task<bool> IsPoemLikedByUserAsync(Guid poemId, Guid userId)
        {
            var likes = await _unitOfWork.Likes.FindAsync(l => l.PoemId == poemId && l.UserId == userId);
            return likes.Any();
        }
    }

}
