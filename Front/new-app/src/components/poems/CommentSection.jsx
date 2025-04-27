import React, { useState, useContext } from "react";
import { addComment, deleteComment } from "../../services/poemService";
import AuthContext from "../../context/AuthContext";
import Button from "../common/Button";
import "./CommentSection.css";

const CommentSection = ({ comments = [], poemId }) => {
  const { isAuthenticated, currentUser } = useContext(AuthContext);
  const [commentText, setCommentText] = useState("");
  const [displayedComments, setDisplayedComments] = useState(comments);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  const handleSubmitComment = async (e) => {
    e.preventDefault();

    if (!commentText.trim() || !isAuthenticated) {
      return;
    }

    setIsSubmitting(true);

    try {
      const newComment = await addComment(poemId, commentText);
      setDisplayedComments([newComment, ...displayedComments]);
      setCommentText("");
    } catch (error) {
      console.error("Error adding comment:", error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDeleteComment = async (commentId) => {
    if (!isAuthenticated) return;

    try {
      await deleteComment(commentId);
      setDisplayedComments(
        displayedComments.filter((comment) => comment.id !== commentId)
      );
    } catch (error) {
      console.error("Error deleting comment:", error);
    }
  };

  return (
    <div className="comment-section">
      <h3>Comments ({displayedComments.length})</h3>

      {isAuthenticated ? (
        <form className="comment-form" onSubmit={handleSubmitComment}>
          <div className="form-group mb-3">
            <label htmlFor="comment" className="form-label">
              Add your comment
            </label>
            <textarea
              className="form-control"
              id="comment"
              rows="3"
              value={commentText}
              onChange={(e) => setCommentText(e.target.value)}
              placeholder="Write your thoughts about this poem..."
              disabled={isSubmitting}
            ></textarea>
          </div>
          <Button
            type="submit"
            variant="primary"
            disabled={isSubmitting || !commentText.trim()}
          >
            {isSubmitting ? "Submitting..." : "Submit Comment"}
          </Button>
        </form>
      ) : (
        <div className="login-prompt">
          <p>
            Please <a href="/login">log in</a> to leave a comment.
          </p>
        </div>
      )}

      <div className="comments-list">
        {displayedComments.length === 0 ? (
          <p className="no-comments">
            No comments yet. Be the first to share your thoughts!
          </p>
        ) : (
          displayedComments.map((comment) => (
            <div key={comment.id} className="comment">
              <div className="comment-header">
                <div className="comment-author">{comment.userName}</div>
                <div className="comment-date">
                  {formatDate(comment.createdAt)}
                </div>
              </div>
              <div className="comment-text">{comment.text}</div>

              {isAuthenticated &&
                currentUser &&
                (currentUser.id === comment.userId ||
                  currentUser.id === poemId.authorId) && (
                  <div className="comment-actions">
                    <button
                      className="btn-delete-comment"
                      onClick={() => handleDeleteComment(comment.id)}
                    >
                      Delete
                    </button>
                  </div>
                )}
            </div>
          ))
        )}
      </div>
    </div>
  );
};

export default CommentSection;
