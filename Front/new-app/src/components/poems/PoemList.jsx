import React, { useEffect } from "react";
import PoemCard from "./PoemCard";
import "./PoemList.css";

const PoemList = ({ poems, loading }) => {
  // Log the poems data when it changes to help with debugging
  useEffect(() => {
    if (poems && poems.length > 0) {
      console.log(`PoemList received ${poems.length} poems`);
      console.log("First poem sample:", {
        title: poems[0].title,
        content: poems[0].content
          ? `Content length: ${poems[0].content.length}`
          : "No content",
        excerpt: poems[0].excerpt
          ? `Excerpt length: ${poems[0].excerpt.length}`
          : "No excerpt",
      });
    }
  }, [poems]);

  if (loading) {
    return (
      <div className="loading-container">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (!poems || poems.length === 0) {
    return (
      <div className="no-poems">
        <p>No poems found. Try adjusting your search criteria.</p>
      </div>
    );
  }

  return (
    <div className="poem-list-container">
      <div className="grid-card">
        {poems.map((poem) => (
          <div key={poem.id} className="card-item">
            <PoemCard poem={poem} />
          </div>
        ))}
      </div>
    </div>
  );
};

export default PoemList;
