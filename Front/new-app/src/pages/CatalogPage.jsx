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

  // Fetch poems on initial load and when search params change
  useEffect(() => {
    fetchPoems();
  }, [currentPage, searchParams]);

  const fetchPoems = async () => {
    setLoading(true);
    try {
      const result = await getPoems({
        ...searchParams,
        pageNumber: currentPage,
      });
      setPoems(result.items);
      setTotalPages(result.totalPages);
      setLoading(false);
    } catch (err) {
      console.error("Error fetching poems:", err);
      setError("Failed to load poems. Please try again later.");
      setLoading(false);
    }
  };

  const handleSearch = (filters) => {
    // Reset to page 1 when applying new filters
    setCurrentPage(1);
    setSearchParams({
      ...searchParams,
      ...filters,
      pageNumber: 1,
    });
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
