import React, { useState, useEffect } from "react";
import {
  getUserCollections,
  addPoemToCollection,
} from "../../services/poemService";
import Button from "../common/Button";
import "./CollectionModal.css";

const CollectionModal = ({ isOpen, onClose, poemId, onSuccess }) => {
  const [collections, setCollections] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedCollectionId, setSelectedCollectionId] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState("");

  // Fetch user collections when modal opens
  useEffect(() => {
    if (isOpen) {
      fetchCollections();
    }
  }, [isOpen]);

  const fetchCollections = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getUserCollections();
      setCollections(data || []);

      // Preselect the first collection if available
      if (data && data.length > 0) {
        setSelectedCollectionId(data[0].id);
      }

      setLoading(false);
    } catch (err) {
      console.error("Error fetching collections:", err);
      setError("Failed to load your collections. Please try again later.");
      setLoading(false);
    }
  };

  const handleCollectionSelect = (collectionId) => {
    setSelectedCollectionId(collectionId);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!selectedCollectionId) {
      setError("Please select a collection.");
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);

      await addPoemToCollection(selectedCollectionId, poemId);

      setSuccessMessage("Poem added to collection successfully!");

      // Reset and notify parent after short delay
      setTimeout(() => {
        if (onSuccess) onSuccess();
        setSuccessMessage("");
        onClose();
      }, 1500);
    } catch (err) {
      console.error("Error adding poem to collection:", err);
      setError("Failed to add poem to collection. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  // If the modal is not open, don't render anything
  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="collection-modal">
        <div className="modal-header">
          <h3>Save to Collection</h3>
          <button className="close-button" onClick={onClose}>
            ×
          </button>
        </div>

        <div className="modal-body">
          {loading ? (
            <div className="loading-container">
              <div className="spinner-border" role="status">
                <span className="visually-hidden">Loading...</span>
              </div>
            </div>
          ) : error ? (
            <div className="error-message">{error}</div>
          ) : successMessage ? (
            <div className="success-message">{successMessage}</div>
          ) : collections.length === 0 ? (
            <div className="no-collections">
              <p>You don't have any collections yet.</p>
              <Button
                to="/cabinet/collections"
                variant="primary"
                onClick={onClose}
              >
                Create a Collection
              </Button>
            </div>
          ) : (
            <form onSubmit={handleSubmit}>
              <p>Select a collection to save this poem:</p>
              <div className="collections-list">
                {collections.map((collection) => (
                  <div
                    key={collection.id}
                    className={`collection-item ${
                      selectedCollectionId === collection.id ? "selected" : ""
                    }`}
                    onClick={() => handleCollectionSelect(collection.id)}
                  >
                    <div className="collection-radio">
                      <input
                        type="radio"
                        name="collection"
                        id={`collection-${collection.id}`}
                        value={collection.id}
                        checked={selectedCollectionId === collection.id}
                        onChange={() => handleCollectionSelect(collection.id)}
                      />
                    </div>
                    <div className="collection-info">
                      <label
                        htmlFor={`collection-${collection.id}`}
                        className="collection-name"
                      >
                        {collection.name}
                      </label>
                      <span className="collection-count">
                        {collection.poemCount}{" "}
                        {collection.poemCount === 1 ? "poem" : "poems"}
                      </span>
                    </div>
                  </div>
                ))}
              </div>

              <div className="modal-actions">
                <Button
                  type="button"
                  variant="outline"
                  onClick={onClose}
                  disabled={isSubmitting}
                >
                  Cancel
                </Button>
                <Button
                  type="submit"
                  variant="primary"
                  disabled={!selectedCollectionId || isSubmitting}
                >
                  {isSubmitting ? "Saving..." : "Save"}
                </Button>
              </div>
            </form>
          )}
        </div>
      </div>
    </div>
  );
};

export default CollectionModal;
