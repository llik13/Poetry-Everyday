import React, { useState } from "react";
import {
  updateUserProfile,
  changePassword,
  uploadProfileImage,
  updateNotificationSettings,
} from "../../services/userService";
import Button from "../common/Button";
import "./UserSettings.css";

const UserSettings = ({ profile }) => {
  // Profile form state
  const [profileFormData, setProfileFormData] = useState({
    userName: profile.userName || "",
    biography: profile.biography || "",
  });

  // Password form state
  const [passwordFormData, setPasswordFormData] = useState({
    currentPassword: "",
    newPassword: "",
    confirmNewPassword: "",
  });

  // Notification settings state
  const [notificationSettings, setNotificationSettings] = useState({
    emailNotifications:
      profile.notificationSettings?.emailNotifications ?? true,
    forumReplies: profile.notificationSettings?.forumReplies ?? true,
    poemComments: profile.notificationSettings?.poemComments ?? true,
    poemLikes: profile.notificationSettings?.poemLikes ?? true,
    newsletter: profile.notificationSettings?.newsletter ?? true,
  });

  // File upload state
  const [selectedImage, setSelectedImage] = useState(null);
  const [previewUrl, setPreviewUrl] = useState(profile.profileImageUrl || "");

  // Form status states
  const [profileUpdateSuccess, setProfileUpdateSuccess] = useState(false);
  const [profileUpdateError, setProfileUpdateError] = useState(null);
  const [passwordUpdateSuccess, setPasswordUpdateSuccess] = useState(false);
  const [passwordUpdateError, setPasswordUpdateError] = useState(null);
  const [notificationUpdateSuccess, setNotificationUpdateSuccess] =
    useState(false);
  const [notificationUpdateError, setNotificationUpdateError] = useState(null);
  const [imageUploadSuccess, setImageUploadSuccess] = useState(false);
  const [imageUploadError, setImageUploadError] = useState(null);

  // Handle profile form changes
  const handleProfileInputChange = (e) => {
    const { name, value } = e.target;
    setProfileFormData({
      ...profileFormData,
      [name]: value,
    });
  };

  // Handle password form changes
  const handlePasswordInputChange = (e) => {
    const { name, value } = e.target;
    setPasswordFormData({
      ...passwordFormData,
      [name]: value,
    });
  };

  // Handle notification settings changes
  const handleNotificationChange = (e) => {
    const { name, checked } = e.target;
    setNotificationSettings({
      ...notificationSettings,
      [name]: checked,
    });
  };

  // Handle image selection
  const handleImageChange = (e) => {
    if (e.target.files && e.target.files[0]) {
      const file = e.target.files[0];
      setSelectedImage(file);

      // Create a preview URL
      const reader = new FileReader();
      reader.onloadend = () => {
        setPreviewUrl(reader.result);
      };
      reader.readAsDataURL(file);
    }
  };

  // Submit profile update
  const handleProfileSubmit = async (e) => {
    e.preventDefault();
    setProfileUpdateSuccess(false);
    setProfileUpdateError(null);

    try {
      await updateUserProfile(profileFormData);
      setProfileUpdateSuccess(true);
    } catch (err) {
      console.error("Error updating profile:", err);
      setProfileUpdateError("Failed to update profile. Please try again.");
    }
  };

  // Submit password change
  const handlePasswordSubmit = async (e) => {
    e.preventDefault();
    setPasswordUpdateSuccess(false);
    setPasswordUpdateError(null);

    if (passwordFormData.newPassword !== passwordFormData.confirmNewPassword) {
      setPasswordUpdateError("New passwords do not match.");
      return;
    }

    try {
      await changePassword({
        currentPassword: passwordFormData.currentPassword,
        newPassword: passwordFormData.newPassword,
        confirmNewPassword: passwordFormData.confirmNewPassword,
      });

      setPasswordUpdateSuccess(true);
      setPasswordFormData({
        currentPassword: "",
        newPassword: "",
        confirmNewPassword: "",
      });
    } catch (err) {
      console.error("Error changing password:", err);
      setPasswordUpdateError(
        "Failed to change password. Please check your current password and try again."
      );
    }
  };

  // Submit notification settings
  const handleNotificationSubmit = async (e) => {
    e.preventDefault();
    setNotificationUpdateSuccess(false);
    setNotificationUpdateError(null);

    try {
      await updateNotificationSettings(notificationSettings);
      setNotificationUpdateSuccess(true);
    } catch (err) {
      console.error("Error updating notification settings:", err);
      setNotificationUpdateError(
        "Failed to update notification settings. Please try again."
      );
    }
  };

  // Submit image upload
  const handleImageSubmit = async (e) => {
    e.preventDefault();
    setImageUploadSuccess(false);
    setImageUploadError(null);

    if (!selectedImage) {
      setImageUploadError("Please select an image to upload.");
      return;
    }

    try {
      await uploadProfileImage(selectedImage);
      setImageUploadSuccess(true);
    } catch (err) {
      console.error("Error uploading image:", err);
      setImageUploadError("Failed to upload image. Please try again.");
    }
  };

  return (
    <div className="user-settings">
      <div className="settings-header">
        <h2>Account Settings</h2>
      </div>

      <div className="settings-grid">
        {/* Profile Settings */}
        <div className="settings-card">
          <h3 className="settings-card-title">Profile Information</h3>
          {profileUpdateSuccess && (
            <div className="alert alert-success">
              Profile updated successfully!
            </div>
          )}
          {profileUpdateError && (
            <div className="alert alert-danger">{profileUpdateError}</div>
          )}
          <form onSubmit={handleProfileSubmit}>
            <div className="form-group">
              <label htmlFor="userName">Username</label>
              <input
                type="text"
                id="userName"
                name="userName"
                className="form-control"
                value={profileFormData.userName}
                onChange={handleProfileInputChange}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="biography">Biography</label>
              <textarea
                id="biography"
                name="biography"
                className="form-control"
                rows="4"
                value={profileFormData.biography}
                onChange={handleProfileInputChange}
                placeholder="Tell others about yourself..."
              />
            </div>
            <Button type="submit" variant="primary">
              Save Changes
            </Button>
          </form>
        </div>

        {/* Profile Image */}
        <div className="settings-card">
          <h3 className="settings-card-title">Profile Picture</h3>
          {imageUploadSuccess && (
            <div className="alert alert-success">
              Profile image updated successfully!
            </div>
          )}
          {imageUploadError && (
            <div className="alert alert-danger">{imageUploadError}</div>
          )}
          <div className="profile-image-container">
            <div className="profile-image-preview">
              {previewUrl ? (
                <img src={previewUrl} alt="Profile preview" />
              ) : (
                <div className="no-image">No image selected</div>
              )}
            </div>
            <form onSubmit={handleImageSubmit}>
              <div className="form-group">
                <input
                  type="file"
                  id="profileImage"
                  name="profileImage"
                  className="form-control-file"
                  accept="image/*"
                  onChange={handleImageChange}
                />
                <small className="form-text text-muted">
                  Maximum file size: 2MB. Supported formats: JPG, PNG, GIF.
                </small>
              </div>
              <Button type="submit" variant="primary" disabled={!selectedImage}>
                Upload Image
              </Button>
            </form>
          </div>
        </div>

        {/* Password Settings */}
        <div className="settings-card">
          <h3 className="settings-card-title">Change Password</h3>
          {passwordUpdateSuccess && (
            <div className="alert alert-success">
              Password changed successfully!
            </div>
          )}
          {passwordUpdateError && (
            <div className="alert alert-danger">{passwordUpdateError}</div>
          )}
          <form onSubmit={handlePasswordSubmit}>
            <div className="form-group">
              <label htmlFor="currentPassword">Current Password</label>
              <input
                type="password"
                id="currentPassword"
                name="currentPassword"
                className="form-control"
                value={passwordFormData.currentPassword}
                onChange={handlePasswordInputChange}
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="newPassword">New Password</label>
              <input
                type="password"
                id="newPassword"
                name="newPassword"
                className="form-control"
                value={passwordFormData.newPassword}
                onChange={handlePasswordInputChange}
                required
                minLength="6"
              />
              <small className="form-text text-muted">
                Password must be at least 6 characters long.
              </small>
            </div>
            <div className="form-group">
              <label htmlFor="confirmNewPassword">Confirm New Password</label>
              <input
                type="password"
                id="confirmNewPassword"
                name="confirmNewPassword"
                className="form-control"
                value={passwordFormData.confirmNewPassword}
                onChange={handlePasswordInputChange}
                required
              />
            </div>
            <Button type="submit" variant="primary">
              Change Password
            </Button>
          </form>
        </div>

        {/* Notification Settings */}
        <div className="settings-card">
          <h3 className="settings-card-title">Notification Settings</h3>
          {notificationUpdateSuccess && (
            <div className="alert alert-success">
              Notification settings updated successfully!
            </div>
          )}
          {notificationUpdateError && (
            <div className="alert alert-danger">{notificationUpdateError}</div>
          )}
          <form onSubmit={handleNotificationSubmit}>
            <div className="form-check">
              <input
                type="checkbox"
                id="emailNotifications"
                name="emailNotifications"
                className="form-check-input"
                checked={notificationSettings.emailNotifications}
                onChange={handleNotificationChange}
              />
              <label htmlFor="emailNotifications" className="form-check-label">
                Email Notifications
              </label>
            </div>
            <div className="form-check">
              <input
                type="checkbox"
                id="forumReplies"
                name="forumReplies"
                className="form-check-input"
                checked={notificationSettings.forumReplies}
                onChange={handleNotificationChange}
              />
              <label htmlFor="forumReplies" className="form-check-label">
                Forum Replies
              </label>
            </div>
            <div className="form-check">
              <input
                type="checkbox"
                id="poemComments"
                name="poemComments"
                className="form-check-input"
                checked={notificationSettings.poemComments}
                onChange={handleNotificationChange}
              />
              <label htmlFor="poemComments" className="form-check-label">
                Poem Comments
              </label>
            </div>
            <div className="form-check">
              <input
                type="checkbox"
                id="poemLikes"
                name="poemLikes"
                className="form-check-input"
                checked={notificationSettings.poemLikes}
                onChange={handleNotificationChange}
              />
              <label htmlFor="poemLikes" className="form-check-label">
                Poem Likes
              </label>
            </div>
            <div className="form-check">
              <input
                type="checkbox"
                id="newsletter"
                name="newsletter"
                className="form-check-input"
                checked={notificationSettings.newsletter}
                onChange={handleNotificationChange}
              />
              <label htmlFor="newsletter" className="form-check-label">
                Newsletter
              </label>
            </div>
            <Button type="submit" variant="primary" className="mt-3">
              Save Notification Settings
            </Button>
          </form>
        </div>
      </div>
    </div>
  );
};

export default UserSettings;
