import React, { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { getUserActivity } from "../../services/userService";
import "./UserActivity.css";

const UserActivity = () => {
  const [activities, setActivities] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    fetchUserActivity();
  }, [currentPage]);

  const fetchUserActivity = async () => {
    try {
      setLoading(true);
      const data = await getUserActivity(currentPage, 20);
      setActivities(data);

      // Calculate total pages based on available data
      // This might be adjusted based on the API response
      setTotalPages(Math.ceil(data.length / 20) || 1);

      setLoading(false);
    } catch (err) {
      console.error("Error fetching user activity:", err);
      setError("Failed to load your activity. Please try again later.");
      setLoading(false);
    }
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "long",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const getActivityIcon = (activityType) => {
    switch (activityType.toLowerCase()) {
      case "login":
        return <i className="fas fa-sign-in-alt activity-icon"></i>;
      case "register":
        return <i className="fas fa-user-plus activity-icon"></i>;
      case "publish":
        return <i className="fas fa-upload activity-icon"></i>;
      case "updateprofile":
        return <i className="fas fa-user-edit activity-icon"></i>;
      case "comment":
        return <i className="fas fa-comment activity-icon"></i>;
      case "like":
        return <i className="fas fa-heart activity-icon"></i>;
      case "savepoem":
        return <i className="fas fa-bookmark activity-icon"></i>;
      case "forumpost":
        return <i className="fas fa-comments activity-icon"></i>;
      case "forumreply":
        return <i className="fas fa-reply activity-icon"></i>;
      default:
        return <i className="fas fa-circle activity-icon"></i>;
    }
  };

  const getActivityLabel = (activity) => {
    const { type, description, relatedTitle } = activity;

    if (description) return description;

    switch (type.toLowerCase()) {
      case "login":
        return "Logged in to your account";
      case "register":
        return "Registered a new account";
      case "publish":
        return relatedTitle
          ? `Published a new poem: "${relatedTitle}"`
          : "Published a new poem";
      case "updateprofile":
        return "Updated your profile information";
      case "comment":
        return relatedTitle
          ? `Commented on the poem: "${relatedTitle}"`
          : "Commented on a poem";
      case "like":
        return relatedTitle
          ? `Liked the poem: "${relatedTitle}"`
          : "Liked a poem";
      case "savepoem":
        return relatedTitle
          ? `Saved the poem: "${relatedTitle}" to your collection`
          : "Saved a poem to your collection";
      default:
        return "Performed an activity";
    }
  };

  const handlePageChange = (page) => {
    setCurrentPage(page);
    window.scrollTo(0, 0);
  };

  if (loading) {
    return (
      <div className="loading-container">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="user-activity">
      <div className="activity-header">
        <h2>My Activity</h2>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {activities.length === 0 ? (
        <div className="no-activity">
          <p>No activity found.</p>
        </div>
      ) : (
        <>
          <div className="activity-timeline">
            {activities.map((activity, index) => (
              <div key={index} className="activity-item">
                <div className="activity-icon-container">
                  {getActivityIcon(activity.type)}
                </div>
                <div className="activity-content">
                  <div className="activity-description">
                    {getActivityLabel(activity)}
                  </div>
                  <div className="activity-time">
                    {formatDate(activity.createdAt)}
                  </div>
                  {activity.relatedId && (
                    <div className="activity-actions">
                      {activity.type.toLowerCase() === "comment" ||
                      activity.type.toLowerCase() === "like" ||
                      activity.type.toLowerCase() === "savepoem" ? (
                        <Link
                          to={`/poems/${activity.relatedId}`}
                          className="view-item-link"
                        >
                          View Poem
                        </Link>
                      ) : activity.type.toLowerCase() === "forumpost" ||
                        activity.type.toLowerCase() === "forumreply" ? (
                        <Link
                          to={`/forum/post/${activity.relatedId}`}
                          className="view-item-link"
                        >
                          View Forum Post
                        </Link>
                      ) : null}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>

          {totalPages > 1 && (
            <div className="pagination-container">
              <nav aria-label="Activity pagination">
                <ul className="pagination">
                  <li
                    className={`page-item ${
                      currentPage === 1 ? "disabled" : ""
                    }`}
                  >
                    <button
                      className="page-link"
                      onClick={() => handlePageChange(currentPage - 1)}
                      disabled={currentPage === 1}
                    >
                      Previous
                    </button>
                  </li>

                  {[...Array(totalPages)].map((_, i) => (
                    <li
                      key={i}
                      className={`page-item ${
                        currentPage === i + 1 ? "active" : ""
                      }`}
                    >
                      <button
                        className="page-link"
                        onClick={() => handlePageChange(i + 1)}
                      >
                        {i + 1}
                      </button>
                    </li>
                  ))}

                  <li
                    className={`page-item ${
                      currentPage === totalPages ? "disabled" : ""
                    }`}
                  >
                    <button
                      className="page-link"
                      onClick={() => handlePageChange(currentPage + 1)}
                      disabled={currentPage === totalPages}
                    >
                      Next
                    </button>
                  </li>
                </ul>
              </nav>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default UserActivity;
