import React from 'react';
import './LoadingSpinner.css';

interface LoadingSpinnerProps {
  inline?: boolean;
  size?: 'small' | 'medium' | 'large';
}

const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({ inline = false, size = 'medium' }) => {
  if (inline) {
    return (
      <div className={`loading-spinner-inline loading-spinner-${size}`}>
        <div className="spinner"></div>
      </div>
    );
  }

  return (
    <div className="loading-spinner-overlay">
      <div className="loading-spinner">
        <div className="spinner"></div>
        <p>Loading...</p>
      </div>
    </div>
  );
};

export default LoadingSpinner;
