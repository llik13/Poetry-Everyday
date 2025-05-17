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
    // Для отладки: выведем данные, которые отправляем на сервер
    console.log("Sending registration data:", userData);

    // Проверим, что все поля соответствуют ожидаемому формату
    const registrationData = {
      userName: userData.userName,
      email: userData.email,
      password: userData.password,
      confirmPassword: userData.confirmPassword,
    };

    console.log("Formatted registration data:", registrationData);

    const response = await api.post("/identity/register", registrationData);
    return response.data;
  } catch (error) {
    // Для отладки: выведем детальную информацию об ошибке
    console.error("Registration error details:", {
      status: error.response?.status,
      data: error.response?.data,
      headers: error.response?.headers,
    });

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
      `/identity/verify-email?userId=${encodeURIComponent(
        userId
      )}&token=${encodeURIComponent(token)}`
    );

    // Return true if verification was successful
    return response.data && response.status === 200;
  } catch (error) {
    console.error("Email verification error:", error);

    // Check if we have a response with a message
    if (error.response && error.response.data && error.response.data.message) {
      throw new Error(error.response.data.message);
    }

    // Generic error
    throw new Error(
      "Failed to verify email. Please try again or contact support."
    );
  }
};

// Resend verification email
export const resendVerificationEmail = async (email) => {
  try {
    const response = await api.post("/identity/resend-verification", { email });
    return response.data;
  } catch (error) {
    throw error;
  }
};
