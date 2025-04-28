import React from "react";
import { Link } from "react-router-dom";
import "./Poemcard.css";

const PoemCard = ({ poem }) => {
  // Truncate content to create a preview
  const createPreview = (content, maxLength = 120) => {
    if (!content) return "";
    if (content.length <= maxLength) return content;

    // Try to find the last complete line within the limit
    const lastNewlineIndex = content.substring(0, maxLength).lastIndexOf("\n");
    if (lastNewlineIndex > maxLength / 2) {
      return content.substring(0, lastNewlineIndex) + "...";
    }

    // If no newline found, truncate by character
    return content.substring(0, maxLength) + "...";
  };

  // Format date
  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  };

  return (
    <div className="card poem-card h-100">
      <div className="card-body">
        <h5 className="card-title">{poem.title}</h5>
        <h6 className="card-subtitle mb-2 text-muted">
          <Link to={`/author/${poem.authorId}`}>{poem.authorName}</Link>
        </h6>
        <pre className="poem">{createPreview(poem.content)}</pre>
        <Link to={`/poems/${poem.id}`} className="poem-btn">
          Read more
        </Link>
        <div className="poem-stats">
          <span>
            <i className="fas fa-eye"></i> {poem.statistics?.viewCount || 0}
          </span>
          <span>
            <i className="fas fa-heart"></i> {poem.statistics?.likeCount || 0}
          </span>
          <span>
            <i className="fas fa-comment"></i>{" "}
            {poem.statistics?.commentCount || 0}
          </span>
        </div>
      </div>
      <div className="card-footer bg-transparent text-muted">
        <small>{formatDate(poem.createdAt)}</small>
      </div>
    </div>
  );
};

export default PoemCard;
