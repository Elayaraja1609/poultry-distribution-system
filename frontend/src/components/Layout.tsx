import React from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { getNavItemsForRole } from '../config/navConfig';
import NotificationBell from './NotificationBell';
import './Layout.css';

interface LayoutProps {
  children: React.ReactNode;
}

const Layout: React.FC<LayoutProps> = ({ children }) => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const navItems = getNavItemsForRole(user?.role || '');
  const getPageTitle = () => {
    const item = navItems.find(item => item.path === location.pathname);
    return item ? item.label : 'Dashboard';
  };

  const getBreadcrumbs = () => {
    const pathParts = location.pathname.split('/').filter(Boolean);
    if (pathParts.length === 0) return 'Dashboard';
    return pathParts.map(part => part.charAt(0).toUpperCase() + part.slice(1)).join(' / ');
  };

  return (
    <div className="layout">
      {/* Left Sidebar */}
      <aside className="sidebar">
        <div className="sidebar-header">
          <div className="sidebar-logo">
            <span className="logo-icon">🐔</span>
            <div className="logo-text">
              <div className="logo-title">PoultryDistro</div>
              <div className="logo-subtitle">{user?.role === 'Admin' ? 'Enterprise Admin' : 'Portal'}</div>
            </div>
          </div>
        </div>

        <nav className="sidebar-nav">
          {navItems.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <Link
                key={item.path}
                to={item.path}
                className={`nav-item ${isActive ? 'active' : ''}`}
              >
                <span className="nav-icon">{item.icon}</span>
                <span className="nav-label">{item.label}</span>
              </Link>
            );
          })}
        </nav>

        <div className="sidebar-footer">
          <div className="user-profile">
            <div className="user-avatar">
              {user?.fullName?.[0] || user?.username?.[0] || 'U'}
            </div>
            <div className="user-info">
              <div className="user-name">{user?.fullName || user?.username || 'User'}</div>
              <div className="user-role">{user?.role || 'Admin'}</div>
            </div>
          </div>
          <button onClick={handleLogout} className="btn-logout">
            Logout
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <div className="main-wrapper">
        {/* Top Header */}
        <header className="top-header">
          <div className="header-left">
            <div className="breadcrumbs">{getBreadcrumbs()}</div>
            <h1 className="page-title">{getPageTitle()}</h1>
          </div>
          <div className="header-center">
            <div className="search-bar">
              <span className="search-icon">🔍</span>
              <input
                type="text"
                placeholder="Search data, orders..."
                className="search-input"
              />
            </div>
          </div>
          <div className="header-right">
            <NotificationBell />
            <div className="user-avatar-small">
              {user?.fullName?.[0] || user?.username?.[0] || 'U'}
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="main-content">{children}</main>
      </div>
    </div>
  );
};

export default Layout;
