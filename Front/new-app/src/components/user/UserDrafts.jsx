import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import {
  getUserDrafts,
  deletePoem,
  publishPoem,
} from "../../services/poemService";
import Button from "../common/Button";
import "./UserDrafts.css";

const UserDrafts = () => {
  const [drafts, setDrafts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchDrafts = async () => {
      try {
        setLoading(true);
        const data = await getUserDrafts();
        setDrafts(data.items || []);
        setLoading(false);
      } catch (err) {
        console.error("Error fetching drafts:", err);
        setError("Failed to load your drafts. Please try again later.");
        setLoading(false);
      }
    };

    fetchDrafts();
  }, []);

  const handlePublish = async (draftId) => {
    try {
      await publishPoem(draftId);
      // Update the drafts list by removing the published poem
      setDrafts(drafts.filter((draft) => draft.id !== draftId));
    } catch (err) {
      console.error("Error publishing poem:", err);
      setError("Failed to publish poem. Please try again.");
    }
  };

  const handleDelete = async (draftId) => {
    if (
      window.confirm(
        "Are you sure you want to delete this draft? This action cannot be undone."
      )
    ) {
      try {
        await deletePoem(draftId);
        // Update the drafts list
        setDrafts(drafts.filter((draft) => draft.id !== draftId));
      } catch (err) {
        console.error("Error deleting draft:", err);
        setError("Failed to delete draft. Please try again.");
      }
    }
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
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
    <div className="user-drafts">
      <div className="drafts-header">
        <h2>My Drafts</h2>
        <Button to="/cabinet/create-poem" variant="primary">
          <i className="fas fa-plus"></i> New Poem
        </Button>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {drafts.length === 0 ? (
        <div className="no-drafts">
          <p>You don't have any drafts yet. Start writing your first poem!</p>
          <Button to="/cabinet/create-poem" variant="primary" size="lg">
            Create New Poem
          </Button>
        </div>
      ) : (
        <div className="drafts-list">
          <div className="table-responsive">
            <table className="table table-hover">
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Last Updated</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {drafts.map((draft) => (
                  <tr key={draft.id}>
                    <td>
                      <Link to={`/cabinet/edit-poem/${draft.id}`}>
                        {draft.title || "Untitled"}
                      </Link>
                    </td>
                    <td>{formatDate(draft.updatedAt || draft.createdAt)}</td>
                    <td className="draft-actions">
                      <Button
                        to={`/cabinet/edit-poem/${draft.id}`}
                        variant="outline"
                        size="sm"
                      >
                        <i className="fas fa-edit"></i> Edit
                      </Button>
                      <Button
                        variant="primary"
                        size="sm"
                        onClick={() => handlePublish(draft.id)}
                      >
                        <i className="fas fa-upload"></i> Publish
                      </Button>
                      <Button
                        variant="secondary"
                        size="sm"
                        onClick={() => handleDelete(draft.id)}
                      >
                        <i className="fas fa-trash"></i> Delete
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

export default UserDrafts;
