import React, { useState, useEffect, useContext } from "react";
import { Routes, Route, Navigate, useLocation, Link } from "react-router-dom";
import PageLayout from "../components/layout/PageLayout";
import UserProfile from "../components/user/UserProfile";
import UserPoems from "../components/user/UserPoems";
import UserDrafts from "../components/user/UserDrafts";
import UserCollections from "../components/user/UserCollections";
import UserComments from "../components/user/UserComments";
import UserActivity from "../components/user/UserActivity";
import UserSettings from "../components/user/UserSettings";
import CabinetNavigation from "../components/user/CabinetNavigation";
import AuthContext from "../context/AuthContext";
import { getUserProfile } from "../services/userService";
import "./UserCabinetPage.css";

const UserCabinetPage = () => {
  const { currentUser, logout } = useContext(AuthContext);
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const location = useLocation();

  // Define fetchProfile outside useEffect so it can be reused
  const fetchProfile = async () => {
    try {
      setLoading(true);
      setError(null);
      const userData = await getUserProfile();
      setProfile(userData);
      setLoading(false);
    } catch (err) {
      console.error("Error fetching user profile:", err);
      setError(
        err.message || "Failed to load user profile. Please try again later."
      );
      setLoading(false);
    }
  };

  useEffect(() => {
    if (currentUser) {
      fetchProfile();
    }
  }, [currentUser]);

  const handleRetryFetch = () => {
    fetchProfile();
  };

  const handleLogoutClick = async () => {
    try {
      await logout();
    } catch (error) {
      console.error("Logout error:", error);
    }
  };

  if (loading) {
    return (
      <PageLayout>
        <div className="loading-container">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Loading profile data...</span>
          </div>
        </div>
      </PageLayout>
    );
  }

  if (error) {
    return (
      <PageLayout>
        <div className="error-container">
          <h2>Error Loading Profile</h2>
          <p>{error}</p>
          <div className="error-actions">
            <button className="btn btn-primary" onClick={handleRetryFetch}>
              Retry
            </button>
            <button
              className="btn btn-secondary ms-3"
              onClick={handleLogoutClick}
            >
              Logout
            </button>
          </div>
        </div>
      </PageLayout>
    );
  }

  if (!profile) {
    return (
      <PageLayout>
        <div className="error-container">
          <h2>Profile Not Found</h2>
          <p>
            We couldn't find your profile information. Please try logging in
            again.
          </p>
          <Link to="/login" className="btn btn-primary">
            Go to Login
          </Link>
        </div>
      </PageLayout>
    );
  }

  // Default route is the first tab
  if (location.pathname === "/cabinet") {
    return <Navigate to="/cabinet/poems" replace />;
  }

  return (
    <PageLayout>
      <div className="cabinet-container">
        <UserProfile profile={profile} />

        <CabinetNavigation />

        <Routes>
          <Route path="/poems" element={<UserPoems />} />
          <Route path="/drafts" element={<UserDrafts />} />
          <Route path="/collections" element={<UserCollections />} />
          <Route path="/comments" element={<UserComments />} />
          <Route path="/activity" element={<UserActivity />} />
          <Route
            path="/settings"
            element={<UserSettings profile={profile} />}
          />
          <Route path="*" element={<Navigate to="/cabinet/poems" replace />} />
        </Routes>
      </div>
    </PageLayout>
  );
};

export default UserCabinetPage;
