import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Button from "../common/Button";
import "./PoemForm.css";

const PoemForm = ({ initialData, onSubmit, isEditMode }) => {
  const navigate = useNavigate();

  // Form state with default values
  const [formData, setFormData] = useState({
    title: "",
    content: "",
    excerpt: "",
    tags: [],
    categories: [],
    isPublished: false,
  });

  // Track current tag/category input state
  const [currentTag, setCurrentTag] = useState("");
  const [currentCategory, setCurrentCategory] = useState("");

  // Update form if initialData changes (edit mode)
  useEffect(() => {
    if (initialData) {
      setFormData({
        title: initialData.title || "",
        content: initialData.content || "",
        excerpt: initialData.excerpt || "",
        tags: initialData.tags || [],
        categories: initialData.categories || [],
        isPublished: initialData.isPublished || false,
      });
    }
  }, [initialData]);

  // Auto-generate excerpt from content if none provided
  useEffect(() => {
    if (!formData.excerpt && formData.content) {
      // Get first 100 characters or first 3 lines, whichever is shorter
      const lines = formData.content.split("\n").slice(0, 3).join("\n");
      const shortened = formData.content.substring(0, 100);
      const excerpt = lines.length < shortened.length ? lines : shortened;
      setFormData((prev) => ({ ...prev, excerpt: excerpt.trim() }));
    }
  }, [formData.content, formData.excerpt]);

  // Handle form field changes
  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  // Add a tag to the tags array
  const handleAddTag = (e) => {
    e.preventDefault();
    if (currentTag.trim() && !formData.tags.includes(currentTag.trim())) {
      setFormData((prev) => ({
        ...prev,
        tags: [...prev.tags, currentTag.trim()],
      }));
      setCurrentTag("");
    }
  };

  // Remove a tag from the tags array
  const handleRemoveTag = (tag) => {
    setFormData((prev) => ({
      ...prev,
      tags: prev.tags.filter((t) => t !== tag),
    }));
  };

  // Add a category to the categories array
  const handleAddCategory = (e) => {
    e.preventDefault();
    if (
      currentCategory.trim() &&
      !formData.categories.includes(currentCategory.trim())
    ) {
      setFormData((prev) => ({
        ...prev,
        categories: [...prev.categories, currentCategory.trim()],
      }));
      setCurrentCategory("");
    }
  };

  // Remove a category from the categories array
  const handleRemoveCategory = (category) => {
    setFormData((prev) => ({
      ...prev,
      categories: prev.categories.filter((c) => c !== category),
    }));
  };

  // Save as draft (isPublished = false)
  const handleSaveDraft = (e) => {
    e.preventDefault();
    onSubmit(formData, false);
  };

  // Publish poem (isPublished = true)
  const handlePublish = (e) => {
    e.preventDefault();
    onSubmit(formData, true);
  };

  // Cancel and go back
  const handleCancel = () => {
    if (
      window.confirm(
        "Are you sure you want to cancel? Any unsaved changes will be lost."
      )
    ) {
      navigate(-1);
    }
  };

  return (
    <div className="poem-form-container">
      <form className="poem-form">
        <div className="form-group">
          <label htmlFor="title">Title</label>
          <input
            type="text"
            id="title"
            name="title"
            className="form-control"
            value={formData.title}
            onChange={handleChange}
            placeholder="Enter poem title"
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="content">Poem Content</label>
          <textarea
            id="content"
            name="content"
            className="form-control poem-content"
            value={formData.content}
            onChange={handleChange}
            placeholder="Write your poem here..."
            rows="15"
            required
          ></textarea>
          <small className="form-text">
            Use line breaks to format your poem. You can add spacing between
            stanzas.
          </small>
        </div>

        <div className="form-group">
          <label htmlFor="excerpt">Excerpt (Preview)</label>
          <textarea
            id="excerpt"
            name="excerpt"
            className="form-control"
            value={formData.excerpt}
            onChange={handleChange}
            placeholder="Brief preview of your poem (auto-generated if left empty)"
            rows="3"
          ></textarea>
          <small className="form-text">
            This will be shown in poem listings. If left empty, it will be
            generated automatically.
          </small>
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="tags">Tags</label>
            <div className="input-group">
              <input
                type="text"
                id="tags"
                className="form-control"
                value={currentTag}
                onChange={(e) => setCurrentTag(e.target.value)}
                placeholder="Add tags..."
              />
              <button
                className="btn-add"
                onClick={handleAddTag}
                disabled={!currentTag.trim()}
              >
                Add
              </button>
            </div>
            <small className="form-text">
              Tags help readers find your poem. Press Add or Enter after each
              tag.
            </small>

            {formData.tags.length > 0 && (
              <div className="tags-list">
                {formData.tags.map((tag, index) => (
                  <span key={index} className="tag">
                    {tag}
                    <button
                      type="button"
                      className="btn-remove-tag"
                      onClick={() => handleRemoveTag(tag)}
                    >
                      ×
                    </button>
                  </span>
                ))}
              </div>
            )}
          </div>

          <div className="form-group">
            <label htmlFor="categories">Categories</label>
            <div className="input-group">
              <input
                type="text"
                id="categories"
                className="form-control"
                value={currentCategory}
                onChange={(e) => setCurrentCategory(e.target.value)}
                placeholder="Add categories..."
              />
              <button
                className="btn-add"
                onClick={handleAddCategory}
                disabled={!currentCategory.trim()}
              >
                Add
              </button>
            </div>
            <small className="form-text">
              Categories help organize your poem. Examples: Nature, Love,
              Philosophy.
            </small>

            {formData.categories.length > 0 && (
              <div className="categories-list">
                {formData.categories.map((category, index) => (
                  <span key={index} className="category">
                    {category}
                    <button
                      type="button"
                      className="btn-remove-category"
                      onClick={() => handleRemoveCategory(category)}
                    >
                      ×
                    </button>
                  </span>
                ))}
              </div>
            )}
          </div>
        </div>

        <div className="form-actions">
          <Button type="button" variant="secondary" onClick={handleCancel}>
            Cancel
          </Button>
          <Button type="button" variant="outline" onClick={handleSaveDraft}>
            Save as Draft
          </Button>
          <Button
            type="button"
            variant="primary"
            onClick={handlePublish}
            disabled={!formData.title || !formData.content}
          >
            {isEditMode ? "Update & Publish" : "Publish Poem"}
          </Button>
        </div>
      </form>
    </div>
  );
};

export default PoemForm;
