import React from "react";
import PoemCard from "./PoemCard";
import "./PoemList.css";

const PoemList = ({ poems, loading }) => {
  if (loading) {
    return (
      <div className="loading-container">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (poems.length === 0) {
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
