import React, { useEffect } from 'react';
import './ErrorAlert.css';

interface ErrorAlertProps {
  message: string;
  type?: 'error' | 'warning' | 'info';
  onClose?: () => void;
  autoDismiss?: boolean;
  dismissTime?: number;
}

const ErrorAlert: React.FC<ErrorAlertProps> = ({ 
  message, 
  type = 'error',
  onClose,
  autoDismiss = true,
  dismissTime = 5000
}) => {
  useEffect(() => {
    if (autoDismiss && onClose) {
      const timer = setTimeout(() => {
        onClose();
      }, dismissTime);
      return () => clearTimeout(timer);
    }
  }, [autoDismiss, dismissTime, onClose]);

  return (
    <div className={`error-alert error-alert-${type}`}>
      <span className="error-icon">
        {type === 'error' ? '⚠' : type === 'warning' ? '⚠' : 'ℹ'}
      </span>
      <span className="error-message">{message}</span>
      {onClose && (
        <button className="error-close" onClick={onClose} aria-label="Close">
          ×
        </button>
      )}
    </div>
  );
};

export default ErrorAlert;
