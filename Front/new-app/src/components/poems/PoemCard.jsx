import React from "react";
import { Link } from "react-router-dom";
import "./Poemcard.css";

const PoemCard = ({ poem }) => {
  // Improved preview function to display first few lines of the poem
  const createPreview = (content, maxLines = 4, maxChars = 300) => {
    // Check if content exists and is not empty
    if (!content || content.trim() === "") {
      return "No preview available...";
    }

    // Split content into lines
    const lines = content.split("\n").filter((line) => line.trim() !== "");

    // Take only the first few lines (up to maxLines)
    let previewLines = lines.slice(0, maxLines);

    // Join the lines back together
    let preview = previewLines.join("\n");

    // If there are more lines, add ellipsis on a new line
    if (lines.length > maxLines) {
      preview += "\n...";
    }

    return preview;
  };

  // Format date
  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  };

  // Try to get content from either content or excerpt fields
  const poemContent = poem.content || poem.excerpt || "";

  // Debug log
  console.log(
    `Rendering poem card: ${poem.title}, Content length: ${poemContent.length}`
  );

  return (
    <div className="card poem-card h-100">
      <div className="card-body">
        <h5 className="card-title">{poem.title}</h5>
        <h6 className="card-subtitle mb-2 text-muted">{poem.authorName}</h6>

        {/* Display the poem preview with proper formatting */}
        <div className="poem-preview-container">
          <pre className="poem-preview">{createPreview(poemContent)}</pre>
        </div>

        <div className="read-more-container">
          <Link to={`/poems/${poem.id}`} className="poem-btn">
            Read more
          </Link>
        </div>
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
