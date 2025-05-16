import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import {
  getUserCollections,
  getCollection,
  createCollection,
  deleteCollection,
  removePoemFromCollection,
} from "../../services/poemService";
import Button from "../common/Button";
import "./UserCollections.css";

const UserCollections = () => {
  const [collections, setCollections] = useState([]);
  const [selectedCollection, setSelectedCollection] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [actionStatus, setActionStatus] = useState({ type: "", message: "" });
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [newCollectionName, setNewCollectionName] = useState("");
  const [newCollectionDesc, setNewCollectionDesc] = useState("");
  const [isPublic, setIsPublic] = useState(false);

  useEffect(() => {
    fetchCollections();
  }, []);

  const fetchCollections = async () => {
    try {
      setLoading(true);
      const data = await getUserCollections();
      setCollections(data || []);
      setLoading(false);
    } catch (err) {
      console.error("Error fetching collections:", err);
      setError("Failed to load your collections. Please try again later.");
      setLoading(false);
    }
  };

  const handleViewCollection = async (collectionId) => {
    try {
      setLoading(true);
      const data = await getCollection(collectionId);
      setSelectedCollection(data);
      setLoading(false);
    } catch (err) {
      console.error("Error fetching collection details:", err);
      setError("Failed to load collection details. Please try again.");
      setLoading(false);
    }
  };

  const handleCreateCollection = async (e) => {
    e.preventDefault();
    if (!newCollectionName.trim()) return;

    try {
      const collectionData = {
        name: newCollectionName,
        description: newCollectionDesc,
        isPublic: isPublic,
      };

      await createCollection(collectionData);
      setNewCollectionName("");
      setNewCollectionDesc("");
      setIsPublic(false);
      setShowCreateForm(false);

      setActionStatus({
        type: "success",
        message: "Collection created successfully!",
      });

      // Clear status after 3 seconds
      setTimeout(() => setActionStatus({ type: "", message: "" }), 3000);

      fetchCollections();
    } catch (err) {
      console.error("Error creating collection:", err);
      setActionStatus({
        type: "error",
        message: "Failed to create collection. Please try again.",
      });
    }
  };

  const handleDeleteCollection = async (collectionId) => {
    if (
      window.confirm(
        "Are you sure you want to delete this collection? This action cannot be undone."
      )
    ) {
      try {
        await deleteCollection(collectionId);
        setCollections(collections.filter((c) => c.id !== collectionId));

        if (selectedCollection && selectedCollection.id === collectionId) {
          setSelectedCollection(null);
        }

        setActionStatus({
          type: "success",
          message: "Collection deleted successfully!",
        });

        // Clear status after 3 seconds
        setTimeout(() => setActionStatus({ type: "", message: "" }), 3000);
      } catch (err) {
        console.error("Error deleting collection:", err);
        setActionStatus({
          type: "error",
          message: "Failed to delete collection. Please try again.",
        });
      }
    }
  };

  const handleRemovePoemFromCollection = async (collectionId, poemId) => {
    if (
      window.confirm(
        "Are you sure you want to remove this poem from the collection?"
      )
    ) {
      try {
        await removePoemFromCollection(collectionId, poemId);

        // Update the local state to reflect the change
        if (selectedCollection && selectedCollection.id === collectionId) {
          setSelectedCollection({
            ...selectedCollection,
            poems: selectedCollection.poems.filter(
              (poem) => poem.id !== poemId
            ),
            poemCount: selectedCollection.poemCount - 1,
          });
        }

        // Update the collections list with updated poem count
        setCollections(
          collections.map((collection) => {
            if (collection.id === collectionId) {
              return {
                ...collection,
                poemCount: collection.poemCount - 1,
              };
            }
            return collection;
          })
        );

        setActionStatus({
          type: "success",
          message: "Poem removed from collection successfully!",
        });

        // Clear status after 3 seconds
        setTimeout(() => setActionStatus({ type: "", message: "" }), 3000);
      } catch (err) {
        console.error("Error removing poem from collection:", err);
        setActionStatus({
          type: "error",
          message: "Failed to remove poem from collection. Please try again.",
        });
      }
    }
  };

  if (loading && !selectedCollection && collections.length === 0) {
    return (
      <div className="loading-container">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="user-collections">
      <div className="collections-header">
        <h2>My Collections</h2>
        <Button
          variant="primary"
          onClick={() => setShowCreateForm(!showCreateForm)}
        >
          <i className="fas fa-plus"></i> New Collection
        </Button>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {actionStatus.message && (
        <div
          className={`alert ${
            actionStatus.type === "success" ? "alert-success" : "alert-danger"
          }`}
        >
          {actionStatus.message}
        </div>
      )}

      {showCreateForm && (
        <div className="create-collection-form">
          <h3>Create New Collection</h3>
          <form onSubmit={handleCreateCollection}>
            <div className="form-group">
              <label htmlFor="collection-name">Collection Name</label>
              <input
                type="text"
                id="collection-name"
                className="form-control"
                value={newCollectionName}
                onChange={(e) => setNewCollectionName(e.target.value)}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="collection-desc">Description</label>
              <textarea
                id="collection-desc"
                className="form-control"
                value={newCollectionDesc}
                onChange={(e) => setNewCollectionDesc(e.target.value)}
                rows="3"
              />
            </div>
            <div className="form-check">
              <input
                type="checkbox"
                id="is-public"
                className="form-check-input"
                checked={isPublic}
                onChange={(e) => setIsPublic(e.target.checked)}
              />
              <label htmlFor="is-public" className="form-check-label">
                Make this collection public
              </label>
            </div>
            <div className="form-actions">
              <Button type="submit" variant="primary">
                Create Collection
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => setShowCreateForm(false)}
              >
                Cancel
              </Button>
            </div>
          </form>
        </div>
      )}

      {collections.length === 0 && !loading ? (
        <div className="no-collections">
          <p>You don't have any collections yet.</p>
          <Button variant="primary" onClick={() => setShowCreateForm(true)}>
            Create Your First Collection
          </Button>
        </div>
      ) : (
        <div className="collections-container">
          <div className="collections-list">
            {collections.map((collection) => (
              <div
                key={collection.id}
                className={`collection-item ${
                  selectedCollection && selectedCollection.id === collection.id
                    ? "active"
                    : ""
                }`}
                onClick={() => handleViewCollection(collection.id)}
              >
                <div className="collection-info">
                  <h3 className="collection-name">{collection.name}</h3>
                  <p className="collection-count">
                    {collection.poemCount}{" "}
                    {collection.poemCount === 1 ? "poem" : "poems"}
                  </p>
                </div>
                <div className="collection-actions">
                  <button
                    className="btn-delete-collection"
                    onClick={(e) => {
                      e.stopPropagation();
                      handleDeleteCollection(collection.id);
                    }}
                  >
                    <i className="fas fa-trash"></i>
                  </button>
                </div>
              </div>
            ))}
          </div>

          {selectedCollection && (
            <div className="collection-details">
              <div className="collection-details-header">
                <h3>{selectedCollection.name}</h3>
                {selectedCollection.isPublic && (
                  <span className="public-badge">Public</span>
                )}
              </div>
              <p className="collection-description">
                {selectedCollection.description || "No description provided."}
              </p>

              {selectedCollection.poems &&
              selectedCollection.poems.length > 0 ? (
                <div className="collection-poems">
                  <h4>Poems in this collection</h4>
                  <div className="table-responsive">
                    <table className="table">
                      <thead>
                        <tr>
                          <th>Title</th>
                          <th>Author</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {selectedCollection.poems.map((poem) => (
                          <tr key={poem.id}>
                            <td>
                              <Link to={`/poems/${poem.id}`}>{poem.title}</Link>
                            </td>
                            <td>{poem.authorName}</td>
                            <td>
                              <button
                                className="btn-remove-from-collection"
                                onClick={() =>
                                  handleRemovePoemFromCollection(
                                    selectedCollection.id,
                                    poem.id
                                  )
                                }
                              >
                                <i className="fas fa-times"></i> Remove
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              ) : (
                <div className="no-poems-in-collection">
                  <p>
                    This collection is empty. Add poems while browsing the
                    catalog.
                  </p>
                </div>
              )}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default UserCollections;
