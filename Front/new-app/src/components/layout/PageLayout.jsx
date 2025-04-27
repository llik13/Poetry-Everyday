import React from "react";
import Header from "../common/Header";
import Footer from "../common/Footer";
import "./PageLayout.css";

const PageLayout = ({ children }) => {
  return (
    <div className="page-layout">
      <Header />
      <main className="main-content">{children}</main>
      <Footer />
    </div>
  );
};

export default PageLayout;
