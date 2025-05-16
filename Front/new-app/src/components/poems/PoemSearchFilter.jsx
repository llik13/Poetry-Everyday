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
  const [sortOption, setSortOption] = useState("");
  const [appliedFilters, setAppliedFilters] = useState([]);

  // Define sort options with clear mappings
  const sortOptions = [
    {
      value: "CreatedAt-desc",
      label: "Newest First",
      apiField: "CreatedAt",
      apiDirection: true,
    },
    {
      value: "CreatedAt-asc",
      label: "Oldest First",
      apiField: "CreatedAt",
      apiDirection: false,
    },
    {
      value: "ViewCount-desc",
      label: "Most Popular",
      apiField: "ViewCount",
      apiDirection: true,
    },
    {
      value: "Title-asc",
      label: "Title (A-Z)",
      apiField: "Title",
      apiDirection: false,
    },
    {
      value: "Title-desc",
      label: "Title (Z-A)",
      apiField: "Title",
      apiDirection: true,
    },
  ];

  // Set initial sort option based on initialFilters
  useEffect(() => {
    if (initialFilters.sortBy && initialFilters.sortDescending !== undefined) {
      const matchingOption = sortOptions.find(
        (option) =>
          option.apiField === initialFilters.sortBy &&
          option.apiDirection === initialFilters.sortDescending
      );

      if (matchingOption) {
        setSortOption(matchingOption.value);
      } else {
        // Default to newest first
        setSortOption("CreatedAt-desc");
      }
    } else {
      // Default to newest first
      setSortOption("CreatedAt-desc");
    }
  }, [initialFilters]);

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
    triggerSearch();
  };

  const triggerSearch = () => {
    // Find the selected sort option to get API parameters
    const selectedOption = sortOptions.find(
      (option) => option.value === sortOption
    );

    if (!selectedOption) {
      console.error("Invalid sort option:", sortOption);
      return;
    }

    const searchParams = {
      searchTerm,
      tags,
      categories,
      sortBy: selectedOption.apiField,
      sortDescending: selectedOption.apiDirection,
    };

    console.log("Search params:", searchParams);
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

    // After removing a filter, trigger search with updated filters
    setTimeout(triggerSearch, 0);
  };

  const handleSortChange = (e) => {
    const newSortValue = e.target.value;
    console.log("Sort changed to:", newSortValue);
    setSortOption(newSortValue);

    // We need to wait for state to update before triggering search
    setTimeout(() => {
      const selectedOption = sortOptions.find(
        (option) => option.value === newSortValue
      );

      if (!selectedOption) {
        console.error("Invalid sort option:", newSortValue);
        return;
      }

      const searchParams = {
        searchTerm,
        tags,
        categories,
        sortBy: selectedOption.apiField,
        sortDescending: selectedOption.apiDirection,
      };

      console.log("Applying sort with params:", searchParams);
      onSearch(searchParams);
    }, 10);
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
            value={sortOption}
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
