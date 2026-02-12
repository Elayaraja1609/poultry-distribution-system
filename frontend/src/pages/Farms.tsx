import React, { useState, useEffect } from 'react';
import farmService, { Farm, CreateFarmDto, UpdateFarmDto } from '../services/farmService';
import './Farms.css';

const Farms: React.FC = () => {
  const [farms, setFarms] = useState<Farm[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingFarm, setEditingFarm] = useState<Farm | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [formData, setFormData] = useState<CreateFarmDto>({
    name: '',
    location: '',
    capacity: 0,
    managerName: '',
    phone: '',
  });

  useEffect(() => {
    loadFarms();
  }, [pageNumber]);

  const loadFarms = async () => {
    try {
      setLoading(true);
      const response = await farmService.getAll(pageNumber, 10);
      if (response.success && response.data) {
        setFarms(response.data.items);
        setTotalPages(response.data.totalPages);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load farms');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingFarm) {
        const updateData: UpdateFarmDto = {
          ...formData,
          isActive: editingFarm.isActive,
        };
        await farmService.update(editingFarm.id, updateData);
      } else {
        await farmService.create(formData);
      }
      setShowModal(false);
      resetForm();
      loadFarms();
    } catch (err: any) {
      setError(err.message || 'Failed to save farm');
    }
  };

  const handleEdit = (farm: Farm) => {
    setEditingFarm(farm);
    setFormData({
      name: farm.name,
      location: farm.location,
      capacity: farm.capacity,
      managerName: farm.managerName,
      phone: farm.phone,
    });
    setShowModal(true);
  };

  const handleDelete = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this farm?')) {
      try {
        await farmService.delete(id);
        loadFarms();
      } catch (err: any) {
        setError(err.message || 'Failed to delete farm');
      }
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      location: '',
      capacity: 0,
      managerName: '',
      phone: '',
    });
    setEditingFarm(null);
  };

  const getCapacityPercentage = (current: number, capacity: number) => {
    if (capacity === 0) return 0;
    return Math.round((current / capacity) * 100);
  };

  return (
    <div className="farms-page">
      <div className="page-header">
        <h1>Farms</h1>
        <button className="btn-primary" onClick={() => { resetForm(); setShowModal(true); }}>
          Add Farm
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {loading ? (
        <div>Loading...</div>
      ) : (
        <>
          <div className="farms-grid">
            {farms.map((farm) => {
              const capacityPercent = getCapacityPercentage(farm.currentCount, farm.capacity);
              return (
                <div key={farm.id} className="farm-card">
                  <div className="farm-header">
                    <h3>{farm.name}</h3>
                    <span className={`status-badge ${farm.isActive ? 'active' : 'inactive'}`}>
                      {farm.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </div>
                  <div className="farm-details">
                    <p><strong>Location:</strong> {farm.location}</p>
                    <p><strong>Manager:</strong> {farm.managerName}</p>
                    <p><strong>Phone:</strong> {farm.phone}</p>
                    <div className="capacity-info">
                      <div className="capacity-label">
                        <span>Capacity: {farm.currentCount} / {farm.capacity}</span>
                        <span>{capacityPercent}%</span>
                      </div>
                      <div className="capacity-bar">
                        <div
                          className="capacity-fill"
                          style={{ width: `${capacityPercent}%` }}
                        />
                      </div>
                    </div>
                  </div>
                  <div className="farm-actions">
                    <button className="btn-edit" onClick={() => handleEdit(farm)}>
                      Edit
                    </button>
                    <button className="btn-delete" onClick={() => handleDelete(farm.id)}>
                      Delete
                    </button>
                  </div>
                </div>
              );
            })}
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
        </>
      )}

      {showModal && (
        <div className="modal-overlay" onClick={() => { setShowModal(false); resetForm(); }}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h2>{editingFarm ? 'Edit Farm' : 'Add Farm'}</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Name *</label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Location *</label>
                <input
                  type="text"
                  value={formData.location}
                  onChange={(e) => setFormData({ ...formData, location: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Capacity *</label>
                <input
                  type="number"
                  value={formData.capacity}
                  onChange={(e) => setFormData({ ...formData, capacity: parseInt(e.target.value) || 0 })}
                  required
                  min="1"
                />
              </div>
              <div className="form-group">
                <label>Manager Name *</label>
                <input
                  type="text"
                  value={formData.managerName}
                  onChange={(e) => setFormData({ ...formData, managerName: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Phone *</label>
                <input
                  type="text"
                  value={formData.phone}
                  onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                  required
                />
              </div>
              <div className="modal-actions">
                <button type="submit" className="btn-primary">
                  {editingFarm ? 'Update' : 'Create'}
                </button>
                <button
                  type="button"
                  className="btn-secondary"
                  onClick={() => { setShowModal(false); resetForm(); }}
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default Farms;
