import React, { useState, useContext, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Formik, Form, Field, ErrorMessage } from "formik";
import * as Yup from "yup";
import { forgotPassword } from "../services/authService";
import PageLayout from "../components/layout/PageLayout";
import Button from "../components/common/Button";
import AuthContext from "../context/AuthContext";
import "./ForgotPasswordPage.css";

const ForgotPasswordPage = () => {
  const { isAuthenticated } = useContext(AuthContext);
  const navigate = useNavigate();
  const [serverError, setServerError] = useState("");
  const [success, setSuccess] = useState(false);
  const [email, setEmail] = useState("");

  // Redirect if already logged in
  useEffect(() => {
    if (isAuthenticated) {
      navigate("/cabinet");
    }
  }, [isAuthenticated, navigate]);

  // Validation schema
  const validationSchema = Yup.object({
    email: Yup.string()
      .email("Invalid email address")
      .required("Email is required"),
  });

  // Initial values
  const initialValues = {
    email: "",
  };

  // Handle form submission
  const handleSubmit = async (values, { setSubmitting }) => {
    setServerError("");
    setSuccess(false);

    try {
      await forgotPassword(values.email);
      setEmail(values.email);
      setSuccess(true);
    } catch (error) {
      setServerError(
        error.message ||
          "Failed to process password reset request. Please try again."
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <PageLayout>
      <div className="forgot-password-container">
        <div className="forgot-password-card">
          <h2>Forgot Password</h2>

          <p className="description">
            Enter your email address below and we'll send you a link to reset
            your password.
          </p>

          {serverError && <div className="error-message">{serverError}</div>}

          {success ? (
            <div className="success-container">
              <div className="success-icon">
                <i className="fas fa-check-circle"></i>
              </div>
              <h3>Reset Link Sent!</h3>
              <p>
                We've sent a password reset link to <strong>{email}</strong>.
                Please check your inbox (and spam folder) and follow the
                instructions to reset your password.
              </p>
              <div className="action-links">
                <Button to="/login" variant="primary">
                  Back to Login
                </Button>
              </div>
            </div>
          ) : (
            <Formik
              initialValues={initialValues}
              validationSchema={validationSchema}
              onSubmit={handleSubmit}
            >
              {({ isSubmitting }) => (
                <Form className="forgot-password-form">
                  <div className="form-group">
                    <label htmlFor="email">Email Address</label>
                    <Field
                      type="email"
                      id="email"
                      name="email"
                      className="form-control"
                      placeholder="Enter your email"
                    />
                    <ErrorMessage
                      name="email"
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
                    {isSubmitting ? "Sending..." : "Send Reset Link"}
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
        </div>
      </div>
    </PageLayout>
  );
};

export default ForgotPasswordPage;
