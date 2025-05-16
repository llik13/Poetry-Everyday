import React, { useState, useContext } from "react";
import { Link } from "react-router-dom";
import AuthContext from "../../context/AuthContext";
import CommentSection from "./CommentSection";
import CollectionModal from "./CollectionModal";
import { likePoem, unlikePoem } from "../../services/poemService";
import "./PoemDetail.css";

const PoemDetail = ({ poem }) => {
  const { isAuthenticated, currentUser } = useContext(AuthContext);
  const [liked, setLiked] = useState(poem.isLikedByCurrentUser);
  const [likeCount, setLikeCount] = useState(poem.statistics.likeCount);
  const [showCollectionModal, setShowCollectionModal] = useState(false);
  const [savedSuccess, setSavedSuccess] = useState(false);

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

    setShowCollectionModal(true);
  };

  const handleShareClick = () => {
    // Copy URL to clipboard
    const poemUrl = window.location.href;
    navigator.clipboard.writeText(poemUrl);

    // Show success notification
    alert("Link copied to clipboard!");
  };

  const handleCollectionSuccess = () => {
    setSavedSuccess(true);
    setTimeout(() => setSavedSuccess(false), 3000);
  };

  // Ensure we always have the total number of comments from statistics
  // This is critical for maintaining consistent comment count across pagination
  const totalCommentCount =
    poem.statistics && poem.statistics.commentCount !== undefined
      ? poem.statistics.commentCount
      : poem.comments
      ? poem.comments.length
      : 0;

  // Debug logging for comment count
  console.log(
    `PoemDetail: Total comment count from statistics: ${totalCommentCount}`
  );
  console.log(
    `PoemDetail: Initial comments length: ${
      poem.comments ? poem.comments.length : 0
    }`
  );

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
          <span>Comments: {totalCommentCount}</span>
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

      {savedSuccess && (
        <div className="save-success-message">
          Poem saved to collection successfully!
        </div>
      )}

      {/* Pass the correct comment count from statistics */}
      <CommentSection
        initialComments={poem.comments || []}
        poemId={poem.id}
        totalCommentCount={totalCommentCount}
      />

      {/* Collection Modal */}
      <CollectionModal
        isOpen={showCollectionModal}
        onClose={() => setShowCollectionModal(false)}
        poemId={poem.id}
        onSuccess={handleCollectionSuccess}
      />
    </div>
  );
};

export default PoemDetail;
