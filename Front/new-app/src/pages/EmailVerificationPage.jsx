import React, { useState, useEffect } from "react";
import { useSearchParams, Link } from "react-router-dom";
import PageLayout from "../components/layout/PageLayout";
import { verifyEmail } from "../services/authService";
import Button from "../components/common/Button";
import "./EmailVerificationPage.css";

const EmailVerificationPage = () => {
  const [searchParams] = useSearchParams();
  const [verificationStatus, setVerificationStatus] = useState("verifying"); // verifying, success, error
  const [error, setError] = useState(null);

  useEffect(() => {
    const verifyUserEmail = async () => {
      try {
        // Get userId and token from URL params
        const userId = searchParams.get("userId");
        const token = searchParams.get("token");

        if (!userId || !token) {
          setVerificationStatus("error");
          setError(
            "Missing verification information. Please check your email link."
          );
          return;
        }

        // Call API to verify email
        const result = await verifyEmail(userId, token);

        if (result) {
          setVerificationStatus("success");
        } else {
          setVerificationStatus("error");
          setError(
            "Email verification failed. The link may be expired or invalid."
          );
        }
      } catch (err) {
        console.error("Email verification error:", err);
        setVerificationStatus("error");
        setError(
          err.message ||
            "An error occurred during email verification. Please try again."
        );
      }
    };

    verifyUserEmail();
  }, [searchParams]);

  return (
    <PageLayout>
      <div className="email-verification-container">
        {verificationStatus === "verifying" && (
          <div className="verification-card">
            <div className="verification-icon verifying">
              <i className="fas fa-spinner fa-spin"></i>
            </div>
            <h2>Verifying Your Email</h2>
            <p>Please wait while we verify your email address...</p>
          </div>
        )}

        {verificationStatus === "success" && (
          <div className="verification-card success">
            <div className="verification-icon success">
              <i className="fas fa-check-circle"></i>
            </div>
            <h2>Email Verified Successfully!</h2>
            <p>
              Your email has been verified. You can now log in to your account
              and enjoy all the features of Poetry Everyday.
            </p>
            <div className="verification-actions">
              <Button to="/login" variant="primary">
                Log In
              </Button>
            </div>
          </div>
        )}

        {verificationStatus === "error" && (
          <div className="verification-card error">
            <div className="verification-icon error">
              <i className="fas fa-exclamation-circle"></i>
            </div>
            <h2>Verification Failed</h2>
            <p>{error || "An error occurred during verification."}</p>
            <div className="verification-actions">
              <Button to="/login" variant="outline">
                Back to Login
              </Button>
              <Link to="/register" className="resend-link">
                Need to create a new account?
              </Link>
            </div>
          </div>
        )}
      </div>
    </PageLayout>
  );
};

export default EmailVerificationPage;
