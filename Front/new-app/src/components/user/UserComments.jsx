import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { getUserActivity } from "../../services/userService";
import "./UserComments.css";

const UserComments = () => {
  const [comments, setComments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    fetchUserComments();
  }, [currentPage]);

  const fetchUserComments = async () => {
    try {
      setLoading(true);
      // We're using the getUserActivity function but filtering for comments
      const data = await getUserActivity(currentPage, 10);

      // Filter for comment activities only
      const commentActivities = data.filter(
        (activity) => activity.type === "Comment"
      );

      setComments(commentActivities);

      // For pagination, assuming we get this info from the API
      // You might need to adjust based on your actual API response
      setTotalPages(Math.ceil(data.length / 10) || 1);

      setLoading(false);
    } catch (err) {
      console.error("Error fetching user comments:", err);
      setError("Failed to load your comments. Please try again later.");
      setLoading(false);
    }
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "long",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const handlePageChange = (page) => {
    setCurrentPage(page);
  };

  if (loading) {
    return (
      <div className="loading-container">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="user-comments">
      <div className="comments-header">
        <h2>My Comments</h2>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {comments.length === 0 ? (
        <div className="no-comments">
          <p>You haven't posted any comments yet.</p>
          <Link to="/catalog" className="btn btn-primary">
            Browse Poems
          </Link>
        </div>
      ) : (
        <div className="comments-list">
          {comments.map((comment, index) => (
            <div key={index} className="comment-card">
              <div className="comment-header">
                <Link
                  to={`/poems/${comment.relatedId}`}
                  className="comment-poem-title"
                >
                  {comment.relatedTitle || "Untitled Poem"}
                </Link>
                <span className="comment-date">
                  {formatDate(comment.createdAt)}
                </span>
              </div>
              <div className="comment-content">{comment.description}</div>
              <div className="comment-actions">
                <Link
                  to={`/poems/${comment.relatedId}`}
                  className="btn-view-poem"
                >
                  View Poem
                </Link>
              </div>
            </div>
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="pagination-container">
          <nav aria-label="Comments pagination">
            <ul className="pagination">
              <li
                className={`page-item ${currentPage === 1 ? "disabled" : ""}`}
              >
                <button
                  className="page-link"
                  onClick={() => handlePageChange(currentPage - 1)}
                  disabled={currentPage === 1}
                >
                  Previous
                </button>
              </li>

              {[...Array(totalPages)].map((_, i) => (
                <li
                  key={i}
                  className={`page-item ${
                    currentPage === i + 1 ? "active" : ""
                  }`}
                >
                  <button
                    className="page-link"
                    onClick={() => handlePageChange(i + 1)}
                  >
                    {i + 1}
                  </button>
                </li>
              ))}

              <li
                className={`page-item ${
                  currentPage === totalPages ? "disabled" : ""
                }`}
              >
                <button
                  className="page-link"
                  onClick={() => handlePageChange(currentPage + 1)}
                  disabled={currentPage === totalPages}
                >
                  Next
                </button>
              </li>
            </ul>
          </nav>
        </div>
      )}
    </div>
  );
};

export default UserComments;
