import React, { useContext, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import PageLayout from "../components/layout/PageLayout";
import LoginForm from "../components/auth/LoginForm";
import AuthContext from "../context/AuthContext";

const LoginPage = () => {
  const { isAuthenticated } = useContext(AuthContext);
  const navigate = useNavigate();

  // Redirect if already logged in
  useEffect(() => {
    if (isAuthenticated) {
      navigate("/cabinet");
    }
  }, [isAuthenticated, navigate]);

  return (
    <PageLayout>
      <div className="container">
        <LoginForm />
      </div>
    </PageLayout>
  );
};

export default LoginPage;
