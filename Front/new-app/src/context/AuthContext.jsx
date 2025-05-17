import React, { createContext, useState, useEffect } from "react";
import {
  loginUser,
  refreshToken,
  registerUser,
  logoutUser,
} from "../services/authService.js";
import { jwtDecode } from "jwt-decode";

export const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [currentUser, setCurrentUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem("token") || null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [refreshingToken, setRefreshingToken] = useState(false);

  // Initialize authentication state
  useEffect(() => {
    const initAuth = async () => {
      // Skip authentication check if on password reset or verification pages
      if (
        window.location.pathname === "/reset-password" ||
        window.location.pathname === "/verify-email" ||
        window.location.pathname === "/forgot-password"
      ) {
        console.log(
          "Skipping auth check for auth-related page:",
          window.location.pathname
        );
        setLoading(false);
        return;
      }

      const storedToken = localStorage.getItem("token");

      if (storedToken) {
        try {
          // Check if token is expired
          const decodedToken = jwtDecode(storedToken);
          const currentTime = Date.now() / 1000;

          if (decodedToken.exp < currentTime) {
            console.log("Token expired, attempting to refresh...");
            // Token is expired, try to refresh
            setRefreshingToken(true);
            try {
              const refreshResult = await refreshToken();
              if (refreshResult && refreshResult.accessToken) {
                setToken(refreshResult.accessToken);
                localStorage.setItem("token", refreshResult.accessToken);
                setCurrentUser({
                  id: refreshResult.userId,
                  username: refreshResult.userName,
                });
                console.log("Token refreshed successfully");
              } else {
                // Refresh failed, log out
                console.log("Token refresh failed, logging out");
                setToken(null);
                setCurrentUser(null);
                localStorage.removeItem("token");
              }
            } catch (refreshError) {
              console.error("Error refreshing token:", refreshError);
              setToken(null);
              setCurrentUser(null);
              localStorage.removeItem("token");
            } finally {
              setRefreshingToken(false);
            }
          } else {
            // Token is still valid
            console.log("Token is valid, setting user");
            setCurrentUser({
              id: decodedToken.sub,
              username:
                decodedToken[
                  Object.keys(decodedToken).find((key) => key.includes("name"))
                ],
            });
          }
        } catch (error) {
          console.error("Auth initialization error:", error);
          setToken(null);
          setCurrentUser(null);
          localStorage.removeItem("token");
        }
      }

      setLoading(false);
    };

    initAuth();
  }, []);

  // Set up token refresh timer
  useEffect(() => {
    if (!token) return;

    try {
      const decodedToken = jwtDecode(token);
      const expirationTime = decodedToken.exp * 1000; // Convert to milliseconds
      const currentTime = Date.now();

      // Calculate time until token expiration
      const timeUntilExpiration = expirationTime - currentTime;

      // If token is about to expire, refresh 5 minutes before expiration
      if (timeUntilExpiration > 0) {
        const refreshTime = timeUntilExpiration - 5 * 60 * 1000; // 5 minutes before expiration
        const minRefreshTime = 10000; // At least 10 seconds from now

        const timerDelay = Math.max(refreshTime, minRefreshTime);

        const refreshTimer = setTimeout(async () => {
          console.log("Auto-refreshing token...");
          try {
            setRefreshingToken(true);
            const refreshResult = await refreshToken();
            if (refreshResult && refreshResult.accessToken) {
              setToken(refreshResult.accessToken);
              localStorage.setItem("token", refreshResult.accessToken);
              console.log("Token auto-refreshed successfully");
            }
          } catch (error) {
            console.error("Error auto-refreshing token:", error);
          } finally {
            setRefreshingToken(false);
          }
        }, timerDelay);

        return () => clearTimeout(refreshTimer);
      }
    } catch (error) {
      console.error("Error setting up token refresh:", error);
    }
  }, [token]);

  const login = async (credentials) => {
    try {
      setError(null);
      const response = await loginUser(credentials);

      if (response && response.accessToken) {
        localStorage.setItem("token", response.accessToken);
        setToken(response.accessToken);
        setCurrentUser({
          id: response.userId,
          username: response.userName,
        });
        return true;
      }
      return false;
    } catch (err) {
      const errorMessage =
        err.response?.data?.message ||
        "Login failed. Please check your credentials and try again.";
      setError(errorMessage);
      console.error("Login error:", err);
      return false;
    }
  };

  const register = async (userData) => {
    try {
      setError(null);

      // Log for debugging
      console.log("Registering user with data:", userData);

      const response = await registerUser(userData);

      console.log("Registration response:", response);

      return { success: true, message: response.message };
    } catch (err) {
      // Extended error handling
      console.error("Registration error:", err);

      let errorMessage = "Registration failed. Please try again.";
      let errors = [];

      // Check for response data
      if (err.response?.data) {
        const responseData = err.response.data;

        // Check different error formats the server might return
        if (responseData.errors && Array.isArray(responseData.errors)) {
          // Identity errors format with array of errors
          errors = responseData.errors;
          errorMessage = responseData.errors.join(". ");
        } else if (
          responseData.errors &&
          typeof responseData.errors === "object"
        ) {
          // ASP.NET Core validation errors format
          errors = Object.values(responseData.errors).flat();
          errorMessage = errors.join(". ");
        } else if (responseData.message) {
          // Single error message format
          errorMessage = responseData.message;
        } else if (typeof responseData === "string") {
          // Text error format
          errorMessage = responseData;
        }
      }

      setError(errorMessage);

      return {
        success: false,
        message: errorMessage,
        errors: errors,
      };
    }
  };

  const logout = async () => {
    try {
      await logoutUser();
    } catch (error) {
      console.error("Logout error:", error);
    } finally {
      localStorage.removeItem("token");
      setToken(null);
      setCurrentUser(null);
    }
  };

  const value = {
    currentUser,
    token,
    loading: loading || refreshingToken,
    error,
    login,
    register,
    logout,
    isAuthenticated: !!token,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export default AuthContext;
