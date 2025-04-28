import axios from "axios";

// Create an instance of axios with default configuration
const api = axios.create({
  baseURL: "https://localhost:7000/gateway", // Using the Ocelot Gateway URL
  headers: {
    "Content-Type": "application/json",
  },
  withCredentials: true, // Important for cookies
});

// Add a request interceptor to include auth token for authorized endpoints
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("token");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Add a response interceptor to handle token refresh
api.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    // Check if there is response data
    if (!error.response) {
      console.error("Network error or service unavailable:", error);
      return Promise.reject(new Error("Network error or service unavailable"));
    }

    const originalRequest = error.config;

    // If the error is 401 Unauthorized and we haven't tried to refresh yet
    if (error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        // Try to refresh the token
        const response = await axios.post(
          "https://localhost:7000/gateway/identity/refresh-token",
          {},
          { withCredentials: true } // Important to send the refresh token cookie
        );

        if (response.data.accessToken) {
          // Update the token in local storage
          localStorage.setItem("token", response.data.accessToken);

          // Update the authorization header and retry the request
          originalRequest.headers.Authorization = `Bearer ${response.data.accessToken}`;
          return axios(originalRequest);
        }
      } catch (refreshError) {
        // If refresh failed, redirect to login
        localStorage.removeItem("token");
        window.location.href = "/login";
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api;
