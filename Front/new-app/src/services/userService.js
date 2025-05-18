import api from "./api";

// Get user profile with better error handling
export const getUserProfile = async () => {
  try {
    const response = await api.get("/profile");
    return response.data;
  } catch (error) {
    console.error("Failed to fetch user profile:", error);

    // Check if we have a response
    if (error.response) {
      // Server responded with a status code outside the 2xx range
      if (error.response.status === 401) {
        throw new Error("Not authenticated. Please log in again.");
      } else {
        throw new Error(`Server error: ${error.response.status}`);
      }
    } else if (error.request) {
      // The request was made but no response was received
      throw new Error("No response from server. Please check your connection.");
    } else {
      // Something happened in setting up the request
      throw error;
    }
  }
};

// Update user profile
export const updateUserProfile = async (profileData) => {
  try {
    const response = await api.put("/profile", profileData);
    return response.data;
  } catch (error) {
    handleApiError(error, "updating profile");
  }
};

// Change user password
export const changePassword = async (passwordData) => {
  try {
    const response = await api.post("/profile/change-password", passwordData);
    return response.data;
  } catch (error) {
    handleApiError(error, "changing password");
  }
};

// Get user activity
export const getUserActivity = async (page = 1, pageSize = 10) => {
  try {
    const response = await api.get(
      `/profile/activity?page=${page}&pageSize=${pageSize}`
    );
    return response.data;
  } catch (error) {
    handleApiError(error, "fetching activity");
  }
};

// Update notification settings
export const updateNotificationSettings = async (settingsData) => {
  try {
    const response = await api.put(
      "/profile/notification-settings",
      settingsData
    );
    return response.data;
  } catch (error) {
    handleApiError(error, "updating notification settings");
  }
};

// Get user's notifications
export const getUserNotifications = async (unreadOnly = false) => {
  try {
    const response = await api.get(`/notifications?unreadOnly=${unreadOnly}`);
    return response.data;
  } catch (error) {
    handleApiError(error, "fetching notifications");
  }
};

// Mark notification as read
export const markNotificationAsRead = async (notificationId) => {
  try {
    await api.put(`/notifications/read/${notificationId}`);
    return true;
  } catch (error) {
    handleApiError(error, "marking notification as read");
  }
};

// Mark all notifications as read
export const markAllNotificationsAsRead = async () => {
  try {
    await api.put("/notifications/read-all");
    return true;
  } catch (error) {
    handleApiError(error, "marking all notifications as read");
  }
};

// Helper function for error handling
const handleApiError = (error, action) => {
  console.error(`Error ${action}:`, error);

  if (error.response) {
    // The request was made and the server responded with a status code
    // that falls out of the range of 2xx
    if (error.response.status === 401) {
      throw new Error("Authentication error. Please log in again.");
    } else if (error.response.status === 403) {
      throw new Error("You don't have permission to perform this action.");
    } else {
      throw new Error(`Server error while ${action}. Please try again.`);
    }
  } else if (error.request) {
    // The request was made but no response was received
    throw new Error("No response from server. Please check your connection.");
  } else {
    // Something happened in setting up the request
    throw error;
  }
};

export default {
  getUserProfile,
  updateUserProfile,
  changePassword,
  getUserActivity,
  updateNotificationSettings,
  getUserNotifications,
  markNotificationAsRead,
  markAllNotificationsAsRead,
};
