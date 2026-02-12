import React, { useState, useEffect } from 'react';
import distributionService, { Distribution, CreateDistributionDto } from '../services/distributionService';
import vehicleService from '../services/vehicleService';
import chickenService from '../services/chickenService';
import shopService from '../services/shopService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import './Distributions.css';

const Distributions: React.FC = () => {
  const [distributions, setDistributions] = useState<Distribution[]>([]);
  const [vehicles, setVehicles] = useState<any[]>([]);
  const [chickens, setChickens] = useState<any[]>([]);
  const [shops, setShops] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [formData, setFormData] = useState<CreateDistributionDto>({
    vehicleId: '',
    scheduledDate: new Date().toISOString().split('T')[0],
    items: [],
  });
  const [newItem, setNewItem] = useState({ chickenId: '', shopId: '', quantity: 0 });

  useEffect(() => {
    loadData();
  }, [pageNumber]);

  const loadData = async () => {
    try {
      setLoading(true);
      setError('');
      
      const [distributionsResponse, vehiclesResponse, chickensResponse, shopsResponse] = await Promise.all([
        distributionService.getAll(pageNumber, 10),
        vehicleService.getAll(1, 100),
        chickenService.getAll(1, 100),
        shopService.getAll(1, 100),
      ]);

      if (distributionsResponse.success && distributionsResponse.data) {
        setDistributions(distributionsResponse.data.items);
        setTotalPages(distributionsResponse.data.totalPages);
      }

      if (vehiclesResponse.success && vehiclesResponse.data) {
        setVehicles(vehiclesResponse.data.items);
      }

      if (chickensResponse.success && chickensResponse.data) {
        setChickens(chickensResponse.data.items.filter((c: any) => c.status === 'ReadyForDistribution'));
      }

      if (shopsResponse.success && shopsResponse.data) {
        setShops(shopsResponse.data.items);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const handleAddItem = () => {
    if (newItem.chickenId && newItem.shopId && newItem.quantity > 0) {
      setFormData({
        ...formData,
        items: [...formData.items, { ...newItem, chickenId: newItem.chickenId, shopId: newItem.shopId, quantity: newItem.quantity }],
      });
      setNewItem({ chickenId: '', shopId: '', quantity: 0 });
    }
  };

  const handleRemoveItem = (index: number) => {
    setFormData({
      ...formData,
      items: formData.items.filter((_, i) => i !== index),
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (formData.items.length === 0) {
      setError('Please add at least one distribution item');
      return;
    }

    try {
      setError('');
      await distributionService.create(formData);
      setShowModal(false);
      resetForm();
      loadData();
    } catch (err: any) {
      setError(err.message || 'Failed to create distribution');
    }
  };

  const handleStatusUpdate = async (id: string, status: string) => {
    try {
      setError('');
      await distributionService.updateStatus(id, status);
      loadData();
    } catch (err: any) {
      setError(err.message || 'Failed to update status');
    }
  };

  const resetForm = () => {
    setFormData({
      vehicleId: '',
      scheduledDate: new Date().toISOString().split('T')[0],
      items: [],
    });
    setNewItem({ chickenId: '', shopId: '', quantity: 0 });
  };

  const getStatusBadgeClass = (status: string) => {
    const statusMap: { [key: string]: string } = {
      'Scheduled': 'info',
      'InTransit': 'info',
      'Completed': 'success',
      'Cancelled': 'error',
    };
    return statusMap[status] || 'pending';
  };

  if (loading && distributions.length === 0) {
    return <LoadingSpinner />;
  }

  return (
    <div className="distributions-page">
      <div className="page-header">
        <h1>Distributions</h1>
        <button className="btn-primary" onClick={() => { resetForm(); setShowModal(true); }}>
          Schedule Distribution
        </button>
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}

      <div className="distributions-grid">
        {distributions.map((distribution) => (
          <div key={distribution.id} className="distribution-card">
            <div className="distribution-header">
              <h3>Distribution #{distribution.id.substring(0, 8)}</h3>
              <span className={`status-badge ${getStatusBadgeClass(distribution.status)}`}>
                {distribution.status}
              </span>
            </div>
            <div className="distribution-details">
              <p><strong>Vehicle:</strong> {distribution.vehicleNumber}</p>
              <p><strong>Driver:</strong> {distribution.driverName}</p>
              <p><strong>Cleaner:</strong> {distribution.cleanerName}</p>
              <p><strong>Scheduled:</strong> {new Date(distribution.scheduledDate).toLocaleDateString()}</p>
              <p><strong>Items:</strong> {distribution.totalItems}</p>
            </div>
            <div className="distribution-actions">
              {distribution.status === 'Scheduled' && (
                <button
                  className="btn-status"
                  onClick={() => handleStatusUpdate(distribution.id, 'InTransit')}
                >
                  Start Delivery
                </button>
              )}
              {distribution.status === 'InTransit' && (
                <button
                  className="btn-status"
                  onClick={() => handleStatusUpdate(distribution.id, 'Completed')}
                >
                  Complete
                </button>
              )}
            </div>
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

      {showModal && (
        <div className="modal-overlay" onClick={() => { setShowModal(false); resetForm(); }}>
          <div className="modal-content large" onClick={(e) => e.stopPropagation()}>
            <h2>Schedule Distribution</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Vehicle *</label>
                <select
                  value={formData.vehicleId}
                  onChange={(e) => setFormData({ ...formData, vehicleId: e.target.value })}
                  required
                >
                  <option value="">Select Vehicle</option>
                  {vehicles.filter(v => v.isActive).map((vehicle) => (
                    <option key={vehicle.id} value={vehicle.id}>
                      {vehicle.vehicleNumber} - {vehicle.driverName}
                    </option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label>Scheduled Date *</label>
                <input
                  type="date"
                  value={formData.scheduledDate}
                  onChange={(e) => setFormData({ ...formData, scheduledDate: e.target.value })}
                  required
                />
              </div>

              <div className="distribution-items">
                <h3>Distribution Items</h3>
                <div className="add-item-form">
                  <select
                    value={newItem.chickenId}
                    onChange={(e) => setNewItem({ ...newItem, chickenId: e.target.value })}
                  >
                    <option value="">Select Chicken Batch</option>
                    {chickens.map((chicken) => (
                      <option key={chicken.id} value={chicken.id}>
                        {chicken.batchNumber} - Qty: {chicken.quantity}
                      </option>
                    ))}
                  </select>
                  <select
                    value={newItem.shopId}
                    onChange={(e) => setNewItem({ ...newItem, shopId: e.target.value })}
                  >
                    <option value="">Select Shop</option>
                    {shops.map((shop) => (
                      <option key={shop.id} value={shop.id}>
                        {shop.name}
                      </option>
                    ))}
                  </select>
                  <input
                    type="number"
                    placeholder="Quantity"
                    value={newItem.quantity || ''}
                    onChange={(e) => setNewItem({ ...newItem, quantity: parseInt(e.target.value) || 0 })}
                    min="1"
                  />
                  <button type="button" className="btn-add" onClick={handleAddItem}>
                    Add Item
                  </button>
                </div>

                <div className="items-list">
                  {formData.items.map((item, index) => {
                    const chicken = chickens.find(c => c.id === item.chickenId);
                    const shop = shops.find(s => s.id === item.shopId);
                    return (
                      <div key={index} className="item-row">
                        <span>{chicken?.batchNumber || 'Unknown'} → {shop?.name || 'Unknown'} ({item.quantity})</span>
                        <button type="button" className="btn-remove" onClick={() => handleRemoveItem(index)}>
                          Remove
                        </button>
                      </div>
                    );
                  })}
                </div>
              </div>

              <div className="modal-actions">
                <button type="submit" className="btn-primary" disabled={formData.items.length === 0}>
                  Create Distribution
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

export default Distributions;
