// ResetPasswordPage.jsx
import React, { useState, useEffect, useContext } from "react";
import { Link, useNavigate, useLocation } from "react-router-dom";
import { Formik, Form, Field, ErrorMessage } from "formik";
import * as Yup from "yup";
import { resetPassword } from "../services/authService";
import PageLayout from "../components/layout/PageLayout";
import Button from "../components/common/Button";
import AuthContext from "../context/AuthContext";
import "./ResetPasswordPage.css";

const ResetPasswordPage = () => {
  const { isAuthenticated } = useContext(AuthContext);
  const navigate = useNavigate();
  const location = useLocation();
  const [serverError, setServerError] = useState("");
  const [success, setSuccess] = useState(false);

  // Get query parameters from URL
  const queryParams = new URLSearchParams(location.search);
  const token = queryParams.get("token");
  const email = queryParams.get("email");

  // Debug logging
  console.log("Reset password page mounted with:", {
    token: token ? "Token exists" : "No token",
    tokenLength: token?.length,
    email: email || "No email",
  });

  // Redirect if already logged in
  useEffect(() => {
    if (isAuthenticated) {
      navigate("/cabinet");
    }
  }, [isAuthenticated, navigate]);

  // Check if token and email are present in URL
  const missingParams = !token || !email;

  // Validation schema
  const validationSchema = Yup.object({
    newPassword: Yup.string()
      .min(8, "Password must be at least 8 characters")
      .required("New password is required")
      .matches(
        /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/,
        "Password must include uppercase, lowercase, number and special character"
      ),
    confirmNewPassword: Yup.string()
      .oneOf([Yup.ref("newPassword"), null], "Passwords must match")
      .required("Confirm password is required"),
  });

  // Initial values
  const initialValues = {
    newPassword: "",
    confirmNewPassword: "",
  };

  // Handle form submission
  const handleSubmit = async (values, { setSubmitting }) => {
    setServerError("");
    setSuccess(false);

    try {
      // Prepare the reset data exactly as the API expects it
      const resetData = {
        email: email,
        token: token,
        newPassword: values.newPassword,
        confirmNewPassword: values.confirmNewPassword,
      };

      console.log("Sending reset password request...");

      const result = await resetPassword(resetData);
      console.log("Reset password response:", result);

      setSuccess(true);

      // Redirect to login after 3 seconds
      setTimeout(() => {
        navigate("/login");
      }, 3000);
    } catch (error) {
      console.error("Password reset error:", error);
      if (error.response && error.response.data && error.response.data.errors) {
        // Format validation errors nicely
        const errorMessages = Object.values(error.response.data.errors)
          .flat()
          .join(". ");
        setServerError(errorMessages);
      } else if (
        error.response &&
        error.response.data &&
        error.response.data.message
      ) {
        // Server returned a specific message
        setServerError(error.response.data.message);
      } else {
        // Generic error
        setServerError(
          "Failed to reset password. The link may be expired or invalid."
        );
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <PageLayout>
      <div className="reset-password-container">
        <div className="reset-password-card">
          <h2>Reset Your Password</h2>

          {missingParams ? (
            <div className="error-container">
              <div className="error-icon">
                <i className="fas fa-exclamation-circle"></i>
              </div>
              <h3>Invalid Reset Link</h3>
              <p>
                The password reset link is invalid or incomplete. Please request
                a new password reset link.
              </p>
              <div className="action-links">
                <Button to="/forgot-password" variant="primary">
                  Request New Link
                </Button>
                <Link to="/login" className="back-link">
                  Back to Login
                </Link>
              </div>
            </div>
          ) : (
            <>
              <p className="description">
                Enter your new password below to reset your account password.
              </p>

              {serverError && (
                <div className="error-message">{serverError}</div>
              )}

              {success ? (
                <div className="success-container">
                  <div className="success-icon">
                    <i className="fas fa-check-circle"></i>
                  </div>
                  <h3>Password Reset Successful!</h3>
                  <p>
                    Your password has been reset successfully. You will be
                    redirected to the login page in a few seconds.
                  </p>
                </div>
              ) : (
                <Formik
                  initialValues={initialValues}
                  validationSchema={validationSchema}
                  onSubmit={handleSubmit}
                >
                  {({ isSubmitting }) => (
                    <Form className="reset-password-form">
                      <div className="form-group">
                        <label htmlFor="newPassword">New Password</label>
                        <Field
                          type="password"
                          id="newPassword"
                          name="newPassword"
                          className="form-control"
                          placeholder="Enter new password"
                        />
                        <ErrorMessage
                          name="newPassword"
                          component="div"
                          className="field-error"
                        />
                      </div>

                      <div className="form-group">
                        <label htmlFor="confirmNewPassword">
                          Confirm New Password
                        </label>
                        <Field
                          type="password"
                          id="confirmNewPassword"
                          name="confirmNewPassword"
                          className="form-control"
                          placeholder="Confirm new password"
                        />
                        <ErrorMessage
                          name="confirmNewPassword"
                          component="div"
                          className="field-error"
                        />
                      </div>

                      <Button
                        type="submit"
                        variant="primary"
                        className="submit-btn"
                        disabled={isSubmitting}
                      >
                        {isSubmitting ? "Resetting..." : "Reset Password"}
                      </Button>

                      <div className="action-links">
                        <Link to="/login" className="back-link">
                          Back to Login
                        </Link>
                      </div>
                    </Form>
                  )}
                </Formik>
              )}
            </>
          )}
        </div>
      </div>
    </PageLayout>
  );
};

export default ResetPasswordPage;
