import React, { useState, useEffect } from 'react';
import deliveryService, { Delivery, UpdateDeliveryDto } from '../services/deliveryService';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import './Deliveries.css';

const Deliveries: React.FC = () => {
  const [deliveries, setDeliveries] = useState<Delivery[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingDelivery, setEditingDelivery] = useState<Delivery | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [formData, setFormData] = useState<UpdateDeliveryDto>({
    verifiedQuantity: 0,
    deliveryStatus: 'Pending',
    notes: '',
  });

  useEffect(() => {
    loadDeliveries();
  }, [pageNumber]);

  const loadDeliveries = async () => {
    try {
      setLoading(true);
      setError('');
      const response = await deliveryService.getAll(pageNumber, 10);
      if (response.success && response.data) {
        setDeliveries(response.data.items);
        setTotalPages(response.data.totalPages);
      }
    } catch (err: any) {
      setError(err.message || 'Failed to load deliveries');
    } finally {
      setLoading(false);
    }
  };

  const handleEdit = (delivery: Delivery) => {
    setEditingDelivery(delivery);
    setFormData({
      verifiedQuantity: delivery.verifiedQuantity,
      deliveryStatus: delivery.deliveryStatus,
      notes: delivery.notes || '',
    });
    setShowModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingDelivery) return;

    try {
      setError('');
      await deliveryService.update(editingDelivery.id, formData);
      setShowModal(false);
      resetForm();
      loadDeliveries();
    } catch (err: any) {
      setError(err.message || 'Failed to update delivery');
    }
  };

  const resetForm = () => {
    setFormData({
      verifiedQuantity: 0,
      deliveryStatus: 'Pending',
      notes: '',
    });
    setEditingDelivery(null);
  };

  const getStatusBadgeClass = (status: string) => {
    const statusMap: { [key: string]: string } = {
      'Pending': 'warning',
      'Completed': 'success',
      'Partial': 'info',
      'Cancelled': 'error',
    };
    return statusMap[status] || 'pending';
  };

  if (loading && deliveries.length === 0) {
    return <LoadingSpinner />;
  }

  return (
    <div className="deliveries-page">
      <div className="page-header">
        <h1>Deliveries</h1>
      </div>

      {error && <ErrorAlert message={error} onClose={() => setError('')} />}

      <table className="deliveries-table">
        <thead>
          <tr>
            <th>Shop</th>
            <th>Delivery Date</th>
            <th>Total Quantity</th>
            <th>Verified Quantity</th>
            <th>Status</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {deliveries.map((delivery) => (
            <tr key={delivery.id}>
              <td>{delivery.shopName}</td>
              <td>{new Date(delivery.deliveryDate).toLocaleDateString()}</td>
              <td>{delivery.totalQuantity}</td>
              <td>{delivery.verifiedQuantity}</td>
              <td>
                <span className={`status-badge ${getStatusBadgeClass(delivery.deliveryStatus)}`}>
                  {delivery.deliveryStatus}
                </span>
              </td>
              <td>
                <button className="btn-edit" onClick={() => handleEdit(delivery)}>
                  Update
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

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

      {showModal && editingDelivery && (
        <div className="modal-overlay" onClick={() => { setShowModal(false); resetForm(); }}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <h2>Update Delivery</h2>
            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Shop</label>
                <input type="text" value={editingDelivery.shopName} disabled />
              </div>
              <div className="form-group">
                <label>Total Quantity</label>
                <input type="number" value={editingDelivery.totalQuantity} disabled />
              </div>
              <div className="form-group">
                <label>Verified Quantity *</label>
                <input
                  type="number"
                  value={formData.verifiedQuantity}
                  onChange={(e) => setFormData({ ...formData, verifiedQuantity: parseInt(e.target.value) || 0 })}
                  required
                  max={editingDelivery.totalQuantity}
                  min="0"
                />
              </div>
              <div className="form-group">
                <label>Delivery Status *</label>
                <select
                  value={formData.deliveryStatus}
                  onChange={(e) => setFormData({ ...formData, deliveryStatus: e.target.value })}
                  required
                >
                  <option value="Pending">Pending</option>
                  <option value="Completed">Completed</option>
                  <option value="Partial">Partial</option>
                  <option value="Cancelled">Cancelled</option>
                </select>
              </div>
              <div className="form-group">
                <label>Notes</label>
                <textarea
                  value={formData.notes}
                  onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                  rows={3}
                />
              </div>
              <div className="modal-actions">
                <button type="submit" className="btn-primary">
                  Update
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

export default Deliveries;
