import React, { useState, useContext, useEffect } from "react";
import { useNavigate, useParams } from "react-router-dom";
import PageLayout from "../components/layout/PageLayout";
import PoemForm from "../components/poems/PoemForm";
import AuthContext from "../context/AuthContext";
import { createPoem, getPoem, updatePoem } from "../services/poemService";
import "./CreatePoemPage.css";

const CreatePoemPage = () => {
  const { id } = useParams(); // If present, we're editing a poem
  const { currentUser } = useContext(AuthContext);
  const navigate = useNavigate();

  const [loading, setLoading] = useState(id ? true : false);
  const [error, setError] = useState(null);
  const [success, setSuccess] = useState(false);
  const [initialPoemData, setInitialPoemData] = useState(null);

  // Check if we're in edit mode (id exists) or create mode
  const isEditMode = !!id;

  // Fetch poem data if we're in edit mode
  useEffect(() => {
    const fetchPoemData = async () => {
      if (!isEditMode) return;

      try {
        setLoading(true);
        const poemData = await getPoem(id);
        setInitialPoemData(poemData);
        setLoading(false);
      } catch (err) {
        console.error("Error fetching poem data:", err);
        setError("Failed to load poem. Please try again later.");
        setLoading(false);
      }
    };

    fetchPoemData();
  }, [id, isEditMode]);

  const handleSubmit = async (poemData, publish) => {
    try {
      setLoading(true);
      setError(null);

      let savedPoem;

      // Ensure the current user is set as the author
      const submissionData = {
        ...poemData,
        authorId: currentUser?.id,
        authorName: currentUser?.username,
        // If publish is true, set isPublished to true
        isPublished: publish,
      };

      if (isEditMode) {
        // Update existing poem
        savedPoem = await updatePoem({
          ...submissionData,
          id: id,
          // Add any additional fields needed for updating
          changeNotes: "Updated poem",
        });
      } else {
        // Create new poem
        savedPoem = await createPoem(submissionData);
      }

      setSuccess(true);
      setLoading(false);

      // Redirect after a short delay to show success message
      setTimeout(() => {
        if (publish) {
          // If published, redirect to the poem's public page
          navigate(`/poems/${savedPoem.id}`);
        } else {
          // If saved as draft, redirect to the drafts page
          navigate("/cabinet/drafts");
        }
      }, 1500);
    } catch (err) {
      console.error("Error saving poem:", err);
      setError("Failed to save poem. Please try again.");
      setLoading(false);
    }
  };

  return (
    <PageLayout>
      <div className="create-poem-container">
        <div className="create-poem-header">
          <h2>{isEditMode ? "Edit Poem" : "Create New Poem"}</h2>
          <p>
            {isEditMode
              ? "Make changes to your poem and save or publish."
              : "Express yourself through the art of poetry. Save as a draft or publish directly."}
          </p>
        </div>

        {error && <div className="error-message">{error}</div>}

        {success && (
          <div className="success-message">
            Poem {isEditMode ? "updated" : "created"} successfully!
            Redirecting...
          </div>
        )}

        {loading ? (
          <div className="loading-container">
            <div className="spinner-border" role="status">
              <span className="visually-hidden">Loading...</span>
            </div>
          </div>
        ) : (
          <PoemForm
            initialData={initialPoemData}
            onSubmit={handleSubmit}
            isEditMode={isEditMode}
          />
        )}
      </div>
    </PageLayout>
  );
};

export default CreatePoemPage;
