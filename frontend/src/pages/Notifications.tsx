import React, { useState, useEffect } from 'react';
import notificationService, { Notification } from '../services/notificationService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import './Notifications.css';

const Notifications: React.FC = () => {
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filters, setFilters] = useState({
    isRead: undefined as boolean | undefined,
    type: '',
  });
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    loadNotifications();
  }, [pageNumber, filters]);

  const loadNotifications = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await notificationService.getAll(
        filters.isRead,
        filters.type || undefined,
        pageNumber,
        20
      );
      if (response.success && response.data) {
        setNotifications(response.data.items);
        setTotalPages(response.data.totalPages);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load notifications');
    } finally {
      setLoading(false);
    }
  };

  const handleMarkAsRead = async (id: string) => {
    try {
      await notificationService.markAsRead(id);
      setNotifications(notifications.map(n => n.id === id ? { ...n, isRead: true, readAt: new Date().toISOString() } : n));
    } catch (err: any) {
      setError(err.message || 'Failed to mark notification as read');
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      await notificationService.markAllAsRead();
      setNotifications(notifications.map(n => ({ ...n, isRead: true, readAt: new Date().toISOString() })));
    } catch (err: any) {
      setError(err.message || 'Failed to mark all as read');
    }
  };

  const getNotificationIcon = (type: string) => {
    const icons: { [key: string]: string } = {
      'DeliveryScheduled': '🚚',
      'PaymentReminder': '💰',
      'OrderApproved': '✅',
      'OrderRejected': '❌',
      'OrderFulfilled': '📦',
      'LowStock': '⚠️',
      'SystemAlert': '🔔',
    };
    return icons[type] || '🔔';
  };

  if (loading && notifications.length === 0) {
    return <LoadingSpinner />;
  }

  return (
    <div className="notifications-page">
      <div className="page-header">
        <h1>Notifications</h1>
        {notifications.some(n => !n.isRead) && (
          <button className="btn-primary" onClick={handleMarkAllAsRead}>
            Mark All as Read
          </button>
        )}
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}

      <div className="filters">
        <select
          value={filters.isRead === undefined ? '' : filters.isRead.toString()}
          onChange={(e) => setFilters({ ...filters, isRead: e.target.value === '' ? undefined : e.target.value === 'true' })}
        >
          <option value="">All</option>
          <option value="false">Unread</option>
          <option value="true">Read</option>
        </select>
        <select
          value={filters.type}
          onChange={(e) => setFilters({ ...filters, type: e.target.value })}
        >
          <option value="">All Types</option>
          <option value="DeliveryScheduled">Delivery Scheduled</option>
          <option value="PaymentReminder">Payment Reminder</option>
          <option value="OrderApproved">Order Approved</option>
          <option value="OrderRejected">Order Rejected</option>
          <option value="OrderFulfilled">Order Fulfilled</option>
          <option value="LowStock">Low Stock</option>
          <option value="SystemAlert">System Alert</option>
        </select>
      </div>

      <div className="notifications-list">
        {notifications.map((notification) => (
          <div
            key={notification.id}
            className={`notification-card ${!notification.isRead ? 'unread' : ''}`}
          >
            <div className="notification-header">
              <span className="notification-icon">{getNotificationIcon(notification.type)}</span>
              <div className="notification-info">
                <h3>{notification.title}</h3>
                <span className="notification-time">
                  {new Date(notification.createdAt).toLocaleString()}
                </span>
              </div>
              {!notification.isRead && (
                <button
                  className="btn-mark-read"
                  onClick={() => handleMarkAsRead(notification.id)}
                >
                  Mark as Read
                </button>
              )}
            </div>
            <p className="notification-message">{notification.message}</p>
          </div>
        ))}
      </div>

      <div className="pagination">
        <button
          disabled={pageNumber === 1}
          onClick={() => setPageNumber(pageNumber - 1)}
        >
          Previous
        </button>
        <span>Page {pageNumber} of {totalPages}</span>
        <button
          disabled={pageNumber === totalPages}
          onClick={() => setPageNumber(pageNumber + 1)}
        >
          Next
        </button>
      </div>
    </div>
  );
};

export default Notifications;
