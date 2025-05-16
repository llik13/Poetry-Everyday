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

      // Создаем объект данных для отправки на сервер
      const submissionData = {
        title: poemData.title,
        content: poemData.content,
        excerpt: poemData.excerpt,
        authorId: currentUser?.id,
        authorName: currentUser?.username,
        tags: poemData.tags || [],
        categories: poemData.categories || [],
        isPublished: publish, // Важно: здесь устанавливаем статус публикации
      };

      let savedPoem;

      if (isEditMode) {
        // Обновление существующего стихотворения
        savedPoem = await updatePoem({
          ...submissionData,
          id: id,
          changeNotes: "Updated poem",
        });
      } else {
        // Создание нового стихотворения
        savedPoem = await createPoem(submissionData);
      }

      setSuccess(true);
      setLoading(false);

      // Редирект после короткой задержки для показа сообщения об успехе
      setTimeout(() => {
        if (publish) {
          // Если опубликовано, перенаправляем на публичную страницу стихотворения
          navigate(`/poems/${savedPoem.id}`);
        } else {
          // Если сохранено как черновик, перенаправляем на страницу черновиков
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
