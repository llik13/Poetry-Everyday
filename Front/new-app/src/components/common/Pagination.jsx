import React, { useEffect, useState } from "react";
import "./Pagination.css";

const Pagination = ({ currentPage, totalPages, onPageChange }) => {
  // Internal state for currentPage to ensure UI consistency
  const [activePage, setActivePage] = useState(currentPage);

  // Sync with external currentPage prop when it changes
  useEffect(() => {
    if (currentPage !== activePage) {
      setActivePage(currentPage);
    }
  }, [currentPage]);

  // Ensure totalPages is at least 1
  const validTotalPages = Math.max(1, totalPages || 1);

  // Ensure currentPage is within valid range
  const validCurrentPage = Math.max(1, Math.min(activePage, validTotalPages));

  // Return null if there's only one page (but maintain the container in parent)
  if (validTotalPages <= 1) {
    return null;
  }

  // Debug logging
  console.log(
    `Rendering Pagination - Active Page: ${validCurrentPage}, Total Pages: ${validTotalPages}`
  );

  // Build the page numbers to display
  const getPageNumbers = () => {
    const pages = [];
    const maxPagesToShow = 5;

    if (validTotalPages <= maxPagesToShow) {
      // If we have 5 or fewer pages, show all of them
      for (let i = 1; i <= validTotalPages; i++) {
        pages.push(i);
      }
    } else {
      // Always show the first page
      pages.push(1);

      // Calculate the range to show around the current page
      let startPage = Math.max(2, validCurrentPage - 1);
      let endPage = Math.min(validTotalPages - 1, validCurrentPage + 1);

      // Handle edge cases
      if (validCurrentPage <= 2) {
        // Close to the beginning, show more pages after the current page
        endPage = Math.min(validTotalPages - 1, 4);
      } else if (validCurrentPage >= validTotalPages - 1) {
        // Close to the end, show more pages before the current page
        startPage = Math.max(2, validTotalPages - 3);
      }

      // Add ellipsis if needed before the range
      if (startPage > 2) {
        pages.push("...");
      }

      // Add the range of pages
      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }

      // Add ellipsis if needed after the range
      if (endPage < validTotalPages - 1) {
        pages.push("...");
      }

      // Always show the last page
      if (validTotalPages > 1) {
        pages.push(validTotalPages);
      }
    }

    return pages;
  };

  const pageNumbers = getPageNumbers();

  const handlePageClick = (page) => {
    if (page !== "..." && page !== validCurrentPage) {
      console.log(`Page button clicked: ${page}`);
      // Update internal state for immediate UI feedback
      setActivePage(page);
      // Notify parent component
      onPageChange(page);
    }
  };

  return (
    <ul className="pagination">
      {/* Previous button */}
      <li className={`page-item ${validCurrentPage === 1 ? "disabled" : ""}`}>
        <button
          type="button"
          className="page-link"
          onClick={() => handlePageClick(validCurrentPage - 1)}
          disabled={validCurrentPage === 1}
          aria-label="Previous page"
        >
          Previous
        </button>
      </li>

      {/* Page numbers */}
      {pageNumbers.map((page, index) => (
        <li
          key={index}
          className={`page-item ${page === validCurrentPage ? "active" : ""} ${
            page === "..." ? "disabled" : ""
          }`}
        >
          <button
            type="button"
            className="page-link"
            onClick={() => handlePageClick(page)}
            disabled={page === "..." || page === validCurrentPage}
            aria-label={page === "..." ? "More pages" : `Page ${page}`}
            aria-current={page === validCurrentPage ? "page" : undefined}
          >
            {page}
          </button>
        </li>
      ))}

      {/* Next button */}
      <li
        className={`page-item ${
          validCurrentPage === validTotalPages ? "disabled" : ""
        }`}
      >
        <button
          type="button"
          className="page-link"
          onClick={() => handlePageClick(validCurrentPage + 1)}
          disabled={validCurrentPage === validTotalPages}
          aria-label="Next page"
        >
          Next
        </button>
      </li>
    </ul>
  );
};

export default Pagination;
