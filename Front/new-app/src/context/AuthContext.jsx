import React, { createContext, useState, useEffect } from "react";
import {
  loginUser,
  refreshToken,
  registerUser,
  logoutUser,
} from "../services/authService.js";
import jwtDecode from "jwt-decode";

export const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [currentUser, setCurrentUser] = useState(null);
  const [token, setToken] = useState(localStorage.getItem("token") || null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const initAuth = async () => {
      const storedToken = localStorage.getItem("token");

      if (storedToken) {
        try {
          // Check if token is expired
          const decodedToken = jwtDecode(storedToken);
          const currentTime = Date.now() / 1000;

          if (decodedToken.exp < currentTime) {
            // Token is expired, try to refresh
            const refreshResult = await refreshToken();
            if (refreshResult.accessToken) {
              setToken(refreshResult.accessToken);
              localStorage.setItem("token", refreshResult.accessToken);
              setCurrentUser({
                id: refreshResult.userId,
                username: refreshResult.userName,
              });
            } else {
              // Refresh failed, log out
              setToken(null);
              setCurrentUser(null);
              localStorage.removeItem("token");
            }
          } else {
            // Token is still valid
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

  const login = async (credentials) => {
    try {
      setError(null);
      const response = await loginUser(credentials);

      if (response.accessToken) {
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
      setError(err.response?.data?.message || "Login failed");
      return false;
    }
  };

  const register = async (userData) => {
    try {
      setError(null);
      const response = await registerUser(userData);
      return { success: true, message: response.message };
    } catch (err) {
      setError(err.response?.data?.message || "Registration failed");
      return {
        success: false,
        message: err.response?.data?.message || "Registration failed",
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
    loading,
    error,
    login,
    register,
    logout,
    isAuthenticated: !!token,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export default AuthContext;
