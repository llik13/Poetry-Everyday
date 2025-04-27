import React from "react";
import { Link } from "react-router-dom";
import "./Button.css";

const Button = ({
  children,
  variant = "primary",
  size = "md",
  type = "button",
  to = null,
  onClick = null,
  disabled = false,
  className = "",
  ...props
}) => {
  const btnClass = `btn btn-${variant} btn-${size} ${className}`;

  // If 'to' prop is provided, render a Link
  if (to) {
    return (
      <Link to={to} className={btnClass} {...props}>
        {children}
      </Link>
    );
  }

  // Otherwise, render a button
  return (
    <button
      type={type}
      className={btnClass}
      onClick={onClick}
      disabled={disabled}
      {...props}
    >
      {children}
    </button>
  );
};

export default Button;
