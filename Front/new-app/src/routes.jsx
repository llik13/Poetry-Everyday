import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";

// Pages
import HomePage from "./pages/HomePages";
import CatalogPage from "./pages/CatalogPage";
import PoemDetailPage from "./pages/PoemDetailPage";
import UserCabinetPage from "./pages/UserCabinetPage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import CreatePoemPage from "./pages/CreatePoemPage";
import EmailVerificationPage from "./pages/EmailVerificationPage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import ResetPasswordPage from "./pages/ResetPasswordPage";

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
      <Route path="/verify-email" element={<EmailVerificationPage />} />{" "}
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />
      {/* New route for email verification */}
      {/* Protected Routes */}
      <Route
        path="/cabinet/*"
        element={
          <PrivateRoute>
            <UserCabinetPage />
          </PrivateRoute>
        }
      />
      {/* Create/Edit Poem Routes - Protected */}
      <Route
        path="/cabinet/create-poem"
        element={
          <PrivateRoute>
            <CreatePoemPage />
          </PrivateRoute>
        }
      />
      <Route
        path="/cabinet/edit-poem/:id"
        element={
          <PrivateRoute>
            <CreatePoemPage />
          </PrivateRoute>
        }
      />
      {/* 404 - Not Found */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

export default AppRoutes;
