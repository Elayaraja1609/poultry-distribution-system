import React, { useEffect } from 'react';
import './SuccessAlert.css';

interface SuccessAlertProps {
  message: string;
  onClose?: () => void;
  autoDismiss?: boolean;
  dismissTime?: number;
}

const SuccessAlert: React.FC<SuccessAlertProps> = ({ 
  message, 
  onClose,
  autoDismiss = true,
  dismissTime = 3000
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
    <div className="success-alert">
      <span className="success-icon">✓</span>
      <span className="success-message">{message}</span>
      {onClose && (
        <button className="success-close" onClick={onClose} aria-label="Close">
          ×
        </button>
      )}
    </div>
  );
};

export default SuccessAlert;
