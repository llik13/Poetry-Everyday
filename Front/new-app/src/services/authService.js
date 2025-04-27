import api from "./api";
import axios from "axios";

// Login user
export const loginUser = async (credentials) => {
  try {
    const response = await api.post("/identity/login", credentials);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Register user
export const registerUser = async (userData) => {
  try {
    const response = await api.post("/identity/register", userData);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Refresh token - uses direct axios since we need to handle cookies and avoid interceptors
export const refreshToken = async () => {
  try {
    const response = await axios.post(
      "https://localhost:7000/gateway/identity/refresh-token",
      {},
      { withCredentials: true }
    );
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Logout user
export const logoutUser = async () => {
  try {
    const response = await api.post(
      "/identity/logout",
      {},
      { withCredentials: true }
    );
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Forgot password
export const forgotPassword = async (email) => {
  try {
    const response = await api.post("/identity/forgot-password", { email });
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Reset password
export const resetPassword = async (resetData) => {
  try {
    const response = await api.post("/identity/reset-password", resetData);
    return response.data;
  } catch (error) {
    throw error;
  }
};

// Verify email
export const verifyEmail = async (userId, token) => {
  try {
    const response = await api.get(
      `/identity/verify-email?userId=${userId}&token=${token}`
    );
    return response.data;
  } catch (error) {
    throw error;
  }
};
