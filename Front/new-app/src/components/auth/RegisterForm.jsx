import React, { useState, useContext } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Formik, Form, Field, ErrorMessage } from "formik";
import * as Yup from "yup";
import AuthContext from "../../context/AuthContext";
import Button from "../common/Button";
import "./AuthForms.css";

const RegisterForm = () => {
  const { register } = useContext(AuthContext);
  const [serverError, setServerError] = useState("");
  const [registrationSuccess, setRegistrationSuccess] = useState(false);
  const [registeredEmail, setRegisteredEmail] = useState("");
  const navigate = useNavigate();

  // Validation schema
  const validationSchema = Yup.object({
    userName: Yup.string()
      .min(3, "Username must be at least 3 characters")
      .max(50, "Username cannot be longer than 50 characters")
      .required("Username is required"),
    email: Yup.string()
      .email("Invalid email address")
      .required("Email is required"),
    password: Yup.string()
      .min(6, "Password must be at least 6 characters")
      .required("Password is required"),
    confirmPassword: Yup.string()
      .oneOf([Yup.ref("password"), null], "Passwords must match")
      .required("Confirm Password is required"),
  });

  // Initial values
  const initialValues = {
    userName: "",
    email: "",
    password: "",
    confirmPassword: "",
  };

  // Handle form submission
  const handleSubmit = async (values, { setSubmitting, resetForm }) => {
    setServerError("");
    setRegistrationSuccess(false);

    try {
      const result = await register(values);

      if (result.success) {
        setRegisteredEmail(values.email);
        setRegistrationSuccess(true);
        resetForm();

        // We won't automatically redirect to login since email verification is required
      } else {
        setServerError(
          result.message || "Registration failed. Please try again."
        );
      }
    } catch (error) {
      setServerError(
        error.message ||
          "An error occurred during registration. Please try again."
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-form-container">
      <h2 className="auth-title">Create Account</h2>

      {serverError && <div className="auth-error-message">{serverError}</div>}

      {registrationSuccess && (
        <div className="auth-success-message">
          <p>
            Registration successful! Please check your email to verify your
            account.
          </p>
          <p>
            We've sent a verification link to <strong>{registeredEmail}</strong>
            .
          </p>
          <p>You'll need to verify your email before you can log in.</p>
        </div>
      )}

      <Formik
        initialValues={initialValues}
        validationSchema={validationSchema}
        onSubmit={handleSubmit}
      >
        {({ isSubmitting }) => (
          <Form className="auth-form">
            <div className="form-group">
              <label htmlFor="userName">Username</label>
              <Field
                type="text"
                id="userName"
                name="userName"
                className="form-control"
                placeholder="Choose a username"
              />
              <ErrorMessage
                name="userName"
                component="div"
                className="field-error"
              />
            </div>

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
                placeholder="Create a password"
              />
              <ErrorMessage
                name="password"
                component="div"
                className="field-error"
              />
            </div>

            <div className="form-group">
              <label htmlFor="confirmPassword">Confirm Password</label>
              <Field
                type="password"
                id="confirmPassword"
                name="confirmPassword"
                className="form-control"
                placeholder="Confirm your password"
              />
              <ErrorMessage
                name="confirmPassword"
                component="div"
                className="field-error"
              />
            </div>

            <Button
              type="submit"
              variant="primary"
              className="auth-submit-btn"
              disabled={isSubmitting || registrationSuccess}
            >
              {isSubmitting ? "Signing up..." : "Sign Up"}
            </Button>

            <div className="auth-links">
              <Link to="/login" className="auth-link">
                Already have an account? Sign in
              </Link>
            </div>
          </Form>
        )}
      </Formik>
    </div>
  );
};

export default RegisterForm;
