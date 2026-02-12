import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import type { RegisterRequest } from '../services/authService';
import './AuthLayout.css';

const BRAND = 'PoultryDistro';

const Register: React.FC = () => {
  const [fullName, setFullName] = useState('');
  const [companyName, setCompanyName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { register } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (password !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }

    if (password.length < 8) {
      setError('Password must be at least 8 characters.');
      return;
    }

    setLoading(true);

    try {
      const username = email.replace(/@.+$/, '').replace(/\W/g, '') || email;
      const payload: RegisterRequest = {
        username,
        email,
        password,
        confirmPassword,
        fullName,
        phone: companyName.trim() || '',
        role: 'ShopOwner',
      };
      await register(payload);
      navigate('/login');
    } catch (err: unknown) {
      setError(
        err instanceof Error ? err.message : 'Registration failed. Please try again.'
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
        <h1 className="auth-headline">Join the poultry distribution platform</h1>
        <p className="auth-subtitle">
          Scale your operations with a single system for tracking, inventory, and partner coordination.
        </p>
        <ul className="auth-features">
          <li>Secure access and data protection</li>
          <li>Live tracking and reporting</li>
          <li>Team access and role-based controls</li>
        </ul>
      </div>

      <div className="auth-panel-right">
        <header className="auth-header">
          <span className="auth-logo-text">{BRAND}</span>
          <nav className="auth-header-nav">
            <Link to="/">Support</Link>
            <Link to="/">Help</Link>
            <Link to="/login" className="auth-cta-link">Log in</Link>
          </nav>
        </header>

        <div className="auth-form-wrap">
          <h2 className="auth-title">Create account</h2>
          <p className="auth-description">Enter your details to get started.</p>

          <form className="auth-form" onSubmit={handleSubmit}>
            {error && <div className="auth-error">{error}</div>}

            <div className="form-group">
              <label htmlFor="reg-fullname">Full name</label>
              <input
                id="reg-fullname"
                type="text"
                autoComplete="name"
                placeholder="e.g. Jane Smith"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                required
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label htmlFor="reg-company">Company name</label>
              <input
                id="reg-company"
                type="text"
                autoComplete="organization"
                placeholder="e.g. Acme Poultry Ltd"
                value={companyName}
                onChange={(e) => setCompanyName(e.target.value)}
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label htmlFor="reg-email">Work email</label>
              <input
                id="reg-email"
                type="email"
                autoComplete="email"
                placeholder="jane@company.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                disabled={loading}
              />
            </div>

            <div className="form-group">
              <label htmlFor="reg-password">Password</label>
              <div className="auth-input-wrap">
                <input
                  id="reg-password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="new-password"
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
              <p className="auth-hint">At least 8 characters, including symbols if possible.</p>
            </div>

            <div className="form-group">
              <label htmlFor="reg-confirm">Confirm password</label>
              <input
                id="reg-confirm"
                type={showPassword ? 'text' : 'password'}
                autoComplete="new-password"
                placeholder="••••••••"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
                disabled={loading}
              />
            </div>

            <button type="submit" className="auth-submit" disabled={loading}>
              {loading ? 'Creating account…' : 'Create account'}
            </button>

            <p className="auth-switch">
              Already have an account? <Link to="/login">Sign in</Link>
            </p>

            <p className="auth-legal">
              By creating an account, you agree to our{' '}
              <Link to="/terms">Terms of Service</Link> and{' '}
              <Link to="/privacy">Privacy Policy</Link>.
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

export default Register;
