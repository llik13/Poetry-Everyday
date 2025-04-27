import React, { useState, useContext } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Formik, Form, Field, ErrorMessage } from "formik";
import * as Yup from "yup";
import AuthContext from "../../context/AuthContext";
import Button from "../common/Button";
import "./AuthForms.css";

const LoginForm = () => {
  const { login } = useContext(AuthContext);
  const [serverError, setServerError] = useState("");
  const navigate = useNavigate();

  // Validation schema
  const validationSchema = Yup.object({
    email: Yup.string()
      .email("Invalid email address")
      .required("Email is required"),
    password: Yup.string().required("Password is required"),
    rememberMe: Yup.boolean(),
  });

  // Initial values
  const initialValues = {
    email: "",
    password: "",
    rememberMe: false,
  };

  // Handle form submission
  const handleSubmit = async (values, { setSubmitting }) => {
    setServerError("");

    try {
      const success = await login(values);
      if (success) {
        navigate("/cabinet");
      } else {
        setServerError("Login failed. Please check your credentials.");
      }
    } catch (error) {
      setServerError(
        error.message || "An error occurred during login. Please try again."
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-form-container">
      <h2 className="auth-title">Sign In</h2>

      {serverError && <div className="auth-error-message">{serverError}</div>}

      <Formik
        initialValues={initialValues}
        validationSchema={validationSchema}
        onSubmit={handleSubmit}
      >
        {({ isSubmitting }) => (
          <Form className="auth-form">
            <div className="form-group">
              <label htmlFor="email">Email</label>
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

            <div className="form-group">
              <label htmlFor="password">Password</label>
              <Field
                type="password"
                id="password"
                name="password"
                className="form-control"
                placeholder="Enter your password"
              />
              <ErrorMessage
                name="password"
                component="div"
                className="field-error"
              />
            </div>

            <div className="form-check">
              <Field
                type="checkbox"
                id="rememberMe"
                name="rememberMe"
                className="form-check-input"
              />
              <label htmlFor="rememberMe" className="form-check-label">
                Remember me
              </label>
            </div>

            <Button
              type="submit"
              variant="primary"
              className="auth-submit-btn"
              disabled={isSubmitting}
            >
              {isSubmitting ? "Signing in..." : "Sign In"}
            </Button>

            <div className="auth-links">
              <Link to="/forgot-password" className="auth-link">
                Forgot password?
              </Link>
              <span className="auth-separator">|</span>
              <Link to="/register" className="auth-link">
                Don't have an account? Sign up
              </Link>
            </div>
          </Form>
        )}
      </Formik>
    </div>
  );
};

export default LoginForm;
