import React, { useState, useEffect, useContext } from "react";
import { Routes, Route, Navigate, useLocation } from "react-router-dom";
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
  const { currentUser } = useContext(AuthContext);
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const location = useLocation();

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        setLoading(true);
        const userData = await getUserProfile();
        setProfile(userData);
        setLoading(false);
      } catch (err) {
        console.error("Error fetching user profile:", err);
        setError("Failed to load user profile. Please try again later.");
        setLoading(false);
      }
    };

    if (currentUser) {
      fetchProfile();
    }
  }, [currentUser]);

  if (loading) {
    return (
      <PageLayout>
        <div className="loading-container">
          <div className="spinner-border" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      </PageLayout>
    );
  }

  if (error || !profile) {
    return (
      <PageLayout>
        <div className="error-container">
          <h2>Error</h2>
          <p>{error || "Failed to load user profile."}</p>
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
