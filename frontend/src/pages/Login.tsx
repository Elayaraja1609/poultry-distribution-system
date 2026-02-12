import React, { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Login.css';
import './AuthLayout.css';

const BRAND = 'PoultryDistro';

const Login: React.FC = () => {
  const [usernameOrEmail, setUsernameOrEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [keepSignedIn, setKeepSignedIn] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { user, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  // Navigate to dashboard only after auth state has updated (avoids ProtectedRoute seeing stale user=null)
  useEffect(() => {
    if (user && location.pathname === '/login') {
      navigate('/dashboard', { replace: true });
    }
  }, [user, location.pathname, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      await login({ usernameOrEmail, password });
      // Do not navigate here - useEffect above runs after setUser and then redirects
    } catch (err: unknown) {
      setError(
        err instanceof Error ? err.message : 'Sign-in failed. Please check your credentials.'
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-page">
      <div
        className="auth-panel-left"
        style={{
          backgroundImage: `url(${process.env.PUBLIC_URL || ''}/images/auth-bg-left.jpg)`,
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          backgroundRepeat: 'no-repeat',
        }}
      >
        <div className="auth-logo">{BRAND}</div>
        <h1 className="auth-headline">Manage your distribution fleet from one place</h1>
        <p className="auth-subtitle">
          Use your account to track deliveries, check stock, and work with partners across the network.
        </p>
        <ul className="auth-features">
          <li>Secure access and data protection</li>
          <li>Live tracking and reporting</li>
          <li>Team access and role-based controls</li>
        </ul>
      </div>

      <div className="auth-panel-right">
        <header className="auth-header">
          <span className="auth-logo-text"></span>
          <nav className="auth-header-nav">
            <Link to="/">Support</Link>
            <Link to="/">Help</Link>
            <Link to="/register" className="auth-cta-link">Sign up</Link>
          </nav>
        </header>

        <div className="auth-form-wrap">
          <h2 className="auth-title">Welcome back</h2>
          <p className="auth-description">Sign in to your account.</p>

          <form className="auth-form" onSubmit={handleSubmit}>
            {error && <div className="auth-error">{error}</div>}

            <div className="form-group">
              <label htmlFor="login-email">Email address</label>
              <input
                id="login-email"
                type="text"
                autoComplete="username email"
                placeholder="name@company.com"
                value={usernameOrEmail}
                onChange={(e) => setUsernameOrEmail(e.target.value)}
                required
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label htmlFor="login-password">
                Password
                <Link to="/forgot-password" className="auth-link">Forgot password?</Link>
              </label>
              <div className="auth-input-wrap">
                <input
                  id="login-password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                  disabled={loading}
                  className="auth-input-with-icon"
                />
                <button
                  type="button"
                  className="auth-password-toggle"
                  onClick={() => setShowPassword(!showPassword)}
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                >
                  {showPassword ? (
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" />
                      <line x1="1" y1="1" x2="23" y2="23" />
                    </svg>
                  ) : (
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                      <circle cx="12" cy="12" r="3" />
                    </svg>
                  )}
                </button>
              </div>
            </div>

            <div className="auth-checkbox-wrap">
              <input
                id="login-keep"
                type="checkbox"
                checked={keepSignedIn}
                onChange={(e) => setKeepSignedIn(e.target.checked)}
                disabled={loading}
              />
              <label htmlFor="login-keep">Keep me signed in</label>
            </div>

            <button type="submit" className="auth-submit" disabled={loading}>
              {loading ? 'Signing in…' : 'Sign in'}
            </button>

            <p className="auth-switch">
              Don&apos;t have an account? <Link to="/register">Create account</Link>
            </p>
          </form>
        </div>

        <footer className="auth-footer">
          <Link to="/">Status</Link>
          <Link to="/">Security</Link>
          <Link to="/">API</Link>
        </footer>
      </div>
    </div>
  );
};

export default Login;
