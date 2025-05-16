import api from "./api";

// Get a list of poems with filters
export const getPoems = async (searchParams) => {
  try {
    // Build query string from search params
    const queryParams = new URLSearchParams();

    if (searchParams.searchTerm) {
      queryParams.append("SearchTerm", searchParams.searchTerm);
    }

    if (searchParams.tags && searchParams.tags.length > 0) {
      searchParams.tags.forEach((tag) => queryParams.append("Tags", tag));
    }

    if (searchParams.categories && searchParams.categories.length > 0) {
      searchParams.categories.forEach((category) =>
        queryParams.append("Categories", category)
      );
    }

    if (searchParams.authorId) {
      queryParams.append("AuthorId", searchParams.authorId);
    }

    queryParams.append("PageNumber", searchParams.pageNumber || 1);
    queryParams.append("PageSize", searchParams.pageSize || 10);

    // Handle SortBy param - default to CreatedAt if not specified
    if (searchParams.sortBy) {
      queryParams.append("SortBy", searchParams.sortBy);
    } else {
      queryParams.append("SortBy", "CreatedAt");
    }

    // Handle SortDescending param - convert to string and make sure default is true
    let sortDescending = true;
    if (typeof searchParams.sortDescending === "boolean") {
      sortDescending = searchParams.sortDescending;
    }

    queryParams.append("SortDescending", sortDescending.toString());

    // Handle IsPublished param
    const isPublished = searchParams.isPublished ?? true;
    queryParams.append("IsPublished", isPublished.toString());

    console.log("API Request with params:", {
      sortBy: searchParams.sortBy,
      sortDescending: sortDescending,
      fullUrl: `/poems?${queryParams.toString()}`,
    });

    const response = await api.get(`/poems?${queryParams.toString()}`);
    return response.data;
  } catch (error) {
    console.error("Error fetching poems:", error);
    throw error;
  }
};

// Get detailed information for a single poem
export const getPoemDetails = async (poemId) => {
  try {
    const response = await api.get(`/poems/poemDetailed/${poemId}`);

    // Track the view asynchronously
    await incrementPoemViews(poemId).catch((error) =>
      console.error("Error tracking view:", error)
    );

    return response.data;
  } catch (error) {
    throw error;
  }
};

// Get poems by author
export const getPoemsByAuthor = async (authorId) => {
  try {
    const response = await api.get(`/poems/author/${authorId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Get content of a poem
export const getPoemContent = async (poemId) => {
  try {
    const response = await api.get(`/poems/content/${poemId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Get comments for a poem
export const getPoemComments = async (
  poemId,
  pageNumber = 1,
  pageSize = 10
) => {
  try {
    console.log(
      `API Call: Getting comments for poem ${poemId}, page ${pageNumber}, size ${pageSize}`
    );

    const response = await api.get(
      `/poems/comments/${poemId}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );

    const data = response.data;

    // Store the original total count from headers if available
    let totalCount = parseInt(response.headers["x-total-count"]) || null;

    // If the API doesn't return a paginated structure,
    // let's wrap it in one to maintain consistency
    if (Array.isArray(data)) {
      console.log(`Received ${data.length} comments as array`);

      // Check if the API might have more pages but didn't send the count
      const mightHaveMorePages = data.length === pageSize;

      // If we don't have a total count but we filled a page, make a separate request
      // to get the total count (only if this is the first page)
      if (totalCount === null && mightHaveMorePages && pageNumber === 1) {
        try {
          // Try to make a HEAD request to get just the count
          const countResponse = await api.head(`/poems/comments/${poemId}`);
          totalCount = parseInt(countResponse.headers["x-total-count"]);
          console.log(`Retrieved total count via HEAD: ${totalCount}`);
        } catch (countError) {
          console.log("Failed to retrieve total count via HEAD request");
        }
      }

      // If we still don't have the count, estimate based on current page data
      if (totalCount === null) {
        if (mightHaveMorePages) {
          // If we filled a page, assume there's at least one more page
          totalCount = pageNumber * pageSize + 1;
        } else {
          // If we didn't fill a page, this is probably all the comments
          totalCount = (pageNumber - 1) * pageSize + data.length;
        }
      }

      // Create a paginated structure from the array
      return {
        items: data,
        totalCount: totalCount,
        totalPages: Math.max(1, Math.ceil(totalCount / pageSize)),
        pageNumber: pageNumber,
        pageSize: pageSize,
        hasPreviousPage: pageNumber > 1,
        hasNextPage: pageNumber * pageSize < totalCount,
      };
    }

    // If it's already in a paginated structure
    if (data && typeof data === "object") {
      // Use the header count if available, otherwise use the count in the data
      if (totalCount === null) {
        totalCount = data.totalCount || data.items?.length || 0;
      }

      console.log(`Received paginated data with total count: ${totalCount}`);

      // Make sure all required fields exist
      return {
        items: data.items || data.results || [],
        totalCount: totalCount,
        totalPages:
          data.totalPages || Math.max(1, Math.ceil(totalCount / pageSize)),
        pageNumber: data.pageNumber || pageNumber,
        pageSize: data.pageSize || pageSize,
        hasPreviousPage: data.hasPreviousPage || pageNumber > 1,
        hasNextPage: data.hasNextPage || pageNumber * pageSize < totalCount,
      };
    }

    // Fallback for unexpected response formats
    console.log(
      "Unexpected data format from API, falling back to simple array"
    );
    return {
      items: [],
      totalCount: 0,
      totalPages: 1,
      pageNumber: pageNumber,
      pageSize: pageSize,
      hasPreviousPage: false,
      hasNextPage: false,
    };
  } catch (error) {
    console.error("Error fetching poem comments:", error);
    throw error;
  }
};

// Add a comment to a poem
export const addComment = async (poemId, commentText) => {
  try {
    const response = await api.post(
      `/poems/comments/${poemId}`,
      JSON.stringify(commentText),
      {
        headers: {
          "Content-Type": "application/json",
        },
      }
    );
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Delete a comment
export const deleteComment = async (id) => {
  try {
    await api.delete(`/poems/comments/${id}`);
    return true;
  } catch (error) {
    throw error;
  }
};

// Like a poem
export const likePoem = async (poemId) => {
  try {
    await api.post(`/poems/like/${poemId}`);
    return true;
  } catch (error) {
    throw error;
  }
};

// Unlike a poem
export const unlikePoem = async (poemId) => {
  try {
    await api.delete(`/poems/like/${poemId}`);
    return true;
  } catch (error) {
    throw error;
  }
};

// Check if user liked a poem
export const isPoemLiked = async (poemId) => {
  try {
    const response = await api.get(`/poems/liked/${poemId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Increment view count for a poem
export const incrementPoemViews = async (poemId) => {
  try {
    // This is often done silently in the background
    await api.post(`/poems/view/${poemId}`);
    return true;
  } catch (error) {
    // Silently fail - we don't want view tracking to break the app
    console.error("Failed to track view:", error);
    return false;
  }
};

// Get user's poems (published)
export const getUserPoems = async () => {
  try {
    const response = await api.get("/mypoems");
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Get user's draft poems
export const getUserDrafts = async () => {
  try {
    const response = await api.get("/mypoems/drafts");
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Create a new poem
export const createPoem = async (poemData) => {
  try {
    const response = await api.post("/mypoems", poemData);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Update an existing poem
export const updatePoem = async (poemData) => {
  try {
    const response = await api.put(`/mypoems/${poemData.id}`, poemData);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Delete a poem
export const deletePoem = async (poemId) => {
  try {
    await api.delete(`/mypoems/${poemId}`);
    return true;
  } catch (error) {
    throw error;
  }
};

// Publish a poem (change from draft to published)
export const publishPoem = async (poemId) => {
  try {
    const response = await api.put(`/mypoems/publish/${poemId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Unpublish a poem (change from published to draft)
export const unpublishPoem = async (poemId) => {
  try {
    await api.put(`/mypoems/unpublish/${poemId}`);
    return true;
  } catch (error) {
    throw error;
  }
};

// Get user's collections
export const getUserCollections = async () => {
  try {
    const response = await api.get("/collections");
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Get a specific collection with poems
export const getCollection = async (collectionId) => {
  try {
    const response = await api.get(`/collections/${collectionId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Create a new collection
export const createCollection = async (collectionData) => {
  try {
    const response = await api.post("/collections", collectionData);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Add a poem to a collection
export const addPoemToCollection = async (collectionId, poemId) => {
  try {
    await api.post(`/collections/${collectionId}/poems/${poemId}`);
    return true;
  } catch (error) {
    throw error;
  }
};

// Remove a poem from a collection
export const removePoemFromCollection = async (collectionId, poemId) => {
  try {
    await api.delete(`/collections/${collectionId}/poems/${poemId}`);
    return true;
  } catch (error) {
    throw error;
  }
};

// Delete a collection
export const deleteCollection = async (collectionId) => {
  try {
    await api.delete(`/collections/${collectionId}`);
    return true;
  } catch (error) {
    throw error;
  }
};

export const getPoem = async (poemId) => {
  try {
    const response = await api.get(`/mypoems/${poemId}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};
