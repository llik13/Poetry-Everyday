import React, { useState, useContext, useEffect, useRef } from "react";
import {
  getPoemComments,
  addComment,
  deleteComment,
} from "../../services/poemService";
import AuthContext from "../../context/AuthContext";
import Button from "../common/Button";
import Pagination from "../common/Pagination";
import "./CommentSection.css";

const CommentSection = ({
  initialComments = [],
  poemId,
  totalCommentCount = 0,
}) => {
  const { isAuthenticated, currentUser } = useContext(AuthContext);
  const [commentText, setCommentText] = useState("");
  const [displayedComments, setDisplayedComments] = useState(initialComments);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalComments, setTotalComments] = useState(
    totalCommentCount || initialComments.length
  );
  const COMMENTS_PER_PAGE = 10;

  const isInitialMount = useRef(true);
  const previousPage = useRef(currentPage);

  // Initialize with the provided total comment count
  // IMPORTANT: Use totalCommentCount as the source of truth for total comments
  useEffect(() => {
    // Always use totalCommentCount if available, because it's the server's total count
    if (totalCommentCount > 0) {
      setTotalComments(totalCommentCount);
      const calculatedTotalPages = Math.max(
        1,
        Math.ceil(totalCommentCount / COMMENTS_PER_PAGE)
      );
      setTotalPages(calculatedTotalPages);
    } else if (initialComments.length > 0) {
      // Fallback to initialComments length only if totalCommentCount isn't provided
      setTotalComments(initialComments.length);
      const calculatedTotalPages = Math.max(
        1,
        Math.ceil(initialComments.length / COMMENTS_PER_PAGE)
      );
      setTotalPages(calculatedTotalPages);
    }

    console.log(
      `CommentSection initialized: Total comments: ${totalCommentCount}, Initial comments: ${initialComments.length}`
    );
  }, [totalCommentCount, initialComments.length]);

  // Fetch comments when page changes, but not on first render
  useEffect(() => {
    if (isInitialMount.current) {
      isInitialMount.current = false;
      return;
    }

    // Only fetch if the page actually changed
    if (previousPage.current !== currentPage) {
      console.log(
        `Page changed from ${previousPage.current} to ${currentPage}, fetching comments...`
      );
      previousPage.current = currentPage;
      fetchComments(currentPage);
    }
  }, [currentPage, poemId]);

  const fetchComments = async (page) => {
    if (!poemId) return;

    setIsLoading(true);
    try {
      console.log(
        `Fetching comments for poem ${poemId}, page ${page}, size ${COMMENTS_PER_PAGE}`
      );
      const commentsData = await getPoemComments(
        poemId,
        page,
        COMMENTS_PER_PAGE
      );

      // If the response is an array, assume it's a simple array of comments
      if (Array.isArray(commentsData)) {
        setDisplayedComments(commentsData);

        // Do NOT update the total comment count here - keep using the initial totalCommentCount
        // Otherwise we lose the total count after page change
      }
      // If the response has a paginated structure with items property
      else if (
        commentsData &&
        typeof commentsData === "object" &&
        commentsData.items
      ) {
        setDisplayedComments(commentsData.items);

        // Only update total pages if available in response, but KEEP the original totalComments value
        if (commentsData.totalPages) {
          setTotalPages(commentsData.totalPages);
        }

        // We DON'T update totalComments here to preserve the original count
        // setTotalComments(commentsData.totalCount || totalComments);
      }
      // If it has a different structure but has a results array
      else if (
        commentsData &&
        typeof commentsData === "object" &&
        commentsData.results
      ) {
        setDisplayedComments(commentsData.results);

        // Only update total pages if available, keep original total comments
        if (commentsData.totalPages) {
          setTotalPages(commentsData.totalPages);
        }
      }
      // Any other structure, just use the object as is if possible
      else if (commentsData && typeof commentsData === "object") {
        setDisplayedComments([commentsData]);
      }

      console.log(
        `Comments loaded for page ${page}. Displayed comments count: ${displayedComments.length}`
      );
    } catch (error) {
      console.error("Error fetching comments:", error);
    } finally {
      setIsLoading(false);
    }
  };

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

      // If we're on page 1, add the new comment to the beginning
      if (currentPage === 1) {
        // Add the new comment but ensure we don't exceed page size
        const updatedComments = [newComment, ...displayedComments];
        if (updatedComments.length > COMMENTS_PER_PAGE) {
          updatedComments.pop(); // Remove the last comment to maintain page size
        }
        setDisplayedComments(updatedComments);
      } else {
        // If not on page 1, go to page 1 to see the new comment
        setCurrentPage(1);
        previousPage.current = 1; // Update the ref to avoid re-fetching
        await fetchComments(1);
      }

      // Update total comment count
      setTotalComments((prevTotal) => prevTotal + 1);

      // Recalculate total pages
      const newTotalPages = Math.max(
        1,
        Math.ceil((totalComments + 1) / COMMENTS_PER_PAGE)
      );
      setTotalPages(newTotalPages);

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

      // Remove the comment from the displayed comments
      const updatedComments = displayedComments.filter(
        (comment) => comment.id !== commentId
      );
      setDisplayedComments(updatedComments);

      // Update total comment count
      setTotalComments((prevTotal) => Math.max(0, prevTotal - 1));

      // Recalculate total pages
      const newTotalPages = Math.max(
        1,
        Math.ceil((totalComments - 1) / COMMENTS_PER_PAGE)
      );
      setTotalPages(newTotalPages);

      // If we deleted the last comment on the page and it's not page 1, go to previous page
      if (
        updatedComments.length === 0 &&
        currentPage > 1 &&
        currentPage > newTotalPages
      ) {
        setCurrentPage(currentPage - 1);
        previousPage.current = currentPage - 1; // Update ref to avoid re-fetching
      }
      // If we're still on the same page but need to fetch more comments to fill the gap
      else if (
        updatedComments.length < COMMENTS_PER_PAGE &&
        totalComments - 1 > updatedComments.length
      ) {
        fetchComments(currentPage);
      }
    } catch (error) {
      console.error("Error deleting comment:", error);
    }
  };

  const handlePageChange = (page) => {
    console.log(`Changing to page ${page} (current: ${currentPage})`);
    if (page !== currentPage) {
      setCurrentPage(page);
    }
  };

  return (
    <div className="comment-section">
      {/* Always show the total comments count, never the currently displayed count */}
      <h3>Comments ({totalComments})</h3>

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
        {isLoading ? (
          <div className="loading-container">
            <div className="spinner-border" role="status">
              <span className="visually-hidden">Loading comments...</span>
            </div>
          </div>
        ) : displayedComments.length === 0 ? (
          <p className="no-comments">
            No comments yet. Be the first to share your thoughts!
          </p>
        ) : (
          <>
            {displayedComments.map((comment) => (
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
            ))}
          </>
        )}
      </div>

      {/* Always render pagination container to maintain layout */}
      <div className="pagination-container">
        {totalPages > 1 && (
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            onPageChange={handlePageChange}
          />
        )}
      </div>
    </div>
  );
};

export default CommentSection;
