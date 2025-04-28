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
    queryParams.append("SortBy", searchParams.sortBy || "CreatedAt");
    queryParams.append("SortDescending", searchParams.sortDescending || true);
    queryParams.append("IsPublished", searchParams.isPublished ?? true);

    const response = await api.get(`/poems?${queryParams.toString()}`);
    return response.data;
  } catch (error) {
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
  pageSize = 20
) => {
  try {
    const response = await api.get(
      `/poems/comments/${poemId}?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
    return response.data;
  } catch (error) {
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
export const deleteComment = async (commentId) => {
  try {
    await api.delete(`/poems/comments/${commentId}`);
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
