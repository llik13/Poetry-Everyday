import React from "react";
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

  return (
    <div className="user-profile">
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
      </div>

      <div className="profile-actions">
        <Button variant="outline" size="sm" to="/cabinet/settings">
          Edit Profile
        </Button>

        <Button variant="primary" size="sm" to="/cabinet/create-poem">
          Write New Poem
        </Button>
      </div>
    </div>
  );
};

export default UserProfile;
