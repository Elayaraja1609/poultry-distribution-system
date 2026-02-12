import React, { useState, useEffect } from 'react';
import authService from '../services/authService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import SuccessAlert from '../components/SuccessAlert';
import './Profile.css';

interface UserProfile {
  id: string;
  username: string;
  email: string;
  role: string;
  fullName?: string;
}

const Profile: React.FC = () => {
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({
    fullName: '',
    phone: '',
    address: '',
  });

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await authService.getProfile();
      if (response.success && response.data) {
        setProfile(response.data);
        setFormData({
          fullName: response.data.fullName || '',
          phone: '',
          address: '',
        });
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load profile');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setError('');
      setSuccess('');
      const response = await authService.updateProfile(formData);
      if (response.success && response.data) {
        setProfile(response.data);
        setIsEditing(false);
        setSuccess('Profile updated successfully');
        setTimeout(() => setSuccess(''), 3000);
      } else {
        setError(response.message || 'Failed to update profile');
      }
    } catch (err: any) {
      setError(err.message || 'Failed to update profile');
    }
  };

  const handleCancel = () => {
    setIsEditing(false);
    if (profile) {
      setFormData({
        fullName: profile.fullName || '',
        phone: '',
        address: '',
      });
    }
  };

  if (loading) {
    return <LoadingSpinner />;
  }

  if (!profile) {
    return (
      <div className="profile-page">
        <ErrorAlert message="Failed to load profile" onClose={() => setError('')} />
      </div>
    );
  }

  return (
    <div className="profile-page">
      <div className="page-header-section">
        <div className="page-title-section">
          <h1>My Profile</h1>
          <p>Manage your account details.</p>
        </div>
        {!isEditing && (
          <button className="btn-primary" onClick={() => setIsEditing(true)}>
            Edit Profile
          </button>
        )}
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}
      {success && <SuccessAlert message={success} onClose={() => setSuccess('')} />}

      {!isEditing ? (
        <div className="profile-info card">
          <div className="info-row">
            <label>Username:</label>
            <span>{profile.username}</span>
          </div>
          <div className="info-row">
            <label>Email:</label>
            <span>{profile.email}</span>
          </div>
          <div className="info-row">
            <label>Role:</label>
            <span>{profile.role}</span>
          </div>
          <div className="info-row">
            <label>Full Name:</label>
            <span>{profile.fullName || 'Not set'}</span>
          </div>
        </div>
      ) : (
        <form onSubmit={handleSubmit} className="profile-form card">
          <div className="form-group">
            <label>Username:</label>
            <input type="text" value={profile.username} disabled />
          </div>
          <div className="form-group">
            <label>Email:</label>
            <input type="email" value={profile.email} disabled />
          </div>
          <div className="form-group">
            <label>Full Name:</label>
            <input
              type="text"
              value={formData.fullName}
              onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
              placeholder="Enter your full name"
            />
          </div>
          <div className="form-group">
            <label>Phone:</label>
            <input
              type="tel"
              value={formData.phone}
              onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
              placeholder="Enter your phone number"
            />
          </div>
          <div className="form-group">
            <label>Address:</label>
            <textarea
              value={formData.address}
              onChange={(e) => setFormData({ ...formData, address: e.target.value })}
              placeholder="Enter your address"
              rows={3}
            />
          </div>
          <div className="form-actions">
            <button type="submit" className="btn-primary">
              Save Changes
            </button>
            <button type="button" className="btn-secondary" onClick={handleCancel}>
              Cancel
            </button>
          </div>
        </form>
      )}
    </div>
  );
};

export default Profile;
