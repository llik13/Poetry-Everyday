import React, { useState, useEffect } from "react";
import PageLayout from "../components/layout/PageLayout";
import PoemSearchFilter from "../components/poems/PoemSearchFilter";
import PoemList from "../components/poems/PoemList";
import Pagination from "../components/common/Pagination";
import { getPoems } from "../services/poemService";
import "./CatalogPage.css";

const CatalogPage = () => {
  const [poems, setPoems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [searchParams, setSearchParams] = useState({
    pageNumber: 1,
    pageSize: 12,
    sortBy: "CreatedAt",
    sortDescending: true,
    isPublished: true,
  });

  // Fetch poems when searchParams or currentPage changes
  useEffect(() => {
    // Create a copy of search params with the current page
    const paramsWithPage = {
      ...searchParams,
      pageNumber: currentPage,
    };

    fetchPoems(paramsWithPage);
  }, [searchParams, currentPage]);

  const fetchPoems = async (params) => {
    setLoading(true);
    try {
      console.log("Fetching poems with params:", params);

      const result = await getPoems(params);
      setPoems(result.items || []);
      setTotalPages(result.totalPages || 1);
      setLoading(false);
    } catch (err) {
      console.error("Error fetching poems:", err);
      setError("Failed to load poems. Please try again later.");
      setLoading(false);
    }
  };

  const handleSearch = (filters) => {
    console.log("Applying new search filters:", filters);

    // Reset to page 1 when applying new filters
    setCurrentPage(1);

    // Update search parameters - this will trigger the useEffect
    setSearchParams((prevParams) => ({
      ...prevParams,
      ...filters,
      pageNumber: 1, // Always reset to page 1 with new filters
    }));
  };

  const handlePageChange = (page) => {
    setCurrentPage(page);
    window.scrollTo(0, 0);
  };

  return (
    <PageLayout>
      <main className="catalog-container">
        <h2 className="page-title">Poetry Catalog</h2>

        {/* Search and Filter section */}
        <PoemSearchFilter
          initialFilters={searchParams}
          onSearch={handleSearch}
          loading={loading}
        />

        {/* Error message if any */}
        {error && <div className="error-message">{error}</div>}

        {/* Results info */}
        {!loading && !error && poems.length > 0 && (
          <div className="results-info">
            <div className="results-count">Showing {poems.length} poems</div>
          </div>
        )}

        {/* Poems list */}
        <PoemList poems={poems} loading={loading} />

        {/* Pagination */}
        <Pagination
          currentPage={currentPage}
          totalPages={totalPages}
          onPageChange={handlePageChange}
        />
      </main>
    </PageLayout>
  );
};

export default CatalogPage;
