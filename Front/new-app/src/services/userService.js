import api from "./api";

// Get user profile
export const getUserProfile = async () => {
  try {
    const response = await api.get("/profile");
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Update user profile
export const updateUserProfile = async (profileData) => {
  try {
    const response = await api.put("/profile", profileData);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Change user password
export const changePassword = async (passwordData) => {
  try {
    const response = await api.post("/profile/change-password", passwordData);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Upload profile image
export const uploadProfileImage = async (imageFile) => {
  try {
    const formData = new FormData();
    formData.append("image", imageFile);

    const response = await api.post("/profile/upload-image", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });

    return response.data;
  } catch (error) {
    throw error;
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
    throw error;
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
    throw error;
  }
};

// Get user's notifications
export const getUserNotifications = async (unreadOnly = false) => {
  try {
    const response = await api.get(`/notifications?unreadOnly=${unreadOnly}`);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Mark notification as read
export const markNotificationAsRead = async (notificationId) => {
  try {
    await api.put(`/notifications/read/${notificationId}`);
    return true;
  } catch (error) {
    throw error;
  }
};

// Mark all notifications as read
export const markAllNotificationsAsRead = async () => {
  try {
    await api.put("/notifications/read-all");
    return true;
  } catch (error) {
    throw error;
  }
};
