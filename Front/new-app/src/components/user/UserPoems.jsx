import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import {
  getUserPoems,
  unpublishPoem,
  deletePoem,
  getUserCollections,
  getCollection,
  removePoemFromCollection,
} from "../../services/poemService";
import Button from "../common/Button";
import "./UserPoems.css";

const UserPoems = () => {
  const [poems, setPoems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchPoems = async () => {
      try {
        setLoading(true);
        const data = await getUserPoems();
        setPoems(data || []);
        setLoading(false);
      } catch (err) {
        console.error("Error fetching poems:", err);
        setError("Failed to load your poems. Please try again later.");
        setLoading(false);
      }
    };

    fetchPoems();
  }, []);

  const handleUnpublish = async (poemId) => {
    try {
      // First, get all user collections to check which ones contain the poem
      const userCollections = await getUserCollections();

      // Unpublish the poem
      await unpublishPoem(poemId);

      // For each collection that contains this poem, remove it
      if (userCollections && userCollections.length > 0) {
        // We need to fetch details for each collection to see if it contains the poem
        for (const collection of userCollections) {
          try {
            const collectionDetails = await getCollection(collection.id);
            if (collectionDetails && collectionDetails.poems) {
              // Check if this collection contains the poem
              const hasPoemInCollection = collectionDetails.poems.some(
                (poem) => poem.id === poemId
              );

              // If the poem is in this collection, remove it
              if (hasPoemInCollection) {
                await removePoemFromCollection(collection.id, poemId);
                console.log(
                  `Removed unpublished poem from collection: ${collection.name}`
                );
              }
            }
          } catch (collectionError) {
            console.error(
              `Error checking collection ${collection.id}:`,
              collectionError
            );
          }
        }
      }

      // Move to drafts by removing from the published list
      setPoems(poems.filter((poem) => poem.id !== poemId));
    } catch (err) {
      console.error("Error unpublishing poem:", err);
      setError("Failed to unpublish poem. Please try again.");
    }
  };

  const handleDelete = async (poemId) => {
    if (
      window.confirm(
        "Are you sure you want to delete this poem? This action cannot be undone."
      )
    ) {
      try {
        await deletePoem(poemId);
        // Update the poems list
        setPoems(poems.filter((poem) => poem.id !== poemId));
      } catch (err) {
        console.error("Error deleting poem:", err);
        setError("Failed to delete poem. Please try again.");
      }
    }
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
    <div className="user-poems">
      <div className="poems-header">
        <h2>My Published Poems</h2>
        <Button to="/cabinet/create-poem" variant="primary">
          <i className="fas fa-plus"></i> New Poem
        </Button>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {poems.length === 0 ? (
        <div className="no-poems">
          <p>You haven't published any poems yet.</p>
          <div className="no-poems-actions">
            <Button to="/cabinet/drafts" variant="primary">
              Go to Your Drafts
            </Button>
            <Link to="/cabinet/create-poem" className="btn btn-primary">
              Create New Poem
            </Link>
          </div>
        </div>
      ) : (
        <div className="poems-grid">
          {poems.map((poem) => (
            <div key={poem.id} className="poem-card">
              <h3 className="poem-title">
                <Link to={`/poems/${poem.id}`}>{poem.title}</Link>
              </h3>
              <div className="poem-meta">
                <span>
                  <i className="fas fa-eye"></i>{" "}
                  {poem.statistics?.viewCount || 0}
                </span>
                <span>
                  <i className="fas fa-heart"></i>{" "}
                  {poem.statistics?.likeCount || 0}
                </span>
                <span>
                  <i className="fas fa-comment"></i>{" "}
                  {poem.statistics?.commentCount || 0}
                </span>
              </div>
              <div className="poem-excerpt">
                {poem.excerpt || poem.content.substring(0, 100)}
              </div>
              <div className="poem-tags">
                {poem.tags &&
                  poem.tags.map((tag, index) => (
                    <span key={index} className="poem-tag">
                      {tag}
                    </span>
                  ))}
              </div>
              <div className="poem-actions">
                <Link to={`/poems/${poem.id}`} className="btn-view">
                  <i className="fas fa-eye"></i> View
                </Link>
                <Link to={`/cabinet/edit-poem/${poem.id}`} className="btn-edit">
                  <i className="fas fa-edit"></i> Edit
                </Link>
                <button
                  className="btn-unpublish"
                  onClick={() => handleUnpublish(poem.id)}
                >
                  <i className="fas fa-archive"></i> Unpublish
                </button>
                <button
                  className="btn-delete"
                  onClick={() => handleDelete(poem.id)}
                >
                  <i className="fas fa-trash"></i> Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default UserPoems;
