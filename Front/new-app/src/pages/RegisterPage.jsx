import React, { useContext, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import PageLayout from "../components/layout/PageLayout";
import RegisterForm from "../components/auth/RegisterForm";
import AuthContext from "../context/AuthContext";

const RegisterPage = () => {
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
        <RegisterForm />
      </div>
    </PageLayout>
  );
};

export default RegisterPage;
