import React from "react";
import { NavLink } from "react-router-dom";
import "./CabinetNavigation.css";

const CabinetNavigation = () => {
  return (
    <ul className="nav nav-tabs cabinet-nav" role="tablist">
      <li className="nav-item" role="presentation">
        <NavLink
          to="/cabinet/poems"
          className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}
        >
          My Poems
        </NavLink>
      </li>
      <li className="nav-item" role="presentation">
        <NavLink
          to="/cabinet/drafts"
          className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}
        >
          Drafts
        </NavLink>
      </li>
      <li className="nav-item" role="presentation">
        <NavLink
          to="/cabinet/collections"
          className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}
        >
          Collections
        </NavLink>
      </li>
      <li className="nav-item" role="presentation">
        <NavLink
          to="/cabinet/comments"
          className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}
        >
          My Comments
        </NavLink>
      </li>
      <li className="nav-item" role="presentation">
        <NavLink
          to="/cabinet/activity"
          className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}
        >
          Activity
        </NavLink>
      </li>
      <li className="nav-item" role="presentation">
        <NavLink
          to="/cabinet/settings"
          className={({ isActive }) => `nav-link ${isActive ? "active" : ""}`}
        >
          Settings
        </NavLink>
      </li>
    </ul>
  );
};

export default CabinetNavigation;
