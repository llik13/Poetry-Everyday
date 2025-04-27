import React, { useState, useEffect } from "react";
import "./PoemSearchFilter.css";

const PoemSearchFilter = ({
  initialFilters = {},
  onSearch,
  loading = false,
}) => {
  const [searchTerm, setSearchTerm] = useState(initialFilters.searchTerm || "");
  const [tags, setTags] = useState(initialFilters.tags || []);
  const [categories, setCategories] = useState(initialFilters.categories || []);
  const [author, setAuthor] = useState(initialFilters.authorId || "");
  const [sortBy, setSortBy] = useState(initialFilters.sortBy || "CreatedAt");
  const [sortDirection, setSortDirection] = useState(
    initialFilters.sortDescending !== false
  );
  const [appliedFilters, setAppliedFilters] = useState([]);

  // List of pre-defined filters (would typically come from API)
  const authorOptions = [
    { id: "", name: "All Authors" },
    { id: "1", name: "William Shakespeare" },
    { id: "2", name: "Emily Dickinson" },
    { id: "3", name: "Robert Frost" },
    { id: "4", name: "Edgar Allan Poe" },
    { id: "5", name: "John Keats" },
  ];

  const categoryOptions = [
    { value: "", label: "All Categories" },
    { value: "nature", label: "Nature" },
    { value: "love", label: "Love" },
    { value: "philosophy", label: "Philosophy" },
    { value: "life", label: "Life & Death" },
    { value: "seasons", label: "Seasons" },
  ];

  const timeOptions = [
    { value: "", label: "All Periods" },
    { value: "16th", label: "16th Century" },
    { value: "18th", label: "18th Century" },
    { value: "19th", label: "19th Century" },
    { value: "20th", label: "20th Century" },
  ];

  const formOptions = [
    { value: "", label: "All Forms" },
    { value: "sonnet", label: "Sonnet" },
    { value: "ode", label: "Ode" },
    { value: "elegy", label: "Elegy" },
    { value: "free", label: "Free Verse" },
    { value: "ballad", label: "Ballad" },
  ];

  const sortOptions = [
    { value: "relevance", label: "Relevance" },
    { value: "CreatedAt", label: "Newest First" },
    { value: "CreatedAt-asc", label: "Oldest First" },
    { value: "ViewCount", label: "Most Popular" },
    { value: "Title", label: "Title (A-Z)" },
    { value: "Title-desc", label: "Title (Z-A)" },
  ];

  // Update applied filters when inputs change
  useEffect(() => {
    const newAppliedFilters = [];

    // Author filter
    if (author) {
      const authorName = authorOptions.find((a) => a.id === author)?.name;
      if (authorName) {
        newAppliedFilters.push({
          type: "author",
          id: author,
          label: authorName,
        });
      }
    }

    // Add tag filters
    tags.forEach((tag) => {
      newAppliedFilters.push({
        type: "tag",
        id: tag,
        label: tag,
      });
    });

    // Add category filters
    categories.forEach((category) => {
      const categoryLabel = categoryOptions.find(
        (c) => c.value === category
      )?.label;
      if (categoryLabel) {
        newAppliedFilters.push({
          type: "category",
          id: category,
          label: categoryLabel,
        });
      }
    });

    setAppliedFilters(newAppliedFilters);
  }, [author, tags, categories]);

  const handleSearch = (e) => {
    e.preventDefault();

    const searchParams = {
      searchTerm,
      tags,
      categories,
      authorId: author || undefined,
      sortBy: sortBy.split("-")[0],
      sortDescending:
        sortBy === "relevance"
          ? true // Relevance is always descending
          : sortBy.includes("-desc") ||
            (!sortBy.includes("-asc") && sortDirection),
    };

    onSearch(searchParams);
  };

  const handleRemoveFilter = (filter) => {
    switch (filter.type) {
      case "author":
        setAuthor("");
        break;
      case "tag":
        setTags(tags.filter((t) => t !== filter.id));
        break;
      case "category":
        setCategories(categories.filter((c) => c !== filter.id));
        break;
      default:
        break;
    }
  };

  const handleSortChange = (e) => {
    const value = e.target.value;
    setSortBy(value);

    // Update sort direction based on the selected option
    if (value.includes("-asc")) {
      setSortDirection(false);
    } else if (value.includes("-desc")) {
      setSortDirection(true);
    }
  };

  return (
    <div className="search-filter-section">
      <form onSubmit={handleSearch}>
        <div className="search-bar">
          <input
            type="text"
            className="search-input"
            placeholder="Search poems, authors, or themes..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
          <button type="submit" className="search-button" disabled={loading}>
            {loading ? (
              <span className="spinner-border spinner-border-sm" />
            ) : (
              <i className="fas fa-search"></i>
            )}
          </button>
        </div>

        <div className="filter-row">
          <div className="filter-group">
            <label className="filter-label" htmlFor="author-filter">
              Author
            </label>
            <select
              className="filter-select"
              id="author-filter"
              value={author}
              onChange={(e) => setAuthor(e.target.value)}
            >
              {authorOptions.map((option) => (
                <option key={option.id} value={option.id}>
                  {option.name}
                </option>
              ))}
            </select>
          </div>

          <div className="filter-group">
            <label className="filter-label" htmlFor="category-filter">
              Category
            </label>
            <select
              className="filter-select"
              id="category-filter"
              value={categories[0] || ""}
              onChange={(e) => {
                if (e.target.value) {
                  setCategories([e.target.value]);
                } else {
                  setCategories([]);
                }
              }}
            >
              {categoryOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          <div className="filter-group">
            <label className="filter-label" htmlFor="time-filter">
              Time Period
            </label>
            <select className="filter-select" id="time-filter">
              {timeOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>

          <div className="filter-group">
            <label className="filter-label" htmlFor="form-filter">
              Form
            </label>
            <select className="filter-select" id="form-filter">
              {formOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* Applied Filters Section */}
        {appliedFilters.length > 0 && (
          <div className="filter-tags">
            {appliedFilters.map((filter, index) => (
              <div key={index} className="filter-tag">
                {filter.label}{" "}
                <span
                  className="tag-remove"
                  onClick={() => handleRemoveFilter(filter)}
                >
                  ✕
                </span>
              </div>
            ))}
          </div>
        )}
      </form>

      {/* Results and Sort Controls */}
      <div className="results-info">
        <div className="sort-options">
          <span className="sort-label">Sort by:</span>
          <select
            className="sort-select"
            value={sortBy}
            onChange={handleSortChange}
          >
            {sortOptions.map((option) => (
              <option key={option.value} value={option.value}>
                {option.label}
              </option>
            ))}
          </select>
        </div>
      </div>
    </div>
  );
};

export default PoemSearchFilter;
