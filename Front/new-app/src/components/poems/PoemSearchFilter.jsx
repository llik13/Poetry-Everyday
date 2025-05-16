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
  const [sortBy, setSortBy] = useState(initialFilters.sortBy || "CreatedAt");
  const [sortDirection, setSortDirection] = useState(
    initialFilters.sortDescending !== false
  );
  const [appliedFilters, setAppliedFilters] = useState([]);

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
      newAppliedFilters.push({
        type: "category",
        id: category,
        label: category,
      });
    });

    setAppliedFilters(newAppliedFilters);
  }, [tags, categories]);

  const handleSearch = (e) => {
    e.preventDefault();

    const searchParams = {
      searchTerm,
      tags,
      categories,
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
