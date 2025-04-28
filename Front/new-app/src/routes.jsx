import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";

// Pages
import HomePage from "./pages/HomePages";
import CatalogPage from "./pages/CatalogPage";
import PoemDetailPage from "./pages/PoemDetailPage";
import UserCabinetPage from "./pages/UserCabinetPage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";

// Auth protected route component
import PrivateRoute from "./components/common/PrivateRoute";

// This can be a separate component that handles all the application routing
const AppRoutes = () => {
  return (
    <Routes>
      {/* Public Routes */}
      <Route path="/" element={<HomePage />} />
      <Route path="/catalog" element={<CatalogPage />} />
      <Route path="/poems/:id" element={<PoemDetailPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      {/* Protected Routes */}
      <Route
        path="/cabinet/*"
        element={
          <PrivateRoute>
            <UserCabinetPage />
          </PrivateRoute>
        }
      />

      {/* 404 - Not Found */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

export default AppRoutes;
