using Contract.Interfaces;
using DataAccess.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace BusinessLogic.Consumer
{
    public class UserNameChangedConsumer : IConsumer<IUserNameChangedEvent>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserNameChangedConsumer> _logger;

        public UserNameChangedConsumer(IUnitOfWork unitOfWork, ILogger<UserNameChangedConsumer> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IUserNameChangedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation(
                "Received UserNameChanged event. UserId: {UserId}, OldName: {OldName}, NewName: {NewName}",
                message.UserId, message.OldUserName, message.NewUserName);

            try
            {
                // Convert string UserId to Guid for poem service
                if (!Guid.TryParse(message.UserId, out Guid userGuid))
                {
                    _logger.LogError("Failed to parse UserId {UserId} to Guid", message.UserId);
                    return;
                }

                // Find all poems by this author
                var authorPoems = await _unitOfWork.Poems.GetPoemsByAuthorIdAsync(userGuid);

                foreach (var poem in authorPoems)
                {
                    // Update the author name
                    poem.AuthorName = message.NewUserName;
                    _unitOfWork.Poems.Update(poem);
                }

                // Find all comments by this user
                var comments = await _unitOfWork.Comments.FindAsync(c => c.UserId == userGuid);

                foreach (var comment in comments)
                {
                    // Update the username in comments
                    comment.UserName = message.NewUserName;
                    _unitOfWork.Comments.Update(comment);
                }

                // Save all changes
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation(
                    "Successfully updated username for {PoemCount} poems and {CommentCount} comments",
                    authorPoems.Count(), comments.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating username in PoetryService");
                // Consider retrying or sending to an error queue
                throw; // MassTransit will handle retry policies
            }
        }
    }
}
