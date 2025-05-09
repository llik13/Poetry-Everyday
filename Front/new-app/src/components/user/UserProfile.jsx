import React from "react";
import { Link } from "react-router-dom";
import Button from "../common/Button";
import "./UserProfile.css";

const UserProfile = ({ profile }) => {
  const formatDate = (dateString) => {
    if (!dateString) return "N/A";
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  const getDefaultProfileImage = () => {
    return "/assets/images/default-profile.jpg"; // Default profile image path
  };

  return (
    <div className="user-profile">
      <div className="profile-picture">
        <img
          src={profile.profileImageUrl || getDefaultProfileImage()}
          alt={`${profile.userName}'s profile`}
        />
      </div>

      <div className="profile-info">
        <h1 className="user-name">{profile.userName}</h1>

        <p className="user-bio">
          {profile.biography ||
            "No biography provided. Tell others about yourself by updating your profile."}
        </p>

        <div className="user-meta">
          <span>Member since: {formatDate(profile.createdAt)}</span>
          {profile.lastLogin && (
            <span>Last login: {formatDate(profile.lastLogin)}</span>
          )}
        </div>

        <div className="user-stats">
          {/* These stats would typically come from the backend */}
          <div className="stat-item">
            <span className="stat-value">15</span>
            <span className="stat-label">Poems</span>
          </div>
          <div className="stat-item">
            <span className="stat-value">127</span>
            <span className="stat-label">Comments</span>
          </div>
          <div className="stat-item">
            <span className="stat-value">243</span>
            <span className="stat-label">Likes</span>
          </div>
        </div>
      </div>

      <div className="profile-actions">
        <Button variant="outline" size="sm" to="/cabinet/settings">
          Edit Profile
        </Button>

        <Button variant="primary" size="sm" to="/cabinet/drafts">
          Write New Poem
        </Button>
      </div>
    </div>
  );
};

export default UserProfile;
