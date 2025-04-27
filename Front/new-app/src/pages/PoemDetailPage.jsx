import React, { useState, useEffect, useContext } from "react";
import { useParams, useNavigate } from "react-router-dom";
import PageLayout from "../components/layout/PageLayout";
import PoemDetail from "../components/poems/PoemDetail";
import { getPoemDetails, addPoemToCollection } from "../services/poemService";
import AuthContext from "../context/AuthContext";
import "./PoemDetailPage.css";

const PoemDetailPage = () => {
  const { id } = useParams();
  const { isAuthenticated } = useContext(AuthContext);
  const navigate = useNavigate();

  const [poem, setPoem] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchPoemDetails = async () => {
      try {
        setLoading(true);
        const poemData = await getPoemDetails(id);
        setPoem(poemData);
        setLoading(false);
      } catch (err) {
        console.error("Error fetching poem details:", err);
        setError(
          "Failed to load poem details. The poem may not exist or has been removed."
        );
        setLoading(false);
      }
    };

    if (id) {
      fetchPoemDetails();
    }
  }, [id]);

  const handleSaveToCollection = async (poemId) => {
    if (!isAuthenticated) {
      navigate("/login");
      return;
    }

    try {
      // This would typically show a collection selection UI
      // For now, we're just passing the poem ID
      await addPoemToCollection(poemId);
      // Show success message
    } catch (err) {
      console.error("Error saving poem to collection:", err);
      // Show error message
    }
  };

  if (loading) {
    return (
      <PageLayout>
        <div className="loading-container">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      </PageLayout>
    );
  }

  if (error || !poem) {
    return (
      <PageLayout>
        <div className="error-container">
          <h2>Poem Not Found</h2>
          <p>{error || "The requested poem could not be found."}</p>
          <button className="btn btn-primary" onClick={() => navigate(-1)}>
            Go Back
          </button>
        </div>
      </PageLayout>
    );
  }

  return (
    <PageLayout>
      <PoemDetail poem={poem} onSaveToCollection={handleSaveToCollection} />
    </PageLayout>
  );
};

export default PoemDetailPage;
