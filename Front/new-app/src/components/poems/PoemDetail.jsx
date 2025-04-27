import React, { useState, useContext } from "react";
import { Link } from "react-router-dom";
import AuthContext from "../../context/AuthContext";
import CommentSection from "./CommentSection";
import { likePoem, unlikePoem } from "../../services/poemService";
import "./PoemDetail.css";

const PoemDetail = ({ poem, onSaveToCollection }) => {
  const { isAuthenticated, currentUser } = useContext(AuthContext);
  const [liked, setLiked] = useState(poem.isLikedByCurrentUser);
  const [likeCount, setLikeCount] = useState(poem.statistics.likeCount);
  const [showSaveModal, setShowSaveModal] = useState(false);

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  const handleLikeClick = async () => {
    if (!isAuthenticated) {
      // Redirect to login or show login prompt
      return;
    }

    try {
      if (liked) {
        await unlikePoem(poem.id);
        setLiked(false);
        setLikeCount((prevCount) => prevCount - 1);
      } else {
        await likePoem(poem.id);
        setLiked(true);
        setLikeCount((prevCount) => prevCount + 1);
      }
    } catch (error) {
      console.error("Error toggling like:", error);
    }
  };

  const handleSaveClick = () => {
    if (!isAuthenticated) {
      // Redirect to login or show login prompt
      return;
    }

    setShowSaveModal(true);
  };

  const handleShareClick = () => {
    // Copy URL to clipboard
    const poemUrl = window.location.href;
    navigator.clipboard.writeText(poemUrl);

    // Show success notification
    alert("Link copied to clipboard!");
  };

  return (
    <div className="poem-container">
      <div className="poem-header">
        <h1 className="poem-title">{poem.title}</h1>
        <p className="poem-author">
          <Link to={`/author/${poem.authorId}`}>{poem.authorName}</Link>
        </p>

        <div className="poem-meta">
          <span>Published: {formatDate(poem.createdAt)}</span> •
          <span>Views: {poem.statistics.viewCount}</span> •
          <span>Likes: {likeCount}</span> •
          <span>Comments: {poem.statistics.commentCount}</span>
        </div>

        {poem.tags && poem.tags.length > 0 && (
          <div className="poem-tags">
            {poem.tags.map((tag, index) => (
              <span key={index} className="poem-tag">
                {tag}
              </span>
            ))}
          </div>
        )}
      </div>

      <pre className="poem-content">{poem.content}</pre>

      <div className="actions-panel">
        <button
          className={`btn-action btn-like ${liked ? "liked" : ""}`}
          onClick={handleLikeClick}
        >
          <span>{liked ? "❤️" : "❤"}</span> {liked ? "Liked" : "Like"} (
          {likeCount})
        </button>

        <button className="btn-action btn-save" onClick={handleSaveClick}>
          <span>🔖</span> Save to collection
        </button>

        <button className="btn-action btn-share" onClick={handleShareClick}>
          <span>🔗</span> Share
        </button>
      </div>

      {poem.comments && (
        <CommentSection comments={poem.comments} poemId={poem.id} />
      )}

      {/* Save to Collection Modal */}
      {showSaveModal && (
        <div className="modal show-modal">
          <div className="modal-content">
            <div className="modal-header">
              <h4>Save to Collection</h4>
              <button
                className="close-modal"
                onClick={() => setShowSaveModal(false)}
              >
                &times;
              </button>
            </div>
            <div className="modal-body">
              {/* Collection list component would go here */}
              <p>Select a collection to save this poem:</p>
              <div className="collection-list">
                {/* This would be populated from an API call */}
                <p>Loading collections...</p>
              </div>
            </div>
            <div className="modal-footer">
              <button
                className="btn btn-outline"
                onClick={() => setShowSaveModal(false)}
              >
                Cancel
              </button>
              <button
                className="btn btn-primary"
                onClick={() => {
                  onSaveToCollection && onSaveToCollection(poem.id);
                  setShowSaveModal(false);
                }}
              >
                Save
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PoemDetail;
