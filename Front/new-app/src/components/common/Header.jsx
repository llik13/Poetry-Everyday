import React, { useContext } from "react";
import { Link, useLocation } from "react-router-dom";
import AuthContext from "../../context/AuthContext";
import "./Header.css";

const Header = () => {
  const { isAuthenticated, currentUser, logout } = useContext(AuthContext);
  const location = useLocation();

  const handleLogout = async (e) => {
    e.preventDefault();
    await logout();
  };

  return (
    <header className="header">
      <h2 className="header-main">Poetry Everyday</h2>
      <nav>
        <ul className="main-nav-list">
          <li>
            <Link
              className={`main-nav-link ${
                location.pathname === "/" ? "active" : ""
              }`}
              to="/"
            >
              Home
            </Link>
          </li>
          <li>
            <Link
              className={`main-nav-link ${
                location.pathname === "/catalog" ? "active" : ""
              }`}
              to="/catalog"
            >
              Poems
            </Link>
          </li>
          <li>
            <Link
              className={`main-nav-link ${
                location.pathname.includes("/forum") ? "active" : ""
              }`}
              to="/forum"
            >
              Forum
            </Link>
          </li>

          {isAuthenticated ? (
            <>
              <li>
                <Link
                  className={`main-nav-link ${
                    location.pathname.includes("/cabinet") ? "active" : ""
                  }`}
                  to="/cabinet"
                >
                  Cabinet
                </Link>
              </li>
              <li>
                <a href="#" className="main-nav-link" onClick={handleLogout}>
                  Logout
                </a>
              </li>
            </>
          ) : (
            <>
              <li>
                <Link
                  className={`main-nav-link ${
                    location.pathname === "/login" ? "active" : ""
                  }`}
                  to="/login"
                >
                  Login
                </Link>
              </li>
              <li>
                <Link
                  className={`main-nav-link ${
                    location.pathname === "/register" ? "active" : ""
                  }`}
                  to="/register"
                >
                  Sign up
                </Link>
              </li>
            </>
          )}
        </ul>
      </nav>
    </header>
  );
};

export default Header;
