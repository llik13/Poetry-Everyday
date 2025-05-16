import React, { useState, useEffect, useContext } from "react";
import { useParams, useNavigate } from "react-router-dom";
import PageLayout from "../components/layout/PageLayout";
import PoemDetail from "../components/poems/PoemDetail";
import { getPoemDetails, getPoemComments } from "../services/poemService";
import AuthContext from "../context/AuthContext";
import "./PoemDetailPage.css";

const PoemDetailPage = () => {
  const { id } = useParams();
  const { isAuthenticated } = useContext(AuthContext);
  const navigate = useNavigate();

  const [poem, setPoem] = useState(null);
  const [comments, setComments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchPoemDetails = async () => {
      try {
        setLoading(true);

        // Get basic poem details
        const poemData = await getPoemDetails(id);

        // Fetch initial comments (first page)
        const commentsData = await getPoemComments(id, 1, 10);

        // Update the poem data
        if (Array.isArray(commentsData)) {
          setComments(commentsData);
          poemData.comments = commentsData;
        } else if (commentsData && typeof commentsData === "object") {
          setComments(commentsData.items || []);
          poemData.comments = commentsData.items || [];
        }

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
      <PoemDetail poem={poem} />
    </PageLayout>
  );
};

export default PoemDetailPage;
