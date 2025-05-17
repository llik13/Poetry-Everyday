import React, { useState } from "react";
import { Link } from "react-router-dom";
import { Formik, Form, Field, ErrorMessage } from "formik";
import * as Yup from "yup";
import { resendVerificationEmail } from "../services/authService";
import PageLayout from "../components/layout/PageLayout";
import Button from "../components/common/Button";
import "./ResendVerificationPage.css";

const ResendVerificationPage = () => {
  const [serverError, setServerError] = useState("");
  const [success, setSuccess] = useState(false);

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
      await resendVerificationEmail(values.email);
      setSuccess(true);
    } catch (error) {
      setServerError(
        error.message ||
          "Failed to resend verification email. Please try again."
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <PageLayout>
      <div className="resend-verification-container">
        <div className="resend-verification-card">
          <h2>Resend Verification Email</h2>

          <p className="description">
            If you haven't received the verification email or if the link has
            expired, enter your email address below and we'll send a new
            verification link.
          </p>

          {serverError && <div className="error-message">{serverError}</div>}

          {success ? (
            <div className="success-container">
              <div className="success-icon">
                <i className="fas fa-check-circle"></i>
              </div>
              <h3>Verification Email Sent!</h3>
              <p>
                We've sent a new verification link to your email address. Please
                check your inbox (and spam folder) and click the link to verify
                your account.
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
                <Form className="resend-form">
                  <div className="form-group">
                    <label htmlFor="email">Email Address</label>
                    <Field
                      type="email"
                      id="email"
                      name="email"
                      className="form-control"
                      placeholder="Enter your registered email"
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
                    {isSubmitting ? "Sending..." : "Resend Verification Email"}
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

export default ResendVerificationPage;
